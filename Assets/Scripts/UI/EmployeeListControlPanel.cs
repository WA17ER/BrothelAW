using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Для Button и Image
using System.Linq; // Для использования LINQ
using TMPro; // Для TMP_Text

public class EmployeeListControlPanel : MonoBehaviour
{
    [SerializeField] private GameObject employeePanelPrefab; // Префаб панели сотрудницы
    [SerializeField] private Transform employeeGrid; // Ссылка на EmployeeGrid
    [SerializeField] private Button confirmButton; // Кнопка подтверждения
    [SerializeField] private Button cancelButton; // Кнопка отмены
    [SerializeField] private GameObject clientOrderPanelPrefab; // Префаб панели заказа клиента
    [SerializeField] private GameObject employeeInfoPanelPrefab; // Префаб панели информации о сотруднице
    [SerializeField] private TMP_Text employeeNameText; // Текстовое поле для имени
    [SerializeField] private TMP_Text employeeRaceText; // Текстовое поле для расы
    [SerializeField] private TMP_Text employeeBodyTypeText; // Текстовое поле для типа тела
    [SerializeField] private TMP_Text employeeBreastSizeText; // Текстовое поле для размера груди
    [SerializeField] private TMP_Text employeeStaminaText; // Текстовое поле для стамины
    [SerializeField] private TMP_Text employeeSkillText; // Текстовое поле для навыка
    private ClientData currentClient; // Текущий клиент
    private Employee preparedEmployee; // Предварительно выбранная сотрудница
    private ClientInteractionController parentController; // Ссылка на родительский контроллер
    private bool isDestroyed = false; // Флаг для проверки уничтожения
    private GameObject clientOrderPanelInstance; // Инстанс панели заказа
    private GameObject employeeInfoPanelInstance; // Инстанс панели информации о сотруднице

    // Получение всех сотрудников из EmployeeManager с фильтрацией
    private List<EmployeeDataSO> GetAllAvailableEmployees()
    {
        if (isDestroyed) return new List<EmployeeDataSO>(); // Проверка на уничтожение
        if (EmployeeManager.Instance != null)
        {
            List<Employee> allEmployees = EmployeeManager.Instance.GetAllEmployees();
            List<EmployeeDataSO> filteredEmployees = new List<EmployeeDataSO>();
            foreach (var employee in allEmployees)
            {
                if (employee != null && employee.Data != null)
                {
                    // Исключаем Healing и будущий Marketing (пока заглушка для Marketing)
                    if (employee.GetState() != Employee.EmployeeState.Healing)
                    {
                        filteredEmployees.Add(employee.Data); // Добавляем EmployeeDataSO
                    }
                }
            }
            return filteredEmployees;
        }
        Debug.LogWarning("EmployeeManager.Instance не найден, возвращаем пустой список!");
        return new List<EmployeeDataSO>();
    }

