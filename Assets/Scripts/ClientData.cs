using System.Collections.Generic;
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
                    bodyTypes.AddRange(employee.Data.BodyTypes);
                    breastSizes.Add(employee.Data.BreastSize);
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
                    races.Add(employee.Data.Race);
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

    [ContextMenu("Назначить первую сотрудницу")]
    public void AssignFirstEmployee()
    {
        if (availableEmployees.Count == 0)
        {
            Debug.LogError($"Нет доступных сотрудниц для клиента {gameObject.name}.");
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
            Debug.LogError($"CustomerMovement component missing on client {gameObject.name}.");
            return;
        }
        var state = customerMovement.CurrentState;
        if (state == CustomerState.Waiting || state == CustomerState.OnChair)
        {
            SpecificEmployee = employee;
            GameManager.Instance.onEmployeeAssigned.Invoke(this, employee);
            Debug.Log($"SelectEmployee: State = {state}, Calling SendToService for client {gameObject.name}.");
            customerMovement.SendToService();
            Debug.Log($"Сотрудница {employee.name} назначена для клиента {gameObject.name}.");
        }
        else
        {
            Debug.LogError($"Нельзя выбрать сотрудницу: Недопустимое состояние {state} для клиента {gameObject.name}.");
        }
    }
}