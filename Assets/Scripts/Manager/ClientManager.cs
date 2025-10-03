using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
    public List<ClientData> AllClients => allClients.AsReadOnly().ToList(); // Read-only view

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
        var clientsCopy = allClients.ToList();
        foreach (ClientData client in clientsCopy)
        {
            if (client == null) continue;
            UpdateClientState(client);
        }
    }

    public void OnClientStateChanged(ClientData client)
    {
        if (client != null) UpdateClientState(client);
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
        if (client == null) return;
        bool hasWaitingOrOnChair = waitingClients.Count > 0 || onChairClients.Count > 0;
        var state = client.State;
        Coroutine timer = null;
        switch (state)
        {
            case ClientData.ClientState.Waiting when !waitingClients.Contains(client):
                waitingClients.Add(client);
                client.waitTime = waitingTime;
                timerProgress[client] = waitingTime;
                timer = StartCoroutine(WaitingTimer(client));
                activeTimers[client] = timer;
                OnClientWaiting?.Invoke(client);
                break;
            case ClientData.ClientState.OnOccupyChair when waitingClients.Contains(client):
                waitingClients.Remove(client);
                client.waitTime = onChairTime;
                timerProgress[client] = onChairTime;
                if (activeTimers.TryGetValue(client, out timer))
                {
                    StopCoroutine(timer);
                }
                activeTimers.Remove(client);
                OnClientWaiting?.Invoke(client);
                break;
            case ClientData.ClientState.OnChair when !onChairClients.Contains(client):
                waitingClients.Remove(client);
                onChairClients.Add(client);
                client.waitTime = onChairTime;
                timerProgress[client] = onChairTime;
                if (!activeTimers.ContainsKey(client))
                {
                    timer = StartCoroutine(OnChairTimer(client));
                    activeTimers[client] = timer;
                }
                OnClientWaiting?.Invoke(client);
                break;
            case ClientData.ClientState.MovingToService or ClientData.ClientState.Servicing:
                waitingClients.Remove(client);
                onChairClients.Remove(client);
                if (activeTimers.TryGetValue(client, out timer))
                {
                    StopCoroutine(timer);
                }
                activeTimers.Remove(client);
                timerProgress.Remove(client);
                OnClientWaiting?.Invoke(client);
                break;
            case ClientData.ClientState.Exiting:
                waitingClients.Remove(client);
                onChairClients.Remove(client);
                allClients.Remove(client);
                if (activeTimers.TryGetValue(client, out timer))
                {
                    StopCoroutine(timer);
                }
                activeTimers.Remove(client);
                timerProgress.Remove(client);
                OnClientWaiting?.Invoke(client);
                break;
        }
        if (hasWaitingOrOnChair != (waitingClients.Count > 0 || onChairClients.Count > 0))
        {
            GameManager.Instance.onStateChange.Invoke();
        }
    }

    IEnumerator WaitingTimer(ClientData client)
    {
        while (timerProgress.ContainsKey(client) && timerProgress[client] > 0)
        {
            timerProgress[client] -= Time.deltaTime;
            client.waitTime = timerProgress[client];
            yield return null;
        }
        if (waitingClients.Contains(client) && client.State == ClientData.ClientState.Waiting)
        {
            client.SetState(ClientData.ClientState.Exiting);
            waitingClients.Remove(client);
        }
    }

    IEnumerator OnChairTimer(ClientData client)
    {
        while (timerProgress.ContainsKey(client) && timerProgress[client] > 0)
        {
            timerProgress[client] -= Time.deltaTime;
            client.waitTime = timerProgress[client];
            yield return null;
        }
        if (onChairClients.Contains(client) && client.State == ClientData.ClientState.OnChair)
        {
            client.SetState(ClientData.ClientState.Exiting);
            onChairClients.Remove(client);
        }
    }
}