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
}