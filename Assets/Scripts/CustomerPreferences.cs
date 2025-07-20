using UnityEngine;
using System;
using System.Collections.Generic;

public class CustomerPreferences : MonoBehaviour
{
    [SerializeField] private List<string> availableServices = new List<string> { "BJ", "Boob Job", "Missionary", "Cowgirl", "Reverse Cowgirl", "Standing", "Doggy", "Amazon" };
    [SerializeField] private List<string> availableRaces = new List<string> { "Human", "Elf", "Dark Elf", "Neko", "Kitsune", "Oni", "Angel", "Driad", "Harpy", "Succubus", "Doppelganger" };
    [SerializeField] private List<char> availableBreastSizes = new List<char> { 'A', 'B', 'C', 'D', 'F' };
    [SerializeField] private List<string> availableBodyTypes = new List<string> { "Slim", "Tall", "Sport", "Fit" };

    private string requestedService;
    private string requestedRace;
    private char? requestedBreastSize;
    private string requestedBodyType;
    private string requestedEmployeeID;

    public event Action OnPreferencesGenerated;
    public event Action<BaseEmployeeDataSO> OnEmployeeSelected;

    public string RequestedService => requestedService;
    public string RequestedRace => requestedRace;
    public char? RequestedBreastSize => requestedBreastSize;
    public string RequestedBodyType => requestedBodyType;
    public string RequestedEmployeeID => requestedEmployeeID;

    private void Start()
    {
        GeneratePreferences();
        OnPreferencesGenerated?.Invoke();
    }

    private void GeneratePreferences()
    {
        requestedService = availableServices[UnityEngine.Random.Range(0, availableServices.Count)];
        requestedRace = UnityEngine.Random.value < 0.5f ? availableRaces[UnityEngine.Random.Range(0, availableRaces.Count)] : null;
        requestedBreastSize = UnityEngine.Random.value < 0.5f ? availableBreastSizes[UnityEngine.Random.Range(0, availableBreastSizes.Count)] : null;
        requestedBodyType = UnityEngine.Random.value < 0.5f ? availableBodyTypes[UnityEngine.Random.Range(0, availableBodyTypes.Count)] : null;

        // ѕоздние этапы: шанс 20% запросить конкретную сотрудницу
        if (GameManager.Instance.GameDay >= 10 && UnityEngine.Random.value < 0.2f)
        {
            var employees = EmployeeManager.Instance.GetAvailableEmployees();
            if (employees.Count > 0)
            {
                requestedEmployeeID = employees[UnityEngine.Random.Range(0, employees.Count)].ID;
                requestedService = null; // ≈сли выбрана сотрудница, услуга может игнорироватьс€
                requestedRace = null;
                requestedBreastSize = null;
                requestedBodyType = null;
            }
        }
    }

    public bool IsEmployeeValid(BaseEmployeeDataSO employee)
    {
        // Doppelganger, Succubus, Angel игнорируют несоответстви€
        if (employee.Race == "Doppelganger" || employee.Race == "Succubus" || employee.Race == "Angel")
            return true;

        // ≈сли запрошена конкретна€ сотрудница
        if (!string.IsNullOrEmpty(requestedEmployeeID))
            return employee.ID == requestedEmployeeID;

        // ѕроверка услуги
        bool hasService = false;
        foreach (var skill in employee.Skills)
        {
            if (skill.skillName == requestedService)
            {
                hasService = true;
                break;
            }
        }
        if (!hasService) return false;

        // ѕроверка необ€зательных параметров
        if (requestedRace != null && employee.Race != requestedRace) return false;
        if (requestedBreastSize.HasValue && employee.BreastSize != requestedBreastSize.Value) return false;
        if (requestedBodyType != null && employee.BodyType != requestedBodyType) return false;

        return true;
    }

    public void SelectEmployee(BaseEmployeeDataSO employee)
    {
        if (IsEmployeeValid(employee))
        {
            OnEmployeeSelected?.Invoke(employee);
        }
    }
}