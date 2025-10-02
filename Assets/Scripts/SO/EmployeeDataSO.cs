using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewEmployeeData", menuName = "ScriptableObjects/EmployeeDataSO")]
public class EmployeeDataSO : ScriptableObject
{
    public string employeeName;
    public enum BreastSize { A, B, C, D, E, F, None }
    public enum BodyType { Обычное, Доска, Милое, Высокое, Спортивное, Желанное, Великан, Перевёртыш, None }
    public enum Race { Человек, Тёмный_Эльф, Эльф, Некоматана, Дриада, Они, Китсуне, Гарпия, Допельгангер, Суккуб, Ангел, None }
    public BreastSize breastSize;
    public BodyType bodyType;
    public Race race;
    public float StaminaMax = 10f;
    public List<SicknessSO> PossibleSicknesses;
    public string[] BaseSkills;
    public float sickResistance;
    public float hireCost = 100f;
    public float progressionSpeed = 10f;
    public Sprite listIcon; // Иконка для списка сотрудниц в EmployeeSelectionPanel
    public Sprite portraitIcon; // Иконка для панели сотрудниц в EmployeePanel
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