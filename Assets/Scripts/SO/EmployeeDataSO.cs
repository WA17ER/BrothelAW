using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeData", menuName = "ScriptableObjects/EmployeeDataSO", order = 1)]
public class EmployeeDataSO : ScriptableObject
{
    public string employeeName;
    public string Race;
    public List<string> BodyTypes;
    public char BreastSize;
    public List<string> BaseSkills;
    public float hireCost;
    public float hirePopularity;
    [Tooltip("Процентное снижение шанса болезни (0-100%)")]
    public float sickResistance = 0f;
    [Tooltip("Цены за услуги сотрудницы")]
    public Dictionary<string, float> servicePrices = new Dictionary<string, float>
    {
        { "Дрочка", 50f },
        { "Миньет", 60f },
        { "Дрочка Сиськами", 70f },
        { "Миссионерская", 80f },
        { "Наездница", 90f },
        { "Амазонка", 70f },
        { "Раком", 50f },
        { "Стоя", 80f }
    };
}