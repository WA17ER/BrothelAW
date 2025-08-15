using System.Collections.Generic;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    [SerializeField] private EmployeeBonusesSO bonuses;
    [SerializeField] private List<string> availableServices;
    private List<Employee> availableEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private List<Employee> healingEmployees = new List<Employee>();
    private List<Employee> onServiceEmployees = new List<Employee>();

    public List<Employee> AvailableEmployees => availableEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> HealingEmployees => healingEmployees;
    public List<Employee> OnServiceEmployees => onServiceEmployees;

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
        }
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

        switch (employee.GetState())
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
        }
        Debug.Log($"Сотрудница {employee.Data.employeeName} перемещена в {employee.GetState()}.");
    }

    public string GetRandomService()
    {
        if (availableServices.Count == 0)
        {
            Debug.LogWarning("Список availableServices пуст.");
            return null;
        }
        return availableServices[Random.Range(0, availableServices.Count)];
    }

    public float AssignEmployee(ClientData client, Employee employee)
    {
        if (!availableEmployees.Contains(employee))
        {
            Debug.LogWarning($"Сотрудница {employee.Data.employeeName} не доступна для назначения.");
            return 0f;
        }

        float reward = CalculateReward(client, employee);
        employee.StaminaCurrent -= 1f;
        employee.SetState(Employee.EmployeeState.OnService);
        return reward;
    }

    public void CompleteService(Employee employee, string service, bool hasSickness)
    {
        if (employee == null)
        {
            Debug.LogWarning("Сотрудница null при завершении услуги.");
            return;
        }

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

        if (employee.Skills.ContainsKey(service))
        {
            var skill = employee.Skills[service];
            skill.progress += 1;
            if (skill.progress >= 5)
            {
                skill.level += 1;
                skill.progress = 0;
                Debug.Log($"Сотрудница {employee.Data.employeeName} повысила уровень навыка {service} до {skill.level}.");
            }
            employee.Skills[service] = skill;
        }
        else
        {
            Debug.LogWarning($"Сотрудница {employee.Data.employeeName} не имеет навыка {service}.");
        }
        MoveEmployeeToList(employee);
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
                    GameManager.Instance.HealingEmployees.Remove(employee);
                    Debug.Log($"Сотрудница {employee.Data.employeeName} выздоровела.");
                }
            }
        }
        Debug.Log($"Конец дня: обработано {healingEmployees.Count} сотрудниц на лечении.");
    }

    private float CalculateReward(ClientData client, Employee employee)
    {
        float clientTypeMultiplier = client.clientType;
        float skillLevel = employee.Skills.ContainsKey(client.RequestedService) ? employee.Skills[client.RequestedService].level : 0;
        bool isSpecialRace = employee.Race == EmployeeDataSO.Race.Допельгангер || employee.Race == EmployeeDataSO.Race.Суккуб || employee.Race == EmployeeDataSO.Race.Ангел;

        float reward = 100f * clientTypeMultiplier * (1f + skillLevel * 0.1f);

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

        Debug.Log($"Рассчитана награда для клиента {client.clientName} с сотрудницей {employee.Data.employeeName}: {reward}");
        return reward;
    }

    public List<Employee> GetAllEmployees()
    {
        List<Employee> allEmployees = new List<Employee>();
        allEmployees.AddRange(availableEmployees);
        allEmployees.AddRange(sickEmployees);
        allEmployees.AddRange(healingEmployees);
        allEmployees.AddRange(onServiceEmployees);
        return allEmployees;
    }
}