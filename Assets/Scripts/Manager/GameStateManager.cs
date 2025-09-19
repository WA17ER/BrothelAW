using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    [SerializeField] private List<ClientDataSO> extraVisitors; // Поле для хранения дополнительных клиентов
    [SerializeField] private float gold = 0f;
    [SerializeField] private float popularity = 0f;
    [SerializeField] private float temporaryPopularity = 0f;
    private int dayCount = 0;
    private int difficultyLevel = 1;
    [SerializeField] private List<Employee> employees = new List<Employee>();

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform registerPoint;
    [SerializeField] private Transform servicePoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform[] chairs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            extraVisitors = new List<ClientDataSO>(); // Инициализация списка
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadEmployees(); // Инициализация сотрудников из ресурсов
    }

    private void Start()
    {
        Debug.Log($"GameStateManager Start: dayCount = {dayCount}");
        if (dayCount == 0 || dayCount == 1) // Проверка первого запуска
        {
            Debug.Log("Очистка популярности районов: первый запуск обнаружен (dayCount = 0 или 1)");
            SetTemporaryPopularity(0f);
            if (DistrictManager.Instance != null)
            {
                foreach (var district in DistrictManager.Instance.GetDistricts())
                {
                    Debug.Log($"Очистка District {district.DistrictName}: DistrictPopularity = {district.DistrictPopularity}, PreliminaryPopularity = {district.PreliminaryPopularity}");
                    district.DistrictPopularity = 0;
                    district.PreliminaryPopularity = 0;
                    Debug.Log($"После очистки District {district.DistrictName}: DistrictPopularity = {district.DistrictPopularity}, PreliminaryPopularity = {district.PreliminaryPopularity}");
                }
                Debug.Log("Очистка популярности для всех районов завершена");
            }
            else
            {
                Debug.LogWarning("DistrictManager.Instance не найден при попытке очистки популярности");
            }
        }
        else
        {
            Debug.Log($"Очистка не выполнена: dayCount = {dayCount}");
        }
    }

    private void LoadEmployees()
    {
        Debug.Log($"GameStateManager LoadEmployees: Начало загрузки сотрудников, текущий список: {employees.Count}");
        if (employees.Count == 0)
        {
            // Загрузка всех EmployeeDataSO из папки Resources/Employees
            EmployeeDataSO[] employeeDataSOs = Resources.LoadAll<EmployeeDataSO>("Employees");
            if (employeeDataSOs != null && employeeDataSOs.Length > 0)
            {
                foreach (var data in employeeDataSOs)
                {
                    GameObject employeeObj = new GameObject(data.employeeName); // Используем имя из данных
                    Employee employee = employeeObj.AddComponent<Employee>();
                    employee.SetData(data); // Установка данных
                    DontDestroyOnLoad(employeeObj); // Сохранение между сценами
                    employees.Add(employee);
                    Debug.Log($"Загружен сотрудник: {data.employeeName}");
                }
                Debug.Log($"Загружено сотрудников: {employees.Count}");
            }
            else
            {
                Debug.LogWarning("Ни один EmployeeDataSO не найден в папке Resources/Employees.");
            }
        }
    }

    public void UpdateGold(float newGold)
    {
        gold = newGold;
    }

    public void UpdatePopularity(float newPopularity)
    {
        popularity = newPopularity;
    }

    public void ApplyTemporaryPopularity()
    {
        popularity += temporaryPopularity;
        temporaryPopularity = 0f;
    }

    public void SetTemporaryPopularity(float value)
    {
        temporaryPopularity = value;
    }

    public float Gold => gold;
    public float Popularity => popularity;
    public float TemporaryPopularity => temporaryPopularity;
    public int DayCount => dayCount;
    public int DifficultyLevel => difficultyLevel;
    public List<Employee> Employees => employees;
    public List<ClientDataSO> ExtraVisitors => extraVisitors; // Геттер для доступа к extraVisitors

    public void UpdateDayCount(int newDayCount)
    {
        dayCount = newDayCount;
    }

    public void UpdateDifficultyLevel(int newDifficultyLevel)
    {
        difficultyLevel = newDifficultyLevel;
    }

    public void AddEmployee(Employee employee)
    {
        if (!employees.Contains(employee))
        {
            employees.Add(employee);
        }
    }

    public void RemoveEmployee(Employee employee)
    {
        if (employees.Contains(employee))
        {
            employees.Remove(employee);
        }
    }

    public (Transform spawnPoint, Transform registerPoint, Transform servicePoint, Transform exitPoint, Transform[] chairs) RestorePoints()
    {
        return (spawnPoint, registerPoint, servicePoint, exitPoint, chairs);
    }

    public void SavePoints(Transform spawn, Transform register, Transform service, Transform exit, Transform[] chairArray)
    {
        spawnPoint = spawn;
        registerPoint = register;
        servicePoint = service;
        exitPoint = exit;
        chairs = chairArray;
    }
    [ContextMenu("TransferExtraVisitorsFromDistrictManager")]

    public void TransferExtraVisitorsFromDistrictManager()
    {
        if (DistrictManager.Instance == null)
        {
            Debug.LogWarning("DistrictManager.Instance не найден при попытке переноса extraVisitors.");
            return;
        }

        if (dayCount == 0)
        {
            extraVisitors.Clear();
            Debug.Log("День 0: extraVisitors очищен, добавление не выполнено.");
            return;
        }

        Debug.Log("Начало переноса extraVisitors для dayCount: " + dayCount);
        extraVisitors.Clear(); // Очистка перед переносом
        DistrictManager.Instance.GenerateDistrictExtraVisitors(); // Генерация списка на лету
        Debug.Log("GenerateDistrictExtraVisitors вызван.");
        var clientCountByType = new Dictionary<int, int>(); // Подсчет текущего количества по типам

        foreach (var district in DistrictManager.Instance.GetDistricts())
        {
            Debug.Log($"Обработка района: {district.DistrictName}, DistrictPopularity: {district.DistrictPopularity}");
            int baseClients = Mathf.FloorToInt(district.DistrictPopularity / 10);
            foreach (var clientTypeWithMultiplier in district.ClientTypes)
            {
                var clientType = clientTypeWithMultiplier.ClientType;
                int multiplier = clientTypeWithMultiplier.Multiplier;
                int clientsToAdd = Mathf.FloorToInt(district.DistrictPopularity / multiplier);
                for (int j = 0; j < clientsToAdd; j++)
                {
                    int typeId = (int)clientType.clientType;
                    int currentCount = clientCountByType.ContainsKey(typeId) ? clientCountByType[typeId] : 0;
                    if (currentCount < clientType.maxClientPerScene)
                    {
                        extraVisitors.Add(clientType);
                        clientCountByType[typeId] = currentCount + 1;
                        Debug.Log($"Добавлен клиент {clientType.name} (тип {typeId}) в extraVisitors из района {district.DistrictName}.");
                    }
                    else
                    {
                        Debug.Log($"Превышен лимит maxClientPerScene ({clientType.maxClientPerScene}) для типа {clientType.clientType}, клиент не добавлен.");
                    }
                }
            }
        }

        Debug.Log($"Перенос завершен, общее количество extraVisitors: {extraVisitors.Count}");
        Debug.Log("Метод TransferExtraVisitorsFromDistrictManager завершен.");
    }

    public void HandleSceneTransition(string sceneName, LoadSceneMode mode, bool dayCompleted, string previousScene, int dayCount)
    {
        Debug.Log($"HandleSceneTransition: scene = {sceneName}, mode = {mode}, dayCompleted = {dayCompleted}, previousScene = {previousScene}, dayCount = {dayCount}");
        if (sceneName == "ManagmentScene" && mode == LoadSceneMode.Additive && dayCompleted && previousScene == "MainScene" && dayCount > 0)
        {
            Debug.Log("HandleSceneTransition: Условие выполнено, начало обработки.");
            DistrictManager.Instance.CalculatePopularityGain();
            float totalPreliminaryPopularity = 0;
            foreach (var district in GameManager.Instance.Districts)
            {
                Debug.Log($"PreliminaryPopularity для {district.DistrictName}: {district.PreliminaryPopularity}");
                totalPreliminaryPopularity += district.PreliminaryPopularity;
            }
            Debug.Log($"Общий totalPreliminaryPopularity: {totalPreliminaryPopularity}");
            UpdatePopularity(popularity + totalPreliminaryPopularity);
            Debug.Log($"Новая популярность в GameStateManager: {popularity}");
            DistrictManager.Instance.ApplyPreliminaryPopularityOnly();
            foreach (var district in GameManager.Instance.Districts)
            {
                Debug.Log($"Обновлённая districtPopularity для {district.DistrictName}: {district.DistrictPopularity}");
            }
            DistrictManager.Instance.TransferPopularityToGameState();
            ApplyTemporaryPopularity();
            DistrictManager.Instance.ClearActiveEmployees();
            DistrictManager.Instance.GenerateDistrictExtraVisitors();
            Debug.Log("GenerateDistrictExtraVisitors executed.");
            GameManager.Instance.SetDayCompleted(false); // Сбрасываем флаг после выполнения
            Debug.Log("GenerateDistrictExtraVisitors executed, dayCompleted reset to false");
        }
        else
        {
            Debug.Log("HandleSceneTransition: Условие не выполнено, обработка пропущена.");
        }
    }
}