using UnityEngine;

public class Chair : MonoBehaviour
{
    [SerializeField] private Transform bottomPoint, topPoint;
    public enum ChairState { Free, OnOccupation, IsOccupied }

    private ChairState chairState = ChairState.Free;

    public Transform BottomPoint => bottomPoint;
    public Transform TopPoint => topPoint;
    public ChairState CurrentState { get => chairState; set => chairState = value; }

    void OnValidate()
    {
        if (bottomPoint == null || topPoint == null)
        {
            Debug.LogError($"{name} missing BottomPoint or TopPoint.");
            enabled = false;
        }
    }
}