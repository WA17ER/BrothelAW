using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float dayDuration = 300f; // 5 минут на игровой день
    [SerializeField] private EmployeeManager employeeManager; // Ссылка на EmployeeManager

    private float dayTimer;
    private int gameDay = 1;
    private int gold = 1000; // Начальное золото
    private int popularity = 0; // Начальная популярность
    private List<string> availableRaces = new List<string>();
    private List<char> availableBreastSizes = new List<char>();
    private List<string> availableBodyTypes = new List<string>();
    private static GameManager instance;

    public static GameManager Instance => instance;
    public int GameDay => gameDay; // Доступ к текущему дню

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (employeeManager == null)
        {
            Debug.LogError("GameManager: EmployeeManager is not assigned.");
            enabled = false;
        }

        UpdateAvailableParameters();
        employeeManager.OnEmployeeAdded += UpdateAvailableParameters;
    }

    private void Start()
    {
        dayTimer = dayDuration;
    }

    private void Update()
    {
        dayTimer -= Time.deltaTime;
        if (dayTimer <= 0)
        {
            EndDay();
        }
    }

    private void EndDay()
    {
        gameDay++;
        dayTimer = dayDuration;

        // Обновление сотрудниц
        foreach (var employee in employeeManager.GetAvailableEmployees())
        {
            employee.RestoreStamina(20);
            if (employee.Disease != null)
            {
                employee.Disease.DecreaseDuration();
                if (employee.Disease.Duration <= 0)
                {
                    employee.SetDisease(null);
                }
            }
        }
    }

    private void UpdateAvailableParameters()
    {
        availableRaces.Clear();
        availableBreastSizes.Clear();
        availableBodyTypes.Clear();

        var employees = employeeManager.GetAvailableEmployees();
        foreach (var employee in employees)
        {
            if (!availableRaces.Contains(employee.Race))
                availableRaces.Add(employee.Race);
            if (!availableBreastSizes.Contains(employee.BreastSize))
                availableBreastSizes.Add(employee.BreastSize);
            foreach (var bodyType in employee.BodyTypes)
            {
                if (!availableBodyTypes.Contains(bodyType))
                    availableBodyTypes.Add(bodyType);
            }
        }
    }

    public List<string> GetAvailableRaces() => new List<string>(availableRaces);
    public List<char> GetAvailableBreastSizes() => new List<char>(availableBreastSizes);
    public List<string> GetAvailableBodyTypes() => new List<string>(availableBodyTypes);
    public int GetAmountGold() => gold;
    public int GetPopularity() => popularity;
    public void AddGold(int amount) => gold += amount;
    public bool SpendGold(int amount) => gold >= amount ? (gold -= amount) == (gold - amount) : false;
}