using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    [SerializeField] private float waitingTime = 10f;
    [SerializeField] private float onChairTime = 20f;

    public float WaitingTime => waitingTime; // Публичный геттер для waitingTime
    public float OnChairTime => onChairTime; // Публичный геттер для onChairTime

    [SerializeField] private List<ClientData> waitingClients = new List<ClientData>(); // Список для инспектора
    [SerializeField] private List<ClientData> onChairClients = new List<ClientData>(); // Список для инспектора
    [SerializeField] private List<ClientData> allClients = new List<ClientData>(); // Список для всех клиентов

    public List<ClientData> WaitingClients => waitingClients; // Геттер для waitingClients
    public List<ClientData> OnChairClients => onChairClients; // Геттер для onChairClients
    public List<ClientData> AllClients => allClients; // Публичный геттер для allClients

    public UnityEvent<ClientData> OnClientWaiting; // Новое событие для изменений в списках ожидания

    private Dictionary<ClientData, Coroutine> activeTimers = new Dictionary<ClientData, Coroutine>(); // Словарь для отслеживания корутин
    private Dictionary<ClientData, float> timerProgress = new Dictionary<ClientData, float>(); // Отслеживание прогресса таймера

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
                    timerProgress[client] = waitingTime; // Инициализация прогресса таймера
                    Debug.Log($"Клиент {client.clientName} добавлен в Waiting, индикатор ожидания создан, стартовал WaitingTimer");
                    Coroutine timer = StartCoroutine(WaitingTimer(client));
                    activeTimers[client] = timer;
                    OnClientWaiting?.Invoke(client); // Вызов события при добавлении
                }
                else if (client.State == ClientData.ClientState.OnOccupyChair && waitingClients.Contains(client))
                {
                    waitingClients.Remove(client);
                    client.waitTime = onChairTime; // Сброс waitTime для нового таймера
                    timerProgress[client] = onChairTime; // Сброс прогресса таймера
                    Debug.Log($"Клиент {client.clientName} перешёл в OnOccupyChair, индикатор ожидания удалён, WaitingTimer остановлен");
                    if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
                    {
                        StopCoroutine(activeTimers[client]);
                        activeTimers.Remove(client);
                    }
                    OnClientWaiting?.Invoke(client); // Вызов события при удалении
                }
                else if (client.State == ClientData.ClientState.OnChair && !onChairClients.Contains(client))
                {
                    waitingClients.Remove(client); // Удаляем из Waiting, если был там
                    onChairClients.Add(client);
                    client.waitTime = onChairTime; // Синхронизация waitTime
                    timerProgress[client] = onChairTime; // Инициализация прогресса таймера для OnChair
                    if (!activeTimers.ContainsKey(client)) // Запускаем OnChairTimer только если его нет
                    {
                        Coroutine timer = StartCoroutine(OnChairTimer(client));
                        activeTimers[client] = timer;
                        Debug.Log($"Клиент {client.clientName} достиг OnChair, создан индикатор ожидания на стуле, стартовал OnChairTimer");
                    }
                    OnClientWaiting?.Invoke(client); // Вызов события при добавлении
                }
                else if (client.State == ClientData.ClientState.MovingToService || client.State == ClientData.ClientState.Servicing)
                {
                    waitingClients.Remove(client);
                    onChairClients.Remove(client);
                    if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
                    {
                        StopCoroutine(activeTimers[client]);
                        activeTimers.Remove(client); // Очистка activeTimers
                        timerProgress.Remove(client); // Очистка прогресса таймера
                        Debug.Log($"Клиент {client.clientName} направлен к услуге, индикатор удалён, таймер остановлен");
                    }
                    OnClientWaiting?.Invoke(client); // Вызов события при удалении
                }
                else if (client.State == ClientData.ClientState.Exiting)
                {
                    waitingClients.Remove(client);
                    onChairClients.Remove(client);
                    allClients.Remove(client); // Удаляем из allClients при выходе
                    if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
                    {
                        StopCoroutine(activeTimers[client]);
                        activeTimers.Remove(client); // Очистка activeTimers при уничтожении клиента
                        timerProgress.Remove(client); // Очистка прогресса таймера
                        Debug.Log($"Клиент {client.clientName} удалён из сцены, индикатор удалён, таймер остановлен при Exiting");
                    }
                    OnClientWaiting?.Invoke(client); // Вызов события при удалении
                }
            }
        }        
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
                timerProgress[client] = waitingTime; // Инициализация прогресса таймера
                Debug.Log($"Клиент {client.clientName} добавлен в Waiting, индикатор ожидания создан, стартовал WaitingTimer");
                Coroutine timer = StartCoroutine(WaitingTimer(client));
                activeTimers[client] = timer;
                OnClientWaiting?.Invoke(client); // Вызов события при добавлении
            }
        }
        else if (client.State == ClientData.ClientState.OnOccupyChair && waitingClients.Contains(client))
        {
            waitingClients.Remove(client);
            client.waitTime = onChairTime; // Сброс waitTime для нового таймера
            timerProgress[client] = onChairTime; // Сброс прогресса таймера
            Debug.Log($"Клиент {client.clientName} перешёл в OnOccupyChair, индикатор ожидания удалён, WaitingTimer остановлен");
            if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
            {
                StopCoroutine(activeTimers[client]);
                activeTimers.Remove(client);
            }
            OnClientWaiting?.Invoke(client); // Вызов события при удалении
        }
        else if (client.State == ClientData.ClientState.OnChair)
        {
            if (!onChairClients.Contains(client))
            {
                waitingClients.Remove(client); // Удаляем из Waiting, если был там
                onChairClients.Add(client);
                client.waitTime = onChairTime; // Синхронизация waitTime
                timerProgress[client] = onChairTime; // Инициализация прогресса таймера для OnChair
                if (!activeTimers.ContainsKey(client)) // Запускаем OnChairTimer только если его нет
                {
                    Coroutine timer = StartCoroutine(OnChairTimer(client));
                    activeTimers[client] = timer;
                    Debug.Log($"Клиент {client.clientName} достиг OnChair, создан индикатор ожидания на стуле, стартовал OnChairTimer");
                }
                OnClientWaiting?.Invoke(client); // Вызов события при добавлении
            }
        }
        else if (client.State == ClientData.ClientState.MovingToService || client.State == ClientData.ClientState.Servicing)
        {
            waitingClients.Remove(client);
            onChairClients.Remove(client);
            if (activeTimers.ContainsKey(client) && activeTimers[client] != null)
            {
                StopCoroutine(activeTimers[client]);
                activeTimers.Remove(client); // Очистка activeTimers
                timerProgress.Remove(client); // Очистка прогресса таймера
                Debug.Log($"Клиент {client.clientName} направлен к услуге, индикатор удалён, таймер остановлен");
            }
            OnClientWaiting?.Invoke(client); // Вызов события при удалении
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
                timerProgress.Remove(client); // Очистка прогресса таймера
                Debug.Log($"Клиент {client.clientName} удалён из сцены, индикатор удалён, таймер остановлен при Exiting");
            }
            OnClientWaiting?.Invoke(client); // Вызов события при удалении
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
        while (timerProgress.ContainsKey(client) && timerProgress[client] > 0)
        {
            yield return null; // Обновление на каждом кадре
            timerProgress[client] -= Time.deltaTime;
            client.waitTime = timerProgress[client]; // Синхронизация waitTime
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime:F2} секунд в WaitingTimer");
        }
        if (waitingClients.Contains(client) && client.State == ClientData.ClientState.Waiting)
        {
            client.SetState(ClientData.ClientState.Exiting);
            waitingClients.Remove(client);
            Debug.Log($"Клиент {client.clientName} перешёл в Exiting из-за истечения времени ожидания");
        }
    }

    IEnumerator OnChairTimer(ClientData client)
    {
        while (timerProgress.ContainsKey(client) && timerProgress[client] > 0)
        {
            yield return null; // Обновление на каждом кадре
            timerProgress[client] -= Time.deltaTime;
            client.waitTime = timerProgress[client]; // Синхронизация waitTime
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime:F2} секунд в OnChairTimer");
        }
        if (onChairClients.Contains(client) && client.State == ClientData.ClientState.OnChair)
        {
            client.SetState(ClientData.ClientState.Exiting);
            onChairClients.Remove(client);
            Debug.Log($"Клиент {client.clientName} перешёл в Exiting из-за истечения времени на стуле");
        }
    }
}