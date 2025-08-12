using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    private List<Employee> availableEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private List<Employee> healingEmployees = new List<Employee>();

    public List<Employee> AvailableEmployees => availableEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> HealingEmployees => healingEmployees;

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
        foreach (var employee in employees)
        {
            MoveEmployeeToList(employee);
        }
        Debug.Log($"Добавлено сотрудниц: Доступно {availableEmployees.Count}, Больны {sickEmployees.Count}, Лечатся {healingEmployees.Count}.");
    }

    public void MoveEmployeeToList(Employee employee)
    {
        availableEmployees.Remove(employee);
        sickEmployees.Remove(employee);
        healingEmployees.Remove(employee);

        if (employee.GetState() == Employee.EmployeeState.Available)
        {
            availableEmployees.Add(employee);
        }
        else if (employee.GetState() == Employee.EmployeeState.Sick)
        {
            sickEmployees.Add(employee);
        }
        else if (employee.GetState() == Employee.EmployeeState.Healing)
        {
            healingEmployees.Add(employee);
        }
    }

    public float AssignEmployee(ClientData client, Employee employee)
    {
        if (!availableEmployees.Contains(employee))
        {
            Debug.LogWarning($"Сотрудница {employee.name} не доступна для назначения.");
            return 0f;
        }

        availableEmployees.Remove(employee);
        client.SpecificEmployee = employee;
        float reward = CalculateReward(client, employee);
        Debug.Log($"Сотрудница {employee.name} назначена клиенту {client.name}, награда: {reward}.");
        return reward;
    }

    public void CompleteService(Employee employee, string service, bool hasSickness)
    {
        if (employee != null)
        {
            if (hasSickness && Random.value < 0.2f)
            {
                employee.SetState(Employee.EmployeeState.Sick);
                MoveEmployeeToList(employee);
            }
            else
            {
                employee.SetState(Employee.EmployeeState.Available);
                MoveEmployeeToList(employee);
            }
        }
    }

    public void EndDayUpdate()
    {
        List<Employee> healingCopy = new List<Employee>(healingEmployees);
        foreach (var employee in healingCopy)
        {
            if (employee != null && employee.GetState() == Employee.EmployeeState.Healing)
            {
                employee.ProgressHealing();
                MoveEmployeeToList(employee);
            }
        }
    }

    public List<Employee> GetAllEmployees()
    {
        List<Employee> allEmployees = new List<Employee>();
        allEmployees.AddRange(availableEmployees);
        allEmployees.AddRange(sickEmployees);
        allEmployees.AddRange(healingEmployees);
        return allEmployees;
    }

    public string GetRandomService()
    {
        return "DefaultService"; // Placeholder
    }

    private float CalculateReward(ClientData client, Employee employee)
    {
        return 100f; // Placeholder
    }
}