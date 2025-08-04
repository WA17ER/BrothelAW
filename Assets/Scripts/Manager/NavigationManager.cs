using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [SerializeField] private Transform registerPosition; // Позиция регистрации
    [SerializeField] private Transform serviceDestination; // Позиция услуги
    [SerializeField] private Transform exitPoint; // Точка выхода

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (registerPosition == null || serviceDestination == null || exitPoint == null)
        {
            Debug.LogWarning("NavigationManager: Одна или несколько точек навигации не назначены.");
        }
    }

    public Transform RegisterPosition => registerPosition;
    public Transform ServiceDestination => serviceDestination;
    public Transform ExitPoint => exitPoint;
}