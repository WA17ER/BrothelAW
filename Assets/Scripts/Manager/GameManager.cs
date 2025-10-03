using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private int baseVisitors = 5;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform registerPoint;
    [SerializeField] private Transform servicePoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private int minSpawnDelay = 1;
    [SerializeField] private int maxSpawnDelay = 5;
    [SerializeField] private float maxDayDuration = 300f;
    [SerializeField] private int maxClientsPerDay = 40;
    [SerializeField] private float progressPerService = 10f;
    [SerializeField] private float minRemainingTimeForLastClient = 30f;
    [SerializeField] private Transform[] chairs;
    [SerializeField] private List<DistrictDataSO> districts;
    [SerializeField] private List<ClientDataSO> baseVisitorsList = new List<ClientDataSO>(); // Список базовых SO для рандома
    private List<ClientDataSO> extraVisitorsList = new List<ClientDataSO>(); // Из GameStateManager.ExtraVisitors
    public List<ClientDataSO> TotalClientList => totalClientList; // Геттер для total
    private List<ClientDataSO> totalClientList = new List<ClientDataSO>(); // Объединённый список для спавна

    private List<ClientData> clientPool = new List<ClientData>();
    private List<ClientDataSO> dailyVisitorList = new List<ClientDataSO>(); // Временный список
    private List<Employee> activeEmployees;
    private int dayCount;
    private int clientsSpawnedToday = 0;
    private int difficultyLevel;
    private bool isDayActive = false;
    private bool isDayPaused = false;
    private Coroutine dayCycleCoroutine;
    private float dayStartTime;
    private float totalPreliminaryPopularity;
    private bool dayCompleted = false;
    private string previousScene = "ManagmentScene";
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
    public int TotalClients
    {
        get
        {
            int total = totalClientList.Count;
            Debug.Log($"TotalClients calculated: {total}");
            return Mathf.Min(total, maxClientsPerDay);
        }
    }
    public List<ClientData> ClientPool => clientPool;
    public int ClientsSpawnedToday { get => clientsSpawnedToday; set { clientsSpawnedToday = value; onStateChange.Invoke(); } }
    public List<Employee> ActiveEmployees => activeEmployees;
    public int DifficultyLevel => difficultyLevel;
    public bool IsDayPaused => isDayPaused;
    public float ProgressPerService => progressPerService;
    public Transform SpawnPoint => spawnPoint;
    public Transform RegisterPoint => registerPoint;
    public Transform ServicePoint => servicePoint;
    public Transform ExitPoint => exitPoint;
    public Transform[] Chairs => chairs;
    public int MaxClientsPerDay => maxClientsPerDay;
    public float MinRemainingTimeForLastClient => minRemainingTimeForLastClient;
    public float DayStartTime => dayStartTime;
    public float CurrentGold { get => GameStateManager.Instance.Gold; }
    public float CurrentPopularity { get => GameStateManager.Instance.Popularity; }
    public List<DistrictDataSO> Districts => districts;
    public float TotalPreliminaryPopularity => totalPreliminaryPopularity;

    public void AddGold(float amount)
    {
        GameStateManager.Instance.UpdateGold(GameStateManager.Instance.Gold + amount);
        Debug.Log($"Добавлено золото: {amount}, итого: {GameStateManager.Instance.Gold}");
        onStateChange.Invoke();
    }
    public void SetDayCompleted(bool value)
    {
        dayCompleted = value;
    }

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
        activeEmployees = GameStateManager.Instance.Employees;
        dayCount = GameStateManager.Instance.DayCount;
        difficultyLevel = GameStateManager.Instance.DifficultyLevel;
        onStateChange = new UnityEvent();
        onStateChange.Invoke();
    }

    private void OnEnable()
    {
        onEmployeeListChanged.AddListener(SyncEmployeeLists);
        onStateChange.AddListener(LogGameState);
        onEmployeeAssigned.AddListener(OnEmployeeAssignedHandler);
        onServiceCompleted.AddListener(OnServiceCompletedHandler);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        onEmployeeListChanged.RemoveListener(SyncEmployeeLists);
        onStateChange.RemoveListener(LogGameState);
        onEmployeeAssigned.RemoveListener(OnEmployeeAssignedHandler);
        onServiceCompleted.RemoveListener(OnServiceCompletedHandler);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void UpdateDailyVisitorList()
    {
        dailyVisitorList.Clear();
        totalClientList.Clear();
        extraVisitorsList.Clear();
        // Base visitors with LINQ
        dailyVisitorList.AddRange(Enumerable.Range(0, baseVisitors).Select(_ => baseVisitorsList[Random.Range(0, baseVisitorsList.Count)]));
        // Extra visitors with LINQ grouping
        if (dayCount >= 1)
        {
            var grouped = GameStateManager.Instance.ExtraVisitors
                .GroupBy(so => (int)so.clientType)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kvp in grouped)
            {
                int typeId = kvp.Key;
                var clients = kvp.Value;
                int max = clients.First().maxClientPerScene;
                int count = Mathf.Min(clients.Count, max);
                extraVisitorsList.AddRange(clients.Take(count));
                dailyVisitorList.AddRange(extraVisitorsList);
            }
        }
        totalClientList.AddRange(dailyVisitorList);
    }

    [ContextMenu("Start Day")]
    public void StartDay()
    {
        if (isDayActive)
        {
            Debug.LogWarning("День уже активен, нельзя начать новый.");
            return;
        }
        dayCount = GameStateManager.Instance.DayCount + 1;
        GameStateManager.Instance.UpdateDayCount(dayCount);
        GameStateManager.Instance.UpdateGold(GameStateManager.Instance.Gold);
        GameStateManager.Instance.UpdatePopularity(GameStateManager.Instance.Popularity);
        if (dayCount % 7 == 0)
        {
            foreach (var so in baseVisitorsList)
            {
                so.minGold += so.stepPerWeek;
                so.maxGold += so.stepPerWeek;
                Debug.Log($"День {dayCount}: Увеличены minGold и maxGold на {so.stepPerWeek} для префаба {so.name}.");
            }
        }
        dayStartTime = Time.time;
        isDayActive = true;
        isDayPaused = false;
        clientsSpawnedToday = 0;
        clientPool.Clear();
        UpdateDailyVisitorList();
        Debug.Log($"StartDay: Ожидается клиентов: всего {TotalClients}, IsDayPaused = {isDayPaused}, SpawnPoint = {spawnPoint?.name}, dayCount = {dayCount}");
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.Initialize(totalClientList);
            foreach (ClientData client in clientPool)
            {
                client.OnStateChanged.AddListener(ClientManager.Instance.OnClientStateChanged);
            }
            SpawnHandler.Instance.OnClientSpawned.AddListener((client) =>
            {
                if (!ClientManager.Instance.AllClients.Contains(client))
                {
                    clientPool.Add(client);
                    client.OnStateChanged.AddListener(ClientManager.Instance.OnClientStateChanged);
                    ClientManager.Instance.RegisterClient(client);
                }
            });
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

    private void OnClientStateChanged(ClientData client)
    {
        Debug.Log($"Событие состояния для клиента {client.clientName} (ID: {client.ClientId}): {client.State}");
        onStateChange.Invoke();
    }

    [ContextMenu("Heal All Employees")]
    public void HealAllEmployees()
    {
        if (EmployeeManager.Instance.SickEmployees.Count == 0)
        {
            Debug.Log("Список sickEmployees пуст, нет сотрудниц для лечения.");
            return;
        }
        List<Employee> copy = new List<Employee>(EmployeeManager.Instance.SickEmployees);
        foreach (var employee in copy)
        {
            if (employee != null && employee.ActiveSick != null && (employee.GetState() == Employee.EmployeeState.Sick || employee.GetState() == Employee.EmployeeState.HeavySick))
            {
                float healingCost = employee.ActiveSick.HealingCost;
                if (GameStateManager.Instance.Gold >= healingCost)
                {
                    GameStateManager.Instance.UpdateGold(GameStateManager.Instance.Gold - healingCost);
                    employee.Heal();
                    EmployeeManager.Instance.HealingEmployees.Add(employee);
                    Debug.Log($"Лечение {employee.Data.employeeName} начато, стоимость: {healingCost}, длительность: {employee.ActiveSick.duration}.");
                }
                else
                {
                    Debug.Log($"Недостаточно золота для лечения {employee.Data.employeeName} ({healingCost} требуется, доступно {GameStateManager.Instance.Gold}).");
                }
            }
            else
            {
                Debug.LogWarning($"Сотрудница {employee?.Data.employeeName} не в состоянии Sick или HeavySick, или ActiveSick отсутствует, пропущена.");
            }
        }
        EmployeeManager.Instance.SickEmployees.Clear();
        Debug.Log("Все сотрудницы обработаны.");
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
        Debug.Log($"DayCycle: Начало цикла, maxDayDuration = {maxDayDuration}");
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
        Debug.Log($"DayCycle: Цикл завершён, вызывается EndDay");
        yield return StartCoroutine(EndDay());
    }

    public void SetPreviousScene(string scene)
    {
        previousScene = scene;
    }

    private IEnumerator EndDay()
    {
        Debug.Log($"EndDay: Начало завершения дня {dayCount}, isDayActive = {isDayActive}");
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.SyncWithGameState();
        }
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.PauseSpawning();
            while (SpawnHandler.Instance.IsSpawning)
            {
                yield return null;
            }
        }
        List<Employee> healingCopy = new List<Employee>(EmployeeManager.Instance.HealingEmployees);
        foreach (var employee in healingCopy)
        {
            if (employee != null && employee.ActiveSick != null)
            {
                employee.ProgressHealing();
                if (employee.HealingTimeRemaining <= 0)
                {
                    EmployeeManager.Instance.HealingEmployees.Remove(employee);
                }
            }
        }
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
        }
        GameStateManager.Instance.ExtraVisitors.Clear();
        dailyVisitorList.Clear(); // Очистка временного списка
        UpdateDailyVisitorList(); // Подготовка к следующему дню
        SetDayCompleted(true);
        previousScene = "MainScene"; // Убедиться, что предыдущая сцена устанавливается корректно
        onStateChange.Invoke();
        Debug.Log($"EndDay: Переход в ManagmentScene, dayCompleted = {dayCompleted}, previousScene = {previousScene}, dayCount = {dayCount}");
        SceneManager.LoadScene("ManagmentScene", LoadSceneMode.Additive);
        yield return null;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        yield break;
    }

    [ContextMenu("End Day")]
    public void TestEndDay()
    {
        StartCoroutine(EndDay());
    }

    public void EndDayPublic()
    {
        StartCoroutine(EndDay());
    }

    private void OnEmployeeAssignedHandler(ClientData client, Employee employee)
    {
        float reward = EmployeeManager.Instance.AssignEmployee(client, employee);
        AddGold(reward);
        onStateChange.Invoke();
    }

    private void OnServiceCompletedHandler(ClientData client, Employee employee)
    {
        if (employee != null)
        {
            EmployeeManager.Instance.CompleteService(employee, client.RequestedService, client.ActiveSick != null);
        }
        GameStateManager.Instance.UpdatePopularity(GameStateManager.Instance.Popularity + client.popularityGain);
        clientPool.RemoveAll(c => c.ClientId == client.ClientId);
        onStateChange.Invoke();
    }

    public void EnterService(CustomerMovement customer)
    {
        ClientData client = customer.GetComponent<ClientData>();
        StartCoroutine(ServiceTimer(10f, customer, client));
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
        onServiceCompleted.Invoke(client, client.SpecificEmployee);
        customer.ExitService();
    }

    private void SyncEmployeeLists()
    {
        foreach (var client in clientPool)
        {
            client.SelectedEmployee = null;
        }
    }

    private void LogGameState()
    {
        Debug.Log($"День: {dayCount}, Золото: {GameStateManager.Instance.Gold}, Популярность: {GameStateManager.Instance.Popularity}, Сложность: {difficultyLevel}, " +
                  $"Активные клиенты: {clientPool.Count}, Активные сотрудницы: {activeEmployees.Count}, " +
                  $"Больные: {EmployeeManager.Instance.SickEmployees.Count}, Лечение: {EmployeeManager.Instance.HealingEmployees.Count}, " +
                  $"Услуги: {EmployeeManager.Instance.OnServiceEmployees.Count}, Доступные: {EmployeeManager.Instance.AvailableEmployees.Count}, " +
                  $"Реклама: {EmployeeManager.Instance.MarketingEmployees.Count}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded: scene = {scene.name}, mode = {mode}, dayCompleted = {dayCompleted}, previousScene = {previousScene}, dayCount = {dayCount}, isDayActive = {isDayActive}");
        if (scene.name == "MainScene" && previousScene == "ManagmentScene")
        {
            Debug.Log("Условие для MainScene выполнено, вызывается TransferExtraVisitorsFromDistrictManager.");
            DistrictManager.Instance.ClearTemporaryEmployees();
            InitializePoints();
            if (dayCount > 0)
            {
                GameStateManager.Instance.TransferExtraVisitorsFromDistrictManager();
            }
            StartDay();
        }
        else if (scene.name == "ManagmentScene" && mode == LoadSceneMode.Additive)
        {
            Debug.Log($"OnSceneLoaded: Вызывается HandleSceneTransition для ManagmentScene");
            GameStateManager.Instance.HandleSceneTransition(scene.name, mode, dayCompleted, previousScene, dayCount);
        }
    }

    private void InitializePoints()
    {
        spawnPoint = GameObject.FindWithTag("SpawnPoint")?.transform;
        registerPoint = GameObject.FindWithTag("RegisterPoint")?.transform;
        servicePoint = GameObject.FindWithTag("ServicePoint")?.transform;
        exitPoint = GameObject.FindWithTag("SpawnPoint")?.transform;
        chairs = GameObject.FindGameObjectsWithTag("Chair").Select(go => go.transform).ToArray();
        if (spawnPoint == null) Debug.LogError("SpawnPoint not found.");
        if (registerPoint == null) Debug.LogError("RegisterPoint not found.");
        if (servicePoint == null) Debug.LogError("ServicePoint not found.");
        if (exitPoint == null) Debug.LogError("ExitPoint (using SpawnPoint) not found.");
        if (chairs == null || chairs.Length == 0) Debug.LogError("Chairs not found.");
    }
}