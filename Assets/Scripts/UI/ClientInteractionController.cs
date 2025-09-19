using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientInteractionController : MonoBehaviour
{
    public ClientData currentClient; // Клиент, чей индикатор был нажат
    public Button waitButton;
    public Button assignButton;
    public Button cancelButton;
    public TMP_Text expectedEmployeeText, expectedRaceText, expectedBodyText, expectedBreastText, desiredServiceText, goldText; // Текстовые поля клиента
    public Image employeeIcon; // Иконка сотрудницы
    public TMP_Text employeeNameText, employeeRaceText, employeeBodyText, employeeBreastText, employeeServiceText, rewardText; // Текстовые поля сотрудницы
    public Image employeePortrait; // Портрет сотрудницы
    [SerializeField] private GameObject employeeListToSelectPanelPrefab; // Префаб панели выбора сотрудниц
    private bool isDestroyed = false; // Флаг для проверки уничтожения
    private Employee lastSelectedEmployee; // Для отслеживания изменений SelectedEmployee
    private Sprite displayedEmployeeIcon; // Отдельное поле для иконки

    void Start()
    {
        if (waitButton != null)
        {
            waitButton.onClick.AddListener(OnWaitClick);
        }
        if (assignButton != null)
        {
            assignButton.onClick.AddListener(OnAssignClick);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClick);
        }
        if (employeePortrait != null && employeePortrait.GetComponent<Button>() != null)
        {
            employeePortrait.GetComponent<Button>().onClick.AddListener(OnEmployeePortraitClick);
        }
        InitializePanel(); // Инициализация панели при старте
    }

    void Update()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (currentClient != null && currentClient.SelectedEmployee != lastSelectedEmployee)
        {
            lastSelectedEmployee = currentClient.SelectedEmployee;
            InitializePanel(); // Обновление панели при изменении SelectedEmployee
        }
    }

    public void InitializeClient(ClientData client)
    {
        if (isDestroyed) return; // Проверка на уничтожение
        currentClient = client;
        lastSelectedEmployee = currentClient.SelectedEmployee; // Инициализация отслеживания
        if (currentClient == null)
        {
            Debug.LogError("Клиент не передан в ClientInteractionController!");
        }
        else
        {
            InitializePanel(); // Обновление данных при получении клиента
            Debug.Log($"Инициализирован клиент {currentClient.clientName} в панели");
        }
    }

    public void UpdateEmployeePanel(EmployeeDataSO selectedEmployee)
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (selectedEmployee != null && employeeIcon != null && employeeNameText != null && employeeRaceText != null &&
            employeeBodyText != null && employeeBreastText != null && employeeServiceText != null && rewardText != null)
        {
            // Проверка соответствия заказа
            bool isMatch = CheckOrderMatch(selectedEmployee);
            if (isMatch)
            {
                employeeIcon.sprite = selectedEmployee.portraitIcon;
                employeeNameText.text = "Имя: " + selectedEmployee.employeeName;
                employeeRaceText.text = "Раса: " + selectedEmployee.race.ToString();
                employeeBodyText.text = "Тип тела: " + selectedEmployee.bodyType.ToString();
                employeeBreastText.text = "Размер груди: " + selectedEmployee.breastSize.ToString();
                employeeServiceText.text = "Услуга (уровень): " + (selectedEmployee.BaseSkills.Length > 0 ? selectedEmployee.BaseSkills[0] + " (1)" : "None");
                rewardText.text = "Стоимость: " + CalculateReward(selectedEmployee);
                // Инициализация или обновление SpecificEmployee
                if (currentClient.SpecificEmployee == null)
                {
                    currentClient.SpecificEmployee = new Employee();
                    currentClient.SpecificEmployee.SetData(selectedEmployee); // Установка данных через метод
                    EmployeeManager.Instance.MoveEmployeeToList(currentClient.SpecificEmployee); // Обновление состояния
                }
                else
                {
                    currentClient.SpecificEmployee.SetData(selectedEmployee); // Обновление данных
                }
                Debug.Log($"Панель сотрудницы обновлена для {selectedEmployee.employeeName}, соответствие заказа подтверждено");
            }
            else
            {
                Debug.LogWarning($"Сотрудница {selectedEmployee.employeeName} не соответствует заказу клиента {currentClient.clientName}");
            }
        }
        else
        {
            Debug.LogWarning("Не удалось обновить панель сотрудницы: данные или элементы null!");
        }
    }

    public bool CheckOrderMatch(EmployeeDataSO employee) // Сделан публичным
    {
        if (isDestroyed || currentClient == null || employee == null) return false;

        // Проверка соответствия параметров заказа с пропуском null-полей
        bool raceMatch = currentClient.clientType == 3 && currentClient.preferredRace != EmployeeDataSO.Race.None ? currentClient.preferredRace == employee.race : true;
        bool bodyMatch = currentClient.clientType == 2 && currentClient.preferredBodyType != EmployeeDataSO.BodyType.None ? currentClient.preferredBodyType == employee.bodyType : true;
        bool breastMatch = currentClient.clientType == 2 && currentClient.preferredBreastSize != EmployeeDataSO.BreastSize.None ? currentClient.preferredBreastSize == employee.breastSize : true;
        bool serviceMatch = currentClient.RequestedService == null ||
                           (employee.BaseSkills != null && employee.BaseSkills.Length > 0 && employee.BaseSkills.Contains(currentClient.RequestedService));
        Debug.Log($"Проверка расы: клиент {currentClient.clientName} ожидает {currentClient.preferredRace}, сотрудница {employee.race}, совпадение: {raceMatch}");
        Debug.Log($"Проверка типа тела: клиент {currentClient.clientName} ожидает {currentClient.preferredBodyType}, сотрудница {employee.bodyType}, совпадение: {bodyMatch}");
        Debug.Log($"Проверка размера груди: клиент {currentClient.clientName} ожидает {currentClient.preferredBreastSize}, сотрудница {employee.breastSize}, совпадение: {breastMatch}");
        Debug.Log($"Проверка услуги: клиент {currentClient.clientName} ожидает {currentClient.RequestedService}, сотрудница {string.Join(", ", employee.BaseSkills)}, совпадение: {serviceMatch}");

        bool isMatch = raceMatch && bodyMatch && breastMatch && serviceMatch;
        if (!isMatch)
        {
            Debug.LogWarning($"Несоответствие заказа для клиента {currentClient.clientName}: раса {raceMatch}, тип тела {bodyMatch}, размер груди {breastMatch}, услуга {serviceMatch}");
        }
        return isMatch;
    }

    private float CalculateReward(EmployeeDataSO employee)
    {
        if (isDestroyed || employee == null || currentClient == null) return 0f;

        // Базовая стоимость услуги из EmployeeManager
        float baseReward = EmployeeManager.Instance.ServicePrices
            .Find(sp => sp.serviceName == currentClient.RequestedService)?.price ?? 0f;

        // Получение бонусов из EmployeeBonusesSO через EmployeeManager
        EmployeeBonusesSO bonuses = EmployeeManager.Instance.GetBonuses();
        if (bonuses == null) return baseReward;

        // Бонус за уровень навыка
        float skillBonus = 0f;
        if (currentClient.SpecificEmployee != null && currentClient.SpecificEmployee.Skills.ContainsKey(currentClient.RequestedService))
        {
            skillBonus = currentClient.SpecificEmployee.Skills[currentClient.RequestedService].level * 0.1f;
        }

        // Проверка на спецрасы (Допельгангер, Суккуб, Ангел)
        bool isSpecialRace = employee.race == EmployeeDataSO.Race.Допельгангер || employee.race == EmployeeDataSO.Race.Суккуб || employee.race == EmployeeDataSO.Race.Ангел;
        if (!isSpecialRace)
        {
            // Бонус за размер груди
            float breastBonus = bonuses.breastSizeBonuses.Find(b => b.breastSize == employee.breastSize).bonus;

            // Бонус за тип тела
            float bodyBonus = bonuses.bodyTypeBonuses.Find(b => b.bodyType == employee.bodyType).bonus;

            // Бонус за расу
            float raceBonus = bonuses.raceBonuses.Find(b => b.race == employee.race).bonus;

            // Общая стоимость с бонусами
            baseReward += breastBonus + bodyBonus + raceBonus + skillBonus;
        }
        else
        {
            // Только бонус за расу для спецрасов
            baseReward += bonuses.raceBonuses.Find(b => b.race == employee.race).bonus + skillBonus;
        }

        return baseReward;
    }

    public void InitializePanel() // Сделан публичным
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (currentClient != null && employeeIcon != null && expectedEmployeeText != null && expectedRaceText != null &&
            expectedBodyText != null && expectedBreastText != null && desiredServiceText != null && goldText != null &&
            employeePortrait != null && employeeNameText != null && employeeRaceText != null &&
            employeeBodyText != null && employeeBreastText != null && employeeServiceText != null && rewardText != null)
        {
            // Обновление текстовых полей клиента с учётом типа
            if (currentClient.clientType == 1)
            {
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = "Раса: None";
                expectedBodyText.text = "Тело: None";
                expectedBreastText.text = "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 2)
            {
                // Тип 2: Один из двух параметров (BodyType или BreastSize), остальные None
                bool useBodyType = currentClient.preferredBodyType != EmployeeDataSO.BodyType.None;
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = "Раса: None";
                expectedBodyText.text = useBodyType && currentClient.preferredBodyType != EmployeeDataSO.BodyType.None ? "Тело: " + currentClient.preferredBodyType.ToString() : "Тело: None";
                expectedBreastText.text = !useBodyType && currentClient.preferredBreastSize != EmployeeDataSO.BreastSize.None ? "Размер груди: " + currentClient.preferredBreastSize.ToString() : "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 3)
            {
                // Тип 3: Только Race, остальные None
                expectedEmployeeText.text = "Сотрудница: None";
                expectedRaceText.text = currentClient.preferredRace != EmployeeDataSO.Race.None ? "Раса: " + currentClient.preferredRace.ToString() : "Раса: None";
                expectedBodyText.text = "Тело: None";
                expectedBreastText.text = "Размер груди: None";
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            else if (currentClient.clientType == 4)
            {
                // Тип 4: Все поля (кроме DesiredServiceText) заполняются данными сотрудницы, если выбрана
                if (currentClient.SelectedEmployee != null && currentClient.SelectedEmployee.Data != null)
                {
                    EmployeeDataSO employeeData = currentClient.SelectedEmployee.Data;
                    expectedEmployeeText.text = "Сотрудница: " + employeeData.employeeName;
                    expectedRaceText.text = "Раса: " + employeeData.race.ToString();
                    expectedBodyText.text = "Тело: " + employeeData.bodyType.ToString();
                    expectedBreastText.text = "Размер груди: " + employeeData.breastSize.ToString();
                }
                else
                {
                    expectedEmployeeText.text = "Сотрудница: None";
                    expectedRaceText.text = "Раса: None";
                    expectedBodyText.text = "Тело: None";
                    expectedBreastText.text = "Размер груди: None";
                }
                desiredServiceText.text = "Услуга: " + (currentClient.RequestedService != null ? currentClient.RequestedService : "Нет");
            }
            goldText.text = "Золото: " + currentClient.clientGold;

            // Обновление EmployeeIcon на основе wantsSpecificEmployee
            if (currentClient.wantsSpecificEmployee && currentClient.SelectedEmployee != null && currentClient.SelectedEmployee.Data != null)
            {
                displayedEmployeeIcon = currentClient.SelectedEmployee.Data.portraitIcon;
            }
            else
            {
                displayedEmployeeIcon = Resources.Load<Sprite>("NoData_Square"); // Дефолтная иконка
            }
            employeeIcon.sprite = displayedEmployeeIcon; // Установка иконки

            // Заполнение EmployeePanel данными из SelectedEmployee, если он задан
            if (currentClient.SelectedEmployee != null && currentClient.SelectedEmployee.Data != null)
            {
                EmployeeDataSO employeeData = currentClient.SelectedEmployee.Data;
                employeePortrait.sprite = employeeData.portraitIcon;
                employeeNameText.text = "Имя: " + employeeData.employeeName;
                employeeRaceText.text = "Раса: " + employeeData.race.ToString();
                employeeBodyText.text = "Тип тела: " + employeeData.bodyType.ToString();
                employeeBreastText.text = "Размер груди: " + employeeData.breastSize.ToString();
                // Проверка наличия RequestedService в Skills
                if (currentClient.SelectedEmployee.Skills.ContainsKey(currentClient.RequestedService))
                {
                    int skillLevel = currentClient.SelectedEmployee.Skills[currentClient.RequestedService].level;
                    employeeServiceText.text = "Услуга (уровень): " + currentClient.RequestedService + " (" + skillLevel + ")";
                }
                else
                {
                    employeeServiceText.text = "Услуга (уровень): None";
                }
                rewardText.text = "Стоимость: " + CalculateReward(employeeData);
            }
            else
            {
                employeePortrait.sprite = Resources.Load<Sprite>("NoData_Square");
                employeeNameText.text = "Имя: None";
                employeeRaceText.text = "Раса: None";
                employeeBodyText.text = "Тип тела: None";
                employeeBreastText.text = "Размер груди: None";
                employeeServiceText.text = "Услуга (уровень): None";
                rewardText.text = "Стоимость: None";
            }

            Debug.Log($"Панель клиента {currentClient.clientName} и сотрудницы обновлена");
        }
        else
        {
            Debug.LogWarning("Одна или несколько ссылок на UI-элементы в ClientInteractionController равны null!");
        }
    }

    void OnWaitClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (currentClient != null)
        {
            currentClient.SendToChair();
            Time.timeScale = 1; // Снятие паузы перед уничтожением
            Destroy(gameObject); // Уничтожение панели
            Debug.Log($"Кнопка Ожидать нажата для клиента {currentClient.clientName}, клиент отправлен на стул, панель уничтожена");
        }
    }

    void OnAssignClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (currentClient != null && currentClient.SelectedEmployee != null)
        {
            // Проверка соответствия сотрудницы заказу
            EmployeeDataSO employeeData = currentClient.SelectedEmployee.Data;
            bool isMatch = CheckOrderMatch(employeeData);
            float reward = CalculateReward(employeeData);
            if (isMatch && currentClient.clientGold >= reward)
            {
                currentClient.AssignEmployee(); // Отправка клиента на услугу
                Time.timeScale = 1; // Снятие паузы перед уничтожением
                Destroy(gameObject); // Уничтожение панели
                Debug.Log($"Кнопка Назначить сотрудницу нажата для клиента {currentClient.clientName}, клиент отправлен на услугу, панель уничтожена");
            }
            else
            {
                currentClient.SetState(ClientData.ClientState.Exiting); // Клиент уходит
                Time.timeScale = 1; // Снятие паузы перед уничтожением
                Destroy(gameObject); // Уничтожение панели
                if (!isMatch)
                {
                    Debug.LogWarning($"Сотрудница {employeeData.employeeName} не соответствует заказу, клиент {currentClient.clientName} уходит");
                }
                else
                {
                    Debug.LogWarning($"У клиента {currentClient.clientName} недостаточно золота. Требуется: {reward}, доступно: {currentClient.clientGold}, клиент уходит");
                }
            }
        }
        else
        {
            Debug.LogWarning("Сотрудница не выбрана для назначения!");
        }
    }

    void OnCancelClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (currentClient != null)
        {
            Time.timeScale = 1; // Снятие паузы перед уничтожением
            Destroy(gameObject); // Уничтожение панели
            Debug.Log($"Кнопка Отмена нажата, панель уничтожена для клиента {currentClient.clientName}");
        }
    }

    void OnEmployeePortraitClick()
    {
        if (isDestroyed) return; // Проверка на уничтожение
        if (employeeListToSelectPanelPrefab != null)
        {
            Time.timeScale = 0; // Пауза игры
            GameObject panelInstance = Instantiate(employeeListToSelectPanelPrefab, transform.parent); // Используем родителя Canvas
            panelInstance.SetActive(true);
            EmployeeListControlPanel controller = panelInstance.GetComponent<EmployeeListControlPanel>();
            if (controller != null)
            {
                controller.Initialize(currentClient, this); // Передача клиента и контроллера
                Debug.Log($"Открыта панель выбора сотрудниц для клиента {currentClient.clientName}");
            }
            else
            {
                Debug.LogError("Компонент EmployeeListControlPanel не найден!");
            }
        }
        else
        {
            Debug.LogError("Префаб EmployeeListToSelectPanel не назначен!");
        }
    }

    void OnDestroy()
    {
        isDestroyed = true; // Установка флага уничтожения
        // Очистка событий для избежания утечек
        if (waitButton != null) waitButton.onClick.RemoveAllListeners();
        if (assignButton != null) assignButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        if (employeePortrait != null && employeePortrait.GetComponent<Button>() != null)
        {
            employeePortrait.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        Debug.Log("ClientInteractionController уничтожен, все слушатели событий очищены");
    }
}