using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomerMovement;

public class ClientData : MonoBehaviour
{
    [SerializeField] private ClientDataSO data;
    public List<Employee> availableEmployees = new List<Employee>();
    public string RequestedService { get; private set; }
    public string bodyType;
    public char breastSize;
    public string race;
    public Employee SpecificEmployee;
    public ClientDataSO Data => data;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"ClientDataSO is not assigned on {gameObject.name}.");
            return;
        }
    }

    public void InitializeClientPreferences()
    {
        if (EmployeeManager.Instance == null)
        {
            Debug.LogError($"EmployeeManager.Instance is null for client {gameObject.name}.");
            return;
        }

        RequestedService = EmployeeManager.Instance.GetRandomService();
        if (string.IsNullOrEmpty(RequestedService))
        {
            Debug.LogError($"Failed to get RequestedService for client {gameObject.name}.");
            return;
        }

        var employees = EmployeeManager.Instance.AvailableEmployees;
        if (employees.Count == 0)
        {
            Debug.LogWarning($"No available employees for client {gameObject.name}. Using default preferences.");
            bodyType = "";
            breastSize = '\0';
            race = "";
            SpecificEmployee = null;
            return;
        }

        switch (data.clientType)
        {
            case GameManager.ClientType.Type1:
                bodyType = "";
                breastSize = '\0';
                race = "";
                SpecificEmployee = null;
                break;
            case GameManager.ClientType.Type2:
                var bodyTypes = new List<string>();
                var breastSizes = new List<char>();
                foreach (var employee in employees)
                {
                    if (!string.IsNullOrEmpty(employee.BodyType))
                        bodyTypes.Add(employee.BodyType);
                    breastSizes.Add(employee.BreastSize);
                }
                bodyType = bodyTypes.Count > 0 ? bodyTypes[Random.Range(0, bodyTypes.Count)] : "";
                breastSize = breastSizes.Count > 0 ? breastSizes[Random.Range(0, breastSizes.Count)] : '\0';
                race = "";
                SpecificEmployee = null;
                break;
            case GameManager.ClientType.Type3:
                var races = new List<string>();
                foreach (var employee in employees)
                {
                    races.Add(employee.Race);
                }
                bodyType = "";
                breastSize = '\0';
                race = races.Count > 0 ? races[Random.Range(0, races.Count)] : "";
                SpecificEmployee = null;
                break;
            case GameManager.ClientType.Type4:
                bodyType = "";
                breastSize = '\0';
                race = "";
                SpecificEmployee = employees[Random.Range(0, employees.Count)];
                break;
        }

        Debug.Log($"Client {gameObject.name} initialized: Type={data.clientType}, RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, race={race}, SpecificEmployee={SpecificEmployee?.name ?? "null"}");
    }

    public bool IsMatch(Employee employee, float serviceCost)
    {
        if (employee == null)
        {
            Debug.LogError($"IsMatch: Employee null дл€ клиента {name}.");
            return false;
        }

        if (Data.totalGold < serviceCost)
        {
            Debug.Log($"IsMatch:  лиент {name} не имеет достаточно золота ({Data.totalGold} < {serviceCost}).");
            return false;
        }

        if (!employee.Data.BaseSkills.Contains(RequestedService))
        {
            Debug.Log($"IsMatch: ”слуга {RequestedService} не найдена в навыках сотрудницы {employee.name}.");
            return false;
        }

        if (employee.Data.GetSpecialRace() != EmployeeDataSO.SpecialRace.None)
        {
            Debug.Log($"IsMatch: —отрудница {employee.name} race {employee.Race} BodyType {(string.IsNullOrEmpty(employee.BodyType) ? "" : employee.BodyType)} BreastSize {employee.BreastSize} соответствует запросу клиента {name} RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, specialEmployee={(SpecificEmployee != null ? SpecificEmployee.name : "null")}, cost={serviceCost}.");
            return true;
        }

        if (!string.IsNullOrEmpty(race) && employee.Race != race)
        {
            Debug.Log($"IsMatch: –аса не совпадает (требуетс€ {race}, найдено {employee.Race}).");
            return false;
        }

        if (!string.IsNullOrEmpty(bodyType) && employee.BodyType != bodyType)
        {
            Debug.Log($"IsMatch: “ип тела не совпадает (требуетс€ {bodyType}, найдено {employee.BodyType}).");
            return false;
        }

        if (breastSize != '\0' && employee.BreastSize != breastSize)
        {
            Debug.Log($"IsMatch: –азмер груди не совпадает (требуетс€ {breastSize}, найдено {employee.BreastSize}).");
            return false;
        }

        if (SpecificEmployee != null && employee != SpecificEmployee)
        {
            Debug.Log($"IsMatch: “ребуетс€ конкретна€ сотрудница {SpecificEmployee.name}, выбрана {employee.name}.");
            return false;
        }

        Debug.Log($"IsMatch: —отрудница {employee.name} race {employee.Race} BodyType {(string.IsNullOrEmpty(employee.BodyType) ? "" : employee.BodyType)} BreastSize {employee.BreastSize} соответствует запросу клиента {name} RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, specialEmployee={(SpecificEmployee != null ? SpecificEmployee.name : "null")}, cost={serviceCost}.");
        return true;
    }

    [ContextMenu("Ќазначить первую сотрудницу")]
    public void AssignFirstEmployee()
    {
        if (availableEmployees.Count == 0)
        {
            Debug.LogError($"Ќет доступных сотрудниц дл€ клиента {gameObject.name}.");
            var customerMovement = GetComponent<CustomerMovement>();
            if (customerMovement != null)
            {
                customerMovement.ForceExit();
            }
            return;
        }
        var employee = availableEmployees[0];
        SelectEmployee(employee);
    }

    public void SelectEmployee(Employee employee)
    {
        var customerMovement = GetComponent<CustomerMovement>();
        if (customerMovement == null)
        {
            Debug.LogError($"CustomerMovement component missing on {gameObject.name}.");
            return;
        }
        var state = customerMovement.CurrentState;
        if (state != CustomerState.Waiting && state != CustomerState.OnChair)
        {
            Debug.LogError($"Ќельз€ выбрать сотрудницу: Ќедопустимое состо€ние {state} дл€ клиента {gameObject.name}.");
            return;
        }

        float serviceCost = EmployeeManager.Instance.CalculateServiceCost(this, employee);
        if (serviceCost == 0f)
        {
            Debug.LogError($"SelectEmployee: Ќе удалось рассчитать стоимость дл€ клиента {gameObject.name} и сотрудницы {employee.name}.");
            customerMovement.ForceExit();
            return;
        }

        if (IsMatch(employee, serviceCost))
        {
            float reward = EmployeeManager.Instance.AssignEmployee(this, employee);
            if (reward == 0f)
            {
                Debug.LogError($"SelectEmployee: Ќе удалось назначить сотрудницу {employee.name} дл€ клиента {gameObject.name} (ошибка в AssignEmployee).");
                customerMovement.ForceExit();
                return;
            }
            SpecificEmployee = employee;
            GameManager.Instance.onEmployeeAssigned.Invoke(this, employee);
            Debug.Log($"SelectEmployee: State = {state}, Calling SendToService for client {gameObject.name}.");
            customerMovement.SendToService();
            Debug.Log($"—отрудница {employee.name} назначена дл€ клиента {gameObject.name}, стоимость: {reward}.");
        }
        else
        {
            Debug.Log($"IsMatch не пройден, клиент {gameObject.name} уходит.");
            customerMovement.ForceExit();
        }
    }
}