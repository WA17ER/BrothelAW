using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEmployeeData", menuName = "ScriptableObjects/EmployeeDataSO")]
public class EmployeeDataSO : ScriptableObject
{
    public string employeeName;
    public enum BreastSize { A, B, C, D, E, F }
    public enum BodyType { Обычное, Доска, Милое, Высокое, Спортивное, Желанное, Великан, Перевёртыш }
    public enum Race { Человек, Тёмный_Эльф, Эльф, Некоматана, Дриада, Они, Китсуне, Гарпия, Допельгангер, Суккуб, Ангел }
    public BreastSize breastSize;
    public BodyType bodyType;
    public Race race;
    public float StaminaMax = 10f;
    public List<SicknessSO> PossibleSicknesses;
    public string[] BaseSkills;
    public float sickResistance;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(employeeName))
        {
            Debug.LogWarning($"EmployeeDataSO: employeeName не задан.");
        }
        if (BaseSkills == null || BaseSkills.Length == 0)
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: BaseSkills пуст или не задан.");
        }
    }
}