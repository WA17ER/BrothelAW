using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class ServicePrice
{
    public string serviceName;
    public float price;
}
[System.Serializable]
public class ServiceStaminaCost
{
    public string serviceName;
    public float cost;
}
public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }
    [SerializeField] private EmployeeBonusesSO bonuses;
    [SerializeField] private List<ServicePrice> servicePrices = new List<ServicePrice>();
    [SerializeField] private List<ServiceStaminaCost> serviceStaminaCosts = new List<ServiceStaminaCost>();
    [SerializeField] private List<Employee> availableEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private List<Employee> healingEmployees = new List<Employee>();
    private List<Employee> onServiceEmployees = new List<Employee>();
    private List<Employee> _allEmployeesCache;
    [SerializeField] private List<Employee> marketingEmployees = new List<Employee>();
    public List<Employee> AvailableEmployees => availableEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> HealingEmployees => healingEmployees;
    public List<Employee> OnServiceEmployees => onServiceEmployees;

    public List<Employee> MarketingEmployees => marketingEmployees;
    public List<ServicePrice> ServicePrices => servicePrices;
    private Dictionary<string, float> servicePriceDict;
    private Dictionary<string, float> serviceStaminaDict;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        InitializeServicePrices();
        InitializeServiceStaminaCosts();
        Debug.Log($"EmployeeManager Awake: Количество сотрудников после загрузки - {availableEmployees.Count + sickEmployees.Count + healingEmployees.Count + onServiceEmployees.Count + marketingEmployees.Count}");
    }
    private void Start()
    {
        SyncWithGameState();
        Debug.Log("EmployeeManager Start: Синхронизация с GameStateManager выполнена.");
    }
    private void InitializeServicePrices()
    {
        servicePriceDict = new Dictionary<string, float>();
        foreach (var service in servicePrices)
        {
            if (!string.IsNullOrEmpty(service.serviceName))
            {
                servicePriceDict[service.serviceName] = service.price;
            }
        }
    }
    private void InitializeServiceStaminaCosts()
    {
        serviceStaminaDict = new Dictionary<string, float>();
        foreach (var service in serviceStaminaCosts)
        {
            if (!string.IsNullOrEmpty(service.serviceName))
            {
                serviceStaminaDict[service.serviceName] = service.cost;
            }
        }
    }
    public void SyncWithGameState()
    {
        if (GameStateManager.Instance != null)
        {
            List<Employee> stateEmployees = GameStateManager.Instance.Employees;
            Debug.Log($"SyncWithGameState: Получено сотрудников из GameStateManager: {stateEmployees.Count}");
            SyncEmployees(stateEmployees);
            Debug.Log("Синхронизация EmployeeManager с GameStateManager завершена.");
        }
        else
        {
            Debug.LogWarning("GameStateManager.Instance не найден при синхронизации.");
        }
    }
    public void SyncEmployees(List<Employee> employees)
    {
        availableEmployees.Clear();
        sickEmployees.Clear();
        healingEmployees.Clear();
        onServiceEmployees.Clear();
        marketingEmployees.Clear();
        foreach (var employee in employees)
        {
            MoveEmployeeToList(employee);
        }
        if (availableEmployees.Count > 0)
        {
            Debug.Log($"Список сотрудниц в EmployeeManager: {string.Join(", ", availableEmployees.Select(e => e.Data.employeeName))}");
        }
        else
        {
            Debug.Log("Список сотрудниц в EmployeeManager пуст.");
        }
        _allEmployeesCache = null;
    }
    public void AddEmployees(List<Employee> employees)
    {
        foreach (var employee in employees)
        {
            MoveEmployeeToList(employee);
        }
    }
    public void MoveEmployeeToList(Employee employee)
    {
        availableEmployees.Remove(employee);
        sickEmployees.Remove(employee);
        healingEmployees.Remove(employee);
        onServiceEmployees.Remove(employee);
        marketingEmployees.Remove(employee);
        var state = employee.GetState();
        switch (state)
        {
            case Employee.EmployeeState.Available:
                availableEmployees.Add(employee);
                break;
            case Employee.EmployeeState.Sick:
                sickEmployees.Add(employee);
                break;
            case Employee.EmployeeState.Healing:
                healingEmployees.Add(employee);
                break;
            case Employee.EmployeeState.OnService:
                onServiceEmployees.Add(employee);
                break;
            case Employee.EmployeeState.HeavySick:
                sickEmployees.Add(employee);
                break;
            case Employee.EmployeeState.Marketing:
                marketingEmployees.Add(employee);
                availableEmployees.Remove(employee);
                break;
            default:
                break;
        }
        _allEmployeesCache = null;
    }
    public void MoveEmployeeToList(Employee employee, Employee.EmployeeState state)
    {
        employee.SetState(state);
        MoveEmployeeToList(employee);
    }
    public string GetRandomService()
    {
        if (servicePriceDict == null || servicePriceDict.Count == 0)
        {
            Debug.LogWarning("Список servicePrices пуст или не инициализирован.");
            return null;
        }
        int index = Random.Range(0, servicePriceDict.Count);
        return servicePriceDict.Keys.ElementAt(index);
    }
    public float AssignEmployee(ClientData client, Employee employee)
    {
        Debug.Log($"AssignEmployee: client = {client?.clientName ?? "null"}, employee = {employee?.Data.employeeName ?? "null"}");
        Debug.Log($"Проверка доступности {employee.Data.employeeName}: {employee.GetState()}");
        if (employee.StaminaCurrent <= 0)
        {
            Debug.LogWarning($"Сотрудница {employee.Data.employeeName} без стамины, не доступна для назначения.");
            return 0f;
        }
        if (!availableEmployees.Contains(employee))
        {
            Debug.LogWarning($"Сотрудница {employee?.Data.employeeName ?? "null"} не доступна для назначения.");
            return 0f;
        }
        float staminaCost = serviceStaminaDict.ContainsKey(client.RequestedService) ? serviceStaminaDict[client.RequestedService] : 1f;
        
        float reward = CalculateReward(client, employee);
        employee.SetState(Employee.EmployeeState.OnService);
        Debug.Log("State set to OnService for " + employee.Data.employeeName);
        MoveEmployeeToList(employee);
        Debug.Log("After assignment, state for " + employee.Data.employeeName + ": " + employee.GetState());
        return reward;
    }
    public float CalculateReward(ClientData client, Employee employee)
    {
        Debug.Log($"CalculateReward: client = {client?.clientName ?? "null"}, employee = {employee?.Data.employeeName ?? "null"}, RequestedService = {client?.RequestedService ?? "null"}");
        Debug.Log($"CalculateReward: client.ClientDataSO = {client?.ClientDataSO != null}, employee.Skills = {employee?.Skills != null}, bonuses = {bonuses != null}");
        float skillLevel = employee.Skills.ContainsKey(client.RequestedService) ? employee.Skills[client.RequestedService].level : 0;
        bool isSpecialRace = employee.Race == EmployeeDataSO.Race.Допельгангер || employee.Race == EmployeeDataSO.Race.Суккуб || employee.Race == EmployeeDataSO.Race.Ангел;
        float baseReward = servicePriceDict != null && servicePriceDict.ContainsKey(client.RequestedService) ? servicePriceDict[client.RequestedService] : 0f;
        float reward = baseReward;
        if (!isSpecialRace)
        {
            foreach (var bonus in bonuses.breastSizeBonuses)
            {
                if (bonus.breastSize == employee.BreastSize)
                {
                    reward += bonus.bonus;
                    break;
                }
            }
            foreach (var bonus in bonuses.bodyTypeBonuses)
            {
                if (bonus.bodyType == employee.BodyType)
                {
                    reward += bonus.bonus;
                    break;
                }
            }
        }
        foreach (var bonus in bonuses.raceBonuses)
        {
            if (bonus.race == employee.Race)
            {
                reward += bonus.bonus;
                break;
            }
        }
        reward += skillLevel * 0.1f;
        Debug.Log($"Рассчитана награда для клиента {client?.clientName ?? "null"} с сотрудницей {employee?.Data.employeeName ?? "null"}: {reward}");
        return reward;
    }
    public void CompleteService(Employee employee, string service, bool hasSickness)
    {
        if (employee == null)
        {
            Debug.LogWarning("Сотрудница null при завершении услуги.");
            return;
        }
        Debug.Log($"Поиск клиента для {employee.Data.employeeName}, clientPool: {string.Join(", ", GameManager.Instance.ClientPool.Select(c => c?.clientName ?? "null"))}");
        ClientData client = GameManager.Instance.ClientPool.Find(c => c.SpecificEmployee == employee);
        if (client == null)
        {
            Debug.LogWarning($"Клиент не найден для сотрудницы {employee.Data.employeeName} при завершении услуги.");
            return;
        }
        Debug.Log($"Проверка болезни клиента {client.clientName}: Болезнь = {client.ActiveSick?.SickName ?? "none"}");
        if (client.ActiveSick != null)
        {
            float effectiveChance = client.ClientDataSO.sickChance - employee.Data.sickResistance;
            float randomChance = Random.Range(0f, 100f);
            Debug.Log($"Проверка заражения для сотрудницы {employee.Data.employeeName}: sickChance = {client.ClientDataSO.sickChance}, sickResistance = {employee.Data.sickResistance}, effectiveChance = {effectiveChance}, randomChance = {randomChance}");
            if (effectiveChance > randomChance)
            {
                employee.ActiveSick = client.ActiveSick;
                employee.SetState(client.ActiveSick.isUnavailable ? Employee.EmployeeState.HeavySick : Employee.EmployeeState.Sick);
                Debug.Log($"Сотрудница {employee.Data.employeeName} заразилась болезнью {client.ActiveSick.SickName} ({(client.ActiveSick.isUnavailable ? "HeavySick" : "Sick")}) от клиента {client.clientName}.");
            }
            else
            {
                employee.SetState(Employee.EmployeeState.Available);
            }
        }
        else
        {
            employee.SetState(Employee.EmployeeState.Available);
        }
        // Deduct stamina cost
        float staminaCost = serviceStaminaDict.ContainsKey(service) ? serviceStaminaDict[service] : 1f;
        Debug.Log($"Stamina cost for {service}: {staminaCost}");
        Debug.Log($"Вычет стамины для {employee.Data.employeeName}: {staminaCost}, текущая: {employee.StaminaCurrent}");
        employee.StaminaCurrent -= staminaCost;
        Debug.Log($"После вычета стамины для {employee.Data.employeeName}: {employee.StaminaCurrent}");
        if (employee.StaminaCurrent <= 0)
        {
            employee.StaminaCurrent = 0;
            Debug.Log($"Сотрудница {employee.Data.employeeName} без стамины, StaminaCurrent = 0");
        }
        if (employee.Skills.ContainsKey(service))
        {
            var skill = employee.Skills[service];
            skill.progress += employee.Data.progressionSpeed;
            Debug.Log($"Сотрудница {employee.Data.employeeName} повысила прогрессию навыка {service} до {skill.progress}.");
            if (skill.progress >= 100)
            {
                skill.level += 1;
                if (skill.level > 10)
                {
                    skill.level = 10;
                    skill.progress = 0;
                }
                else
                {
                    skill.progress -= 100;
                }
                Debug.Log($"Сотрудница {employee.Data.employeeName} повысила уровень навыка {service} до {skill.level}.");
            }
            employee.Skills[service] = skill;
        }
        else
        {
            Debug.LogWarning($"Сотрудница {employee.Data.employeeName} не имеет навыка {service}.");
        }
        MoveEmployeeToList(employee);
        Debug.Log($"Сотрудница {employee.Data.employeeName} состояние после услуги: {employee.GetState()}");
    }
    public void EndDayUpdate()
    {
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
                    MoveEmployeeToList(employee);
                    Debug.Log($"Сотрудница {employee.Data.employeeName} выздоровела.");
                }
            }
        }
        // Reset stamina for all employees
        foreach (var employee in GetAllEmployees())
        {
            employee.StaminaCurrent = employee.StaminaMax;
            Debug.Log($"Сброс стамины для {employee.Data.employeeName}: {employee.StaminaCurrent}");
        }
        Debug.Log($"Конец дня: обработано {healingEmployees.Count} сотрудниц на лечении, {marketingEmployees.Count} на рекламе.");
    }
    public List<Employee> GetAllEmployees()
    {
        return _allEmployeesCache ??= availableEmployees.Concat(sickEmployees)
            .Concat(healingEmployees)
            .Concat(onServiceEmployees)
            .Concat(marketingEmployees)
            .ToList();
    }
    public EmployeeBonusesSO GetBonuses()
    {
        return bonuses;
    }
}