using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmployeeSkill
{
    public int level; // 0-10
    public int progress; // 0-100
}

public class Employee : MonoBehaviour
{
    [SerializeField] private EmployeeDataSO data; // Ссылка на ScriptableObject

    private Dictionary<string, EmployeeSkill> skills = new Dictionary<string, EmployeeSkill>();
    [SerializeField] private float staminaMax = 100f;
    private float staminaCurrent;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError("Employee: Data не назначена.");
            enabled = false;
            return;
        }

        staminaCurrent = staminaMax;

        // Инициализация навыков из baseSkills
        foreach (var skillName in data.BaseSkills)
        {
            skills[skillName] = new EmployeeSkill { level = 0, progress = 0 };
        }
    }

    // Геттеры для параметров из data
    public string Race => data.Race;
    public List<string> BodyTypes => data.BodyTypes;
    public char BreastSize => data.BreastSize;
    public List<string> BaseSkills => data.BaseSkills;

    // Геттеры для динамических параметров
    public Dictionary<string, EmployeeSkill> Skills => skills;
    public float StaminaMax => staminaMax;
    public float StaminaCurrent => staminaCurrent;
}