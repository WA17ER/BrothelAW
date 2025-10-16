using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private GameObject Visual;
    private NavMeshAgent agent;
    private Transform exitPoint;
    private ClientData clientData;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        clientData = GetComponent<ClientData>();
        if (Visual == null) Debug.LogWarning($"Visual not assigned for {name}.");
        if (agent != null)
        {
            agent.radius = 0.3f;
            agent.stoppingDistance = 0.3f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        gameObject.layer = LayerMask.NameToLayer("Client");
    }

    void Start()
    {
        exitPoint = GameManager.Instance?.ExitPoint;
    }

    void Update()
    {
        if (clientData == null) return;
        var state = clientData.State;
        switch (state)
        {
            case ClientData.ClientState.MovingToRegister:
                var registerPoint = GameManager.Instance?.RegisterPoint;
                if (registerPoint != null)
                {
                    agent?.SetDestination(registerPoint.position);
                    if (IsPositionReached(registerPoint.position))
                    {
                        clientData.SetState(ClientData.ClientState.Waiting);
                    }
                }
                else
                {
                    clientData.SetState(ClientData.ClientState.Waiting);
                }
                break;
            case ClientData.ClientState.OnOccupyChair:
                if (clientData.targetChair != null)
                {
                    agent?.SetDestination(clientData.targetChair.position);
                    if (IsPositionReached(clientData.targetChair.position))
                    {
                        clientData.SetState(ClientData.ClientState.OnChair);
                    }
                }
                else
                {
                    clientData.SetState(ClientData.ClientState.Waiting);
                }
                break;
            case ClientData.ClientState.MovingToService:
                var servicePoint = GameManager.Instance?.ServicePoint;
                if (servicePoint != null)
                {
                    agent?.SetDestination(servicePoint.position);
                    if (IsPositionReached(servicePoint.position))
                    {
                        Visual?.SetActive(false);
                        agent.enabled = false;
                        clientData.SetState(ClientData.ClientState.Servicing);
                        GameManager.Instance?.EnterService(this);
                    }
                }
                else
                {
                    clientData.SetState(ClientData.ClientState.Servicing);
                }
                break;
            case ClientData.ClientState.Exiting:
                agent?.SetDestination(exitPoint.position);
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
        Vector3 agentPos = transform.position;
        Vector3 target2D = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 agent2D = new Vector3(agentPos.x, 0, agentPos.z);
        return Vector3.Distance(agent2D, target2D) < (agent?.stoppingDistance ?? 0.3f);
    }

    public void ExitService()
    {
        Visual?.SetActive(true);
        if (agent != null) agent.enabled = true;
        clientData?.ClearChair();
        clientData?.SetState(ClientData.ClientState.Exiting);
    }

    public void Pause()
    {
        if (agent != null) agent.isStopped = true;
    }

    public void Resume()
    {
        if (agent != null) agent.isStopped = false;
    }
}