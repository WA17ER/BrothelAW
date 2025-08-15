using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEmployeeBonuses", menuName = "SO/EmployeeBonuses")]
public class EmployeeBonusesSO : ScriptableObject
{
    [System.Serializable]
    public struct Bonus
    {
        public EmployeeDataSO.BreastSize breastSize;
        public float bonus;
    }

    [System.Serializable]
    public struct BodyTypeBonus
    {
        public EmployeeDataSO.BodyType bodyType;
        public float bonus;
    }

    [System.Serializable]
    public struct RaceBonus
    {
        public EmployeeDataSO.Race race;
        public float bonus;
    }

    public List<Bonus> breastSizeBonuses = new List<Bonus>
    {
        new Bonus { breastSize = EmployeeDataSO.BreastSize.A, bonus = 10f },
        new Bonus { breastSize = EmployeeDataSO.BreastSize.B, bonus = 30f },
        new Bonus { breastSize = EmployeeDataSO.BreastSize.C, bonus = 50f },
        new Bonus { breastSize = EmployeeDataSO.BreastSize.D, bonus = 60f },
        new Bonus { breastSize = EmployeeDataSO.BreastSize.E, bonus = 70f },
        new Bonus { breastSize = EmployeeDataSO.BreastSize.F, bonus = 80f }
    };

    public List<BodyTypeBonus> bodyTypeBonuses = new List<BodyTypeBonus>
    {
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Обычное, bonus = 10f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Доска, bonus = 20f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Милое, bonus = 30f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Высокое, bonus = 50f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Спортивное, bonus = 60f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Желанное, bonus = 100f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Великан, bonus = 80f },
        new BodyTypeBonus { bodyType = EmployeeDataSO.BodyType.Перевёртыш, bonus = 0f }
    };

    public List<RaceBonus> raceBonuses = new List<RaceBonus>
    {
        new RaceBonus { race = EmployeeDataSO.Race.Человек, bonus = 20f },
        new RaceBonus { race = EmployeeDataSO.Race.Тёмный_Эльф, bonus = 35f },
        new RaceBonus { race = EmployeeDataSO.Race.Эльф, bonus = 30f },
        new RaceBonus { race = EmployeeDataSO.Race.Некоматана, bonus = 45f },
        new RaceBonus { race = EmployeeDataSO.Race.Дриада, bonus = 50f },
        new RaceBonus { race = EmployeeDataSO.Race.Они, bonus = 55f },
        new RaceBonus { race = EmployeeDataSO.Race.Гарпия, bonus = 65 },
        new RaceBonus { race = EmployeeDataSO.Race.Китсуне, bonus = 60f },
        new RaceBonus { race = EmployeeDataSO.Race.Допельгангер, bonus = 300f },
        new RaceBonus { race = EmployeeDataSO.Race.Суккуб, bonus = 800f },
        new RaceBonus { race = EmployeeDataSO.Race.Ангел, bonus = 1000f }
    };
}