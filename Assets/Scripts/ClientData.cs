using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class ClientData : MonoBehaviour
{
    public enum ClientState
    {
        MovingToRegister,
        Waiting,
        OnOccupyChair,
        OnChair,
        MovingToService,
        Servicing,
        Exiting
    }
    public string clientName;
    public string RequestedService { get; set; }
    public SicknessSO ActiveSick { get; set; }
    public Employee SpecificEmployee { get; set; }
    [SerializeField] private Employee selectedEmployee;
    public Employee SelectedEmployee { get => selectedEmployee; set => selectedEmployee = value; }
    [SerializeField] private ClientDataSO clientDataSO;
    public ClientDataSO ClientDataSO { get => clientDataSO; set => clientDataSO = value; }
    public ClientState State { get; private set; }
    public float waitTime { get; set; } // Синхронизировано с ClientManager
    public int clientType { get; private set; }
    public EmployeeDataSO.BreastSize preferredBreastSize { get; private set; }
    public EmployeeDataSO.BodyType preferredBodyType { get; private set; }
    public EmployeeDataSO.Race preferredRace { get; private set; }
    public float clientGold { get; private set; }
    public float popularityGain { get; private set; }
    public int ClientId { get; private set; } // Уникальный ID клиента
    private const float WaitingTime = 10f;
    private const float OnChairTime = 20f;
    public Transform targetChair { get; private set; }
    private static List<Transform> occupiedChairs = new List<Transform>();
    [SerializeField] private UnityEvent<ClientData> onStateChanged;
    public UnityEvent<ClientData> OnStateChanged => onStateChanged;
    public bool wantsSpecificEmployee { get; private set; } // Новый параметр
    private void Awake()
    {
        State = ClientState.MovingToRegister;
        waitTime = WaitingTime; // Начальное значение
        selectedEmployee = null;
        wantsSpecificEmployee = false; // Дефолтное значение
    }
    public void InitializeClientPreferences()
    {
        if (clientDataSO != null)
        {
            float step = clientDataSO.goldStep;
            int rangeMin = Mathf.CeilToInt(clientDataSO.minGold / step);
            int rangeMax = Mathf.FloorToInt(clientDataSO.maxGold / step);
            clientGold = Random.Range(rangeMin, rangeMax + 1) * step;
            popularityGain = clientDataSO.popularityGain;
            if (clientDataSO.possibleSicknesses != null && clientDataSO.possibleSicknesses.Count > 0 && Random.Range(0f, 100f) < clientDataSO.sickChance)
            {
                ActiveSick = clientDataSO.possibleSicknesses[Random.Range(0, clientDataSO.possibleSicknesses.Count)];
            }
            else
            {
                ActiveSick = null;
            }
        }
        else
        {
            clientGold = 100f;
            ActiveSick = null;
            Debug.LogWarning($"ClientDataSO не задано для клиента {clientName}. Использовано золото по умолчанию: {clientGold}, Болезнь: none");
        }

        // Сброс предпочтений перед инициализацией
        RequestedService = null;
        preferredRace = EmployeeDataSO.Race.None;
        preferredBodyType = EmployeeDataSO.BodyType.None;
        preferredBreastSize = EmployeeDataSO.BreastSize.None;
        wantsSpecificEmployee = false;
        SpecificEmployee = null;

        // Услуга из глобального списка
        RequestedService = EmployeeManager.Instance.GetRandomService();

        // Выбор случайного RaceVarietySO
        if (clientDataSO.varietyPreferences.Count > 0)
        {
            RaceVarietySO selectedVariety = clientDataSO.varietyPreferences[Random.Range(0, clientDataSO.varietyPreferences.Count)];

            // Race
            if (clientDataSO.RaceChance > Random.value)
            {
                preferredRace = selectedVariety.raceName;
            }

            // BodyType
            if (clientDataSO.BodyTypeChance > Random.value && selectedVariety.possibleBodyTypes.Count > 0)
            {
                preferredBodyType = selectedVariety.possibleBodyTypes[Random.Range(0, selectedVariety.possibleBodyTypes.Count)];
            }

            // BreastSize
            if (clientDataSO.BreastSizeChance > Random.value && selectedVariety.possibleBreastSizes.Count > 0)
            {
                preferredBreastSize = selectedVariety.possibleBreastSizes[Random.Range(0, selectedVariety.possibleBreastSizes.Count)];
            }
        }

        Debug.Log($"Клиент {clientName} инициализирован: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: Race = {preferredRace}, BodyType = {preferredBodyType}, BreastSize = {preferredBreastSize}");
    }
    public void SetState(ClientState newState)
    {
        State = newState;
        Debug.Log($"Состояние клиента {clientName} (ID: {ClientId}) изменено на {newState}");
        onStateChanged?.Invoke(this);
    }
    public void SetTargetChair(Transform chairBottomPoint)
    {
        targetChair = chairBottomPoint;
        Debug.Log($"Клиент {clientName} (ID: {ClientId}) получил цель BottomPoint стула: {chairBottomPoint?.parent.name} на позиции {chairBottomPoint?.position}");
        SetState(ClientData.ClientState.OnOccupyChair);
    }
    [ContextMenu("Ожидать")]
    public void SendToChair()
    {
        if (State != ClientData.ClientState.Waiting)
        {
            Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) не в состоянии Waiting для отправки на стул.");
            return;
        }
        Transform chair = GameManager.Instance.Chairs.FirstOrDefault(c => c.gameObject.activeInHierarchy && !occupiedChairs.Contains(c.Find("BottomPoint")));
        if (chair == null)
        {
            Debug.LogWarning($"Нет доступных стульев для клиента {clientName} (ID: {ClientId}).");
            return;
        }
        Transform bottomPoint = chair.Find("BottomPoint");
        if (bottomPoint == null)
        {
            Debug.LogWarning($"BottomPoint не найден для стула {chair.name}.");
            return;
        }
        occupiedChairs.Add(bottomPoint);
        Debug.Log($"Выбран BottomPoint стула {chair.name} для клиента {clientName} (ID: {ClientId}).");
        SetTargetChair(bottomPoint); // Используем SetTargetChair для установки состояния OnOccupyChair
    }
    [ContextMenu("Назначить сотрудницу")]
    [ContextMenu("Назначить сотрудницу")]
    public void AssignEmployee()
    {
        if (State != ClientData.ClientState.Waiting && State != ClientData.ClientState.OnChair)
        {
            Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) не в состоянии Waiting или OnChair для назначения сотрудницы.");
            return;
        }
        if (selectedEmployee == null)
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {clientName} (ID: {ClientId}).");
            return;
        }
        if (!EmployeeManager.Instance.AvailableEmployees.Contains(selectedEmployee))
        {
            Debug.LogWarning($"Сотрудница {selectedEmployee.Data.employeeName} не доступна для клиента {clientName} (ID: {ClientId}).");
            selectedEmployee = null;
            return;
        }
        bool isSpecialRace = selectedEmployee.Race == EmployeeDataSO.Race.Допельгангер || selectedEmployee.Race == EmployeeDataSO.Race.Суккуб || selectedEmployee.Race == EmployeeDataSO.Race.Ангел;
        if (!isSpecialRace)
        {
            if (!selectedEmployee.Skills.ContainsKey(RequestedService))
            {
                Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) ожидал услугу {RequestedService}, сотрудница {selectedEmployee.Data.employeeName} не поддерживает эту услугу.");
                return;
            }
            if (preferredRace != EmployeeDataSO.Race.None && selectedEmployee.Race != preferredRace)
            {
                Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) ожидал Race: {preferredRace}, сотрудница {selectedEmployee.Data.employeeName} имеет Race: {selectedEmployee.Race}.");
                return;
            }
            if (preferredBodyType != EmployeeDataSO.BodyType.None && selectedEmployee.BodyType != preferredBodyType)
            {
                Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) ожидал BodyType: {preferredBodyType}, сотрудница {selectedEmployee.Data.employeeName} имеет BodyType: {selectedEmployee.BodyType}.");
                return;
            }
            if (preferredBreastSize != EmployeeDataSO.BreastSize.None && selectedEmployee.BreastSize != preferredBreastSize)
            {
                Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) ожидал BreastSize: {preferredBreastSize}, сотрудница {selectedEmployee.Data.employeeName} имеет BreastSize: {selectedEmployee.BreastSize}.");
                return;
            }
        }
        else
        {
            if (!selectedEmployee.Skills.ContainsKey(RequestedService))
            {
                Debug.LogWarning($"Клиент {clientName} (ID: {ClientId}) ожидал услугу {RequestedService}, сотрудница {selectedEmployee.Data.employeeName} не поддерживает эту услугу.");
                return;
            }
        }
        Debug.Log($"Проверка соответствия для клиента {clientName} (ID: {ClientId}): Услуга = {RequestedService}, BreastSize = {selectedEmployee.BreastSize}/{preferredBreastSize}, BodyType = {selectedEmployee.BodyType}/{preferredBodyType}, Race = {selectedEmployee.Race}/{preferredRace}, Выбрана сотрудница: {selectedEmployee.Data.employeeName}");
        float reward = EmployeeManager.Instance.AssignEmployee(this, selectedEmployee);
        if (clientGold >= reward)
        {
            clientGold -= reward;
            GameManager.Instance.AddGold(reward);
            SpecificEmployee = selectedEmployee; // Установить SpecificEmployee для передачи в CompleteService
            ClearChair();
            SetState(ClientData.ClientState.MovingToService);
            Debug.Log($"Заказ успешен для клиента {clientName} (ID: {ClientId}): Награда = {reward}, Остаток золота клиента = {clientGold}.");
        }
        else
        {
            Debug.LogWarning($"У клиента {clientName} (ID: {ClientId}) недостаточно золота. Требуется: {reward}, доступно: {clientGold}.");
        }
    }
    public void ClearChair()
    {
        if (targetChair != null)
        {
            occupiedChairs.Remove(targetChair);
            Debug.Log($"Стул {targetChair.parent.name} освобождён клиентом {clientName} (ID: {ClientId}).");
            targetChair = null;
        }
    }
    // Метод для установки ClientId (вызывается из SpawnHandler)
    public void SetClientId(int id)
    {
        ClientId = id;
    }
}