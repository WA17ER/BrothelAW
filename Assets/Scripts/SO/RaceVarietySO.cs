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
}