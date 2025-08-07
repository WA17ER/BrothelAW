using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager Instance { get; private set; }

    private List<Employee> availableEmployees = new List<Employee>();
    private List<Employee> sickEmployees = new List<Employee>();
    private List<Employee> servicingEmployees = new List<Employee>();
    private List<Employee> tiredEmployees = new List<Employee>();
    private List<string> availableServices = new List<string>
    {
        "Дрочка", "Миньет", "Дрочка Сиськами", "Миссионерская", "Наездница", "Амазонка", "Раком", "Стоя"
    };
    private Dictionary<string, int> serviceBasePrices = new Dictionary<string, int>
    {
        {"Дрочка", 100},
        {"Миньет", 150},
        {"Дрочка Сиськами", 175},
        {"Миссионерская", 200},
        {"Наездница", 225},
        {"Амазонка", 250},
        {"Раком", 275},
        {"Стоя", 300}
    };
    private Dictionary<string, int> raceModifiers = new Dictionary<string, int>
    {
        {"Человек", 10},
        {"Эльф", 15},
        {"Тёмный Эльф", 20},
        {"Некомата", 25},
        {"Дриада", 30},
        {"Гарпия", 35},
        {"Они", 40}
    };
    private Dictionary<string, int> bodyTypeModifiers = new Dictionary<string, int>
    {
        {"Обычное", 5},
        {"Доска", 7},
        {"Милое", 10},
        {"Мускулистое", 12},
        {"Высокая", 15},
        {"Спортивное", 18},
        {"Желанное", 20},
        {"Перевёртыш", 22},
        {"Великан", 25}
    };
    private Dictionary<char, int> breastSizeModifiers = new Dictionary<char, int>
    {
        {'A', 5},
        {'B', 10},
        {'C', 15},
        {'D', 20},
        {'E', 25}
    };
    private Dictionary<EmployeeDataSO.SpecialRace, int> specialRaceModifiers = new Dictionary<EmployeeDataSO.SpecialRace, int>
    {
        {EmployeeDataSO.SpecialRace.Succubus, 200},
        {EmployeeDataSO.SpecialRace.Doppelganger, 120},
        {EmployeeDataSO.SpecialRace.Angel, 300}
    };

    public List<Employee> AvailableEmployees => availableEmployees;
    public List<Employee> SickEmployees => sickEmployees;
    public List<Employee> ServicingEmployees => servicingEmployees;
    public List<Employee> TiredEmployees => tiredEmployees;

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
        availableEmployees.Clear();
        sickEmployees.Clear();
        servicingEmployees.Clear();
        tiredEmployees.Clear();

        foreach (var employee in employees)
        {
            if (employee == null)
            {
                Debug.LogError("AddEmployees: Получена null-сотрудница.");
                continue;
            }
            MoveEmployeeToList(employee, employee.GetState());
            Debug.Log($"Сотрудница {employee.name} добавлена в сцену, состояние: {employee.GetState()}.");
        }
        Debug.Log($"Добавлено {availableEmployees.Count} сотрудниц в EmployeeManager (доступных и больных).");
    }

    public List<Employee> GetAllEmployees()
    {
        List<Employee> allEmployees = new List<Employee>();
        allEmployees.AddRange(availableEmployees);
        allEmployees.AddRange(servicingEmployees);
        allEmployees.AddRange(tiredEmployees);
        return allEmployees;
    }

    public void MoveEmployeeToList(Employee employee, Employee.EmployeeState newState)
    {
        availableEmployees.Remove(employee);
        sickEmployees.Remove(employee);
        servicingEmployees.Remove(employee);
        tiredEmployees.Remove(employee);

        switch (newState)
        {
            case Employee.EmployeeState.Available:
                availableEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в availableEmployees.");
                break;
            case Employee.EmployeeState.Sick:
                availableEmployees.Add(employee); // Больные сотрудницы остаются доступными
                sickEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в sickEmployees и availableEmployees.");
                break;
            case Employee.EmployeeState.Servicing:
                servicingEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в servicingEmployees.");
                break;
            case Employee.EmployeeState.Tired:
                tiredEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.name} перемещена в tiredEmployees.");
                break;
        }
        GameManager.Instance.onEmployeeListChanged.Invoke();
    }

    public float CalculateServiceCost(ClientData client, Employee employee)
    {
        if (employee == null)
        {
            Debug.LogError($"CalculateServiceCost: Employee null для клиента {client.name}.");
            return 0f;
        }
        string service = client.RequestedService;
        if (!employee.Data.BaseSkills.Contains(service))
        {
            Debug.LogError($"CalculateServiceCost: Услуга {service} не найдена в навыках сотрудницы {employee.name}.");
            return 0f;
        }
        if (!serviceBasePrices.ContainsKey(service))
        {
            Debug.LogError($"CalculateServiceCost: Услуга {service} не найдена в serviceBasePrices.");
            return 0f;
        }

        float basePrice = serviceBasePrices[service];
        float reward = basePrice;
        var specialRace = employee.Data.GetSpecialRace();

        if (specialRace != EmployeeDataSO.SpecialRace.None)
        {
            if (!specialRaceModifiers.ContainsKey(specialRace))
            {
                Debug.LogError($"CalculateServiceCost: specialRaceModifier не найден для {specialRace}.");
                return 0f;
            }
            reward += specialRaceModifiers[specialRace];
            reward += basePrice * 0.1f * employee.Skills[service].level;
        }
        else
        {
            int raceModifier = string.IsNullOrEmpty(employee.Race) ? 0 : (raceModifiers.ContainsKey(employee.Race) ? raceModifiers[employee.Race] : 0);
            int bodyModifier = string.IsNullOrEmpty(employee.BodyType) ? 0 : (bodyTypeModifiers.ContainsKey(employee.BodyType) ? bodyTypeModifiers[employee.BodyType] : 0);
            int breastModifier = employee.BreastSize == '\0' ? 0 : (breastSizeModifiers.ContainsKey(employee.BreastSize) ? breastSizeModifiers[employee.BreastSize] : 0);

            reward += raceModifier;
            reward += bodyModifier;
            reward += breastModifier;
            reward += basePrice * 0.1f * employee.Skills[service].level;

            if (raceModifier == 0 && !string.IsNullOrEmpty(employee.Race))
                Debug.LogWarning($"CalculateServiceCost: raceModifier не найден для {employee.Race}, использован 0.");
            if (bodyModifier == 0 && !string.IsNullOrEmpty(employee.BodyType))
                Debug.LogWarning($"CalculateServiceCost: bodyTypeModifier не найден для {employee.BodyType}, использован 0.");
            if (breastModifier == 0 && employee.BreastSize != '\0')
                Debug.LogWarning($"CalculateServiceCost: breastSizeModifier не найден для {employee.BreastSize}, использован 0.");
        }

        // Добавление надбавок за предпочтения клиента
        if (!string.IsNullOrEmpty(client.bodyType))
            reward += 35;
        if (client.breastSize != '\0')
            reward += 20;
        if (!string.IsNullOrEmpty(client.race))
            reward += 50;
        if (client.SpecificEmployee != null)
            reward += 100;

        return reward;
    }

    public float AssignEmployee(ClientData client, Employee employee)
    {
        float reward = CalculateServiceCost(client, employee);
        if (reward == 0f)
        {
            Debug.LogError($"AssignEmployee: Не удалось рассчитать стоимость для клиента {client.name} и сотрудницы {employee.name}.");
            return 0f;
        }

        employee.SetState(Employee.EmployeeState.Servicing);
        MoveEmployeeToList(employee, Employee.EmployeeState.Servicing);
        Debug.Log($"Сотрудница {employee.name} назначена для клиента {client.name}, награда: {reward}.");
        GameManager.Instance.onEmployeeListChanged.Invoke();
        return reward;
    }

    public void CompleteService(Employee employee, string service, bool clientIsSick)
    {
        if (employee == null)
        {
            Debug.LogError("CompleteService: Employee null.");
            return;
        }
        if (!employee.Skills.ContainsKey(service))
        {
            Debug.LogError($"CompleteService: Услуга {service} не найдена в навыках сотрудницы {employee.name}.");
            return;
        }
        employee.CheckSick(service, clientIsSick);
        var currentSkill = employee.Skills[service];
        int newProgress = currentSkill.progress + (int)GameManager.Instance.ProgressPerService;
        int newLevel = currentSkill.level;
        if (newLevel < 10)
        {
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
        employee.UpdateStamina(1f);
        if (employee.GetState() == Employee.EmployeeState.Servicing)
        {
            employee.SetState(Employee.EmployeeState.Available);
            MoveEmployeeToList(employee, Employee.EmployeeState.Available);
        }
    }

    public void EndDayUpdate()
    {
        foreach (var employee in GetAllEmployees())
        {
            Debug.Log($"Обновление дня для сотрудницы {employee.name}, текущее состояние: {employee.GetState()}");
            employee.EndDayUpdate();
            MoveEmployeeToList(employee, employee.GetState());
        }
    }

    public string GetRandomService()
    {
        return availableServices[Random.Range(0, availableServices.Count)];
    }

    public Employee GetRandomEmployee()
    {
        if (availableEmployees.Count == 0) return null;
        return availableEmployees[Random.Range(0, availableEmployees.Count)];
    }
}