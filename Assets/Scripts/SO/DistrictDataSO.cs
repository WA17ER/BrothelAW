using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDistrictData", menuName = "District/DistrictDataSO")]
public class DistrictDataSO : ScriptableObject
{
    [SerializeField] private EmployeePreferenceData employeePreferences;
    [SerializeField] private string districtName;
    [SerializeField] private float popularityToUnlock;
    [SerializeField] private List<ClientTypeWithMultiplier> clientTypes; // Список с элементами ClientTypeWithMultiplier
    [SerializeField] private List<Race> availableRaces;
    [SerializeField] private float districtPopularity;
    [SerializeField] private List<RaceUnlock> raceUnlocks;
    [SerializeField] private float preliminaryPopularity;

    public string DistrictName => districtName;
    public EmployeePreferenceData EmployeePreferences => employeePreferences;
    public float PopularityToUnlock => popularityToUnlock;
    public List<ClientTypeWithMultiplier> ClientTypes => clientTypes; // Доступ через свойство
    public List<Race> AvailableRaces => availableRaces;
    public float DistrictPopularity { get => districtPopularity; set => districtPopularity = value; }
    public List<RaceUnlock> RaceUnlocks => raceUnlocks;
    public float PreliminaryPopularity { get => preliminaryPopularity; set => preliminaryPopularity = value; }

    public void IncreaseDistrictPopularity(float amount)
    {
        districtPopularity += amount;
        Debug.Log($"Популярность района {districtName} увеличена на {amount} до {districtPopularity}.");
    }
}

[System.Serializable]
public class ClientTypeWithMultiplier
{
    [SerializeField] private ClientDataSO clientType; // SO клиента
    [SerializeField] private int multiplier; // Кратность популярности, должна отображаться

    public ClientDataSO ClientType => clientType;
    public int Multiplier => multiplier;
}

[System.Serializable]
public class EmployeePreferenceData
{
    [SerializeField]
    private List<ChestSizePreference> chestSize = new List<ChestSizePreference>
    {
        new ChestSizePreference { Size = EmployeeDataSO.BreastSize.A, Value = 10 },
        new ChestSizePreference { Size = EmployeeDataSO.BreastSize.B, Value = 15 },
        new ChestSizePreference { Size = EmployeeDataSO.BreastSize.C, Value = 20 },
        new ChestSizePreference { Size = EmployeeDataSO.BreastSize.D, Value = 25 },
        new ChestSizePreference { Size = EmployeeDataSO.BreastSize.E, Value = 30 }
    };
    [SerializeField] private List<BodyTypePreference> bodyType = new List<BodyTypePreference>();
    [SerializeField] private List<RacePreference> racePreference = new List<RacePreference>();

    public List<ChestSizePreference> ChestSize => chestSize;
    public List<BodyTypePreference> BodyType => bodyType;
    public List<RacePreference> RacePreference => racePreference;

    public void InitializePreferences()
    {
        if (bodyType.Count == 0)
        {
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Обычное, Value = 10 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Доска, Value = 15 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Милое, Value = 20 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Высокое, Value = 25 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Спортивное, Value = 30 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Желанное, Value = 35 });
            bodyType.Add(new BodyTypePreference { Type = EmployeeDataSO.BodyType.Великан, Value = 40 });
        }
        if (racePreference.Count == 0)
        {
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Человек, Value = 10 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Эльф, Value = 15 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Тёмный_Эльф, Value = 20 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Некоматана, Value = 25 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Они, Value = 30 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Гарпия, Value = 35 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Китсуне, Value = 40 });
            racePreference.Add(new RacePreference { Race = EmployeeDataSO.Race.Дриада, Value = 45 });
        }
    }
}

[System.Serializable]
public class ChestSizePreference
{
    [SerializeField] private EmployeeDataSO.BreastSize size;
    [SerializeField] private int value;

    public EmployeeDataSO.BreastSize Size { get => size; set => size = value; }
    public int Value { get => value; set => this.value = value; }
}

[System.Serializable]
public class BodyTypePreference
{
    [SerializeField] private EmployeeDataSO.BodyType type;
    [SerializeField] private int value;

    public EmployeeDataSO.BodyType Type { get => type; set => type = value; }
    public int Value { get => value; set => this.value = value; }
}

[System.Serializable]
public class RacePreference
{
    [SerializeField] private EmployeeDataSO.Race race;
    [SerializeField] private int value;

    public EmployeeDataSO.Race Race { get => race; set => race = value; }
    public int Value { get => value; set => this.value = value; }
}

[System.Serializable]
public class RaceUnlock
{
    [SerializeField] private Race race;
    [SerializeField] private int popularityThreshold;

    public Race Race => race;
    public int PopularityThreshold => popularityThreshold;
}

public enum Race
{
    Человек,
    Ельф,
    Тёмный_Эльф,
    Некоматана,
    Они,
    Гарпия,
    Китсуне,
    Дриада,
    Ангел,
    Суккуб,
    Допельгангер
}