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
                    StartCoroutine(WaitingTimer(client));
                }
                else if (client.State == ClientData.ClientState.OnChair && !onChairClients.Contains(client))
                {
                    waitingClients.Remove(client); // Удаляем из Waiting, если был там
                    onChairClients.Add(client);
                    client.waitTime = onChairTime; // Синхронизация waitTime
                    Debug.Log($"Клиент {client.clientName} добавлен в onChairClients, время ожидания {onChairTime}");
                    StartCoroutine(OnChairTimer(client));
                }
                else if (client.State == ClientData.ClientState.Exiting)
                {
                    waitingClients.Remove(client);
                    onChairClients.Remove(client);
                    allClients.Remove(client); // Удаляем из allClients при выходе
                    Debug.Log($"Клиент {client.clientName} удалён из всех списков при Exiting");
                }
            }
        }
        Debug.Log($"Клиентов в waitingClients: {waitingClients.Count}, onChairClients: {onChairClients.Count}, allClients: {allClients.Count}");
    }

    public void OnClientStateChanged(ClientData client)
    {
        Debug.Log($"Обработка события для клиента {client.clientName} с состоянием {client.State}");
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
                StartCoroutine(WaitingTimer(client));
            }
        }
        else if (client.State == ClientData.ClientState.OnChair)
        {
            if (!onChairClients.Contains(client))
            {
                waitingClients.Remove(client); // Удаляем из Waiting, если был там
                onChairClients.Add(client);
                client.waitTime = onChairTime; // Синхронизация waitTime
                Debug.Log($"Клиент {client.clientName} добавлен в ожидание на стуле, время ожидания {onChairTime}");
                StartCoroutine(OnChairTimer(client));
            }
        }
        else if (client.State == ClientData.ClientState.Servicing || client.State == ClientData.ClientState.MovingToService)
        {
            waitingClients.Remove(client);
            onChairClients.Remove(client);
        }
        else if (client.State == ClientData.ClientState.Exiting)
        {
            waitingClients.Remove(client);
            onChairClients.Remove(client);
            if (allClients.Contains(client)) allClients.Remove(client); // Удаляем из allClients при выходе
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
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime} секунд");
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
        while (client.waitTime > 0)
        {
            yield return new WaitForSeconds(1f);
            client.waitTime -= 1f;
            Debug.Log($"Осталось времени для {client.clientName}: {client.waitTime} секунд");
        }
        if (onChairClients.Contains(client) && client.State == ClientData.ClientState.OnChair)
        {
            client.SetState(ClientData.ClientState.Exiting);
            onChairClients.Remove(client);
            Debug.Log($"Клиент {client.clientName} перешёл в Exiting из-за истечения времени на стуле");
        }
    }
}