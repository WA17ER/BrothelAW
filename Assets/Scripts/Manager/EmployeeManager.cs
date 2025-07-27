using System.Collections.Generic;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    [SerializeField] private List<string> allServices = new List<string> { "Missionary", "Cow Girl", "Amazon", "BJ", "BoobJob", "Standing", "Hand Job" };

    private HashSet<string> availableRaces = new HashSet<string>();
    private Dictionary<string, HashSet<string>> raceBodyTypes = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<char>> raceBreastSizes = new Dictionary<string, HashSet<char>>();
    private List<Employee> employees = new List<Employee>();

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
            return;
        }

        CollectEmployeeData();
    }

    private void CollectEmployeeData()
    {
        Employee[] allEmployees = Object.FindObjectsByType<Employee>(FindObjectsSortMode.None);
        employees.AddRange(allEmployees);

        foreach (var employee in allEmployees)
        {
            string race = employee.Race;
            availableRaces.Add(race);

            if (!raceBodyTypes.ContainsKey(race))
            {
                raceBodyTypes[race] = new HashSet<string>();
            }
            foreach (var bodyType in employee.BodyTypes)
            {
                raceBodyTypes[race].Add(bodyType);
            }

            if (!raceBreastSizes.ContainsKey(race))
            {
                raceBreastSizes[race] = new HashSet<char>();
            }
            raceBreastSizes[race].Add(employee.BreastSize);
        }

        LogCollectedData();
    }

    private void LogCollectedData()
    {
        Debug.Log("All Services: " + string.Join(", ", allServices));

        Debug.Log("Available Races: " + string.Join(", ", availableRaces));

        foreach (var kvp in raceBodyTypes)
        {
            Debug.Log($"Race {kvp.Key} Body Types: " + string.Join(", ", kvp.Value));
        }

        foreach (var kvp in raceBreastSizes)
        {
            Debug.Log($"Race {kvp.Key} Breast Sizes: " + string.Join(", ", kvp.Value));
        }

        Debug.Log("Total Employees: " + employees.Count);
    }

    // Методы для генерации
    public string GetRandomService()
    {
        return allServices[Random.Range(0, allServices.Count)];
    }

    public string GetRandomRace()
    {
        List<string> races = new List<string>(availableRaces);
        return races[Random.Range(0, races.Count)];
    }

    public string GetRandomBodyType(string race)
    {
        if (raceBodyTypes.TryGetValue(race, out var types))
        {
            List<string> bodyTypes = new List<string>(types);
            return bodyTypes[Random.Range(0, bodyTypes.Count)];
        }
        return null;
    }

    public char GetRandomBreastSize(string race)
    {
        if (raceBreastSizes.TryGetValue(race, out var sizes))
        {
            List<char> breastSizes = new List<char>(sizes);
            return breastSizes[Random.Range(0, breastSizes.Count)];
        }
        return ' ';
    }

    public Employee GetRandomEmployee()
    {
        return employees[Random.Range(0, employees.Count)];
    }
}