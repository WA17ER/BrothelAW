using System.Collections.Generic;
using UnityEngine;

public class Employee : MonoBehaviour
{
    private EmployeeDataSO data;
    private Dictionary<string, (int level, int progress)> skills = new Dictionary<string, (int, int)>();
    private EmployeeState state = EmployeeState.Available;
    private float staminaCurrent;
    private float staminaMax = 10f;

    public enum EmployeeState
    {
        Available,
        Sick,
        Servicing,
        Tired,
        Healing
    }

    public string Race => data.Race;
    public string BodyType => data.BodyType;
    public char BreastSize => data.BreastSize;
    public Dictionary<string, (int level, int progress)> Skills => skills;
    public float StaminaCurrent { get => staminaCurrent; set => staminaCurrent = value; }
    public float StaminaMax => staminaMax;
    public EmployeeDataSO Data => data;

    public void SetData(EmployeeDataSO employeeData)
    {
        data = employeeData;
        name = data.employeeName;
        skills.Clear();
        foreach (var skill in data.BaseSkills)
        {
            skills[skill] = (0, 0);
        }
        staminaCurrent = staminaMax;
    }

    public EmployeeState GetState()
    {
        return state;
    }

    public void SetState(EmployeeState newState)
    {
        state = newState;
    }

    public void UpdateStamina(float staminaCost)
    {
        staminaCurrent = Mathf.Max(0, staminaCurrent - staminaCost);
        if (staminaCurrent <= 0 && state != EmployeeState.Sick)
        {
            SetState(EmployeeState.Tired);
            Debug.Log($"Сотрудница {name} устала, выносливость: {staminaCurrent}.");
        }
    }

    public void CheckSick(string service, bool clientIsSick)
    {
        if (clientIsSick && Random.value > data.sickResistance)
        {
            SetState(EmployeeState.Sick);
            Debug.Log($"Сотрудница {name} заболела после обслуживания клиента.");
        }
    }

    public void EndDayUpdate()
    {
        staminaCurrent = Mathf.Max(0, staminaCurrent - 0.5f);
        if (staminaCurrent <= 0 && state != EmployeeState.Sick)
        {
            SetState(EmployeeState.Tired);
        }
        if (state == EmployeeState.Healing)
        {
            if (Random.value < 0.5f)
            {
                SetState(EmployeeState.Available);
                Debug.Log($"Сотрудница {name} выздоровела.");
            }
        }
    }
}