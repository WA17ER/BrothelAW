using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientData", menuName = "ScriptableObjects/ClientDataSO", order = 2)]
public class ClientDataSO : ScriptableObject
{
    [Tooltip("ћаксимальное золото клиента дл€ оплаты услуг")]
    public float totalGold = 100f;
    [Tooltip("¬еро€тность болезни клиента при спавне (0-100%)")]
    public float sickChance = 10f;
    [Tooltip("Ѕолен ли клиент (устанавливаетс€ при спавне)")]
    public bool isSick;
}