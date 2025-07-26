using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeData", menuName = "Employee/EmployeeData")]
public class EmployeeDataSO : ScriptableObject
{
    [SerializeField] private string race; // Раса
    [SerializeField] private List<string> bodyTypes; // Типы тела
    [SerializeField] private char breastSize; // Размер груди (A, B, C и т.д.)
    [SerializeField] private List<string> baseSkills; // Базовые навыки

    public string Race => race;
    public List<string> BodyTypes => bodyTypes;
    public char BreastSize => breastSize;
    public List<string> BaseSkills => baseSkills;
}