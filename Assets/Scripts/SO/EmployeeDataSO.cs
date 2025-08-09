using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmployeeData", menuName = "ScriptableObjects/EmployeeDataSO")]
public class EmployeeDataSO : ScriptableObject
{
    public string employeeName;
    public string Race;
    public string BodyType;
    public char BreastSize;
    public float StaminaMax = 10f;
    public List<SicknessSO> PossibleSicknesses;
    public string[] BaseSkills;
    public float sickResistance;

    [SerializeField] private SpecialRace specialRace;

    public enum SpecialRace
    {
        None,
        Succubus,
        Doppelganger,
        Angel
    }

    public SpecialRace GetSpecialRace()
    {
        return specialRace;
    }


    private void OnValidate()
    {
        string[] validBodyTypes = { "Обычное", "Доска", "Милое", "Мускулистое", "Высокая", "Спортивное", "Желанное", "Великан", "Перевёртыш" };
        if (!string.IsNullOrEmpty(BodyType) && !System.Array.Exists(validBodyTypes, bt => bt == BodyType))
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: BodyType ({BodyType}) должен быть одним из: {string.Join(", ", validBodyTypes)}.");
        }

        var raceSpecialRaceMap = new System.Collections.Generic.Dictionary<string, SpecialRace>
        {
            {"Суккуб", SpecialRace.Succubus},
            {"Допельгангер", SpecialRace.Doppelganger},
            {"Ангел", SpecialRace.Angel}
        };

        if (specialRace != SpecialRace.None && (!raceSpecialRaceMap.ContainsKey(Race) || raceSpecialRaceMap[Race] != specialRace))
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: SpecialRace ({specialRace}) должен соответствовать Race ({Race}).");
        }
        else if (specialRace == SpecialRace.None && raceSpecialRaceMap.ContainsKey(Race))
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: Race ({Race}) требует SpecialRace ({raceSpecialRaceMap[Race]}), а не None.");
        }
    }
}