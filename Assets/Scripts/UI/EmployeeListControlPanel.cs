using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;
public class EmployeeListControlPanel : MonoBehaviour
{
    [SerializeField] private GameObject employeePanelPrefab;
    [SerializeField] private Transform employeeGrid;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject clientOrderPanelPrefab;
    [SerializeField] private GameObject employeeInfoPanelPrefab;
    [SerializeField] private TMP_Text employeeNameText;
    [SerializeField] private TMP_Text employeeRaceText;
    [SerializeField] private TMP_Text employeeBodyTypeText;
    [SerializeField] private TMP_Text employeeBreastSizeText;
    [SerializeField] private TMP_Text employeeStaminaText;
    [SerializeField] private TMP_Text employeeSkillText;
    private ClientData currentClient;
    private Employee preparedEmployee;
    private ClientInteractionController parentController;
    private bool isDestroyed = false;
    private GameObject clientOrderPanelInstance;
    private GameObject employeeInfoPanelInstance;
    private List<EmployeeDataSO> GetAllAvailableEmployees()
    {
        if (isDestroyed) return new List<EmployeeDataSO>();
        var allEmployees = EmployeeManager.Instance?.GetAllEmployees();
        return allEmployees?.Where(e => e != null && e.Data != null).Select(e => e.Data).ToList() ?? new List<EmployeeDataSO>();
    }
    public void Initialize(ClientData client, ClientInteractionController controller)
    {
        if (isDestroyed) return;
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
        preparedEmployee = null;
        if (clientOrderPanelPrefab != null)
        {
            clientOrderPanelInstance = Instantiate(clientOrderPanelPrefab, transform);
            clientOrderPanelInstance.SetActive(true);
            PopulateClientOrderPanel();
        }
        if (employeeInfoPanelPrefab != null)
        {
            employeeInfoPanelInstance = Instantiate(employeeInfoPanelPrefab, transform);
            employeeInfoPanelInstance.SetActive(true);
            PopulateEmployeeDetailPanel();
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
            expectedEmployeeText.text = "Сотрудница: None";
            expectedRaceText.text = currentClient.preferredRace != EmployeeDataSO.Race.None ? "Раса: " + currentClient.preferredRace.ToString() : "Раса: None";
            expectedBodyText.text = currentClient.preferredBodyType != EmployeeDataSO.BodyType.None ? "Тело: " + currentClient.preferredBodyType.ToString() : "Тело: None";
            expectedBreastText.text = currentClient.preferredBreastSize != EmployeeDataSO.BreastSize.None ? "Размер груди: " + currentClient.preferredBreastSize.ToString() : "Размер груди: None";
            desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
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
        if (isDestroyed) return;
        if (employeePanelPrefab == null || employeeGrid == null)
        {
            Debug.LogWarning("Префаб employeePanelPrefab или employeeGrid не назначен!");
            return;
        }
        foreach (Transform child in employeeGrid)
        {
            Destroy(child.gameObject);
        }
        var availableEmployees = GetAllAvailableEmployees();
        foreach (var empData in availableEmployees)
        {
            if (empData == null) continue;
            var panelInstance = Instantiate(employeePanelPrefab, employeeGrid);
            if (panelInstance == null) continue;
            var panelButton = panelInstance.GetComponent<Button>();
            var employeeIcon = panelInstance.transform.Find("EmployeeIcon")?.GetComponent<Image>();
            var employeeNameText = panelInstance.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
            if (panelButton == null || employeeIcon == null || employeeNameText == null) continue;
            employeeIcon.sprite = empData.listIcon;
            employeeNameText.text = empData.employeeName;
            panelButton.onClick.AddListener(() => OnEmployeePanelClick(empData));
            var relatedEmployee = EmployeeManager.Instance?.GetAllEmployees().FirstOrDefault(e => e.Data == empData);
            if (relatedEmployee != null && (relatedEmployee.GetState() == Employee.EmployeeState.OnService || relatedEmployee.GetState() == Employee.EmployeeState.HeavySick || relatedEmployee.GetState() == Employee.EmployeeState.Marketing||  relatedEmployee.StaminaCurrent <= 0))
            {
                employeeIcon.color = new Color(0.5f, 0.5f, 0.5f);
                employeeNameText.color = new Color(0.5f, 0.5f, 0.5f);
                panelButton.interactable = false;
            }
            Debug.Log($"Добавлена панель для сотрудницы {empData.employeeName} с иконкой, состояние: {relatedEmployee?.GetState().ToString() ?? "Не определено"}");
        }
    }
    void OnEmployeePanelClick(EmployeeDataSO employee)
    {
        if (isDestroyed) return;
        Employee selected = EmployeeManager.Instance.GetAllEmployees().FirstOrDefault(e => e.Data == employee);
        if (selected != null)
        {
            preparedEmployee = selected;
            PopulateEmployeeDetailPanel();
            Debug.Log($"Сотрудница {preparedEmployee.Data.employeeName} предварительно выбрана");
        }
        else
        {
            Debug.LogWarning($"Сотрудница с данными {employee.employeeName} не найдена в списке EmployeeManager!");
            preparedEmployee = null;
        }
    }
    public void OnConfirmClick()
    {
        if (isDestroyed) return;
        if (preparedEmployee != null && currentClient != null)
        {
            currentClient.SelectedEmployee = preparedEmployee;
            parentController.InitializePanel();
            Destroy(gameObject);
            Debug.Log($"Сотрудница {preparedEmployee.Data.employeeName} выбрана и записана в SelectedEmployee для клиента {currentClient.clientName}");
        }
        else
        {
            Debug.LogWarning("Не выбрана сотрудница или клиент не найден для подтверждения");
            Destroy(gameObject);
        }
    }
    public void OnCancelClick()
    {
        if (isDestroyed) return;
        Destroy(gameObject);
        Debug.Log($"Кнопка Отмена нажата, панель уничтожена для клиента {currentClient?.clientName ?? "неизвестного клиента"}");
    }
    void OnDestroy()
    {
        isDestroyed = true;
        if (clientOrderPanelInstance != null) Destroy(clientOrderPanelInstance);
        if (employeeInfoPanelInstance != null) Destroy(employeeInfoPanelInstance);
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        Debug.Log("EmployeeListControlPanel уничтожен, все слушатели событий очищены");
    }
}