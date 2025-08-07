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
    public SicknessSO ActiveSick;
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
            ActiveSick = null;
            Debug.Log($"Клиент {gameObject.name} здоров.");
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

        // Определение болезни клиента
        if (data.sickChance > 0 && Random.value < data.sickChance / 100f)
        {
            if (data.possibleSicknesses != null && data.possibleSicknesses.Count > 0)
            {
                ActiveSick = data.possibleSicknesses[Random.Range(0, data.possibleSicknesses.Count)];
                Debug.Log($"Клиент {gameObject.name} болен {ActiveSick.SickName}.");
            }
            else
            {
                Debug.LogWarning($"Клиент {gameObject.name} должен быть болен, но possibleSicknesses пуст или null.");
                ActiveSick = null;
                Debug.Log($"Клиент {gameObject.name} здоров.");
            }
        }
        else
        {
            ActiveSick = null;
            Debug.Log($"Клиент {gameObject.name} здоров.");
        }

        Debug.Log($"Client {gameObject.name} initialized: Type={data.clientType}, RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, race={race}, SpecificEmployee={SpecificEmployee?.name ?? "null"}, ActiveSick={ActiveSick?.SickName ?? "none"}");
    }

    public bool IsMatch(Employee employee, float serviceCost)
    {
        if (employee == null)
        {
            Debug.LogError($"IsMatch: Employee null для клиента {name}.");
            return false;
        }

        if (Data.totalGold < serviceCost)
        {
            Debug.Log($"IsMatch: Клиент {name} не имеет достаточно золота ({Data.totalGold} < {serviceCost}).");
            return false;
        }

        if (!employee.Data.BaseSkills.Contains(RequestedService))
        {
            Debug.Log($"IsMatch: Услуга {RequestedService} не найдена в навыках сотрудницы {employee.name}.");
            return false;
        }

        if (employee.Data.GetSpecialRace() != EmployeeDataSO.SpecialRace.None)
        {
            Debug.Log($"IsMatch: Сотрудница {employee.name} race {employee.Race} BodyType {(string.IsNullOrEmpty(employee.BodyType) ? "" : employee.BodyType)} BreastSize {employee.BreastSize} соответствует запросу клиента {name} RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, specialEmployee={(SpecificEmployee != null ? SpecificEmployee.name : "null")}, cost={serviceCost}.");
            return true;
        }

        if (!string.IsNullOrEmpty(race) && employee.Race != race)
        {
            Debug.Log($"IsMatch: Раса не совпадает (требуется {race}, найдено {employee.Race}).");
            return false;
        }

        if (!string.IsNullOrEmpty(bodyType) && employee.BodyType != bodyType)
        {
            Debug.Log($"IsMatch: Тип тела не совпадает (требуется {bodyType}, найдено {employee.BodyType}).");
            return false;
        }

        if (breastSize != '\0' && employee.BreastSize != breastSize)
        {
            Debug.Log($"IsMatch: Размер груди не совпадает (требуется {breastSize}, найдено {employee.BreastSize}).");
            return false;
        }

        if (SpecificEmployee != null && employee != SpecificEmployee)
        {
            Debug.Log($"IsMatch: Требуется конкретная сотрудница {SpecificEmployee.name}, выбрана {employee.name}.");
            return false;
        }

        Debug.Log($"IsMatch: Сотрудница {employee.name} race {employee.Race} BodyType {(string.IsNullOrEmpty(employee.BodyType) ? "" : employee.BodyType)} BreastSize {employee.BreastSize} соответствует запросу клиента {name} RequestedService={RequestedService}, bodyType={bodyType}, breastSize={breastSize}, specialEmployee={(SpecificEmployee != null ? SpecificEmployee.name : "null")}, cost={serviceCost}.");
        return true;
    }

    [ContextMenu("Назначить первую сотрудницу")]
    public void AssignFirstEmployee()
    {
        if (availableEmployees.Count == 0)
        {
            Debug.LogError($"Нет доступных сотрудниц для клиента {gameObject.name}.");
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
            Debug.LogError($"Нельзя выбрать сотрудницу: Недопустимое состояние {state} для клиента {gameObject.name}.");
            return;
        }

        float serviceCost = EmployeeManager.Instance.CalculateServiceCost(this, employee);
        if (serviceCost == 0f)
        {
            Debug.LogError($"SelectEmployee: Не удалось рассчитать стоимость для клиента {gameObject.name} и сотрудницы {employee.name}.");
            customerMovement.ForceExit();
            return;
        }

        if (IsMatch(employee, serviceCost))
        {
            float reward = EmployeeManager.Instance.AssignEmployee(this, employee);
            if (reward == 0f)
            {
                Debug.LogError($"SelectEmployee: Не удалось назначить сотрудницу {employee.name} для клиента {gameObject.name} (ошибка в AssignEmployee).");
                customerMovement.ForceExit();
                return;
            }
            SpecificEmployee = employee;
            GameManager.Instance.onEmployeeAssigned.Invoke(this, employee);
            Debug.Log($"SelectEmployee: State = {state}, Calling SendToService for client {gameObject.name}.");
            customerMovement.SendToService();
            Debug.Log($"Сотрудница {employee.name} назначена для клиента {gameObject.name}, стоимость: {reward}.");
        }
        else
        {
            Debug.Log($"IsMatch не пройден, клиент {gameObject.name} уходит.");
            customerMovement.ForceExit();
        }
    }
}