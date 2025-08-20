using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientInteractionController : MonoBehaviour
{
    public ClientData currentClient; // Клиент, чей индикатор был нажат
    public Button waitButton;
    public Button assignButton;
    public Button cancelButton;
    public TMP_Text expectedEmployeeText, expectedRaceText, expectedBodyText, expectedBreastText, desiredServiceText, goldText; // Текстовые поля клиента
    public Image employeeIcon; // Иконка сотрудницы
    public TMP_Text employeeNameText, employeeRaceText, employeeBodyText, employeeBreastText, employeeServiceText, rewardText; // Текстовые поля сотрудницы
    public Image employeePortrait; // Портрет сотрудницы

    void Start()
    {
        if (waitButton != null)
        {
            waitButton.onClick.AddListener(OnWaitClick);
        }
        if (assignButton != null)
        {
            assignButton.onClick.AddListener(OnAssignClick);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClick);
        }
        InitializePanel(); // Инициализация панели при старте
    }

    public void InitializeClient(ClientData client)
    {
        currentClient = client;
        if (currentClient == null)
        {
            Debug.LogError("Клиент не передан в ClientInteractionController!");
        }
        else
        {
            InitializePanel(); // Обновление данных при получении клиента
            Debug.Log($"Инициализирован клиент {currentClient.clientName} в панели");
        }
    }

    void InitializePanel()
    {
        if (currentClient != null && employeeIcon != null && expectedEmployeeText != null && expectedRaceText != null &&
            expectedBodyText != null && expectedBreastText != null && desiredServiceText != null && goldText != null &&
            employeePortrait != null && employeeNameText != null && employeeRaceText != null &&
            employeeBodyText != null && employeeBreastText != null && employeeServiceText != null && rewardText != null)
        {
            // Обновление текстовых полей клиента
            expectedEmployeeText.text = "Ожидаемая сотрудница: " + (currentClient.clientType == 4 && currentClient.SpecificEmployee != null ? currentClient.SpecificEmployee.Data.employeeName : "");
            expectedRaceText.text = "Ожидаемая раса: " + (currentClient.clientType == 3 ? (currentClient.preferredRace != null ? currentClient.preferredRace.ToString() : "") : "");
            expectedBodyText.text = "Ожидаемое тело: " + (currentClient.preferredBodyType != null ? currentClient.preferredBodyType.ToString() : "");
            expectedBreastText.text = "Ожидаемый размер груди: " + (currentClient.preferredBreastSize != null ? currentClient.preferredBreastSize.ToString() : "");
            desiredServiceText.text = "Желаемая услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            goldText.text = "Золото: " + currentClient.clientGold;

            // Установка дефолтного спрайта для иконки клиента
            if (employeeIcon != null && currentClient.SpecificEmployee == null)
            {
                employeeIcon.sprite = Resources.Load<Sprite>("NoData_Square"); // Дефолтный спрайт
            }

            // Обновление текстовых полей сотрудницы (дефолтные значения)
            employeeNameText.text = "Имя: None";
            employeeRaceText.text = "Раса: None";
            employeeBodyText.text = "Тип тела: None";
            employeeBreastText.text = "Размер груди: None";
            employeeServiceText.text = "Услуга (уровень): None";
            rewardText.text = "Стоимость: None";

            // Установка дефолтного спрайта для портрета сотрудницы
            if (employeePortrait != null)
            {
                employeePortrait.sprite = Resources.Load<Sprite>("NoData_Square"); // Дефолтный спрайт
            }

            Debug.Log($"Панель клиента {currentClient.clientName} и сотрудницы обновлена");
        }
        else
        {
            Debug.LogWarning("Одна или несколько ссылок на UI-элементы в ClientInteractionController равны null!");
        }
    }

    void OnWaitClick()
    {
        if (currentClient != null)
        {
            currentClient.SendToChair();
            Time.timeScale = 1; // Снятие паузы
            gameObject.SetActive(false); // Закрытие панели
            Debug.Log($"Кнопка Ожидать нажата для клиента {currentClient.clientName}, клиент отправлен на стул");
        }
    }

    void OnAssignClick()
    {
        if (currentClient != null)
        {
            // Временная реализация с выбором сотрудницы из Inspector
            currentClient.AssignEmployee(); // Пока без конкретной сотрудницы
            Time.timeScale = 1; // Снятие паузы
            gameObject.SetActive(false); // Закрытие панели
            Debug.Log($"Кнопка Назначить сотрудницу нажата для клиента {currentClient.clientName}, клиент отправлен на услугу");
        }
    }

    void OnCancelClick()
    {
        Time.timeScale = 1; // Снятие паузы
        gameObject.SetActive(false); // Закрытие панели
        Debug.Log($"Кнопка Отмена нажата, панель закрыта для клиента {currentClient?.clientName ?? "неизвестного клиента"}");
    }

    void OnDestroy()
    {
        // Очистка событий для избежания утечек
        if (waitButton != null) waitButton.onClick.RemoveAllListeners();
        if (assignButton != null) assignButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
    }
}