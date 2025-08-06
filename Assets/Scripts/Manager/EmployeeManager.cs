using System.Collections.Generic;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    private List<Employee> availableEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private List<Employee> servicingEmployees = new List<Employee>();
    private List<Employee> tiredEmployees = new List<Employee>();
    private List<string> availableServices = new List<string>
    {
        "Дрочка", "Миньет", "Дрочка Сиськами", "Миссионерская", "Наездница", "Амазонка", "Раком", "Стоя"
    };

    public List<Employee> AvailableEmployees => availableEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> ServicingEmployees => servicingEmployees;
    public List<Employee> TiredEmployees => tiredEmployees;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEmployees(List<Employee> employees)
    {
        availableEmployees.Clear();
        sickEmployees.Clear();
        servicingEmployees.Clear();
        tiredEmployees.Clear();

        foreach (var employee in employees)
        {
            if (employee == null)
            {
                Debug.LogError("AddEmployees: Получена null-сотрудница.");
                continue;
            }
            MoveEmployeeToList(employee, employee.GetState());
            Debug.Log($"Сотрудница {employee.name} добавлена в сцену, состояние: {employee.GetState()}.");
        }
        Debug.Log($"Добавлено {employees.Count} сотрудниц в EmployeeManager.");
    }

    public List<Employee> GetAllEmployees()
    {
        List<Employee> allEmployees = new List<Employee>();
        allEmployees.AddRange(availableEmployees);
        allEmployees.AddRange(sickEmployees);
        allEmployees.AddRange(servicingEmployees);
        allEmployees.AddRange(tiredEmployees);
        return allEmployees;
    }

    public void MoveEmployeeToList(Employee employee, Employee.EmployeeState newState)
    {
        availableEmployees.Remove(employee);
        sickEmployees.Remove(employee);
        servicingEmployees.Remove(employee);
        tiredEmployees.Remove(employee);

        switch (newState)
        {
            case Employee.EmployeeState.Available:
                availableEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в availableEmployees.");
                break;
            case Employee.EmployeeState.Sick:
                sickEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в sickEmployees.");
                break;
            case Employee.EmployeeState.Servicing:
                servicingEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в servicingEmployees.");
                break;
            case Employee.EmployeeState.Tired:
                tiredEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в tiredEmployees.");
                break;
            case Employee.EmployeeState.Healing:
                sickEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в sickEmployees (Healing).");
                break;
        }
        GameManager.Instance.onEmployeeListChanged.Invoke();
    }

    public float AssignEmployee(ClientData client, Employee employee)
    {
        if (employee == null)
        {
            Debug.LogError($"AssignEmployee: Employee null для клиента {client.name}.");
            return 0f;
        }
        string service = client.RequestedService;
        if (!employee.Data.servicePrices.ContainsKey(service))
        {
            Debug.LogError($"AssignEmployee: Услуга {service} не найдена в servicePrices сотрудницы {employee.name}.");
            return 0f;
        }
        employee.SetState(Employee.EmployeeState.Servicing);
        MoveEmployeeToList(employee, Employee.EmployeeState.Servicing);
        int clientLevel = (int)client.Data.clientType;
        int skillLevel = employee.Skills[service].level;
        float reward = clientLevel * (skillLevel + 1) * employee.Data.servicePrices[service];
        Debug.Log($"Сотрудница {employee.name} назначена для клиента {client.name}, награда: {reward}.");
        GameManager.Instance.onEmployeeListChanged.Invoke();
        return reward;
    }

    public void CompleteService(Employee employee, string service, bool clientIsSick)
    {
        if (employee == null)
        {
            Debug.LogError("CompleteService: Employee null.");
            return;
        }
        if (!employee.Skills.ContainsKey(service))
        {
            Debug.LogError($"CompleteService: Услуга {service} не найдена в навыках сотрудницы {employee.name}.");
            return;
        }
        employee.CheckSick(service, clientIsSick);
        var currentSkill = employee.Skills[service];
        int newProgress = currentSkill.progress + (int)GameManager.Instance.ProgressPerService;
        int newLevel = currentSkill.level;
        if (newLevel < 10)
        {
            if (newProgress >= 100)
            {
                newLevel++;
                newProgress = 0;
            }
        }
        else
        {
            newProgress = 100;
        }
        employee.Skills[service] = (newLevel, newProgress);
        Debug.Log($"Сотрудница {employee.name} навык {service} уровень {newLevel} прогрессия {newProgress}");
        employee.UpdateStamina(1f);
        if (employee.GetState() == Employee.EmployeeState.Servicing)
        {
            employee.SetState(Employee.EmployeeState.Available);
            MoveEmployeeToList(employee, Employee.EmployeeState.Available);
        }
    }

    public void EndDayUpdate()
    {
        foreach (var employee in GetAllEmployees())
        {
            Debug.Log($"Обновление дня для сотрудницы {employee.name}, текущее состояние: {employee.GetState()}");
            employee.EndDayUpdate();
            MoveEmployeeToList(employee, employee.GetState());
        }
    }

    public string GetRandomService()
    {
        return availableServices[Random.Range(0, availableServices.Count)];
    }

    public Employee GetRandomEmployee()
    {
        if (availableEmployees.Count == 0) return null;
        return availableEmployees[Random.Range(0, availableEmployees.Count)];
    }
}