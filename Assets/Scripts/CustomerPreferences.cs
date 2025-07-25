using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class CustomerPreferences : MonoBehaviour
{
    [SerializeField] private List<string> availableServices = new List<string> { "BJ", "Boob Job", "Missionary", "Cowgirl", "Reverse Cowgirl", "Standing", "Doggy", "Amazon" };
    [SerializeField] private List<string> availableRaces = new List<string> { "Human", "Elf", "Dark Elf", "Neko", "Kitsune", "Oni", "Angel", "Driad", "Harpy", "Succubus", "Doppelganger" };
    [SerializeField] private List<char> availableBreastSizes = new List<char> { 'A', 'B', 'C', 'D', 'F' };
    [SerializeField] private List<string> availableBodyTypes = new List<string> { "Average", "Curvy", "Voluptuous", "Tall", "Slim", "Petite", "Busty", "Muscular" };

    private string requestedService;
    private string requestedRace;
    private char? requestedBreastSize;
    private List<string> requestedBodyTypes;
    private string requestedEmployeeID;
    private BaseEmployeeDataSO selectedEmployee;

    public event Action OnPreferencesGenerated;
    public event Action<BaseEmployeeDataSO> OnEmployeeSelected;

    public string RequestedService => requestedService;
    public string RequestedRace => requestedRace;
    public char? RequestedBreastSize => requestedBreastSize;
    public List<string> RequestedBodyTypes => requestedBodyTypes;
    public string RequestedEmployeeID => requestedEmployeeID;
    public BaseEmployeeDataSO GetSelectedEmployee() => selectedEmployee;

    private void Start()
    {
        GeneratePreferences();
        Debug.Log($"Generated request: Service = {requestedService}"); // Лог для проверки генерации
        Debug.Log("Invoking OnPreferencesGenerated"); // Временный лог
        OnPreferencesGenerated?.Invoke();
    }

    private void GeneratePreferences()
    {
        requestedService = availableServices[UnityEngine.Random.Range(0, availableServices.Count)];
        // Для Типа 1 отключаем остальные параметры
        requestedRace = null;
        requestedBreastSize = null;
        requestedBodyTypes = null;
        requestedEmployeeID = null;

        // Комментарий для будущего: логика для Типов 2-4
        // if (GameManager.Instance.GameDay >= 10 && UnityEngine.Random.value < 0.2f)
        // {
        //     var employees = EmployeeManager.Instance.GetAvailableEmployees();
        //     if (employees.Count > 0)
        //     {
        //         requestedEmployeeID = employees[UnityEngine.Random.Range(0, employees.Count)].ID;
        //         requestedService = null;
        //         requestedRace = null;
        //         requestedBreastSize = null;
        //         requestedBodyTypes = null;
        //     }
        // }
        // else
        // {
        //     requestedRace = UnityEngine.Random.value < 0.5f ? availableRaces[UnityEngine.Random.Range(0, availableRaces.Count)] : null;
        //     requestedBreastSize = UnityEngine.Random.value < 0.5f ? availableBreastSizes[UnityEngine.Random.Range(0, availableBreastSizes.Count)] : null;
        //     requestedBodyTypes = new List<string>();
        //     if (UnityEngine.Random.value < 0.5f)
        //     {
        //         int numTypes = UnityEngine.Random.Range(1, 4); // 1-3 types
        //         for (int i = 0; i < numTypes; i++)
        //         {
        //             string bodyType = availableBodyTypes[UnityEngine.Random.Range(0, availableBodyTypes.Count)];
        //             if (!requestedBodyTypes.Contains(bodyType))
        //             {
        //                 requestedBodyTypes.Add(bodyType);
        //             }
        //         }
        //     }
        // }
    }

    public bool IsEmployeeValid(BaseEmployeeDataSO employee)
    {
        // Doppelganger, Succubus, Angel игнорируют несоответствия
        if (employee.Race == "Doppelganger" || employee.Race == "Succubus" || employee.Race == "Angel")
            return true;

        // Проверка только услуги для Типа 1
        return employee.Skills.Any(skill => skill.skillName == requestedService);
    }

    [ContextMenu("Select Employee")]
    public void SelectEmployeeManually(BaseEmployeeDataSO employee)
    {
        if (IsEmployeeValid(employee))
        {
            selectedEmployee = employee;
            OnEmployeeSelected?.Invoke(employee);
            Debug.Log($"Manually selected employee: {employee.EmployeeName} for service {requestedService}");
        }
        else
        {
            Debug.LogError($"Employee {employee.EmployeeName} is not valid for service {requestedService}");
        }
    }

    public void SelectEmployee(BaseEmployeeDataSO employee)
    {
        if (IsEmployeeValid(employee))
        {
            selectedEmployee = employee;
            OnEmployeeSelected?.Invoke(employee);
        }
    }
}