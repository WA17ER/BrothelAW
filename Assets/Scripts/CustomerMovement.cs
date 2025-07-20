using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private Transform registerPosition; // Точка регистратуры
    [SerializeField] private Transform serviceDestination; // Точка услуги
    [SerializeField] private Transform exitPoint; // Точка выхода
    [SerializeField] private ChairManager chairManager; // Менеджер стульев
    [SerializeField] private Transform visual; // Дочерний объект Visual
    [SerializeField] private float waitTime = 30f; // Время ожидания на стуле

    private NavMeshAgent agent;
    private Chair assignedChair;
    private float waitTimer;
    private bool isServiceChosen;

    private enum CustomerState
    {
        WalkToRegister,
        Waiting,
        OnOccupyChair,
        OnChair,
        OnDestination,
        OnExit
    }
    private CustomerState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null || registerPosition == null || serviceDestination == null || exitPoint == null || chairManager == null || visual == null)
        {
            Debug.LogError($"{gameObject.name}: Missing required components or references.");
            enabled = false;
        }
    }

    private void Start()
    {
        currentState = CustomerState.WalkToRegister;
        MoveTo(registerPosition.position);
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.WalkToRegister:
                if (HasReachedDestination(registerPosition.position))
                {
                    currentState = CustomerState.OnOccupyChair;
                }
                break;

            case CustomerState.Waiting:
                // Ожидание выбора игрока (услуга или стул)
                break;

            case CustomerState.OnOccupyChair:
                if (assignedChair == null)
                {
                    assignedChair = chairManager.GetFreeChair();
                    if (assignedChair == null)
                    {
                        currentState = CustomerState.OnExit;
                        MoveTo(exitPoint.position);
                    }
                    else
                    {
                        MoveTo(assignedChair.BottomPoint.position);
                    }
                }
                else if (HasReachedDestination(assignedChair.BottomPoint.position))
                {
                    currentState = CustomerState.OnChair;
                    visual.position = assignedChair.TopPoint.position; // Перемещение Visual на TopPoint
                    assignedChair.SetState(Chair.ChairState.IsOccupied);
                    waitTimer = waitTime;
                }
                break;

            case CustomerState.OnChair:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    visual.position = assignedChair.BottomPoint.position; // Возврат Visual на BottomPoint
                    currentState = CustomerState.OnExit;
                    chairManager.ReturnChair(assignedChair);
                    assignedChair = null;
                    MoveTo(exitPoint.position);
                }
                break;

            case CustomerState.OnDestination:
                if (HasReachedDestination(serviceDestination.position))
                {
                    currentState = CustomerState.OnExit;
                    MoveTo(exitPoint.position);
                }
                break;

            case CustomerState.OnExit:
                if (HasReachedDestination(exitPoint.position))
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void MoveTo(Vector3 position)
    {
        agent.destination = position;
    }

    private bool HasReachedDestination(Vector3 destination)
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public void ChooseService()
    {
        isServiceChosen = true;
        currentState = CustomerState.OnDestination;
        MoveTo(serviceDestination.position);
    }

    public void ChooseWait()
    {
        isServiceChosen = false;
        currentState = CustomerState.OnOccupyChair;
    }
}