    public void Initialize(ClientData client, ClientInteractionController controller)
    {
        if (isDestroyed) return; // Проверка на уничтожение
        currentClient = client;
        parentController = controller;
        if (currentClient == null)
        {
            Debug.LogError("Клиент не передан в EmployeeListControlPanel!");
            return;
        }
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClick);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClick);
        }
        preparedEmployee = null; // Сброс выбора при открытии
        // Создание и инициализация панели заказа клиента
        if (clientOrderPanelPrefab != null)
        {
            clientOrderPanelInstance = Instantiate(clientOrderPanelPrefab, transform);
            clientOrderPanelInstance.SetActive(true);
            PopulateClientOrderPanel();
        }
        // Создание и инициализация панели информации о сотруднице
        if (employeeInfoPanelPrefab != null)
        {
            employeeInfoPanelInstance = Instantiate(employeeInfoPanelPrefab, transform);
            employeeInfoPanelInstance.SetActive(true);
            PopulateEmployeeDetailPanel(); // Инициализация с пустыми данными или "None"
        }
        PopulateEmployeeGrid();
        Debug.Log($"Инициализирована панель выбора для клиента {currentClient.clientName}");
    }

    void PopulateClientOrderPanel()
    {
        if (isDestroyed || clientOrderPanelInstance == null || currentClient == null) return;

        TMP_Text expectedEmployeeText = clientOrderPanelInstance.transform.Find("ExpectedEmployeeText")?.GetComponent<TMP_Text>();
        TMP_Text expectedRaceText = clientOrderPanelInstance.transform.Find("ExpectedRaceText")?.GetComponent<TMP_Text>();
        TMP_Text expectedBodyText = clientOrderPanelInstance.transform.Find("ExpectedBodyText")?.GetComponent<TMP_Text>();
        TMP_Text expectedBreastText = clientOrderPanelInstance.transform.Find("ExpectedBreastText")?.GetComponent<TMP_Text>();
        TMP_Text desiredServiceText = clientOrderPanelInstance.transform.Find("DesiredServiceText")?.GetComponent<TMP_Text>();

        if (expectedEmployeeText != null && expectedRaceText != null && expectedBodyText != null &&
            expectedBreastText != null && desiredServiceText != null)
        {
            if (currentClient.clientType == 1)
            {
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = "Раса: None";
                expectedBodyText.text = "Тело: None";
                expectedBreastText.text = "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 2)
            {
                bool useBodyType = currentClient.preferredBodyType != EmployeeDataSO.BodyType.None;
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = "Раса: None";
                expectedBodyText.text = useBodyType && currentClient.preferredBodyType != EmployeeDataSO.BodyType.None ? "Тело: " + currentClient.preferredBodyType.ToString() : "Тело: None";
                expectedBreastText.text = !useBodyType && currentClient.preferredBreastSize != EmployeeDataSO.BreastSize.None ? "Размер груди: " + currentClient.preferredBreastSize.ToString() : "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 3)
            {
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = currentClient.preferredRace != EmployeeDataSO.Race.None ? "Раса: " + currentClient.preferredRace.ToString() : "Раса: None";
                expectedBodyText.text = "Тело: None";
                expectedBreastText.text = "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 4)
            {
                if (currentClient.SelectedEmployee != null && currentClient.SelectedEmployee.Data != null)
                {
                    EmployeeDataSO employeeData = currentClient.SelectedEmployee.Data;
                    expectedEmployeeText.text = "Сотрудница: " + employeeData.employeeName;
                    expectedRaceText.text = "Раса: " + employeeData.race.ToString();
                    expectedBodyText.text = "Тело: " + employeeData.bodyType.ToString();
                    expectedBreastText.text = "Размер груди: " + employeeData.breastSize.ToString();
                }
                else
                {
                    expectedEmployeeText.text = "Сотрудница: None";
                    expectedRaceText.text = "Раса: None";
                    expectedBodyText.text = "Тело: None";
                    expectedBreastText.text = "Размер груди: None";
                }
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            Debug.Log($"Панель заказа клиента {currentClient.clientName} заполнена");
        }
        else
        {
            Debug.LogWarning("Одна или несколько ссылок на текстовые компоненты в ClientOrderPanel не найдены!");
        }
    }

    void PopulateEmployeeDetailPanel()
    {
        if (isDestroyed || employeeInfoPanelInstance == null || preparedEmployee == null) return;

        if (employeeNameText != null && employeeRaceText != null && employeeBodyTypeText != null &&
            employeeBreastSizeText != null && employeeStaminaText != null && employeeSkillText != null)
        {
            employeeNameText.text = "Имя: " + (preparedEmployee.Data != null ? preparedEmployee.Data.employeeName : "None");
            employeeRaceText.text = "Раса: " + (preparedEmployee.Data != null ? preparedEmployee.Data.race.ToString() : "None");
            employeeBodyTypeText.text = "Тело: " + (preparedEmployee.Data != null ? preparedEmployee.BodyType.ToString() : "None");
            employeeBreastSizeText.text = "Размер груди: " + (preparedEmployee.Data != null ? preparedEmployee.BreastSize.ToString() : "None");
            employeeStaminaText.text = "Текущая стамина: " + (preparedEmployee != null ? preparedEmployee.StaminaCurrent.ToString("F0") : "0");
            string requestedService = currentClient.RequestedService;
            if (preparedEmployee != null && preparedEmployee.Skills.ContainsKey(requestedService))
            {
                employeeSkillText.text = "Услуга: " + requestedService + " (Уровень: " + preparedEmployee.Skills[requestedService].level + ")";
            }
            else
            {
                employeeSkillText.text = "Услуга: " + (requestedService != null ? requestedService : "None") + " (Уровень: 0)";
            }
            Debug.Log($"Панель информации о сотруднице {preparedEmployee?.Data.employeeName ?? "None"} заполнена");
        }
        else
        {
            Debug.LogWarning("Одна или несколько ссылок на текстовые компоненты в EmployeeInfoPanel не найдены!");
        }
    }

    void PopulateEmployeeGrid()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (employeePanelPrefab != null && employeeGrid != null)
        {
            // Очистка предыдущих элементов
            foreach (Transform child in employeeGrid)
            {
                Destroy(child.gameObject);
            }

            List<EmployeeDataSO> availableEmployees = GetAllAvailableEmployees();
            foreach (var employee in availableEmployees)
            {
                if (employee != null)
                {
                    GameObject panelInstance = Instantiate(employeePanelPrefab, employeeGrid);
                    if (panelInstance != null)
                    {
                        Button panelButton = panelInstance.GetComponent<Button>();
                        Image employeeIcon = panelInstance.transform.Find("EmployeeIcon")?.GetComponent<Image>();
                        TMP_Text employeeNameText = panelInstance.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
                        if (panelButton != null && employeeIcon != null && employeeNameText != null)
                        {
                            employeeIcon.sprite = employee.listIcon;
                            employeeNameText.text = employee.employeeName;
                            panelButton.onClick.AddListener(() => OnEmployeePanelClick(employee));

                            // Временная индикация недоступности (например, для OnService и HeavySick)
                            Employee relatedEmployee = EmployeeManager.Instance.GetAllEmployees().FirstOrDefault(e => e.Data == employee);
                            if (relatedEmployee != null && (relatedEmployee.GetState() == Employee.EmployeeState.OnService ||
                                                           relatedEmployee.GetState() == Employee.EmployeeState.HeavySick))
                            {
                                employeeIcon.color = new Color(0.5f, 0.5f, 0.5f); // Серый для недоступности
                                employeeNameText.color = new Color(0.5f, 0.5f, 0.5f);
                            }
                            Debug.Log($"Добавлена панель для сотрудницы {employee.employeeName} с иконкой, состояние: {relatedEmployee?.GetState().ToString() ?? "Не определено"}");
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Префаб employeePanelPrefab или employeeGrid не назначен!");
        }
    }

    void OnEmployeePanelClick(EmployeeDataSO employee)
    {
        if (isDestroyed) return; // Проверка на уничтожение
        // Поиск соответствующего Employee объекта
        Employee selected = EmployeeManager.Instance.GetAllEmployees().FirstOrDefault(e => e.Data == employee);
        if (selected != null)
        {
            preparedEmployee = selected;
            PopulateEmployeeDetailPanel(); // Обновление панели информации о сотруднице
            Debug.Log($"Сотрудница {preparedEmployee.Data.employeeName} предварительно выбрана");
        }
        else
        {
            Debug.LogWarning($"Сотрудница с данными {employee.employeeName} не найдена в списке EmployeeManager!");
            preparedEmployee = null;
        }
    }

    // Методы для кнопок
    public void OnConfirmClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (preparedEmployee != null && currentClient != null)
        {
            currentClient.SelectedEmployee = preparedEmployee; // Передача данных независимо от соответствия
            parentController.InitializePanel(); // Обновление родительской панели
            Destroy(gameObject); // Уничтожение панели
            Debug.Log($"Сотрудница {preparedEmployee.Data.employeeName} выбрана и записана в SelectedEmployee для клиента {currentClient.clientName}");
        }
        else
        {
            Debug.LogWarning("Не выбрана сотрудница или клиент не найден для подтверждения");
            Destroy(gameObject); // Уничтожение панели при ошибке
        }
    }

    public void OnCancelClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        Destroy(gameObject); // Уничтожение панели без снятия паузы
        Debug.Log($"Кнопка Отмена нажата, панель уничтожена для клиента {currentClient?.clientName ?? "неизвестного клиента"}");
    }

    void OnDestroy()
    {
        isDestroyed = true; // Установка флага уничтожения
        if (clientOrderPanelInstance != null) Destroy(clientOrderPanelInstance); // Уничтожение панели заказа
        if (employeeInfoPanelInstance != null) Destroy(employeeInfoPanelInstance); // Уничтожение панели информации
        // Очистка слушателей событий
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        Debug.Log("EmployeeListControlPanel уничтожен, все слушатели событий очищены");
    }
}