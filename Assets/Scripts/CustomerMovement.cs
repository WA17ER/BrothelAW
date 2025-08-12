using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    public enum ClientState
    {
        MovingToRegister,
        Waiting,
        OnOccupyChair,
        OnChair,
        MovingToService,
        OnService,
        Servicing,
        Exiting
    }

    public GameObject Visual;
    public ClientState State { get; private set; }
    public float waitTime { get; private set; }
    public float maxWaitTime { get; private set; } = 30f;
    private NavMeshAgent agent;
    private Transform targetChair;
    private Transform exitPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (Visual == null)
        {
            Debug.LogWarning($"Visual not assigned for {gameObject.name}.");
        }
        if (agent != null)
        {
            agent.radius = 0.5f;
            agent.stoppingDistance = 0.5f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        gameObject.layer = LayerMask.NameToLayer("Client");
        waitTime = maxWaitTime;
        State = ClientState.MovingToRegister;
    }

    private void Start()
    {
        exitPoint = GameManager.Instance.ExitPoint;
    }

    private void Update()
    {
        if (State == ClientState.Waiting || State == ClientState.OnOccupyChair)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                SetState(ClientState.Exiting);
            }
        }

        switch (State)
        {
            case ClientState.MovingToRegister:
                if (GameManager.Instance.RegisterPoint != null)
                {
                    agent.SetDestination(GameManager.Instance.RegisterPoint.position);
                    if (Vector3.Distance(transform.position, GameManager.Instance.RegisterPoint.position) < 1f)
                    {
                        SetState(ClientState.Waiting);
                    }
                }
                else
                {
                    Debug.LogWarning($"RegisterPoint not assigned in GameManager for {gameObject.name}.");
                    SetState(ClientState.Waiting);
                }
                break;
            case ClientState.Waiting:
            case ClientState.OnService:
                break;
            case ClientState.OnOccupyChair:
            case ClientState.OnChair:
                if (targetChair != null)
                {
                    agent.SetDestination(targetChair.position);
                }
                break;
            case ClientState.MovingToService:
                if (GameManager.Instance.ServicePoint != null)
                {
                    agent.SetDestination(GameManager.Instance.ServicePoint.position);
                    if (Vector3.Distance(transform.position, GameManager.Instance.ServicePoint.position) < 1f)
                    {
                        if (Visual != null) Visual.SetActive(false);
                        if (agent != null) agent.enabled = false;
                        SetState(ClientState.Servicing);
                    }
                }
                else
                {
                    Debug.LogWarning($"ServicePoint not assigned in GameManager for {gameObject.name}.");
                    SetState(ClientState.Servicing);
                }
                break;
            case ClientState.Exiting:
                agent.SetDestination(exitPoint.position);
                if (Vector3.Distance(transform.position, exitPoint.position) < 1f)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    public void SetState(ClientState newState)
    {
        State = newState;
        GameManager.Instance.onStateChange.Invoke();
    }

    public void SetTargetChair(Transform chair)
    {
        targetChair = chair;
        SetState(ClientState.OnOccupyChair);
    }

    public void ExitService()
    {
        if (Visual != null) Visual.SetActive(true);
        if (agent != null) agent.enabled = true;
        SetState(ClientState.Exiting);
    }

    public void Pause()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    public void Resume()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    [ContextMenu("Назначить сотрудницу")]
    public void AssignEmployee()
    {
        if (State != ClientState.Waiting && State != ClientState.OnOccupyChair)
        {
            Debug.LogWarning($"Клиент {name} не в состоянии Waiting или OnOccupyChair для назначения сотрудницы.");
            return;
        }

        ClientData clientData = GetComponent<ClientData>();
        if (clientData == null)
        {
            Debug.LogError($"ClientData отсутствует на {gameObject.name}.");
            return;
        }

        Employee employee = EmployeeManager.Instance.AvailableEmployees.FirstOrDefault();
        if (employee == null)
        {
            Debug.LogWarning($"Нет доступных сотрудниц для клиента {name}.");
            return;
        }

        float reward = EmployeeManager.Instance.AssignEmployee(clientData, employee);
        GameManager.Instance.CurrentGold += reward;
        SetState(ClientState.MovingToService);
        Debug.Log($"Сотрудница {employee.name} назначена клиенту {name}, начислено золото: {reward}.");
    }

    [ContextMenu("Ожидать")]
    public void SendToChair()
    {
        if (State != ClientState.Waiting)
        {
            Debug.LogWarning($"Клиент {name} не в состоянии Waiting для отправки на стул.");
            return;
        }

        Transform chair = GameManager.Instance.Chairs.FirstOrDefault(c => c.gameObject.activeInHierarchy);
        if (chair == null)
        {
            Debug.LogWarning($"Нет доступных стульев для клиента {name}.");
            return;
        }

        SetTargetChair(chair);
        Debug.Log($"Клиент {name} отправлен на стул.");
    }
}