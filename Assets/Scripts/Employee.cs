using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour
{
    public enum EmployeeState
    {
        Available,
        Sick,
        Healing
    }

    public string name;
    public EmployeeDataSO Data { get; private set; }
    public Dictionary<string, (int level, int progress)> Skills { get; private set; }
    public float StaminaCurrent { get; set; }
    public float StaminaMax { get; private set; }
    public SicknessSO ActiveSick { get; set; }
    private EmployeeState state;

    public void SetData(EmployeeDataSO data)
    {
        Data = data;
        name = data.employeeName;
        Skills = new Dictionary<string, (int level, int progress)>();
        foreach (var skill in data.BaseSkills)
        {
            Skills[skill] = (0, 0); // Initialize skills with level 0, progress 0
        }
        StaminaMax = 100f; // Placeholder value
        StaminaCurrent = StaminaMax;
        state = EmployeeState.Available;
        Debug.Log($"Сотрудница {name} инициализирована с навыками: {string.Join(", ", Skills.Keys)}");
    }

    public void SetState(EmployeeState newState)
    {
        state = newState;
        Debug.Log($"Состояние сотрудницы {name} изменено на {state}.");
    }

    public EmployeeState GetState()
    {
        return state;
    }

    public void ProgressHealing()
    {
        if (state == EmployeeState.Healing && ActiveSick != null)
        {
            // Placeholder: Assume healing completes instantly
            ActiveSick = null;
            SetState(EmployeeState.Available);
            Debug.Log($"Сотрудница {name} вылечена.");
        }
    }

    public void Heal()
    {
        if (state == EmployeeState.Sick || state == EmployeeState.Healing)
        {
            ActiveSick = null;
            SetState(EmployeeState.Available);
            EmployeeManager.Instance.MoveEmployeeToList(this);
            Debug.Log($"Сотрудница {name} полностью вылечена.");
        }
    }
}