using System.Collections.Generic;
using UnityEngine;
public class Employee : MonoBehaviour
{
    public enum EmployeeState
    {
        Available,
        Sick,
        Healing,
        OnService,
        HeavySick,
        Marketing
    }

    [SerializeField]
    private EmployeeState currentState;
    public EmployeeState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }
    private EmployeeDataSO data;
    private Dictionary<string, (int level, float progress)> skills = new Dictionary<string, (int level, float progress)>();
    private float staminaCurrent;
    private SicknessSO activeSick;
    private float healingTimeRemaining; // Время, оставшееся до выздоровления
    [SerializeField] private bool isInitialized = false;
    public EmployeeDataSO Data => data;
    public Dictionary<string, (int level, float progress)> Skills => skills;
    public float StaminaCurrent { get => staminaCurrent; set => staminaCurrent = value; }
    public float StaminaMax => data.StaminaMax;
    public SicknessSO ActiveSick { get => activeSick; set => activeSick = value; }
    public float HealingTimeRemaining { get => healingTimeRemaining; set => healingTimeRemaining = value; }
    public EmployeeDataSO.BreastSize BreastSize => data.breastSize;
    public EmployeeDataSO.BodyType BodyType => data.bodyType;
    public EmployeeDataSO.Race Race => data.race;
    public void SetData(EmployeeDataSO employeeData)
    {
        data = employeeData;
        if (!isInitialized)
        {
            staminaCurrent = data.StaminaMax;
            currentState = EmployeeState.Available; // Инициализация состояния
            foreach (var skill in data.BaseSkills)
            {
                skills[skill] = (0, 0);
            }
            activeSick = null;
            healingTimeRemaining = 0f;
            isInitialized = true;
        }
    }
    public EmployeeState GetState()
    {
        if (activeSick != null && healingTimeRemaining > 0)
            return EmployeeState.Healing;
        if (activeSick != null && activeSick.isUnavailable)
            return EmployeeState.HeavySick;
        if (activeSick != null)
            return EmployeeState.Sick;        
        return currentState;
    }
    public void SetState(EmployeeState state)
    {
        if (data == null)
        {
            Debug.LogError("Сотрудница не инициализирована: data равен null.");
            return;
        }
        currentState = state;
        Debug.Log($"Сотрудница {data.employeeName} меняет состояние на {state}.");
    }
    public void Heal()
    {
        if (activeSick == null)
        {
            Debug.LogWarning($"Сотрудница {data.employeeName} не больна, лечение не требуется.");
            return;
        }
        healingTimeRemaining = activeSick.duration;
        SetState(EmployeeState.Healing);
        Debug.Log($"Лечение {data.employeeName} начато, длительность: {healingTimeRemaining}");
    }
    public void ProgressHealing()
    {
        if (activeSick == null || healingTimeRemaining <= 0)
        {
            activeSick = null;
            healingTimeRemaining = 0f;
            SetState(EmployeeState.Available);
        }
        else
        {
            healingTimeRemaining -= 1f;
            if (healingTimeRemaining <= 0)
            {
                activeSick = null;
                SetState(EmployeeState.Available);
            }
            Debug.Log($"Прогресс лечения {data.employeeName}, осталось: {healingTimeRemaining}");
        }
    }

    public string GetSkillLevelString(string skill)
    {
        if (!Skills.ContainsKey(skill)) return "0";
        var (level, _) = Skills[skill];
        return level >= 10 ? "max" : level.ToString();
    }

    public bool CanLevelUpSkill(string skill)
    {
        if (!Skills.ContainsKey(skill)) return true;
        var (level, _) = Skills[skill];
        return level < 10;
    }

    public void LevelUpSkill(string skill)
    {
        if (!CanLevelUpSkill(skill)) return;
        var (level, progress) = Skills.ContainsKey(skill) ? Skills[skill] : (0, 0);
        level += 1;
        Skills[skill] = (level, 0);
    }
}