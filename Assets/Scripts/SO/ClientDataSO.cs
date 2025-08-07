using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClientData", menuName = "ScriptableObjects/ClientDataSO")]
public class ClientDataSO : ScriptableObject
{
    public GameManager.ClientType clientType;
    public float totalGold;
    public float sickChance;
    public List<SicknessSO> possibleSicknesses;
}