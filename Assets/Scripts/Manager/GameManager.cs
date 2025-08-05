using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int baseVisitors = 10;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int minSpawnDelay = 7;
    [SerializeField] private float maxDayDuration = 300f;
    [SerializeField] private float initialGold = 1000f;
    [SerializeField] private float initialPopularity = 100f;
    [SerializeField] private List<GameObject> clientType1Prefabs;
    [SerializeField] private List<GameObject> clientType2Prefabs;
    [SerializeField] private List<GameObject> clientType3Prefabs;
    [SerializeField] private List<GameObject> clientType4Prefabs;
    [SerializeField] private float popularityPenalty = -10f;
    [SerializeField] private float healingCost = 50f;
    [SerializeField] private List<EmployeeDataSO> availableEmployeeData;
    [SerializeField] private EmployeeDataSO[] initialEmployees;
    [SerializeField] private int maxClientsPerDay = 40;
    [SerializeField] private float progressPerService = 10f;

    private List<ClientData> clientPool = new List<ClientData>();
    private Dictionary<int, int> extraVisitors = new Dictionary<int, int>();
    private List<Employee> activeEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private float currentGold;
    private float currentPopularity;
    private int dayCount = 1;
    private int clientsSpawnedToday = 0;
    private int difficultyLevel = 1;
    private Dictionary<int, List<GameObject>> clientVisualModels;
    private bool isDayActive = false;
    private bool isDayPaused = false;
    private Coroutine dayCycleCoroutine;

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
    public float MaxDayDuration => maxDayDuration;
    public Dictionary<int, int> ExtraVisitors => extraVisitors;
    public int TotalClients => Mathf.Min(baseVisitors + extraVisitors.Values.Sum(), maxClientsPerDay);
    public List<ClientData> ClientPool => clientPool;
    public int ClientsSpawnedToday { get => clientsSpawnedToday; set { clientsSpawnedToday = value; onStateChange.Invoke(); } }
    public Dictionary<int, List<GameObject>> ClientVisualModels => clientVisualModels;
    public List<Employee> ActiveEmployees => activeEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public int DifficultyLevel => difficultyLevel;
    public bool IsDayPaused => isDayPaused;
    public float ProgressPerService => progressPerService;

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

        if (initialEmployees.Length != 3)
        {
            Debug.LogError("GameManager: initialEmployees должен содержать ровно 3 сотрудницы.");
        }
        else
        {
            foreach (var employeeData in initialEmployees)
            {
                AddEmployee(employeeData);
            }
        }

        UpdateExtraVisitors();
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
        int remainingClients = maxClientsPerDay - baseVisitors;
        if (remainingClients <= 0) return;

        if (currentPopularity >= 500f)
        {
            extraVisitors[2] = Mathf.Min(4, remainingClients);
            remainingClients -= extraVisitors[2];
        }
        if (currentPopularity >= 1000f && remainingClients > 0)
        {
            extraVisitors[3] = Mathf.Min(2, remainingClients);
            remainingClients -= extraVisitors[3];
        }
        if (currentPopularity >= 1500f && remainingClients > 0)
        {
            extraVisitors[4] = Mathf.Min(1, remainingClients);
        }

        if (baseVisitors + extraVisitors.Values.Sum() > maxClientsPerDay)
        {
            Debug.Log($"Лимит клиентов применён: {baseVisitors + extraVisitors.Values.Sum()} ограничено до {maxClientsPerDay}.");
        }
    }

    [ContextMenu("Start Day")]
    public void StartDay()
    {
        if (isDayActive)
        {
            Debug.LogWarning("День уже активен, нельзя начать новый.");
            return;
        }
        isDayActive = true;
        isDayPaused = false;
        clientsSpawnedToday = 0;
        clientPool.Clear();
        UpdateExtraVisitors();
        Debug.Log($"День {dayCount} начался.");
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.AddEmployees(activeEmployees.Where(e => e.GetState() == Employee.EmployeeState.Available).ToList());
        }
        if (SpawnHandler.Instance != null)
        {
            SpawnHandler.Instance.StartSpawning();
        }
        else
        {
            Debug.LogError("SpawnHandler не найден.");
        }
        dayCycleCoroutine = StartCoroutine(DayCycle());
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
            EmployeeManager.Instance.CompleteService(employee, client.RequestedService, client.Data.isSick);
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
        Debug.Log($"Клиент {customer.name} начал услугу.");
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
        Debug.Log($"Клиент {customer.name} закончил услугу.");
        if (client.SpecificEmployee == null)
        {
            Debug.LogWarning($"No employee assigned for client {client.name} at service completion.");
        }
        onServiceCompleted.Invoke(client, client.SpecificEmployee);
        customer.ExitService();
    }

    public void HealEmployee(Employee employee)
    {
        if (employee == null)
        {
            Debug.LogError("HealEmployee: Сотрудница null.");
            return;
        }
        if (employee.GetState() == Employee.EmployeeState.Sick && currentGold >= healingCost)
        {
            currentGold -= healingCost;
            employee.SetState(Employee.EmployeeState.Healing);
            sickEmployees.Add(employee);
            Debug.Log($"Сотрудница {employee.name} отправлена на лечение за {healingCost} золота.");
            onStateChange.Invoke();
        }
        else
        {
            Debug.LogError($"Нельзя отправить на лечение: Недостаточно золота ({currentGold}/{healingCost}) или сотрудница {employee.name} не больна (состояние: {employee.GetState()}).");
        }
    }

    public void EndDay()
    {
        isDayActive = false;
        isDayPaused = false;
        if (dayCycleCoroutine != null)
        {
            StopCoroutine(dayCycleCoroutine);
            dayCycleCoroutine = null;
        }
        clientPool.Clear();
        clientsSpawnedToday = 0;
        dayCount++;
        difficultyLevel = Mathf.FloorToInt(dayCount / 5f) + 1;
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.EndDayUpdate();
            activeEmployees = EmployeeManager.Instance.GetAllEmployees();
            sickEmployees = EmployeeManager.Instance.SickEmployees;
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
            Debug.LogError($"GameManager: EmployeeDataSO {employeeData?.name} не найдена в availableEmployeeData или null.");
            return;
        }
        if (activeEmployees.Exists(e => e.Data == employeeData))
        {
            Debug.LogWarning($"GameManager: Сотрудница {employeeData.name} уже добавлена.");
            return;
        }
        GameObject employeeGO = new GameObject(employeeData.name);
        Employee employee = employeeGO.AddComponent<Employee>();
        employee.SetData(employeeData);
        activeEmployees.Add(employee);
        if (employee.GetState() == Employee.EmployeeState.Sick || employee.GetState() == Employee.EmployeeState.Healing)
        {
            sickEmployees.Add(employee);
            Debug.Log($"Сотрудница {employee.name} добавлена в sickEmployees.");
        }
        Debug.Log($"Сотрудница {employeeData.name} добавлена.");
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
        int expectedClients = TotalClients;
        Debug.Log($"День: {dayCount}, Золото: {currentGold}, Популярность: {currentPopularity}, Ожидаемые клиенты: {expectedClients}, Сложность: {difficultyLevel}");
        Debug.Log($"Активные клиенты: {clientPool.Count}, Активные сотрудницы: {activeEmployees.Count}, Больные сотрудницы: {EmployeeManager.Instance.SickEmployees.Count}");
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
                    AddEmployee(data);
                    Employee employee = activeEmployees.Find(e => e.Data.name == name);
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
        onStateChange.Invoke();
    }

    [ContextMenu("Heal Test Employee")]
    public void HealTestEmployee()
    {
        if (activeEmployees.Count == 0)
        {
            Debug.LogError("Нет активных сотрудниц для теста лечения.");
            return;
        }
        Employee sickEmployee = sickEmployees.Find(e => e.GetState() == Employee.EmployeeState.Sick);
        if (sickEmployee == null)
        {
            Debug.LogError("HealTestEmployee: Нет больных сотрудниц для лечения.");
            return;
        }
        HealEmployee(sickEmployee);
    }
}