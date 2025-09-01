using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private List<Employee> employees = new List<Employee>();
    [SerializeField] private float gold = 1000f;
    [SerializeField] private float popularity = 50f;

    public List<Employee> Employees => employees;
    public float Gold => gold;
    public float Popularity => popularity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        InitializeGameState();
    }

    void InitializeGameState()
    {
        // Загрузка сотрудников из папки Resources/Employees
        EmployeeDataSO[] employeeDataSOs = Resources.LoadAll<EmployeeDataSO>("Employees");
        employees.Clear();
        foreach (var data in employeeDataSOs)
        {
            GameObject employeeObj = new GameObject(data.employeeName, typeof(Employee));
            Employee employee = employeeObj.GetComponent<Employee>();
            employee.SetData(data);
            employees.Add(employee);
            Debug.Log($"Инициализирована сотрудница: {data.employeeName}");
        }

        // Инициализация начальных ресурсов
        gold = 1000f;
        popularity = 50f;

        // Логирование всех имён
        if (employees.Count > 0)
        {
            string names = "Список сотрудниц: ";
            foreach (var employee in employees)
            {
                names += employee.Data.employeeName + ", ";
            }
            Debug.Log(names.TrimEnd(',', ' '));
        }
        else
        {
            Debug.Log("Список сотрудниц пуст.");
        }
    }

    public void UpdateGold(float amount)
    {
        gold += amount;
    }

    public void UpdatePopularity(float amount)
    {
        popularity += amount;
    }

    public void AddEmployee(Employee employee)
    {
        employees.Add(employee);
    }
}