using UnityEngine;

[CreateAssetMenu(fileName = "NewSickness", menuName = "ScriptableObjects/SicknessSO")]
public class SicknessSO : ScriptableObject
{
    public string SickName;
    public int HealingCost;
    public int duration;
    public int popularityPenalty;
    public int incomePenalty;
    public bool isUnavailable;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(SickName)) SickName = "Unnamed Sickness";
        if (HealingCost < 0) HealingCost = 0;
        if (duration < 1) duration = 1;
        if (popularityPenalty < 0) popularityPenalty = 0;
        if (incomePenalty < 0) incomePenalty = 0;
    }
}