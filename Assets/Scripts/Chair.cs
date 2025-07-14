using UnityEngine;

public class Chair : MonoBehaviour
{
    [SerializeField] private Transform bottomPoint; // Точка у основания (Y=0)
    [SerializeField] private Transform topPoint; // Точка на стуле (Y=0.5)
    public bool IsAvailable { get; private set; } = true;

    public Transform BottomPoint => bottomPoint;
    public Transform TopPoint => topPoint;

    void Start()
    {
        if (bottomPoint == null || topPoint == null)
        {
            Debug.LogError($"{gameObject.name} is missing BottomPoint or TopPoint.");
            enabled = false;
        }
    }

    public void OccupyChair()
    {
        IsAvailable = false;
        Debug.Log($"{gameObject.name} occupied.");
    }

    public void FreeChair()
    {
        IsAvailable = true;
        Debug.Log($"{gameObject.name} freed.");
    }
}