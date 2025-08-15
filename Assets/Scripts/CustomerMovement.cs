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
            agent.radius = 0.5f;
            agent.stoppingDistance = 0.5f;
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
                    if (Vector3.Distance(transform.position, GameManager.Instance.RegisterPoint.position) < 0.5f)
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
                    Debug.Log($"Клиент {clientData.clientName} движется к BottomPoint {clientData.targetChair.parent.name} на позиции {clientData.targetChair.position}");
                    if (Vector3.Distance(transform.position, clientData.targetChair.position) < 0.2f)
                    {
                        Debug.Log($"Клиент {clientData.clientName} достиг BottomPoint стула {clientData.targetChair.parent.name} на позиции {transform.position}");
                        clientData.SetState(ClientData.ClientState.OnChair);
                    }
                }
                else
                {
                    Debug.LogWarning($"Target chair not assigned for {gameObject.name} in OnOccupyChair.");
                    clientData.SetState(ClientData.ClientState.Waiting); // Возврат в Waiting, если стул отсутствует
                }
                break;
            case ClientData.ClientState.OnChair:
                break;
            case ClientData.ClientState.MovingToService:
                if (GameManager.Instance.ServicePoint != null)
                {
                    agent.SetDestination(GameManager.Instance.ServicePoint.position);
                    if (Vector3.Distance(transform.position, GameManager.Instance.ServicePoint.position) < 0.5f)
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
                if (Vector3.Distance(transform.position, exitPoint.position) < 0.5f)
                {
                    clientData.ClearChair();
                    Destroy(gameObject);
                }
                break;
        }
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