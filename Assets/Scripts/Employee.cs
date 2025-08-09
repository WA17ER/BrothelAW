using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour
{
    public enum EmployeeState
    {
        Available,
        Sick,
        Servicing,
        Tired,
        Healing
    }

    private EmployeeDataSO data;
    private string race;
    private string bodyType;
    private char breastSize;
    private float staminaMax;
    private float staminaCurrent;
    private Dictionary<string, (int level, int progress)> skills = new Dictionary<string, (int, int)>();
    private EmployeeState state = EmployeeState.Available;
    public SicknessSO ActiveSick { get; private set; }
    public int DiseaseDuration { get; private set; } // Public property for access

    public EmployeeDataSO Data => data;
    public string Race => race;
    public string BodyType => bodyType;
    public char BreastSize => breastSize;
    public float StaminaMax => staminaMax;
    public float StaminaCurrent
    {
        get => staminaCurrent;
        set => staminaCurrent = value;
    }
    public Dictionary<string, (int level, int progress)> Skills => skills;

    public void SetData(EmployeeDataSO employeeData)
    {
        data = employeeData;
        race = employeeData.Race;
        bodyType = employeeData.BodyType;
        breastSize = employeeData.BreastSize;
        staminaMax = employeeData.StaminaMax;
        staminaCurrent = staminaMax;
        foreach (var skill in employeeData.BaseSkills)
        {
            skills[skill] = (0, 0);
        }
    }

    public void SetState(EmployeeState newState)
    {
        state = newState;
    }

    public EmployeeState GetState()
    {
        return state;
    }

    public void UpdateStamina(float cost)
    {
        staminaCurrent = Mathf.Max(0, staminaCurrent - cost);
        if (staminaCurrent == 0 && state != EmployeeState.Sick && state != EmployeeState.Healing)
        {
            SetState(EmployeeState.Tired);
        }
    }

    public void CheckSick(string service, bool clientIsSick)
    {
        if (clientIsSick && Random.value > data.sickResistance)
        {
            if (data.PossibleSicknesses != null && data.PossibleSicknesses.Count > 0)
            {
                ActiveSick = data.PossibleSicknesses[Random.Range(0, data.PossibleSicknesses.Count)];
                DiseaseDuration = 2; // Initial duration
                SetState(EmployeeState.Sick);
                Debug.Log($"Сотрудница {name} заболела с болезнью {ActiveSick.SickName}, длительность: {DiseaseDuration} дней.");
            }
            else
            {
                Debug.LogWarning($"Сотрудница {name} не может заболеть, так как PossibleSicknesses пуст.");
            }
        }
    }

    public void Heal()
    {
        DiseaseDuration = 2; // Set initial duration for healing
        SetState(EmployeeState.Healing);
        Debug.Log($"Сотрудница {name} начала лечение, время болезни: {DiseaseDuration} дней.");
        EmployeeManager.Instance.MoveEmployeeToList(this, state);
    }

    public void ProgressHealing()
    {
        if (state == EmployeeState.Healing && ActiveSick != null)
        {
            DiseaseDuration--;
            if (DiseaseDuration > 0)
            {
                Debug.Log($"Сотрудница {name} продолжает лечение, время болезни: {DiseaseDuration} дней.");
            }
            else
            {
                ActiveSick = null;
                SetState(EmployeeState.Available);
                Debug.Log($"Сотрудница {name} вылечена.");
            }
            EmployeeManager.Instance.MoveEmployeeToList(this, state);
        }
    }

    public void EndDayUpdate()
    {
        staminaCurrent = staminaMax;
        if (state == EmployeeState.Tired)
        {
            SetState(EmployeeState.Available);
        }
    }
}