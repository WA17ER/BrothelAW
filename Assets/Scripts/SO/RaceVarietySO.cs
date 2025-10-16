using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRaceVariety", menuName = "ScriptableObjects/RaceVarietySO")]
public class RaceVarietySO : ScriptableObject
{
    public EmployeeDataSO.Race raceName;
    public List<EmployeeDataSO.BreastSize> possibleBreastSizes;
    public List<EmployeeDataSO.BodyType> possibleBodyTypes;
    public List<string> possibleNames;
    public List<string> possibleSkills;
    public List<SicknessSO> possibleSicknesses;
    public float minStaminaMax = 10f;
    public float maxStaminaMax = 20f;
    public float minSickResistance = 0f;
    public float maxSickResistance = 50f;
    public float minHireCost = 50f;
    public float maxHireCost = 200f;
    public Sprite Icon;
    public Sprite portraitIcon;

    void OnValidate()
    {
        if (possibleBreastSizes == null || possibleBreastSizes.Count == 0) Debug.LogWarning("possibleBreastSizes empty in " + name);
        if (possibleBodyTypes == null || possibleBodyTypes.Count == 0) Debug.LogWarning("possibleBodyTypes empty in " + name);
        if (possibleNames == null || possibleNames.Count == 0) Debug.LogWarning("possibleNames empty in " + name);
        if (possibleSkills == null || possibleSkills.Count == 0) Debug.LogWarning("possibleSkills empty in " + name);
        if (possibleSicknesses == null || possibleSicknesses.Count == 0) Debug.LogWarning("possibleSicknesses empty in " + name);
        if (minStaminaMax > maxStaminaMax) minStaminaMax = maxStaminaMax;
        if (minSickResistance > maxSickResistance) minSickResistance = maxSickResistance;
        if (minHireCost > maxHireCost) minHireCost = maxHireCost;
    }
}