using UnityEngine;

public class Route : MonoBehaviour
{
    public enum State { Default, Waiting, MoveToWaitingSpot, OnWaitingSpot, OnExit, OnDestination }

    [SerializeField] private Transform registerPoint; // Точка регистратуры (Y=0)
    [SerializeField] private Transform spawnPoint; // Точка спавна/ухода (Y=0)
    [SerializeField] private Transform destinationPoint; // Новая точка назначения (Y=0)
    private Chair currentChair; // Текущий стул
    private float registerWaitTimer = 5f; // 10 секунд у RegisterPoint
    private float chairWaitTimer = 10f; // 30 секунд на стуле
    private float currentWaitTime; // Текущее время ожидания
    private State currentState; // Текущее состояние
    private Vector3 currentTargetPoint; // Текущая цель

    void Start()
    {
        if (registerPoint == null || spawnPoint == null || destinationPoint == null)
        {
            Debug.LogError($"{gameObject.name} is missing RegisterPoint, SpawnPoint, or DestinationPoint.");
            enabled = false;
            return;
        }

        currentTargetPoint = registerPoint.position;
        currentState = State.Default;
        Debug.Log($"{gameObject.name} initialized in Default state.");
    }

    public (Vector3 targetPosition, bool shouldMove, bool shouldDestroy, State currentState) RouteHandler(Vector3 currentPosition)
    {
        switch (currentState)
        {
            case State.Default:
                float distanceToRegister = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z), new Vector2(registerPoint.position.x, registerPoint.position.z));
                if (distanceToRegister < 0.1f)
                {
                    currentState = State.Waiting;
                    currentWaitTime = registerWaitTimer;
                    Debug.Log($"{gameObject.name} reached RegisterPoint, switching to Waiting.");
                    return (registerPoint.position, false, false, currentState);
                }
                return (registerPoint.position, true, false, currentState);

            case State.Waiting:
                currentWaitTime -= Time.deltaTime;
                if (currentWaitTime <= 0)
                {
                    currentState = State.OnExit;
                    Debug.Log($"{gameObject.name} wait time expired at RegisterPoint, switching to OnExit.");
                    return (spawnPoint.position, true, false, currentState);
                }
                return (registerPoint.position, false, false, currentState);

            case State.MoveToWaitingSpot:
                if (currentChair != null)
                {
                    float distanceToBottom = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z), new Vector2(currentChair.BottomPoint.position.x, currentChair.BottomPoint.position.z));
                    if (distanceToBottom < 0.1f)
                    {
                        currentState = State.OnWaitingSpot;
                        currentWaitTime = chairWaitTimer;
                        Debug.Log($"{gameObject.name} reached BottomPoint, switching to OnWaitingSpot.");
                        return (currentChair.TopPoint.position, false, false, currentState); // Исправлено: emaryState -> currentState
                    }
                    return (currentChair.BottomPoint.position, true, false, currentState);
                }
                currentState = State.OnExit;
                return (spawnPoint.position, true, false, currentState);

            case State.OnWaitingSpot:
                currentWaitTime -= Time.deltaTime;
                if (currentWaitTime <= 0)
                {
                    if (currentChair != null)
                    {
                        currentChair.FreeChair();
                        currentChair = null;
                        currentState = State.OnExit;
                        Debug.Log($"{gameObject.name} wait time expired on TopPoint, switching to OnExit.");
                        return (spawnPoint.position, true, false, currentState);
                    }
                }
                return (currentChair.TopPoint.position, false, false, currentState);

            case State.OnDestination:
                float distanceToDestination = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z), new Vector2(destinationPoint.position.x, destinationPoint.position.z));
                if (distanceToDestination < 0.1f)
                {
                    currentState = State.OnExit;
                    Debug.Log($"{gameObject.name} reached DestinationPoint, switching to OnExit.");
                    return (spawnPoint.position, true, false, currentState);
                }
                return (destinationPoint.position, true, false, currentState);

            case State.OnExit:
                float distanceToSpawn = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.z), new Vector2(spawnPoint.position.x, spawnPoint.position.z));
                if (distanceToSpawn < 0.1f)
                {
                    Debug.Log($"{gameObject.name} reached SpawnPoint, should destroy.");
                    return (spawnPoint.position, false, true, currentState);
                }
                return (spawnPoint.position, true, false, currentState);
        }

        Debug.LogWarning($"{gameObject.name} RouteHandler reached default case, returning default values.");
        return (spawnPoint.position, true, false, State.Default);
    }

    private Chair FindFreeChair()
    {
        Chair[] chairs = Object.FindObjectsByType<Chair>(FindObjectsSortMode.None);
        foreach (Chair chair in chairs)
        {
            if (chair.IsAvailable)
            {
                return chair;
            }
        }
        return null;
    }

    public void SwitchToDestination()
    {
        if (currentState == State.Waiting || currentState == State.OnWaitingSpot)
        {
            if (currentChair != null)
            {
                currentChair.FreeChair();
                currentChair = null;
            }
            currentState = State.OnDestination;
            currentTargetPoint = destinationPoint.position;
            Debug.Log($"{gameObject.name} switched to OnDestination.");
        }
    }

    public void TryWaitAtChair()
    {
        if (currentState == State.Waiting || currentState == State.OnWaitingSpot)
        {
            if (currentChair != null)
            {
                currentChair.FreeChair();
                currentChair = null;
            }
            Chair freeChair = FindFreeChair();
            if (freeChair != null)
            {
                currentChair = freeChair;
                currentChair.OccupyChair();
                currentState = State.MoveToWaitingSpot;
                currentTargetPoint = currentChair.BottomPoint.position;
                Debug.Log($"{gameObject.name} found free chair {currentChair.name}, switching to MoveToWaitingSpot.");
            }
            else
            {
                currentState = State.OnExit;
                Debug.Log($"{gameObject.name} no free chairs, switching to OnExit.");
            }
        }
    }
}