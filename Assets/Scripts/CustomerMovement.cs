using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private Transform registerPosition; // Позиция регистрации
    [SerializeField] private Transform serviceDestination; // Позиция услуги
    [SerializeField] private Transform exitPoint; // Точка выхода
    [SerializeField] private ChairManager chairManager; // Менеджер стульев
    [SerializeField] private ServiceController serviceController; // Контроллер услуги

    public UnityEvent onDecisionMade; // Событие при принятии решения

    private NavMeshAgent agent;
    private CustomerState currentState = CustomerState.WalkToRegister;
    private Chair currentChair;
    private Transform visual;
    private bool isDecisionMade = false;
    private Coroutine waitingCoroutine;
    private Coroutine chairCoroutine;

    private enum CustomerState
    {
        WalkToRegister, // Идёт к регистрации
        Waiting, // Ожидание
        OnOccupyChair, // Занимает стул
        OnChair, // На стуле
        MoveToService, // Идёт к услуге
        OnService, // На услуге
        OnExit // На выход
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visual = transform.Find("Visual");
        if (agent == null || registerPosition == null || serviceDestination == null || exitPoint == null || chairManager == null || visual == null || serviceController == null)
        {
            Debug.LogError("CustomerMovement: Отсутствуют компоненты или ссылки.");
            enabled = false;
        }
    }

    private void Start()
    {
        SetDestination(registerPosition.position);
        LogState();
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkToRegister:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.Waiting);
                    waitingCoroutine = StartCoroutine(WaitForDecision(5f));
                }
                break;

            case CustomerState.OnOccupyChair:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.OnChair);
                    if (currentChair != null)
                    {
                        currentChair.SetState(Chair.ChairState.IsOccupied);
                        SeatVisualOnChair();
                    }
                    chairCoroutine = StartCoroutine(WaitOnChair(5f));
                }
                break;

            case CustomerState.MoveToService:
                if (HasReachedDestination())
                {
                    ChangeState(CustomerState.OnService);
                    agent.enabled = false;
                    serviceController.EnterService(this);
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
        yield return new WaitForSeconds(time);
        if (!isDecisionMade)
        {
            Debug.Log("Решение не принято, клиент уходит.");
            SetDestination(exitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private IEnumerator WaitOnChair(float time)
    {
        yield return new WaitForSeconds(time);
        if (currentState == CustomerState.OnChair)
        {
            Debug.Log("Время ожидания на стуле истекло, клиент расстроен и уходит.");
            if (currentChair != null)
            {
                ReturnVisualFromChair();
                chairManager.ReturnChair(currentChair);
                currentChair = null;
            }
            agent.enabled = true;
            SetDestination(exitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private void SetDestination(Vector3 position)
    {
        agent.SetDestination(position);
    }

    private bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f && !agent.hasPath;
    }

    [ContextMenu("Отправить на стул")]
    public void SendToChair()
    {
        if (currentState == CustomerState.Waiting)
        {
            Debug.Log("Отправка клиента на стул.");
            isDecisionMade = true;
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
            }
            OccupyChair();
            onDecisionMade?.Invoke();
        }
        else
        {
            Debug.LogError("Нельзя отправить на стул: Недопустимое состояние.");
        }
    }

    [ContextMenu("Отправить на услугу")]
    public void SendToService()
    {
        if (currentState == CustomerState.Waiting || currentState == CustomerState.OnChair)
        {
            Debug.Log("Отправка клиента на услугу.");
            isDecisionMade = true;
            if (currentState == CustomerState.Waiting && waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
            }
            else if (currentState == CustomerState.OnChair)
            {
                if (chairCoroutine != null)
                {
                    StopCoroutine(chairCoroutine);
                }
                if (currentChair != null)
                {
                    ReturnVisualFromChair();
                    chairManager.ReturnChair(currentChair);
                    currentChair = null;
                }
                agent.enabled = true;
            }
            SetDestination(serviceDestination.position);
            ChangeState(CustomerState.MoveToService);
            onDecisionMade?.Invoke();
        }
        else
        {
            Debug.LogError("Нельзя отправить на услугу: Недопустимое состояние.");
        }
    }

    private void OccupyChair()
    {
        currentChair = chairManager.GetFreeChair();
        if (currentChair != null)
        {
            SetDestination(currentChair.BottomPoint.position);
            ChangeState(CustomerState.OnOccupyChair);
        }
        else
        {
            Debug.Log("Нет свободного стула, клиент уходит.");
            SetDestination(exitPoint.position);
            ChangeState(CustomerState.OnExit);
        }
    }

    private void SeatVisualOnChair()
    {
        agent.enabled = false;
        visual.SetParent(currentChair.TopPoint);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        Debug.Log("Визуал посажен на стул");
    }

    private void ReturnVisualFromChair()
    {
        visual.SetParent(transform);
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        Debug.Log("Визуал возвращён");
    }

    public void ExitService()
    {
        agent.enabled = true;
        SetDestination(exitPoint.position);
        ChangeState(CustomerState.OnExit);
    }

    private void ChangeState(CustomerState newState)
    {
        currentState = newState;
        LogState();
    }

    private void LogState()
    {
        Debug.Log($"Клиент {name} состояние: {currentState}");
    }

    public Transform Visual => visual;
}