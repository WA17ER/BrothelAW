using UnityEngine;

[CreateAssetMenu(fileName = "NewEmployeeData", menuName = "ScriptableObjects/EmployeeDataSO")]
public class EmployeeDataSO : ScriptableObject
{
    public string employeeName;
    public string Race;
    public string BodyType;
    public char BreastSize;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] validBodyTypes = { "Обычное", "Доска", "Милое", "Мускулистое", "Высокая", "Спортивное", "Желанное" };
        if (!string.IsNullOrEmpty(BodyType) && !System.Array.Exists(validBodyTypes, bt => bt == BodyType))
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: BodyType ({BodyType}) должен быть одним из: {string.Join(", ", validBodyTypes)}.");
        }

        if (specialRace != SpecialRace.None && Race != specialRace.ToString())
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: SpecialRace ({specialRace}) должен соответствовать Race ({Race}).");
        }
        else if (specialRace == SpecialRace.None && (Race == "Succubus" || Race == "Doppelganger" || Race == "Angel"))
        {
            Debug.LogWarning($"EmployeeDataSO {employeeName}: Race ({Race}) требует SpecialRace ({Race}), а не None.");
        }
    }
#endif
}