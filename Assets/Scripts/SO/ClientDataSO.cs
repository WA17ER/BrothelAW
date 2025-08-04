using UnityEngine;

[CreateAssetMenu(fileName = "ClientData", menuName = "ScriptableObjects/ClientDataSO", order = 2)]
public class ClientDataSO : ScriptableObject
{
    [Tooltip("Тип клиента (Type1, Type2, Type3, Type4)")]
    public GameManager.ClientType clientType;
    [Tooltip("Максимальное золото клиента для оплаты услуг")]
    public float totalGold = 100f;
    [Tooltip("Вероятность болезни клиента при спавне (0-100%)")]
    public float sickChance = 10f;
    [Tooltip("Болен ли клиент (устанавливается при спавне)")]
    public bool isSick;
}