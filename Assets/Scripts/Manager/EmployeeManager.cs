using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    [SerializeField] private List<EmployeeDataSO> employeeData = new List<EmployeeDataSO>();

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

    public void AddEmployeeData(EmployeeDataSO employeeDataSO)
    {
        if (employeeDataSO != null && !employeeData.Contains(employeeDataSO))
        {
            employeeData.Add(employeeDataSO);            
        }
        else
        {
            Debug.LogWarning($"EmployeeManager: Сотрудница {employeeDataSO?.name} уже добавлена или null.");
        }
    }

    public string GetRandomService()
    {
        List<string> services = employeeData.SelectMany(e => e.BaseSkills).Distinct().ToList();
        if (services.Count == 0)
        {
            Debug.LogWarning("EmployeeManager: Нет доступных услуг в employeeData.");
            return "";
        }
        Debug.Log($"GetRandomService: Доступные услуги: {string.Join(", ", services)}");
        return services[Random.Range(0, services.Count)];
    }

    public string GetRandomBodyType(string race = null)
    {
        List<string> bodyTypes = race == null
            ? employeeData.SelectMany(e => e.BodyTypes).Distinct().ToList()
            : employeeData.Where(e => e.Race == race).SelectMany(e => e.BodyTypes).Distinct().ToList();
        if (bodyTypes.Count == 0)
        {
            Debug.LogWarning($"EmployeeManager: Нет доступных типов тела для расы {(race ?? "всех")}.");
            return "";
        }
        Debug.Log($"GetRandomBodyType: Доступные типы тела для расы {(race ?? "всех")}: {string.Join(", ", bodyTypes)}");
        return bodyTypes[Random.Range(0, bodyTypes.Count)];
    }

    public char GetRandomBreastSize(string bodyType = null)
    {
        List<char> breastSizes = bodyType == null
            ? employeeData.Select(e => e.BreastSize).Distinct().ToList()
            : employeeData.Where(e => e.BodyTypes.Contains(bodyType)).Select(e => e.BreastSize).Distinct().ToList();
        if (breastSizes.Count == 0)
        {
            Debug.LogWarning($"EmployeeManager: Нет доступных размеров груди для типа тела {(bodyType ?? "всех")}.");
            return ' ';
        }
        Debug.Log($"GetRandomBreastSize: Доступные размеры груди для типа тела {(bodyType ?? "всех")}: {string.Join(", ", breastSizes)}");
        return breastSizes[Random.Range(0, breastSizes.Count)];
    }

    public string GetRandomRace()
    {
        List<string> races = employeeData.Select(e => e.Race).Distinct().ToList();
        if (races.Count == 0)
        {
            Debug.LogWarning("EmployeeManager: Нет доступных рас в employeeData.");
            return "";
        }
        Debug.Log($"GetRandomRace: Доступные расы: {string.Join(", ", races)}");
        return races[Random.Range(0, races.Count)];
    }

    public Employee GetRandomEmployee()
    {
        if (employeeData.Count == 0)
        {
            Debug.LogWarning("EmployeeManager: Нет доступных сотрудниц в employeeData.");
            return null;
        }
        EmployeeDataSO randomData = employeeData[Random.Range(0, employeeData.Count)];
        Employee[] employees = Object.FindObjectsByType<Employee>(FindObjectsSortMode.None);
        if (employees.Length == 0)
        {
            Debug.LogWarning("EmployeeManager: Нет активных сотрудниц в сцене.");
            return null;
        }
        foreach (Employee employee in employees)
        {
            if (employee.Data == randomData)
            {
                Debug.Log($"GetRandomEmployee: Выбрана сотрудница {employee.name}.");
                return employee;
            }
        }
        Debug.LogWarning($"GetRandomEmployee: Сотрудница с данными {randomData.name} не найдена в сцене.");
        return null;
    }
}