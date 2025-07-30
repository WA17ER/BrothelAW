using System.Collections.Generic;
using UnityEngine;

public class ClientRequest : MonoBehaviour
{
    [SerializeField] public int clientLevel = 1; // Для префабов, публичное для записи
    [SerializeField] private List<Employee> availableEmployees; // Для теста

    private string requestedService;
    private Dictionary<string, string> preferences = new Dictionary<string, string>(); // race, bodyType, breastSize
    private Employee specificEmployee;

    private CustomerMovement customerMovement;

    public int ClientLevel => clientLevel;
    public string RequestedService => requestedService;

    private void Awake()
    {
        customerMovement = GetComponent<CustomerMovement>();
        if (customerMovement == null)
        {
            Debug.LogError("ClientRequest: CustomerMovement не найден.");
            enabled = false;
            return;
        }
        customerMovement.onEnterWaiting.AddListener(GenerateRequest);
    }

    private void GenerateRequest()
    {
        if (EmployeeManager.Instance == null)
        {
            Debug.LogError("EmployeeManager не найден.");
            return;
        }

        requestedService = EmployeeManager.Instance.GetRandomService();

        preferences.Clear();
        specificEmployee = null;

        if (clientLevel == 2)
        {
            string randomBodyType = EmployeeManager.Instance.GetRandomBodyType();
            preferences.Add("bodyType", randomBodyType);
            char randomBreastSize = EmployeeManager.Instance.GetRandomBreastSize(bodyType: randomBodyType);
            preferences.Add("breastSize", randomBreastSize.ToString());
        }
        else if (clientLevel == 3)
        {
            string randomRace = EmployeeManager.Instance.GetRandomRace();
            preferences.Add("race", randomRace);
        }
        else if (clientLevel == 4)
        {
            specificEmployee = EmployeeManager.Instance.GetRandomEmployee();
        }

        Debug.Log($"Запрос клиента сгенерирован в Waiting (level {clientLevel}): Услуга - {requestedService}, Preferences - {string.Join(", ", preferences)}, Specific: {(specificEmployee != null ? specificEmployee.name : "None")}");
    }

    public bool IsMatch(Employee employee)
    {
        if (!employee.BaseSkills.Contains(requestedService)) return false;

        if (preferences.TryGetValue("race", out string reqRace) && employee.Race != reqRace) return false;
        if (preferences.TryGetValue("bodyType", out string reqBody) && !employee.BodyTypes.Contains(reqBody)) return false;
        if (preferences.TryGetValue("breastSize", out string reqBreast) && employee.BreastSize.ToString() != reqBreast) return false;

        if (specificEmployee != null && employee != specificEmployee) return false;

        return true;
    }

    [ContextMenu("Выбрать сотрудницу 0")]
    public void SelectEmployee0()
    {
        if (availableEmployees.Count > 0) SelectEmployee(availableEmployees[0]);
    }

    public void SelectEmployee(Employee employee)
    {
        var state = customerMovement.CurrentState;
        if (state == CustomerMovement.CustomerState.Waiting || state == CustomerMovement.CustomerState.OnChair)
        {
            if (IsMatch(employee))
            {
                Debug.Log("Запрос совпадает, отправка на услугу.");
                customerMovement.SendToService();
                // Вызов ServeClient из GameManager будет позже
            }
            else
            {
                Debug.Log("Запрос не совпадает, клиент уходит.");
                customerMovement.ForceExit();
            }
        }
        else
        {
            Debug.LogError("Нельзя выбрать: Недопустимое состояние.");
        }
    }
}