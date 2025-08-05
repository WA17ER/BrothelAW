using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CustomerMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private CustomerState currentState = CustomerState.WalkToRegister;
    public Chair currentChair;
    private Transform visual;
    private bool isDecisionMade = false;
    public Coroutine waitingCoroutine;
    public Coroutine chairCoroutine;
    private ClientData clientRequest;
    private bool isWaitingEntered = false;
    private bool isPaused = false;

    public enum CustomerState
    {
        WalkToRegister,
        Waiting,
        OnOccupyChair,
        OnChair,
        MoveToService,
        OnService,
        OnExit
    }

    public UnityEvent onDecisionMade;
    public UnityEvent onEnterWaiting;

    public CustomerState CurrentState => currentState;
    public Transform Visual => visual;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visual = transform.Find("Visual");
        clientRequest = GetComponent<ClientData>();
        ClientData[] clientRequests = GetComponents<ClientData>();
        if (clientRequests.Length > 1)
        {
            Debug.LogWarning($"Найдено {clientRequests.Length} компонентов ClientData на {gameObject.name}. Оставлен только первый.");
            for (int i = 1; i < clientRequests.Length; i++)
            {
                Destroy(clientRequests[i]);
            }
        }
        if (agent == null || visual == null || clientRequest == null || NavigationManager.Instance == null || ChairManager.Instance == null)
        {
            Debug.LogWarning($"CustomerMovement: Отсутствуют компоненты или ссылки на {gameObject.name}.");
            return;
        }
        if (NavigationManager.Instance.RegisterPosition == null || NavigationManager.Instance.ServiceDestination == null || NavigationManager.Instance.ExitPoint == null)
        {
            Debug.LogWarning($"CustomerMovement: Точки навигации не назначены в NavigationManager для {gameObject.name}.");
            return;
        }
        agent.avoidancePriority = Random.Range(0, 100);
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        Debug.Log($"Клиент {name} настроен: avoidancePriority = {agent.avoidancePriority}, obstacleAvoidanceType = {agent.obstacleAvoidanceType}");
    }

    private void Start()
    {
        SetDestination(NavigationManager.Instance.RegisterPosition.position);
    }

    private void Update()
    {
        if (isPaused || GameManager.Instance.IsDayPaused)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }
        if (agent.enabled)
        {
            agent.isStopped = false;
        }

        switch (currentState)
        {
            case CustomerState.WalkToRegister:
                if (HasReachedDestination() && !isWaitingEntered)
                {
                    ChangeState(CustomerState.Waiting);
                    isWaitingEntered = true;
                }
                break;
            case CustomerState.OnOccupyChair:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.OnChair);
                    if (currentChair != null)
                    {
                        currentChair.SetState(Chair.ChairState.IsOccupied);
                    }
                    chairCoroutine = StartCoroutine(WaitOnChair(5f));
                }
                break;
            case CustomerState.MoveToService:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.OnService);
                    if (GameManager.Instance != null)
                    {
                        agent.enabled = false;
                        GameManager.Instance.EnterService(this);
                    }
                    else
                    {
                        Debug.LogWarning($"GameManager не найден для клиента {name}.");
                        ForceExit();
                    }
                }
                break;
            case CustomerState.OnExit:
                if (HasReachedDestination())
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    private IEnumerator WaitForDecision(float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (isPaused || GameManager.Instance.IsDayPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!isDecisionMade)
        {
            Debug.Log($"Решение для клиента {name} не принято, клиент уходит.");
            SetDestination(NavigationManager.Instance.ExitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private IEnumerator WaitOnChair(float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (isPaused || GameManager.Instance.IsDayPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (currentState == CustomerState.OnChair)
        {
            Debug.Log($"Время ожидания на стуле истекло для клиента {name}, клиент уходит.");
            if (currentChair != null)
            {
                ChairManager.Instance.ReturnChair(currentChair);
                currentChair = null;
            }
            agent.enabled = true;
            SetDestination(NavigationManager.Instance.ExitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private void SetDestination(Vector3 position)
    {
        if (agent.enabled)
        {
            agent.SetDestination(position);
            Debug.Log($"Клиент {name} направляется к {position}.");
        }
    }

    private bool HasReachedDestination()
    {
        if (!agent.enabled)
        {
            return false;
        }
        bool reached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f && !agent.hasPath;
        if (reached)
        {
            Debug.Log($"Клиент {name} достиг точки {agent.destination}.");
        }
        return reached;
    }

    [ContextMenu("Отправить на стул")]
    public void SendToChair()
    {
        if (currentState == CustomerState.Waiting)
        {
            Debug.Log($"Отправка клиента {name} на стул.");
            isDecisionMade = true;
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
            OccupyChair();
            onDecisionMade?.Invoke();
        }
        else
        {
            Debug.LogError($"Нельзя отправить на стул: Недопустимое состояние ({currentState}) для клиента {name}.");
        }
    }

    public void SendToService()
    {
        Debug.Log($"SendToService called, CurrentState = {currentState}, isDecisionMade = true for client {name}.");
        if (currentState == CustomerState.Waiting || currentState == CustomerState.OnChair)
        {
            isDecisionMade = true;
            if (currentState == CustomerState.Waiting && waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
            else if (currentState == CustomerState.OnChair)
            {
                if (chairCoroutine != null)
                {
                    StopCoroutine(chairCoroutine);
                    chairCoroutine = null;
                }
                if (currentChair != null)
                {
                    ChairManager.Instance.ReturnChair(currentChair);
                    currentChair = null;
                }
                agent.enabled = true;
            }
            SetDestination(NavigationManager.Instance.ServiceDestination.position);
            ChangeState(CustomerState.MoveToService);
            onDecisionMade?.Invoke();
        }
        else
        {
            Debug.LogError($"Нельзя отправить на услугу: Недопустимое состояние ({currentState}) для клиента {name}.");
        }
    }

    public void ForceExit()
    {
        if (currentState == CustomerState.Waiting || currentState == CustomerState.OnChair)
        {
            Debug.Log($"Принудительный выход клиента {name}.");
            if (currentState == CustomerState.Waiting && waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
            else if (currentState == CustomerState.OnChair)
            {
                if (chairCoroutine != null)
                {
                    StopCoroutine(chairCoroutine);
                    chairCoroutine = null;
                }
                if (currentChair != null)
                {
                    ChairManager.Instance.ReturnChair(currentChair);
                    currentChair = null;
                }
                agent.enabled = true;
            }
            SetDestination(NavigationManager.Instance.ExitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
        else
        {
            Debug.LogError($"Нельзя принудительно выйти: Недопустимое состояние ({currentState}) для клиента {name}.");
        }
    }

    public void ExitService()
    {
        agent.enabled = true;
        SetDestination(NavigationManager.Instance.ExitPoint.position);
        ChangeState(CustomerState.OnExit);
    }

    private void OccupyChair()
    {
        currentChair = ChairManager.Instance.GetFreeChair();
        if (currentChair != null)
        {
            SetDestination(currentChair.BottomPoint.position);
            ChangeState(CustomerState.OnOccupyChair);
        }
        else
        {
            Debug.Log($"Нет свободного стула, клиент {name} уходит.");
            SetDestination(NavigationManager.Instance.ExitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private void ChangeState(CustomerState newState)
    {
        currentState = newState;
        if (newState == CustomerState.Waiting)
        {
            waitingCoroutine = StartCoroutine(WaitForDecision(10f));
            onEnterWaiting?.Invoke();
        }
        Debug.Log($"Клиент {name} перешёл в состояние {currentState}.");
    }

    public void Pause()
    {
        isPaused = true;
        if (agent.enabled)
        {
            agent.isStopped = true;
        }
        Debug.Log($"Клиент {name} поставлен на паузу.");
    }

    public void Resume()
    {
        isPaused = false;
        if (agent.enabled)
        {
            agent.isStopped = false;
        }
        Debug.Log($"Клиент {name} возобновлён.");
    }
}