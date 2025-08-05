using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Employee : MonoBehaviour
{
    public EmployeeDataSO Data { get; private set; }
    public string Race => Data.Race;
    public List<string> BodyTypes => Data.BodyTypes;
    public char BreastSize => Data.BreastSize;
    public List<string> BaseSkills => Data.BaseSkills;
    public Dictionary<string, (int level, int progress)> Skills { get; private set; }
    public float StaminaCurrent { get; set; }
    public float StaminaMax => 10f;
    private EmployeeState state = EmployeeState.Available;

    public enum EmployeeState
    {
        Available,
        Servicing,
        Tired,
        Sick,
        Healing
    }

    public void SetData(EmployeeDataSO data)
    {
        Data = data;
        Skills = new Dictionary<string, (int level, int progress)>();
        foreach (var skill in data.BaseSkills)
        {
            Skills[skill] = (0, 0);
        }
        StaminaCurrent = StaminaMax;
    }

    public EmployeeState GetState()
    {
        return state;
    }

    public void SetState(EmployeeState newState)
    {
        state = newState;
        if (EmployeeManager.Instance != null)
        {
            EmployeeManager.Instance.MoveEmployeeToList(this, newState);
        }
        if (GameManager.Instance != null)
        {
            if (newState == EmployeeState.Sick || newState == EmployeeState.Healing)
            {
                if (!GameManager.Instance.SickEmployees.Contains(this))
                {
                    GameManager.Instance.SickEmployees.Add(this);
                    Debug.Log($"Сотрудница {name} добавлена в GameManager.sickEmployees.");
                }
            }
            else
            {
                GameManager.Instance.SickEmployees.Remove(this);
                Debug.Log($"Сотрудница {name} удалена из GameManager.sickEmployees.");
            }
        }
        GameManager.Instance.onEmployeeListChanged.Invoke();
        Debug.Log($"Состояние сотрудницы {name} изменено на {state}.");
    }

    public void UpdateStamina(float delta)
    {
        StaminaCurrent = Mathf.Clamp(StaminaCurrent - delta, 0, StaminaMax);
        if (StaminaCurrent <= 0)
        {
            SetState(EmployeeState.Tired);
        }
    }

    public void CheckSick(string service, bool clientIsSick)
    {
        if (!clientIsSick)
        {
            Debug.Log($"Сотрудница {name} не заболела: клиент здоров.");
            return;
        }
        float randomValue = Random.value * 100; // Рандомное число 0-100
        if (randomValue > Data.sickResistance)
        {
            SetState(EmployeeState.Sick);
            Debug.Log($"Сотрудница {name} заболела при оказании услуги {service}. Шанс не заболеть: {Data.sickResistance}%");
        }
        else
        {
            Debug.Log($"Сотрудница {name} не заболела при оказании услуги {service}. Шанс не заболеть: {Data.sickResistance}%");
        }
    }

    public void EndDayUpdate()
    {
        if (state == EmployeeState.Healing)
        {
            SetState(EmployeeState.Available);
        }
        if (state == EmployeeState.Tired)
        {
            StaminaCurrent = StaminaMax;
            SetState(EmployeeState.Available);
        }
    }

    [ContextMenu("Set Max Level")]
    public void SetMaxLevel()
    {
        foreach (var skill in Skills.Keys.ToList())
        {
            Skills[skill] = (10, 100);
            Debug.Log($"Сотрудница {name} навык {skill} установлен на уровень 10 прогрессия 100");
        }
    }
}