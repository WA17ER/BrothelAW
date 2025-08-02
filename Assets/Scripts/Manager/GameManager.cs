using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int baseVisitors = 10; // Клиенты типа 1
    [SerializeField] private Transform spawnPoint; // Точка спавна
    [SerializeField] private int minSpawnDelay = 5; // Мин. задержка спавна (сек)
    [SerializeField] private float maxDayDuration = 300f; // Длительность дня (сек)
    [SerializeField] private float initialGold = 1000f; // Начальное золото
    [SerializeField] private float initialPopularity = 100f; // Начальная популярность
    [SerializeField] private List<GameObject> clientType1Prefabs; // Префабы клиентов типа 1
    [SerializeField] private List<GameObject> clientType2Prefabs; // Префабы клиентов типа 2
    [SerializeField] private List<GameObject> clientType3Prefabs; // Префабы клиентов типа 3
    [SerializeField] private List<GameObject> clientType4Prefabs; // Префабы клиентов типа 4
    [SerializeField]
    private Dictionary<string, float> goldPerService = new Dictionary<string, float> // Базовая награда за услугу
    {
        { "Дрочка", 50f },
        { "Миньет", 60f },
        { "Дрочка Сиськами", 70f },
        { "Миссионерская", 80f },
        { "Наездница", 90f },
        { "Амазонка", 70f },
        { "Раком", 50f },
        { "Стоя", 80f }
    };
    [SerializeField] private float popularityPenalty = -10f; // Штраф за провал
    [SerializeField] private float healingCost = 50f; // Стоимость лечения
    [SerializeField] private List<EmployeeDataSO> availableEmployeeData; // Пул EmployeeDataSO
    [SerializeField] private EmployeeDataSO testEmployeeData; // Выбор сотрудницы для теста
    [SerializeField] private ClientType testClientType = ClientType.Type1; // Выбор типа клиента для теста

    private List<ClientRequest> clientPool = new List<ClientRequest>();
    private Dictionary<int, int> extraVisitors = new Dictionary<int, int>();
    private List<Employee> activeEmployees = new List<Employee>();
    private float currentGold;
    private float currentPopularity;
    private int dayCount = 1;
    private int clientsSpawnedToday = 0;
    private int difficultyLevel = 1;
    private Dictionary<int, List<GameObject>> clientVisualModels;

    public enum ClientType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3,
        Type4 = 4
    }

    public int BaseVisitors => baseVisitors;
    public int MinSpawnDelay => minSpawnDelay;
    public float MaxDayDuration => maxDayDuration;
    public Dictionary<int, int> ExtraVisitors => extraVisitors;
    public int TotalClients => baseVisitors + extraVisitors.Values.Sum();
    public List<ClientRequest> ClientPool => clientPool;
    public int ClientsSpawnedToday { get => clientsSpawnedToday; set => clientsSpawnedToday = value; }
    public Dictionary<int, List<GameObject>> ClientVisualModels => clientVisualModels;
    public List<Employee> ActiveEmployees => activeEmployees;
    public int DifficultyLevel => difficultyLevel;

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

        currentGold = initialGold;
        currentPopularity = initialPopularity;

        clientVisualModels = new Dictionary<int, List<GameObject>>
        {
            { 1, clientType1Prefabs },
            { 2, clientType2Prefabs },
            { 3, clientType3Prefabs },
            { 4, clientType4Prefabs }
        };

        UpdateExtraVisitors();
        LogGameState();
    }

    private void UpdateExtraVisitors()
    {
        extraVisitors.Clear();
        if (currentPopularity >= 500f)
        {
            extraVisitors[2] = 4;
        }
        if (currentPopularity >= 1000f)
        {
            extraVisitors[2] = 4;
            extraVisitors[3] = 2;
        }
        if (currentPopularity >= 1500f)
        {
            extraVisitors[2] = 4;
            extraVisitors[3] = 2;
            extraVisitors[4] = 1;
        }
    }

    public void EnterService(CustomerMovement customer)
    {
        ClientRequest client = customer.GetComponent<ClientRequest>();
        Debug.Log($"Клиент {customer.name} начал услугу.");
        customer.Visual.gameObject.SetActive(false);
        StartCoroutine(ServiceTimer(5f, customer, client));
    }

    private IEnumerator ServiceTimer(float time, CustomerMovement customer, ClientRequest client)
    {
        yield return new WaitForSeconds(time);
        customer.Visual.gameObject.SetActive(true);
        Debug.Log($"Клиент {customer.name} закончил услугу.");
        if (client.SelectedEmployee != null)
        {
            client.SelectedEmployee.UpdateStamina(1f);
            client.SelectedEmployee.CheckSick(client.RequestedService);
        }
        else
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {customer.name}.");
        }
        customer.ExitService();
    }

    public void ServeClient(ClientRequest client, Employee employee, bool success)
    {
        if (success)
        {
            int clientLevel = client.ClientLevel;
            string service = client.RequestedService;
            int skillLevel = employee.Skills[service].level;
            float reward = clientLevel * skillLevel * goldPerService[service];
            currentGold += reward;
            currentPopularity += 5f;
            employee.SetState(Employee.EmployeeState.Servicing);

            Debug.Log($"Клиент обслужен: +{reward} золота, +5 популярности.");
        }
        else
        {
            currentPopularity += popularityPenalty;
            Debug.Log($"Клиент не обслужен: {popularityPenalty} популярности.");
        }

        clientPool.Remove(client);
        LogGameState();
    }

    public void HealEmployee(Employee employee)
    {
        if (employee.GetState() == Employee.EmployeeState.Sick && currentGold >= healingCost)
        {
            currentGold -= healingCost;
            employee.SetState(Employee.EmployeeState.Healing);
            Debug.Log($"Сотрудница {employee.name} отправлена на лечение за {healingCost} золота.");
        }
        else
        {
            Debug.LogError($"Нельзя отправить на лечение: Недостаточно золота или сотрудница не больна.");
        }
    }

    public void EndDay()
    {
        clientPool.Clear();
        clientsSpawnedToday = 0;
        dayCount++;
        difficultyLevel = Mathf.FloorToInt(dayCount / 5f) + 1;

        foreach (var employee in activeEmployees)
        {
            Debug.Log($"Обновление дня для сотрудницы {employee.name}, текущее состояние: {employee.GetState()}");
            employee.EndDayUpdate();
        }

        UpdateExtraVisitors();
        LogGameState();
    }

    [ContextMenu("End Day")]
    public void TestEndDay()
    {
        EndDay();
    }

    [ContextMenu("Add Test Employee")]
    public void AddTestEmployee()
    {
        if (testEmployeeData == null)
        {
            Debug.LogError("TestEmployeeData не выбрана в GameManager.");
            return;
        }
        if (availableEmployeeData.Contains(testEmployeeData) && !activeEmployees.Exists(e => e.Data == testEmployeeData))
        {
            GameObject employeeGO = new GameObject(testEmployeeData.name);
            Employee employee = employeeGO.AddComponent<Employee>();
            employee.SetData(testEmployeeData);
            activeEmployees.Add(employee);
            SyncEmployeeLists();
            if (EmployeeManager.Instance != null)
            {
                EmployeeManager.Instance.AddEmployeeData(testEmployeeData);
            }
            Debug.Log($"Тестовая сотрудница {testEmployeeData.name} добавлена.");
        }
        else
        {
            Debug.LogError($"Сотрудница {testEmployeeData.name} уже добавлена или отсутствует в availableEmployeeData.");
        }
    }

    private void SyncEmployeeLists()
    {
        foreach (var client in clientPool)
        {
            client.availableEmployees.Clear();
            client.availableEmployees.AddRange(activeEmployees);
        }
        Debug.Log($"Синхронизировано: {clientPool.Count} клиентов, {activeEmployees.Count} сотрудниц.");
    }

    [ContextMenu("Add Test Client")]
    public void AddTestClient()
    {
        int clientType = (int)testClientType;
        if (!clientVisualModels.ContainsKey(clientType) || clientVisualModels[clientType].Count == 0)
        {
            Debug.LogError($"Нет префабов клиента типа {clientType} для теста.");
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint не назначен в GameManager.");
            return;
        }
        GameObject clientGO = Instantiate(clientVisualModels[clientType][Random.Range(0, clientVisualModels[clientType].Count)], spawnPoint.position, Quaternion.identity);
        ClientRequest client = clientGO.GetComponent<ClientRequest>();
        if (client == null)
        {
            Debug.LogError($"ClientRequest отсутствует на клиенте типа {clientType}.");
            Destroy(clientGO);
            return;
        }
        client.clientLevel = clientType;
        clientPool.Add(client);
        SyncEmployeeLists();
        Debug.Log($"Тестовый клиент типа {clientType} добавлен на позиции {spawnPoint.position}.");
        Debug.Log($"Запрос клиента после спавна: Услуга - {client.RequestedService}, Preferences - {string.Join(", ", client.Preferences)}, Specific: {(client.SpecificEmployee != null ? client.SpecificEmployee.name : "None")}");
    }

    [ContextMenu("Test Serve Client")]
    public void TestServeClient()
    {
        if (activeEmployees.Count == 0 || clientPool.Count == 0)
        {
            Debug.LogError("Тест невозможен: нет активных сотрудниц или клиентов.");
            return;
        }
        ClientRequest client = clientPool[0];
        Employee employee = client.SelectedEmployee != null ? client.SelectedEmployee : activeEmployees[0];
        if (client.SelectedEmployee == null)
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {client.name}, используется {employee.name}.");
        }
        ServeClient(client, employee, client.IsMatch(employee));
    }

    private void LogGameState()
    {
        Debug.Log($"День: {dayCount}, Золото: {currentGold}, Популярность: {currentPopularity}, Клиенты сегодня: {clientsSpawnedToday}, Сложность: {difficultyLevel}");
        Debug.Log($"Активные клиенты: {clientPool.Count}, Активные сотрудницы: {activeEmployees.Count}");
    }

    [ContextMenu("Save Progress")]
    public void SaveProgress()
    {
        PlayerPrefs.SetFloat("CurrentGold", currentGold);
        PlayerPrefs.SetFloat("CurrentPopularity", currentPopularity);
        PlayerPrefs.SetInt("DayCount", dayCount);
        PlayerPrefs.SetInt("DifficultyLevel", difficultyLevel);
        string[] employeeNames = activeEmployees.Select(e => e.Data.name).ToArray();
        PlayerPrefs.SetString("ActiveEmployees", string.Join(",", employeeNames));
        foreach (var employee in activeEmployees)
        {
            PlayerPrefs.SetInt($"EmployeeState_{employee.Data.name}", (int)employee.GetState());
            foreach (var skill in employee.Skills)
            {
                PlayerPrefs.SetInt($"EmployeeSkillLevel_{employee.Data.name}_{skill.Key}", skill.Value.level);
                PlayerPrefs.SetInt($"EmployeeSkillProgress_{employee.Data.name}_{skill.Key}", skill.Value.progress);
            }
            PlayerPrefs.SetFloat($"EmployeeStamina_{employee.Data.name}", employee.StaminaCurrent);
        }
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранён.");
    }

    [ContextMenu("Load Progress")]
    public void LoadProgress()
    {
        currentGold = PlayerPrefs.GetFloat("CurrentGold", initialGold);
        currentPopularity = PlayerPrefs.GetFloat("CurrentPopularity", initialPopularity);
        dayCount = PlayerPrefs.GetInt("DayCount", 1);
        difficultyLevel = PlayerPrefs.GetInt("DifficultyLevel", 1);

        string employeeNames = PlayerPrefs.GetString("ActiveEmployees", "");
        if (!string.IsNullOrEmpty(employeeNames))
        {
            string[] names = employeeNames.Split(',');
            foreach (var name in names)
            {
                EmployeeDataSO data = Resources.Load<EmployeeDataSO>($"SO/Employee/{name}");
                if (data != null)
                {
                    GameObject employeeGO = new GameObject(name);
                    Employee employee = employeeGO.AddComponent<Employee>();
                    employee.SetData(data);
                    activeEmployees.Add(employee);
                    employee.SetState((Employee.EmployeeState)PlayerPrefs.GetInt($"EmployeeState_{name}", (int)Employee.EmployeeState.Available));
                    foreach (var skillName in data.BaseSkills)
                    {
                        employee.Skills[skillName].level = PlayerPrefs.GetInt($"EmployeeSkillLevel_{name}_{skillName}", 0);
                        employee.Skills[skillName].progress = PlayerPrefs.GetInt($"EmployeeSkillProgress_{name}_{skillName}", 0);
                    }
                    employee.StaminaCurrent = PlayerPrefs.GetFloat($"EmployeeStamina_{name}", employee.StaminaMax);
                    if (EmployeeManager.Instance != null)
                    {
                        EmployeeManager.Instance.AddEmployeeData(data);
                    }
                }
            }
        }

        SyncEmployeeLists();
        Debug.Log("Прогресс загружен.");
        LogGameState();
    }

    [ContextMenu("Heal Test Employee")]
    public void HealTestEmployee()
    {
        if (activeEmployees.Count == 0)
        {
            Debug.LogError("Нет активных сотрудниц для теста лечения.");
            return;
        }
        HealEmployee(activeEmployees[0]);
    }
}