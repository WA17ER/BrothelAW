using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBaseSick.asset", menuName = "Diseases/BaseSick")]
public class BaseSickSO : ScriptableObject
{
    [SerializeField] private string diseaseName; // Название болезни
    [SerializeField] private string diseaseType; // Тип: "Magic", "Physical"
    [SerializeField] private int duration; // Длительность в игровых днях
    [SerializeField] private float incomePenalty; // Штраф к доходу (0-1)
    [SerializeField] private int staminaPenalty; // Штраф к стамине (0-3)
    [SerializeField] private bool isHeavy; // Тяжелая болезнь (true = HeavySick)
    [SerializeField] private string description; // Описание болезни
    [SerializeField] private List<string> immuneRaces; // Список иммунных рас

    public string DiseaseName => diseaseName;
    public string DiseaseType => diseaseType;
    public int Duration => duration;
    public float IncomePenalty => incomePenalty;
    public int StaminaPenalty => staminaPenalty;
    public bool IsHeavy => isHeavy;
    public string Description => description;
    public IReadOnlyList<string> ImmuneRaces => immuneRaces.AsReadOnly();

    public void DecreaseDuration()
    {
        if (duration > 0) duration--;
    }
}