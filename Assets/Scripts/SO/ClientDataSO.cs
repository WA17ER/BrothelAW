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
    public int maxClientPerScene; // Максимальное количество клиентов данного типа за сцену

    [System.Serializable]
    public class Preference<T>
    {
        public List<T> preferences;
        public float chance;
    }

    public Preference<EmployeeDataSO.Race> racePreference = new Preference<EmployeeDataSO.Race> { preferences = new List<EmployeeDataSO.Race>(), chance = 0f };
    public Preference<EmployeeDataSO.BodyType> bodyPreference = new Preference<EmployeeDataSO.BodyType> { preferences = new List<EmployeeDataSO.BodyType>(), chance = 0f };
    public Preference<EmployeeDataSO.BreastSize> breastPreference = new Preference<EmployeeDataSO.BreastSize> { preferences = new List<EmployeeDataSO.BreastSize>(), chance = 0f };
}