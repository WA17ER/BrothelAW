using System.Collections.Generic;
using UnityEngine;
using static CustomerMovement;

public class ClientRequest : MonoBehaviour
{
    [SerializeField] public int clientLevel = 1;
    [SerializeField] public List<Employee> availableEmployees;
    private string requestedService;
    private Dictionary<string, string> preferences = new Dictionary<string, string>();
    private Employee specificEmployee;
    private Employee selectedEmployee;

    private CustomerMovement customerMovement;

    public int ClientLevel => clientLevel;
    public string RequestedService => requestedService;
    public Employee SpecificEmployee => specificEmployee;
    public Employee SelectedEmployee => selectedEmployee;
    public Dictionary<string, string> Preferences => preferences;

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
        selectedEmployee = null;

        if (clientLevel == 2)
        {
            string randomBodyType = EmployeeManager.Instance.GetRandomBodyType();
            if (!string.IsNullOrEmpty(randomBodyType))
            {
                preferences.Add("bodyType", randomBodyType);
                char randomBreastSize = EmployeeManager.Instance.GetRandomBreastSize(bodyType: randomBodyType);
                if (randomBreastSize != ' ')
                {
                    preferences.Add("breastSize", randomBreastSize.ToString());
                }
            }
        }
        else if (clientLevel == 3)
        {
            string randomRace = EmployeeManager.Instance.GetRandomRace();
            if (!string.IsNullOrEmpty(randomRace))
            {
                preferences.Add("race", randomRace);
            }
        }
        else if (clientLevel == 4)
        {
            specificEmployee = EmployeeManager.Instance.GetRandomEmployee();
            selectedEmployee = specificEmployee;
        }

        Debug.Log($"Запрос клиента {name} сгенерирован в Waiting (level {clientLevel}): Услуга - {requestedService}, Preferences - {string.Join(", ", preferences)}, Specific: {(specificEmployee != null ? specificEmployee.name : "None")}");
    }

    public bool IsMatch(Employee employee)
    {
        Debug.Log($"Проверяемые характеристики клиента {name} (level {clientLevel}): Услуга - {requestedService}, Preferences - {string.Join(", ", preferences)}, Specific - {(specificEmployee != null ? specificEmployee.name : "None")}");
        Debug.Log($"Сотрудница {employee.name}: Услуга - {(employee.BaseSkills.Contains(requestedService) ? "есть" : "нет")}, Раса - {employee.Race}, Типы тела - {string.Join(", ", employee.BodyTypes)}, Размер груди - {employee.BreastSize}");

        if (!employee.BaseSkills.Contains(requestedService))
        {
            Debug.Log($"Несовпадение: Услуга {requestedService} отсутствует в навыках сотрудницы {employee.name}.");
            return false;
        }

        if (preferences.TryGetValue("race", out string reqRace) && employee.Race != reqRace)
        {
            Debug.Log($"Несовпадение: Раса не совпадает (требуется {reqRace}, найдено {employee.Race}).");
            return false;
        }

        if (preferences.TryGetValue("bodyType", out string reqBody) && !employee.BodyTypes.Contains(reqBody))
        {
            Debug.Log($"Несовпадение: Тип тела не совпадает (требуется {reqBody}, найдено {string.Join(", ", employee.BodyTypes)}).");
            return false;
        }

        if (preferences.TryGetValue("breastSize", out string reqBreast) && employee.BreastSize.ToString() != reqBreast)
        {
            Debug.Log($"Несовпадение: Размер груди не совпадает (требуется {reqBreast}, найдено {employee.BreastSize}).");
            return false;
        }

        if (specificEmployee != null && employee != specificEmployee)
        {
            Debug.Log($"Несовпадение: Требуется конкретная сотрудница {specificEmployee.name}, выбрана {employee.name}.");
            return false;
        }

        Debug.Log($"Сотрудница {employee.name} соответствует запросу клиента {name}.");
        return true;
    }

    [ContextMenu("Выбрать сотрудницу 0")]
    public void SelectEmployee0()
    {
        if (availableEmployees.Count > 0)
        {
            SelectEmployee(availableEmployees[0]);
        }
        else
        {
            Debug.LogError($"Нет доступных сотрудниц для клиента {name}.");
        }
    }

    public void SelectEmployee(Employee employee)
    {
        var state = customerMovement.CurrentState;
        if (state == CustomerState.Waiting || state == CustomerState.OnChair)
        {
            if (IsMatch(employee))
            {
                selectedEmployee = employee;
                Debug.Log($"Сотрудница {employee.name} выбрана для клиента {name}. SelectedEmployee: {(selectedEmployee != null ? selectedEmployee.name : "null")}");
                customerMovement.SendToService();
            }
            else
            {
                Debug.Log($"Запрос не совпадает, клиент {name} уходит.");
                customerMovement.ForceExit();
            }
        }
        else
        {
            Debug.LogError($"Нельзя выбрать: Недопустимое состояние {state}.");
        }
    }
}