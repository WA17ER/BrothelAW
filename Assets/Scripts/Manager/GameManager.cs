using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int baseVisitors = 3;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int minSpawnDelay = 1;
    [SerializeField] private int maxSpawnDelay = 5;
    [SerializeField] private float maxDayDuration = 300f;
    [SerializeField] private float initialGold = 1000f;
    [SerializeField] private float initialPopularity = 100f;
    [SerializeField] private List<GameObject> clientType1Prefabs;
    [SerializeField] private List<GameObject> clientType2Prefabs;
    [SerializeField] private List<GameObject> clientType3Prefabs;
    [SerializeField] private List<GameObject> clientType4Prefabs;
    [SerializeField] private float popularityPenalty = -10f;
    [SerializeField] private List<EmployeeDataSO> availableEmployeeData;
    [SerializeField] private EmployeeDataSO[] initialEmployees;
    [SerializeField] private int maxClientsPerDay = 40;
    [SerializeField] private float progressPerService = 10f;
    [SerializeField] private float minRemainingTimeForLastClient = 30f;
    [SerializeField] private List<Employee> sickEmployees;

    private List<ClientData> clientPool = new List<ClientData>();
    private Dictionary<int, int> extraVisitors = new Dictionary<int, int>();
    private List<Employee> activeEmployees = new List<Employee>();
    private float currentGold;
    private float currentPopularity;
    private int dayCount = 0;
    private int clientsSpawnedToday = 0;
    private int difficultyLevel = 1;
    private Dictionary<int, List<GameObject>> clientVisualModels;
    private bool isDayActive = false;
    private bool isDayPaused = false;
    private Coroutine dayCycleCoroutine;
    private float dayStartTime;

    public UnityEvent onStateChange;
    public UnityEvent onEmployeeListChanged;
    public UnityEvent<ClientData, Employee> onEmployeeAssigned;
    public UnityEvent<ClientData, Employee> onServiceCompleted;

    public enum ClientType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3,
        Type4 = 4
    }

    public int BaseVisitors => baseVisitors;
    public int MinSpawnDelay => minSpawnDelay;
    public int MaxSpawnDelay => maxSpawnDelay;
    public float MaxDayDuration => maxDayDuration;
    public Dictionary<int, int> ExtraVisitors => extraVisitors;
    public int TotalClients => Mathf.Min(baseVisitors + extraVisitors.Values.Sum(), maxClientsPerDay);
    public List<ClientData> ClientPool => clientPool;
    public int ClientsSpawnedToday { get => clientsSpawnedToday; set { clientsSpawnedToday = value; onStateChange.Invoke(); } }
    public Dictionary<int, List<GameObject>> ClientVisualModels => clientVisualModels;
    public List<Employee> ActiveEmployees => activeEmployees;
    public int DifficultyLevel => difficultyLevel;
    public bool IsDayPaused => isDayPaused;
    public float ProgressPerService => progressPerService;
    public Transform SpawnPoint => spawnPoint;
    public int MaxClientsPerDay => maxClientsPerDay;
    public float MinRemainingTimeForLastClient => minRemainingTimeForLastClient;
    public float DayStartTime => dayStartTime;
    public List<Employee> SickEmployees => sickEmployees;

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

        foreach (var employeeData in initialEmployees)
        {
            AddEmployee(employeeData);
        }

        onStateChange.Invoke();
    }

    private void OnEnable()
    {
        onEmployeeListChanged.AddListener(SyncEmployeeLists);
        onStateChange.AddListener(LogGameState);
        onEmployeeAssigned.AddListener(OnEmployeeAssignedHandler);
        onServiceCompleted.AddListener(OnServiceCompletedHandler);
    }

    private void OnDisable()
    {
        onEmployeeListChanged.RemoveListener(SyncEmployeeLists);
        onStateChange.RemoveListener(LogGameState);
        onEmployeeAssigned.RemoveListener(OnEmployeeAssignedHandler);
        onServiceCompleted.RemoveListener(OnServiceCompletedHandler);
    }

    private void UpdateExtraVisitors()
    {
        extraVisitors.Clear();
        extraVisitors[1] = Mathf.Min(baseVisitors + Mathf.FloorToInt(currentPopularity / 100), 20);
        extraVisitors[2] = Mathf.Min(Mathf.FloorToInt(currentPopularity / 300), 10);
        extraVisitors[3] = Mathf.Min(Mathf.FloorToInt(currentPopularity / 500), 10);
        extraVisitors[4] = Mathf.Min(Mathf.FloorToInt(currentPopularity / 1000), 5);

        int total = extraVisitors.Values.Sum();
        if (total > maxClientsPerDay)
        {
            float scale = (float)maxClientsPerDay / total;
            extraVisitors[1] = Mathf.FloorToInt(extraVisitors[1] * scale);
            extraVisitors[2] = Mathf.FloorToInt(extraVisitors[2] * scale);
            extraVisitors[3] = Mathf.FloorToInt(extraVisitors[3] * scale);
            extraVisitors[4] = Mathf.FloorToInt(extraVisitors[4] * scale);
        }

        Debug.Log($"Подготовлено клиентов: Type1 = {extraVisitors[1]}, Type2 = {extraVisitors.GetValueOrDefault(2, 0)}, Type3 = {extraVisitors.GetValueOrDefault(3, 0)}, Type4 = {extraVisitors.GetValueOrDefault(4, 0)}");
    }

    [ContextMenu("Start Day")]
    public void StartDay()
    {
        if (isDayActive)
        {
            Debug.LogWarning("День уже активен, нельзя начать новый.");
            return;
        }
        dayCount++;
        dayStartTime = Time.time;
        isDayActive = true;
        isDayPaused = false;
        clientsSpawnedToday = 0;
        clientPool.Clear();
        currentPopularity = initialPopularity;
        UpdateExtraVisitors();
        Debug.Log($"Ожидается клиентов: всего {extraVisitors.Values.Sum()}, Тип 1: {extraVisitors[1]}, Тип 2: {extraVisitors.GetValueOrDefault(2, 0)}, Тип 3: {extraVisitors.GetValueOrDefault(3, 0)}, Тип 4: {extraVisitors.GetValueOrDefault(4, 0)}");
        Debug.Log($"День {dayCount} начался.");
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.AddEmployees(activeEmployees);
        }
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.Initialize(clientType1Prefabs, clientType2Prefabs, clientType3Prefabs, clientType4Prefabs, extraVisitors);
        }
        else
        {
            Debug.LogError("SpawnHandler не найден.");
        }
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.StartSpawning();
        }
        dayCycleCoroutine = StartCoroutine(DayCycle());
    }

    [ContextMenu("Heal All Employees")]
    public void HealAllEmployees()
    {
        if (sickEmployees.Count == 0)
        {
            Debug.Log("Список sickEmployees пуст, нет сотрудниц для лечения.");
            return;
        }

        float totalCost = 0f;
        List<Employee> copy = new List<Employee>(sickEmployees);
        foreach (var employee in copy)
        {
            if (employee != null && employee.ActiveSick != null)
            {
                float healingCost = employee.ActiveSick.HealingCost;
                if (currentGold >= healingCost)
                {
                    currentGold -= healingCost;
                    totalCost += healingCost;
                    employee.Heal();
                }
                else
                {
                    Debug.Log($"Недостаточно золота для лечения {employee.name} ({healingCost} требуется, доступно {currentGold}).");
                }
            }
        }
        sickEmployees.RemoveAll(e => e == null || e.ActiveSick == null || e.GetState() == Employee.EmployeeState.Healing || e.GetState() == Employee.EmployeeState.Available);
        Debug.Log($"Все сотрудницы обработаны, вылечено за {totalCost} золота.");
        onStateChange.Invoke();
    }

    [ContextMenu("Pause Day")]
    public void PauseDay()
    {
        if (!isDayActive)
        {
            Debug.LogWarning("День не активен, нельзя поставить на паузу.");
            return;
        }
        if (isDayPaused)
        {
            Debug.LogWarning("День уже на паузе.");
            return;
        }
        isDayPaused = true;
        Debug.Log($"День {dayCount} поставлен на паузу.");

        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.PauseSpawning();
        }

        foreach (var client in clientPool)
        {
            var customerMovement = client.GetComponent<CustomerMovement>();
            if (customerMovement != null)
            {
                customerMovement.Pause();
            }
        }
    }

    [ContextMenu("Resume Day")]
    public void ResumeDay()
    {
        if (!isDayActive)
        {
            Debug.LogWarning("День не активен, нельзя возобновить.");
            return;
        }
        if (!isDayPaused)
        {
            Debug.LogWarning("День не на паузе.");
            return;
        }
        isDayPaused = false;
        Debug.Log($"День {dayCount} возобновлён.");

        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.ResumeSpawning();
        }

        foreach (var client in clientPool)
        {
            var customerMovement = client.GetComponent<CustomerMovement>();
            if (customerMovement != null)
            {
                customerMovement.Resume();
            }
        }
    }

    private IEnumerator DayCycle()
    {
        float elapsedTime = 0f;
        while (elapsedTime < maxDayDuration)
        {
            if (isDayPaused)
            {
                yield return null;
                continue;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        EndDay();
        Debug.Log($"День {dayCount} завершён.");
    }

    private void OnEmployeeAssignedHandler(ClientData client, Employee employee)
    {
        float reward = EmployeeManager.Instance.AssignEmployee(client, employee);
        currentGold += reward;
        Debug.Log($"Золото начислено: +{reward} для клиента {client.name} с сотрудницей {employee.name}.");
        onStateChange.Invoke();
    }

    private void OnServiceCompletedHandler(ClientData client, Employee employee)
    {
        currentPopularity += 5f;
        Debug.Log($"Популярность начислена: +5 для клиента {client.name} после обслуживания.");
        if (employee != null)
        {
            EmployeeManager.Instance.CompleteService(employee, client.RequestedService, client.ActiveSick != null);
        }
        else
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {client.name} при завершении обслуживания.");
        }
        clientPool.Remove(client);
        onStateChange.Invoke();
    }

    public void EnterService(CustomerMovement customer)
    {
        ClientData client = customer.GetComponent<ClientData>();
        Debug.Log($"Клиент {client.name} начал услугу.");
        customer.Visual.gameObject.SetActive(false);
        StartCoroutine(ServiceTimer(15f, customer, client));
    }

    private IEnumerator ServiceTimer(float time, CustomerMovement customer, ClientData client)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (isDayPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        customer.Visual.gameObject.SetActive(true);
        Debug.Log($"Клиент {client.name} закончил услугу.");
        if (client.SpecificEmployee == null)
        {
            Debug.LogWarning($"No employee assigned for client {client.name} at service completion.");
        }
        onServiceCompleted.Invoke(client, client.SpecificEmployee);
        customer.ExitService();
    }

    public void EndDay()
    {
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.PauseSpawning();
        }
        float totalCost = 0f;
        List<Employee> healingCopy = new List<Employee>(EmployeeManager.Instance.HealingEmployees);
        foreach (var employee in healingCopy)
        {
            if (employee != null && employee.ActiveSick != null)
            {
                employee.ProgressHealing();
            }
        }
        Debug.Log($"Все сотрудницы обработаны, вылечено за {totalCost} золота.");
        isDayActive = false;
        isDayPaused = false;
        if (dayCycleCoroutine != null)
        {
            StopCoroutine(dayCycleCoroutine);
            dayCycleCoroutine = null;
        }
        clientPool.Clear();
        clientsSpawnedToday = 0;
        difficultyLevel = Mathf.FloorToInt(dayCount / 5f) + 1;
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.EndDayUpdate();
            activeEmployees = EmployeeManager.Instance.GetAllEmployees();
        }
        UpdateExtraVisitors();
        onStateChange.Invoke();
    }

    [ContextMenu("End Day")]
    public void TestEndDay()
    {
        EndDay();
    }

    private void AddEmployee(EmployeeDataSO employeeData)
    {
        if (employeeData == null || !availableEmployeeData.Contains(employeeData))
        {
            Debug.LogError($"GameManager: EmployeeDataSO {employeeData?.employeeName} не найдена в availableEmployeeData или null.");
            return;
        }
        if (activeEmployees.Exists(e => e.Data == employeeData))
        {
            Debug.LogWarning($"GameManager: Сотрудница {employeeData.employeeName} уже добавлена.");
            return;
        }
        GameObject employeeGO = new GameObject(employeeData.employeeName);
        Employee employee = employeeGO.AddComponent<Employee>();
        employee.SetData(employeeData);
        activeEmployees.Add(employee);
        Debug.Log($"Сотрудница {employeeData.employeeName} добавлена в activeEmployees, состояние: {employee.GetState()}.");
        onEmployeeListChanged.Invoke();
    }

    private void SyncEmployeeLists()
    {
        foreach (var client in clientPool)
        {
            client.availableEmployees.Clear();
            client.availableEmployees.AddRange(EmployeeManager.Instance.AvailableEmployees);
        }
        Debug.Log($"Синхронизировано: {clientPool.Count} клиентов, {EmployeeManager.Instance.AvailableEmployees.Count} сотрудниц.");
    }

    private void LogGameState()
    {
        Debug.Log($"День: {dayCount}, Золото: {currentGold}, Популярность: {currentPopularity}, Сложность: {difficultyLevel}, " +
                  $"Активные клиенты: {clientPool.Count}, Активные сотрудницы: {activeEmployees.Count}, " +
                  $"Больные сотрудницы: {EmployeeManager.Instance.SickEmployees.Count}, " +
                  $"Сотрудницы на лечении: {EmployeeManager.Instance.HealingEmployees.Count}, " +
                  $"Доступные сотрудницы: {EmployeeManager.Instance.AvailableEmployees.Count}");
    }

    [ContextMenu("Save Progress")]
    public void SaveProgress()
    {
        PlayerPrefs.SetFloat("CurrentGold", currentGold);
        PlayerPrefs.SetFloat("CurrentPopularity", currentPopularity);
        PlayerPrefs.SetInt("DayCount", dayCount);
        PlayerPrefs.SetInt("DifficultyLevel", difficultyLevel);
        string[] employeeNames = EmployeeManager.Instance.GetAllEmployees().Select(e => e.Data.employeeName).ToArray();
        PlayerPrefs.SetString("ActiveEmployees", string.Join(",", employeeNames));
        foreach (var employee in EmployeeManager.Instance.GetAllEmployees())
        {
            PlayerPrefs.SetInt($"EmployeeState_{employee.Data.employeeName}", (int)employee.GetState());
            foreach (var skill in employee.Skills)
            {
                PlayerPrefs.SetInt($"EmployeeSkillLevel_{employee.Data.employeeName}_{skill.Key}", skill.Value.level);
                PlayerPrefs.SetInt($"EmployeeSkillProgress_{employee.Data.employeeName}_{skill.Key}", skill.Value.progress);
            }
            PlayerPrefs.SetFloat($"EmployeeStamina_{employee.Data.employeeName}", employee.StaminaCurrent);
        }
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранён.");
    }

    [ContextMenu("Load Progress")]
    public void LoadProgress()
    {
        currentGold = PlayerPrefs.GetFloat("CurrentGold", initialGold);
        currentPopularity = PlayerPrefs.GetFloat("CurrentPopularity", initialPopularity);
        dayCount = PlayerPrefs.GetInt("DayCount", 0);
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
                    AddEmployee(data);
                    Employee employee = activeEmployees.Find(e => e.Data.employeeName == name);
                    if (employee != null)
                    {
                        employee.SetState((Employee.EmployeeState)PlayerPrefs.GetInt($"EmployeeState_{name}", (int)Employee.EmployeeState.Available));
                        foreach (var skillName in data.BaseSkills)
                        {
                            var currentSkill = employee.Skills[skillName];
                            employee.Skills[skillName] = (PlayerPrefs.GetInt($"EmployeeSkillLevel_{name}_{skillName}", 0),
                                                         PlayerPrefs.GetInt($"EmployeeSkillProgress_{name}_{skillName}", 0));
                        }
                        employee.StaminaCurrent = PlayerPrefs.GetFloat($"EmployeeStamina_{name}", employee.StaminaMax);
                    }
                }
            }
        }
        onStateChange.Invoke();
    }
}