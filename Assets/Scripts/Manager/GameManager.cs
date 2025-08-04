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
    private float currentGold;
    private float currentPopularity;
    private int dayCount = 1;
    private int clientsSpawnedToday = 0;
    private int difficultyLevel = 1;
    private Dictionary<int, List<GameObject>> clientVisualModels;

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

    private void OnEmployeeAssignedHandler(ClientData client, Employee employee)
    {
        int clientLevel = (int)client.Data.clientType;
        string service = client.RequestedService;
        if (!employee.Data.servicePrices.ContainsKey(service))
        {
            Debug.LogError($"OnEmployeeAssigned: Услуга {service} не найдена в servicePrices сотрудницы {employee.name}.");
            return;
        }
        int skillLevel = employee.Skills[service].level;
        float reward = clientLevel * (skillLevel + 1) * employee.Data.servicePrices[service];
        currentGold += reward;
        employee.SetState(Employee.EmployeeState.Servicing);
        Debug.Log($"Золото начислено: +{reward} для клиента {client.name} с сотрудницей {employee.name}.");
        onStateChange.Invoke();
    }

    private void OnServiceCompletedHandler(ClientData client, Employee employee)
    {
        currentPopularity += 5f;
        Debug.Log($"Популярность начислена: +5 для клиента {client.name} после обслуживания.");
        if (employee != null)
        {
            employee.CheckSick(client.RequestedService, client.Data.isSick);

            string service = client.RequestedService;
            if (!employee.Skills.ContainsKey(service))
            {
                Debug.LogError($"OnServiceCompleted: Услуга {service} не найдена в навыках сотрудницы {employee.name}.");
                return;
            }
            var currentSkill = employee.Skills[service];
            int newProgress = currentSkill.progress;
            int newLevel = currentSkill.level;

            if (newLevel < 10)
            {
                newProgress += (int)progressPerService;
                if (newProgress >= 100)
                {
                    newLevel++;
                    newProgress = 0;
                }
            }
            else
            {
                newProgress = 100;
            }

            employee.Skills[service] = (newLevel, newProgress);
            Debug.Log($"Сотрудница {employee.name} навык {service} уровень {newLevel} прогрессия {newProgress}");
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
        StartCoroutine(ServiceTimer(5f, customer, client));
    }

    private IEnumerator ServiceTimer(float time, CustomerMovement customer, ClientData client)
    {
        yield return new WaitForSeconds(time);
        customer.Visual.gameObject.SetActive(true);
        Debug.Log($"Клиент {customer.name} закончил услугу.");
        onServiceCompleted.Invoke(client, client.SelectedEmployee);
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
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.AddEmployeeData(employeeData);
        }
        Debug.Log($"Сотрудница {employeeData.name} добавлена.");
        onEmployeeListChanged.Invoke();
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

    private void LogGameState()
    {
        int expectedClients = TotalClients;
        Debug.Log($"День: {dayCount}, Золото: {currentGold}, Популярность: {currentPopularity}, Ожидаемые клиенты: {expectedClients}, Сложность: {difficultyLevel}");
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
        Employee sickEmployee = activeEmployees.Find(e => e.GetState() == Employee.EmployeeState.Sick);
        if (sickEmployee == null)
        {
            Debug.LogError("HealTestEmployee: Нет больных сотрудниц для лечения.");
            return;
        }
        HealEmployee(sickEmployee);
    }
}