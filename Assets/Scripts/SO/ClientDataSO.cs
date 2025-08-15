using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewClientData", menuName = "SO/ClientData")]
public class ClientDataSO : ScriptableObject
{
    public enum ClientType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3,
        Type4 = 4
    }

    public ClientType clientType;
    public float minGold;
    public float maxGold;
    public float goldStep;
    public float stepPerWeek = 10f;
    public float sickChance = 30f; // Вероятность заболевания клиента (0–100)
    public List<SicknessSO> possibleSicknesses; // Список возможных болезней
}