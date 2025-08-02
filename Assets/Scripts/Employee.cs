using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour
{
    [SerializeField] private EmployeeDataSO data; // Ссылка на ScriptableObject
    private Dictionary<string, EmployeeSkill> skills = new Dictionary<string, EmployeeSkill>();
    [SerializeField] private float staminaMax = 100f;
    private float staminaCurrent;
    private EmployeeState employeeState = EmployeeState.Available;

    public class EmployeeSkill
    {
        public int level; // 0-10
        public int progress; // 0-100
    }

    public enum EmployeeState
    {
        Available, // Доступна для услуг
        Advertising, // Занимается рекламой
        Servicing, // Оказывает услугу
        Tired, // Устала (стамина 0)
        Sick, // Больна
        Healing // На лечении
    }

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogWarning($"Employee: Data не назначена для {name}. Ожидается установка через SetData.");
            return;
        }

        InitializeEmployee();
    }

    private void InitializeEmployee()
    {
        staminaCurrent = staminaMax;
        skills.Clear();
        foreach (var skillName in data.BaseSkills)
        {
            skills[skillName] = new EmployeeSkill { level = 0, progress = 0 };
        }
        Debug.Log($"Сотрудница {name} инициализирована с данными {data.name}.");
    }

    public void SetData(EmployeeDataSO newData)
    {
        if (newData != null)
        {
            data = newData;
            InitializeEmployee();
            Debug.Log($"Данные сотрудницы {name} обновлены: {data.name}");
        }
        else
        {
            Debug.LogError("Employee: Нельзя установить null Data.");
        }
    }

    public void UpdateStamina(float cost)
    {
        if (employeeState == EmployeeState.Servicing)
        {
            float previousStamina = staminaCurrent;
            staminaCurrent = Mathf.Max(0, staminaCurrent - cost);
            Debug.Log($"Стамина сотрудницы {name} уменьшена на {cost} с {previousStamina} до {staminaCurrent}.");
            if (staminaCurrent <= 0)
            {
                SetState(EmployeeState.Tired);
                Debug.Log($"Сотрудница {name} устала, стамина 0.");
            }
        }
    }

    public void CheckSick(string service)
    {
        if (employeeState == EmployeeState.Servicing)
        {
            if (!skills.ContainsKey(service))
            {
                Debug.LogError($"Услуга {service} не найдена в навыках сотрудницы {name}.");
                return;
            }
            int skillLevel = skills[service].level;
            float sickChance = 0.05f - 0.01f * skillLevel + data.SickChanceModifier;
            Debug.Log($"Шанс болезни для {name} при услуге {service}: {sickChance * 100:F2}%");
            if (Random.value <= sickChance)
            {
                SetState(EmployeeState.Sick);
                Debug.Log($"Сотрудница {name} заболела при оказании услуги {service}.");
            }
        }
    }

    public void ResetStamina()
    {
        staminaCurrent = staminaMax;
        Debug.Log($"Стамина сотрудницы {name} восстановлена до {staminaMax}.");
    }

    public void SetState(EmployeeState newState)
    {
        employeeState = newState;
        Debug.Log($"Состояние сотрудницы {name} изменено на {newState}.");
    }

    public EmployeeState GetState()
    {
        return employeeState;
    }

    public void EndDayUpdate()
    {
        if (employeeState == EmployeeState.Tired || employeeState == EmployeeState.Servicing)
        {
            SetState(EmployeeState.Available);
            ResetStamina();
        }
        else if (employeeState == EmployeeState.Healing)
        {
            SetState(EmployeeState.Available);
            ResetStamina();
            Debug.Log($"Сотрудница {name} вылечилась.");
        }
        else if (employeeState == EmployeeState.Sick)
        {
            if (Random.Range(1, 4) == 1)
            {
                SetState(EmployeeState.Available);
                ResetStamina();
                Debug.Log($"Сотрудница {name} выздоровела автоматически.");
            }
        }
    }

    [ContextMenu("Set Servicing")]
    public void SetServicing()
    {
        SetState(EmployeeState.Servicing);
    }

    [ContextMenu("Test Stamina")]
    public void TestStamina()
    {
        UpdateStamina(100f);
    }

    [ContextMenu("Test Sick")]
    public void TestSick()
    {
        CheckSick("Missionary");
    }

    // Геттеры для параметров из data
    public EmployeeDataSO Data => data;
    public string Race => data.Race;
    public List<string> BodyTypes => data.BodyTypes;
    public char BreastSize => data.BreastSize;
    public List<string> BaseSkills => data.BaseSkills;
    public float SickChanceModifier => data.SickChanceModifier;

    // Геттеры для динамических параметров
    public Dictionary<string, EmployeeSkill> Skills => skills;
    public float StaminaMax => staminaMax;
    public float StaminaCurrent { get => staminaCurrent; set => staminaCurrent = value; }
}