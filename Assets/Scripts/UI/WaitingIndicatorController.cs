using System.Collections.Generic;
using System.Linq;
using TMPro; // Подключение TextMeshPro
using UnityEngine;
using UnityEngine.UI;

public class WaitingIndicatorController : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab; // Префаб индикатора (Image с типом Filled)
    [SerializeField] private GameObject clientInteractionPanelPrefab; // Префаб панели взаимодействия
    private Dictionary<string, GameObject> activeIndicators = new Dictionary<string, GameObject>(); // Хранит индикаторы по client.clientName
    private ClientManager clientManager;

    void Start()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("Indicator Prefab не назначен в WaitingIndicatorController!");
            return;
        }

        clientManager = ClientManager.Instance;
        if (clientManager == null)
        {
            Debug.LogError("ClientManager.Instance не найден!");
            return;
        }

        // Подписка на событие OnClientWaiting
        clientManager.OnClientWaiting.AddListener(OnClientWaitingChanged);

        // Инициализация существующих клиентов
        InitializeExistingClients();
    }

    void OnDestroy()
    {
        if (clientManager != null && clientManager.OnClientWaiting != null)
        {
            clientManager.OnClientWaiting.RemoveListener(OnClientWaitingChanged);
        }
    }

    void InitializeExistingClients()
    {
        foreach (ClientData client in clientManager.WaitingClients)
        {
            CreateIndicator(client, ClientData.ClientState.Waiting);
        }
        foreach (ClientData client in clientManager.OnChairClients)
        {
            CreateIndicator(client, ClientData.ClientState.OnChair);
        }
    }

    void OnClientWaitingChanged(ClientData client)
    {
        if (client == null) return;

        string clientName = client.clientName;
        if (client.State == ClientData.ClientState.Waiting)
        {
            if (!activeIndicators.ContainsKey(clientName))
            {
                CreateIndicator(client, ClientData.ClientState.Waiting);
            }
        }
        else if (client.State == ClientData.ClientState.OnOccupyChair ||
                 client.State == ClientData.ClientState.MovingToService ||
                 client.State == ClientData.ClientState.Servicing)
        {
            if (activeIndicators.ContainsKey(clientName))
            {
                DestroyIndicator(clientName);
                Debug.Log($"Индикатор ожидания для {client.clientName} удалён при переходе в {client.State}");
            }
        }
        else if (client.State == ClientData.ClientState.OnChair)
        {
            if (!activeIndicators.ContainsKey(clientName))
            {
                CreateIndicator(client, ClientData.ClientState.OnChair);
                Debug.Log($"Создан индикатор ожидания на стуле для {client.clientName} в состоянии OnChair");
            }
        }
        else if (client.State == ClientData.ClientState.Exiting)
        {
            if (activeIndicators.ContainsKey(clientName))
            {
                DestroyIndicator(clientName);
                Debug.Log($"Индикатор для {client.clientName} удалён при переходе в Exiting");
            }
        }
        UpdateIndicators();
    }

    void CreateIndicator(ClientData client, ClientData.ClientState state)
    {
        string clientName = client.clientName;
        if (activeIndicators.ContainsKey(clientName)) return;

        GameObject indicator = Instantiate(indicatorPrefab, transform);
        indicator.name = $"Indicator_{clientName}_{state}";
        activeIndicators[clientName] = indicator;

        // Настройка текстового поля WaitingText
        TMP_Text waitingText = indicator.transform.Find("WaitingText")?.GetComponent<TMP_Text>();
        if (waitingText != null)
        {
            waitingText.text = client.clientName;
            Debug.Log($"Установлено имя {client.clientName} для индикатора клиента {client.clientName} в состоянии {state}");
        }
        else
        {
            Debug.LogWarning($"Компонент WaitingText не найден для индикатора клиента {client.clientName}");
        }

        // Добавление компонента Button и настройка события клика
        Button button = indicator.GetComponent<Button>();
        if (button == null)
        {
            button = indicator.AddComponent<Button>();
        }
        button.onClick.AddListener(() => OnIndicatorClick(client));

        if (state == ClientData.ClientState.Waiting)
        {
            Debug.Log($"Создан индикатор ожидания для {client.clientName} в состоянии Waiting");
        }
        else if (state == ClientData.ClientState.OnChair)
        {
            Debug.Log($"Создан индикатор ожидания на стуле для {client.clientName} в состоянии OnChair");
        }
    }

    void DestroyIndicator(string clientName)
    {
        if (activeIndicators.TryGetValue(clientName, out GameObject indicator))
        {
            Destroy(indicator);
            activeIndicators.Remove(clientName);
            Debug.Log($"Удалён индикатор для клиента {clientName}");
        }
    }

    void OnIndicatorClick(ClientData client)
    {
        if (clientInteractionPanelPrefab != null)
        {
            Time.timeScale = 0; // Пауза игры
            GameObject panelInstance = Instantiate(clientInteractionPanelPrefab, transform.parent); // Используем родителя Canvas
            panelInstance.SetActive(true);
            ClientInteractionController controller = panelInstance.GetComponent<ClientInteractionController>();
            if (controller != null)
            {
                controller.InitializeClient(client); // Передача клиента
                Debug.Log($"Открыта панель взаимодействия для клиента {client.clientName}, игра на паузе");
            }
            else
            {
                Debug.LogError("Компонент ClientInteractionPanelController не найден!");
            }
        }
        else
        {
            Debug.LogError("Префаб ClientInteractionPanel не назначен!");
        }
    }

    void UpdateIndicators()
    {
        var validIndicators = activeIndicators.Where(pair => pair.Value != null).ToList();
        foreach (var pair in validIndicators)
        {
            string clientName = pair.Key;
            GameObject indicator = pair.Value;
            var image = indicator.GetComponent<Image>();
            var waitingText = indicator.transform.Find("WaitingText")?.GetComponent<TMP_Text>();
            if (image != null && waitingText != null)
            {
                var client = FindClientByName(clientName);
                if (client != null)
                {
                    float maxTime = client.State == ClientData.ClientState.Waiting ? clientManager.WaitingTime : clientManager.OnChairTime;
                    float fillAmount = client.waitTime / maxTime;
                    image.fillAmount = Mathf.Clamp01(fillAmount);
                    waitingText.text = client.clientName;
                }
            }
        }
    }
    ClientData FindClientByName(string clientName)
    {
        return clientManager.WaitingClients.FirstOrDefault(c => c.clientName == clientName) ??
               clientManager.OnChairClients.FirstOrDefault(c => c.clientName == clientName);
    }

    void Update()
    {
        UpdateIndicators();
    }
}