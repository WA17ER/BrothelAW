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

    public void AddEmployeeData(Employee employee, EmployeeDataSO employeeData)
    {
        if (employee == null || employee.Data != employeeData)
        {
            Debug.LogError($"EmployeeManager: Employee is null or does not match EmployeeDataSO {employeeData?.employeeName}.");
            return;
        }
        if (!availableEmployees.Contains(employee))
        {
            availableEmployees.Add(employee);
            Debug.Log($"Сотрудница {employeeData.employeeName} добавлена в availableEmployees.");
        }
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