using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [SerializeField] private Transform registerPosition;
    [SerializeField] private Transform serviceDestination;
    [SerializeField] private Transform exitPoint;

    public Transform RegisterPosition => registerPosition;
    public Transform ServiceDestination => serviceDestination;
    public Transform ExitPoint => exitPoint;

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
        }
    }
}