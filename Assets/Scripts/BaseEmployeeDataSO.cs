using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BaseEmployeeDataSO.asset", menuName = "Characters/BaseEmployeeData")]
public class BaseEmployeeDataSO : ScriptableObject
{
    [System.Serializable]
    public struct Skill
    {
        public string skillName;
        public int level; // 1-10
        public float progress; // 0-100
    }

    public enum EmployeeState
    {
        Free,
        Working,
        Advertising,
        Sick,
        HeavySick,
        Tired
    }

    [SerializeField] private string id;
    [SerializeField] private string employeeName;
    [SerializeField] private string race;
    [SerializeField] private char breastSize; // A, B, C, D, F
    [SerializeField] private string bodyType; // Slim, Tall, Sport, Fit
    [SerializeField] private Sprite selectionCardImage;
    [SerializeField] private Sprite listIcon;
    [SerializeField] private List<Skill> skills;
    [SerializeField] private BaseSickSO disease;
    [SerializeField] private EmployeeState currentState;
    [SerializeField] private int stamina; // 0-100

    public string ID => id;
    public string EmployeeName => employeeName;
    public string Race => race;
    public char BreastSize => breastSize;
    public string BodyType => bodyType;
    public Sprite SelectionCardImage => selectionCardImage;
    public Sprite ListIcon => listIcon;
    public IReadOnlyList<Skill> Skills => skills.AsReadOnly();
    public BaseSickSO Disease => disease;
    public EmployeeState CurrentState => currentState;
    public int Stamina => stamina;

    public void SetState(EmployeeState newState)
    {
        currentState = newState;
    }

    public void SetDisease(BaseSickSO newDisease)
    {
        disease = newDisease;
        currentState = newDisease != null && newDisease.IsHeavy ? EmployeeState.HeavySick : newDisease != null ? EmployeeState.Sick : EmployeeState.Free;
    }

    public void UpdateSkillProgress(string skillName, float progressIncrement)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].skillName == skillName)
            {
                Skill skill = skills[i];
                skill.progress += progressIncrement;
                if (skill.progress >= 100 && skill.level < 10)
                {
                    skill.level++;
                    skill.progress = 0;
                }
                skills[i] = skill;
                break;
            }
        }
    }

    public void DecreaseStamina(int amount)
    {
        stamina = Mathf.Max(0, stamina - amount);
        if (stamina == 0)
        {
            currentState = EmployeeState.Tired;
        }
    }

    public void RestoreStamina(int amount)
    {
        stamina = Mathf.Min(100, stamina + amount);
        if (stamina > 0 && currentState == EmployeeState.Tired)
        {
            currentState = EmployeeState.Free;
        }
    }
}