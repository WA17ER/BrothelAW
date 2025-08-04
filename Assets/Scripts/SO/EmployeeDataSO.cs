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
    [Tooltip("Процентное увеличение базового шанса болезни (например, 10 для +10%)")]
    public float chanceSickModifier = 10f; // По умолчанию +10% к базовому шансу
}