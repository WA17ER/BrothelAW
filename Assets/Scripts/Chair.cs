using UnityEngine;

public class Chair : MonoBehaviour
{
    [SerializeField] private Transform bottomPoint; // Точка у основания (Y=0)
    [SerializeField] private Transform topPoint; // Точка на стуле (Y=0.5)

    public enum ChairState { Free, OnOccupation, IsOccupied }
    private ChairState chairState = ChairState.Free;

    public Transform BottomPoint => bottomPoint;
    public Transform TopPoint => topPoint;
    public ChairState CurrentState => chairState;

    private void Awake()
    {
        if (bottomPoint == null || topPoint == null)
        {
            Debug.LogError($"{gameObject.name} is missing BottomPoint or TopPoint.");
            enabled = false;
        }
    }

    public void SetState(ChairState newState)
    {
        chairState = newState;
    }
}