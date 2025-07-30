using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeDataSO", menuName = "Employee/EmployeeDataSO")]
public class EmployeeDataSO : ScriptableObject
{
    [SerializeField] private string race; // Раса
    [SerializeField] private List<string> bodyTypes; // Типы тела
    [SerializeField] private char breastSize; // Размер груди (A, B, C и т.д.)
    [SerializeField] private List<string> baseSkills; // Базовые навыки
    [SerializeField] private float sickChanceModifier; // Модификатор шанса болезни (e.g. +0.02 для Succubus, -0.01 для Elf)

    public string Race => race;
    public List<string> BodyTypes => bodyTypes;
    public char BreastSize => breastSize;
    public List<string> BaseSkills => baseSkills;
    public float SickChanceModifier => sickChanceModifier;
}