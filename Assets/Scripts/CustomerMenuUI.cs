using UnityEngine;
using UnityEngine.UI;

public class CustomerMenuUI : MonoBehaviour
{
    [SerializeField] private Button waitButton; // Кнопка "Ожидать"
    [SerializeField] private Button goButton; // Кнопка "Идти"
    [SerializeField] private Button cancelButton; // Кнопка "Отмена"
    private Route route; // Ссылка на Route клиента
    private CustomerMovement customer; // Ссылка на CustomerMovement

    void Awake()
    {
        // Подписываемся на события кнопок
        waitButton.onClick.AddListener(OnWaitButtonClicked);
        goButton.onClick.AddListener(OnGoButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    public void Initialize(Route routeComponent, CustomerMovement customerComponent)
    {
        route = routeComponent;
        customer = customerComponent;
        gameObject.SetActive(true); // Активируем UI
        Time.timeScale = 0f; // Ставим игру на паузу
        Debug.Log("CustomerMenuUI opened and game paused.");
    }

    private void OnWaitButtonClicked()
    {
        route.TryWaitAtChair(); // Пытаемся найти стул
        CloseMenu();
    }

    private void OnGoButtonClicked()
    {
        route.SwitchToDestination(); // Переходим в OnDestination
        CloseMenu();
    }

    private void OnCancelButtonClicked()
    {
        CloseMenu(); // Просто закрываем UI
    }

    private void CloseMenu()
    {
        Time.timeScale = 1f; // Возобновляем игру
        gameObject.SetActive(false); // Отключаем UI
        Debug.Log("CustomerMenuUI closed and game resumed.");
    }
}