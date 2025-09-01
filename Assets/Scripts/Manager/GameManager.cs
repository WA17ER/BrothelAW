using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private float initialGold = 1000f;
    [SerializeField] private float initialPopularity = 1200f;
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
    [SerializeField] private List<Employee> healingEmployees;
    [SerializeField] private Transform[] chairs;

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
    public int TotalClients
    {
        get
        {
            int total = extraVisitors.Values.Sum();
            Debug.Log($"TotalClients calculated: {total}");
            return Mathf.Min(total, maxClientsPerDay);
        }
    }
    public List<ClientData> ClientPool => clientPool;
    public int ClientsSpawnedToday { get => clientsSpawnedToday; set { clientsSpawnedToday = value; onStateChange.Invoke(); } }
    public Dictionary<int, List<GameObject>> ClientVisualModels => clientVisualModels;
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
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> HealingEmployees => healingEmployees;
    public float CurrentGold { get => currentGold; private set => currentGold = value; }
    public float CurrentPopularity { get => currentPopularity; private set => currentPopularity = value; }

    public void AddGold(float amount)
    {
        currentGold += amount;
        Debug.Log($"Добавлено золото: {amount}, итого: {currentGold}");
        onStateChange.Invoke();
    }

    public int GetClientType()
    {
        List<int> availableTypes = new List<int>();
        foreach (var pair in extraVisitors)
        {
            if (pair.Value > 0)
            {
                availableTypes.Add(pair.Key);
            }
        }
        if (availableTypes.Count == 0)
        {
            return 1;
        }
        int selectedType = availableTypes[Random.Range(0, availableTypes.Count)];
        extraVisitors[selectedType]--;
        return selectedType;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохранение между сценами (опционально)
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

        onStateChange = new UnityEvent(); // Инициализация события
        onStateChange.Invoke();
    }

    private void Start()
    {
        foreach (var employeeData in initialEmployees)
        {
            AddEmployee(employeeData);
        }
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

        Debug.Log($"Подготовлено клиентов: Type1 = {extraVisitors[1]}, Type2 = {extraVisitors[2]}, Type3 = {extraVisitors[3]}, Type4 = {extraVisitors[4]}, TotalClients = {TotalClients}");
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
        if (dayCount % 7 == 0)
        {
            foreach (var prefabs in new[] { clientType1Prefabs, clientType2Prefabs, clientType3Prefabs, clientType4Prefabs })
            {
                foreach (var prefab in prefabs)
                {
                    var clientData = prefab.GetComponent<ClientData>();
                    if (clientData != null && clientData.ClientDataSO != null)
                    {
                        clientData.ClientDataSO.minGold += clientData.ClientDataSO.stepPerWeek;
                        clientData.ClientDataSO.maxGold += clientData.ClientDataSO.stepPerWeek;
                        Debug.Log($"День {dayCount}: Увеличены minGold и maxGold на {clientData.ClientDataSO.stepPerWeek} для префаба {prefab.name}.");
                    }
                }
            }
        }
        dayStartTime = Time.time;
        isDayActive = true;
        isDayPaused = false;
        clientsSpawnedToday = 0;
        clientPool.Clear();
        currentPopularity = initialPopularity;
        UpdateExtraVisitors();
        Debug.Log($"Ожидается клиентов: всего {TotalClients}, Тип 1: {extraVisitors[1]}, Тип 2: {extraVisitors[2]}, Тип 3: {extraVisitors[3]}, Тип 4: {extraVisitors[4]}");
        Debug.Log($"День {dayCount} начался.");
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.AddEmployees(activeEmployees);
        }
        if (SpawnHandler.Instance != null)
        {
            Debug.Log($"clientPool перед инициализацией: {string.Join(", ", clientPool.Select(c => c?.clientName ?? "null"))}");
            SpawnHandler.Instance.Initialize(clientType1Prefabs, clientType2Prefabs, clientType3Prefabs, clientType4Prefabs, extraVisitors);
            foreach (ClientData client in clientPool)
            {
                client.OnStateChanged.AddListener(ClientManager.Instance.OnClientStateChanged);
            }
            SpawnHandler.Instance.OnClientSpawned.AddListener((client) =>
            {
                Debug.Log($"Обработчик OnClientSpawned получил клиента {client.clientName}, регистрация начата");
                Debug.Log($"clientPool перед проверкой: {string.Join(", ", clientPool.Select(c => c?.clientName ?? "null"))}");
                if (!ClientManager.Instance.AllClients.Contains(client)) // Изменено условие
                {
                    Debug.Log($"Условие !allClients.Contains(client) сработало для {client.clientName}");
                    clientPool.Add(client);
                    client.OnStateChanged.AddListener(ClientManager.Instance.OnClientStateChanged);
                    Debug.Log($"Попытка вызвать RegisterClient для клиента {client.clientName}, Instance: {ClientManager.Instance != null}");
                    ClientManager.Instance.RegisterClient(client); // Вызов регистрации
                }
                else
                {
                    Debug.Log($"Клиент {client.clientName} уже в allClients, пропущен");
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
        Debug.Log($"Событие состояния для клиента {client.clientName}: {client.State}");
        onStateChange.Invoke();
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
            if (employee != null && employee.ActiveSick != null && (employee.GetState() == Employee.EmployeeState.Sick || employee.GetState() == Employee.EmployeeState.HeavySick))
            {
                float healingCost = employee.ActiveSick.HealingCost;
                if (currentGold >= healingCost)
                {
                    currentGold -= healingCost;
                    totalCost += healingCost;
                    employee.Heal();
                    healingEmployees.Add(employee);
                    Debug.Log($"Лечение {employee.Data.employeeName} начато, стоимость: {healingCost}, длительность: {employee.ActiveSick.duration}.");
                }
                else
                {
                    Debug.Log($"Недостаточно золота для лечения {employee.Data.employeeName} ({healingCost} требуется, доступно {currentGold}).");
                }
            }
            else
            {
                Debug.LogWarning($"Сотрудница {employee?.Data.employeeName} не в состоянии Sick или HeavySick, или ActiveSick отсутствует, пропущена.");
            }
        }
        sickEmployees.Clear();
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
        AddGold(reward);
        Debug.Log($"Золото начислено: +{reward} для клиента {client.clientName} с сотрудницей {employee.Data.employeeName}.");
        onStateChange.Invoke();
    }

    private void OnServiceCompletedHandler(ClientData client, Employee employee)
    {
        if (employee != null)
        {
            EmployeeManager.Instance.CompleteService(employee, client.RequestedService, client.ActiveSick != null);
        }
        else
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {client.clientName} при завершении обслуживания.");
        }
        currentPopularity += 5f;
        Debug.Log($"Популярность начислена: +5 для клиента {client.clientName} после обслуживания.");
        clientPool.Remove(client);
        onStateChange.Invoke();
    }

    public void EnterService(CustomerMovement customer)
    {
        ClientData client = customer.GetComponent<ClientData>();
        Debug.Log($"Клиент {client.clientName} начал услугу.");
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
        Debug.Log($"Клиент {client.clientName} закончил услугу, сотрудница: {client.SpecificEmployee?.Data.employeeName ?? "none"}.");
        if (client.SpecificEmployee == null)
        {
            Debug.LogWarning($"No employee assigned for client {client.clientName} at service completion.");
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
        List<Employee> healingCopy = new List<Employee>(healingEmployees);
        foreach (var employee in healingCopy)
        {
            if (employee != null && employee.ActiveSick != null)
            {
                employee.ProgressHealing();
                Debug.Log($"Сотрудница {employee.Data.employeeName} лечится, осталось {employee.HealingTimeRemaining} дней.");
                if (employee.HealingTimeRemaining <= 0)
                {
                    healingEmployees.Remove(employee);
                    Debug.Log($"Сотрудница {employee.Data.employeeName} выздоровела.");
                }
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
        EmployeeManager.Instance.MoveEmployeeToList(employee);
        Debug.Log($"Сотрудница {employeeData.employeeName} добавлена в activeEmployees, состояние: {employee.GetState()}.");
        onEmployeeListChanged.Invoke();
    }

    private void SyncEmployeeLists()
    {
        foreach (var client in clientPool)
        {
            client.SelectedEmployee = null; // Clear selected employee when employee list changes
            Debug.Log($"Синхронизировано для клиента {client.clientName}: выбранная сотрудница сброшена.");
        }
    }

    private void LogGameState()
    {
        Debug.Log($"День: {dayCount}, Золото: {CurrentGold}, Популярность: {CurrentPopularity}, Сложность: {DifficultyLevel}, " +
                  $"Активные клиенты: {clientPool.Count}, Активные сотрудницы: {activeEmployees.Count}, " +
                  $"Больные сотрудницы: {EmployeeManager.Instance.SickEmployees.Count}, " +
                  $"Сотрудницы на лечении: {EmployeeManager.Instance.HealingEmployees.Count}, " +
                  $"Сотрудницы на услуге: {EmployeeManager.Instance.OnServiceEmployees.Count}, " +
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
            PlayerPrefs.SetFloat($"EmployeeHealingTime_{employee.Data.employeeName}", employee.HealingTimeRemaining);
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
        string[] employeeNames = PlayerPrefs.GetString("ActiveEmployees", "").Split(',');
        foreach (var name in employeeNames)
        {
            if (!string.IsNullOrEmpty(name))
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
                        employee.HealingTimeRemaining = PlayerPrefs.GetFloat($"EmployeeHealingTime_{name}", 0f);
                    }
                }
            }
        }
        onStateChange.Invoke();
    }
}