using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float dayDuration = 300f; // 5 минут на игровой день
    [SerializeField] private EmployeeManager employeeManager; // Ссылка на EmployeeManager

    private float dayTimer;
    private int gameDay = 1;
    private static GameManager instance;

    public static GameManager Instance => instance;
    public int GameDay => gameDay;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (employeeManager == null)
        {
            Debug.LogError("GameManager: EmployeeManager is not assigned.");
            enabled = false;
        }
    }

    private void Start()
    {
        dayTimer = dayDuration;
    }

    private void Update()
    {
        dayTimer -= Time.deltaTime;
        if (dayTimer <= 0)
        {
            EndDay();
        }
    }

    private void EndDay()
    {
        gameDay++;
        dayTimer = dayDuration;

        // Обновление сотрудниц
        foreach (var employee in employeeManager.GetAvailableEmployees())
        {
            // Восстановление Stamina
            employee.RestoreStamina(20);

            // Уменьшение длительности болезни
            if (employee.Disease != null)
            {
                employee.Disease.DecreaseDuration();
                if (employee.Disease.Duration <= 0)
                {
                    employee.SetDisease(null); // Выздоровление
                }
            }
        }
    }
}