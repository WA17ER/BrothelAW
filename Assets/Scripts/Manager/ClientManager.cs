using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    [SerializeField] private float waitingTime = 10f;
    [SerializeField] private float onChairTime = 20f;

    [SerializeField] private List<ClientData> waitingClients = new List<ClientData>(); // Список для инспектора
    [SerializeField] private List<ClientData> onChairClients = new List<ClientData>(); // Список для инспектора
    [SerializeField] private List<ClientData> allClients = new List<ClientData>(); // Список для всех клиентов

    public List<ClientData> AllClients => allClients; // Публичный геттер для allClients

    private Dictionary<ClientData, Coroutine> activeTimers = new Dictionary<ClientData, Coroutine>(); // Словарь для отслеживания корутин

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохранение между сценами (опционально)
            Debug.Log("ClientManager инициализирован как Instance");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Подписка будет управляться через GameManager.StartDay
    }

    void Update()
    {
        foreach (ClientData client in new List<ClientData>(allClients)) // Копия для безопасного удаления
        {
            if (client != null)
            {
                Debug.Log($"Проверка клиента {client.clientName} в allClients, состояние: {client.State}");
                if (client.State == ClientData.ClientState.Waiting && !waitingClients.Contains(client))
                {
                    waitingClients.Add(client);
                    client.waitTime = waitingTime; // Синхронизация waitTime
                    Debug.Log($"Клиент {client.clientName} добавлен в waitingClients, время ожидания {waitingTime}");
                    Coroutine timer = StartCoroutine(WaitingTimer(client));
                    activeTimers[client] = timer;
                    Debug.Log($"Запущен WaitingTimer для {client.clientName}");
                }
                else if (client.State == ClientData.ClientState.OnOccupyChair && waitingClients.Contains(client))
                {
                    waitingClients.Remove(client);
                    Debug.Log($"Клиент {client.clientName} в OnOccupyChair, удалён из waitingClients");
                    if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
                    {
                        StopCoroutine(activeTimers[client]);
                        activeTimers.Remove(client);
                        Debug.Log($"Остановлен WaitingTimer для {client.clientName}");
                    }
                }
                else if (client.State == ClientData.ClientState.OnChair && !onChairClients.Contains(client) && !waitingClients.Contains(client))
                {
                    onChairClients.Add(client);
                    client.waitTime = onChairTime; // Синхронизация waitTime
                    Debug.Log($"Клиент {client.clientName} добавлен в onChairClients, время ожидания {onChairTime}");
                    Coroutine timer = StartCoroutine(OnChairTimer(client));
                    activeTimers[client] = timer;
                    Debug.Log($"Запущен OnChairTimer для {client.clientName}");
                }
                else if (client.State == ClientData.ClientState.Exiting)
                {
                    waitingClients.Remove(client);
                    onChairClients.Remove(client);
                    if (allClients.Contains(client)) allClients.Remove(client); // Удаляем из allClients при выходе
                    if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
                    {
                        StopCoroutine(activeTimers[client]);
                        activeTimers.Remove(client); // Очистка activeTimers при уничтожении клиента
                        Debug.Log($"Очищен activeTimers и остановлен таймер для {client.clientName} при Exiting");
                    }
                    Debug.Log($"Клиент {client.clientName} удалён из всех списков при Exiting");
                }
            }
        }
        Debug.Log($"Клиентов в waitingClients: {waitingClients.Count}, onChairClients: {onChairClients.Count}, allClients: {allClients.Count}");
    }

    public void OnClientStateChanged(ClientData client)
    {
        Debug.Log($"Обработка события для клиента {client.clientName} с состоянием {client.State} через OnClientStateChanged");
        UpdateClientState(client);
    }

    public void RegisterClient(ClientData client)
    {
        Debug.Log($"Начало регистрации клиента {client.clientName} с состоянием {client.State}");
        if (!allClients.Contains(client))
        {
            allClients.Add(client);
            Debug.Log($"Клиент {client.clientName} добавлен в allClients, текущее состояние: {client.State}");
        }
    }

    void UpdateClientState(ClientData client)
    {
        Debug.Log($"Обновление состояния клиента {client.clientName}: {client.State}");
        bool hasWaitingOrOnChair = waitingClients.Count > 0 || onChairClients.Count > 0;

        if (client.State == ClientData.ClientState.Waiting)
        {
            if (!waitingClients.Contains(client))
            {
                waitingClients.Add(client);
                client.waitTime = waitingTime; // Синхронизация waitTime
                Debug.Log($"Клиент {client.clientName} добавлен в ожидание, время ожидания {waitingTime}");
                Coroutine timer = StartCoroutine(WaitingTimer(client));
                activeTimers[client] = timer;
                Debug.Log($"Запущен WaitingTimer для {client.clientName}");
            }
        }
        else if (client.State == ClientData.ClientState.OnOccupyChair && waitingClients.Contains(client))
        {
            waitingClients.Remove(client);
            Debug.Log($"Клиент {client.clientName} в OnOccupyChair, удалён из waitingClients через UpdateClientState");
            if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
            {
                StopCoroutine(activeTimers[client]);
                activeTimers.Remove(client);
                Debug.Log($"Остановлен WaitingTimer для {client.clientName} через UpdateClientState");
            }
        }
        else if (client.State == ClientData.ClientState.OnChair)
        {
            if (!onChairClients.Contains(client) && !waitingClients.Contains(client))
            {
                waitingClients.Remove(client); // Удаляем из Waiting, если был там
                onChairClients.Add(client);
                client.waitTime = onChairTime; // Синхронизация waitTime
                Debug.Log($"Клиент {client.clientName} добавлен в ожидание на стуле, время ожидания {onChairTime}");
                Coroutine timer = StartCoroutine(OnChairTimer(client));
                activeTimers[client] = timer;
                Debug.Log($"Запущен OnChairTimer для {client.clientName}");
            }
        }
        else if (client.State == ClientData.ClientState.Servicing || client.State == ClientData.ClientState.MovingToService)
        {
            waitingClients.Remove(client);
            onChairClients.Remove(client);
            if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
            {
                StopCoroutine(activeTimers[client]);
                activeTimers.Remove(client); // Очистка activeTimers при смене на Servicing
                Debug.Log($"Остановлен таймер для {client.clientName} при Servicing");
            }
        }
        else if (client.State == ClientData.ClientState.Exiting)
        {
            waitingClients.Remove(client);
            onChairClients.Remove(client);
            if (allClients.Contains(client)) allClients.Remove(client); // Удаляем из allClients при выходе
            if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
            {
                StopCoroutine(activeTimers[client]);
                activeTimers.Remove(client); // Очистка activeTimers при уничтожении клиента
                Debug.Log($"Очищен activeTimers и остановлен таймер для {client.clientName} при Exiting");
            }
            Debug.Log($"Клиент {client.clientName} удалён из всех списков при Exiting");
        }

        if (!hasWaitingOrOnChair && (waitingClients.Count > 0 || onChairClients.Count > 0))
        {
            GameManager.Instance.onStateChange.Invoke();
        }
        else if (hasWaitingOrOnChair && waitingClients.Count == 0 && onChairClients.Count == 0)
        {
            GameManager.Instance.onStateChange.Invoke();
        }
    }

    IEnumerator WaitingTimer(ClientData client)
    {
        while (client.waitTime > 0)
        {
            yield return new WaitForSeconds(1f);
            client.waitTime -= 1f;
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime} секунд в WaitingTimer");
        }
        if (waitingClients.Contains(client) && client.State == ClientData.ClientState.Waiting)
        {
            client.SetState(ClientData.ClientState.Exiting);
            waitingClients.Remove(client);
            Debug.Log($"WaitingTimer завершён для {client.clientName}, клиент перешёл в Exiting");
        }
    }

    IEnumerator OnChairTimer(ClientData client)
    {
        while (client.waitTime > 0)
        {
            yield return new WaitForSeconds(1f);
            client.waitTime -= 1f;
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime} секунд в OnChairTimer");
        }
        if (onChairClients.Contains(client) && client.State == ClientData.ClientState.OnChair)
        {
            client.SetState(ClientData.ClientState.Exiting);
            onChairClients.Remove(client);
            Debug.Log($"Клиент {client.clientName} перешёл в Exiting из-за истечения времени на стуле");
        }
    }
}