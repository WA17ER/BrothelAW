using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    public GameObject Visual;
    private NavMeshAgent agent;
    private Transform exitPoint;
    private ClientData clientData;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        clientData = GetComponent<ClientData>();
        if (Visual == null)
        {
            Debug.LogWarning($"Visual not assigned for {gameObject.name}.");
        }
        if (agent != null)
        {
            agent.radius = 0.3f; // Уменьшен радиус агента для лучшей навигации
            agent.stoppingDistance = 0.3f; // Увеличен stoppingDistance для учета отклонений
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        gameObject.layer = LayerMask.NameToLayer("Client");
    }

    private void Start()
    {
        exitPoint = GameManager.Instance.ExitPoint;
    }

    private void Update()
    {
        switch (clientData.State)
        {
            case ClientData.ClientState.MovingToRegister:
                if (GameManager.Instance.RegisterPoint != null)
                {
                    agent.SetDestination(GameManager.Instance.RegisterPoint.position);
                    if (IsPositionReached(GameManager.Instance.RegisterPoint.position))
                    {
                        clientData.SetState(ClientData.ClientState.Waiting);
                    }
                }
                else
                {
                    Debug.LogWarning($"RegisterPoint not assigned in GameManager for {gameObject.name}.");
                    clientData.SetState(ClientData.ClientState.Waiting);
                }
                break;
            case ClientData.ClientState.Waiting:
                break;
            case ClientData.ClientState.OnOccupyChair:
                if (clientData.targetChair != null)
                {
                    agent.SetDestination(clientData.targetChair.position);
                    if (IsPositionReached(clientData.targetChair.position))
                    {
                        Debug.Log($"Клиент {clientData.clientName} достиг targetChair {clientData.targetChair.parent.name} на позиции {transform.position}");
                        clientData.SetState(ClientData.ClientState.OnChair);
                    }
                    else if (Vector3.Distance(new Vector3(agent.destination.x, 0, agent.destination.z),
                                              new Vector3(clientData.targetChair.position.x, 0, clientData.targetChair.position.z)) > 0.5f)
                    {
                        Debug.LogWarning($"Значительное расхождение в X/Z для клиента {clientData.clientName}, destination: {agent.destination}, target: {clientData.targetChair.position}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Target chair not assigned for {gameObject.name}.");
                    clientData.SetState(ClientData.ClientState.Waiting);
                }
                break;
            case ClientData.ClientState.OnChair:
                break;
            case ClientData.ClientState.MovingToService:
                if (GameManager.Instance.ServicePoint != null)
                {
                    agent.SetDestination(GameManager.Instance.ServicePoint.position);
                    if (IsPositionReached(GameManager.Instance.ServicePoint.position))
                    {
                        if (Visual != null) Visual.SetActive(false);
                        if (agent != null) agent.enabled = false;
                        clientData.SetState(ClientData.ClientState.Servicing);
                        GameManager.Instance.EnterService(this);
                    }
                }
                else
                {
                    Debug.LogWarning($"ServicePoint not assigned in GameManager for {gameObject.name}.");
                    clientData.SetState(ClientData.ClientState.Servicing);
                }
                break;
            case ClientData.ClientState.Servicing:
                break;
            case ClientData.ClientState.Exiting:
                agent.SetDestination(exitPoint.position);
                if (IsPositionReached(exitPoint.position))
                {
                    clientData.ClearChair();
                    Destroy(gameObject);
                }
                break;
        }
    }

    private bool IsPositionReached(Vector3 targetPosition)
    {
        Vector3 agentPosition = transform.position;
        Vector3 target2D = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 agent2D = new Vector3(agentPosition.x, 0, agentPosition.z);
        return Vector3.Distance(agent2D, target2D) < agent.stoppingDistance;
    }

    public void ExitService()
    {
        if (Visual != null) Visual.SetActive(true);
        if (agent != null) agent.enabled = true;
        clientData.ClearChair();
        clientData.SetState(ClientData.ClientState.Exiting);
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
}