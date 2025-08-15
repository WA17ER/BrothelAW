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
    public ClientDataSO ClientDataSO { get => clientDataSO; }
    public ClientState State { get; private set; }
    public float waitTime { get; set; } // Синхронизировано с ClientManager
    public int clientType { get; private set; }
    public EmployeeDataSO.BreastSize preferredBreastSize { get; private set; }
    public EmployeeDataSO.BodyType preferredBodyType { get; private set; }
    public EmployeeDataSO.Race preferredRace { get; private set; }
    public float clientGold { get; private set; }
    private const float WaitingTime = 10f;
    private const float OnChairTime = 20f;
    public Transform targetChair { get; private set; }
    private static List<Transform> occupiedChairs = new List<Transform>();

    [SerializeField] private UnityEvent<ClientData> onStateChanged;
    public UnityEvent<ClientData> OnStateChanged => onStateChanged;

    private void Awake()
    {
        State = ClientState.MovingToRegister;
        waitTime = WaitingTime; // Начальное значение
        selectedEmployee = null;
    }

    public void InitializeClientPreferences(int type)
    {
        clientType = type;
        Employee employee = EmployeeManager.Instance.AvailableEmployees.FirstOrDefault();

        if (clientDataSO != null)
        {
            float step = clientDataSO.goldStep;
            int rangeMin = Mathf.CeilToInt(clientDataSO.minGold / step);
            int rangeMax = Mathf.FloorToInt(clientDataSO.maxGold / step);
            clientGold = Random.Range(rangeMin, rangeMax + 1) * step;

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
            clientGold = 100f; // Fallback value
            ActiveSick = null;
            Debug.LogWarning($"ClientDataSO не задано для клиента {clientName}. Использовано золото по умолчанию: {clientGold}, Болезнь: none");
        }

        if (employee != null)
        {
            bool isSpecialRace = employee.Race == EmployeeDataSO.Race.Допельгангер || employee.Race == EmployeeDataSO.Race.Суккуб || employee.Race == EmployeeDataSO.Race.Ангел;
            if (clientType == 1)
            {
                RequestedService = isSpecialRace ? EmployeeManager.Instance.GetRandomService() : employee.Skills.Keys.ElementAt(Random.Range(0, employee.Skills.Count));
                preferredBreastSize = EmployeeDataSO.BreastSize.A;
                preferredBodyType = EmployeeDataSO.BodyType.Обычное;
                preferredRace = EmployeeDataSO.Race.Человек;
                Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: не заданы");
            }
            else if (clientType == 2)
            {
                RequestedService = isSpecialRace ? EmployeeManager.Instance.GetRandomService() : employee.Skills.Keys.ElementAt(Random.Range(0, employee.Skills.Count));
                if (Random.value < 0.5f)
                {
                    preferredBreastSize = employee.BreastSize;
                    preferredBodyType = EmployeeDataSO.BodyType.Обычное;
                    Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован на основе сотрудницы {employee.Data.employeeName}: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: BreastSize = {preferredBreastSize}");
                }
                else
                {
                    preferredBreastSize = EmployeeDataSO.BreastSize.A;
                    preferredBodyType = employee.BodyType;
                    Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован на основе сотрудницы {employee.Data.employeeName}: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: BodyType = {preferredBodyType}");
                }
                preferredRace = EmployeeDataSO.Race.Человек;
            }
            else if (clientType == 3)
            {
                RequestedService = isSpecialRace ? EmployeeManager.Instance.GetRandomService() : employee.Skills.Keys.ElementAt(Random.Range(0, employee.Skills.Count));
                preferredBreastSize = EmployeeDataSO.BreastSize.A;
                preferredBodyType = EmployeeDataSO.BodyType.Обычное;
                preferredRace = employee.Race;
                Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован на основе сотрудницы {employee.Data.employeeName}: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: Race = {preferredRace}");
            }
            else if (clientType == 4)
            {
                SpecificEmployee = employee;
                RequestedService = EmployeeManager.Instance.GetRandomService();
                preferredBreastSize = EmployeeDataSO.BreastSize.A;
                preferredBodyType = EmployeeDataSO.BodyType.Обычное;
                preferredRace = EmployeeDataSO.Race.Человек;
                Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован: Конкретная сотрудница = {employee.Data.employeeName}, Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: не заданы");
            }
        }
        else
        {
            RequestedService = EmployeeManager.Instance.GetRandomService();
            preferredBreastSize = EmployeeDataSO.BreastSize.A;
            preferredBodyType = EmployeeDataSO.BodyType.Обычное;
            preferredRace = EmployeeDataSO.Race.Человек;
            Debug.Log($"Клиент {clientName} (Тип {clientType}) инициализирован без сотрудниц: Услуга = {RequestedService}, Золото = {clientGold}, Болезнь = {ActiveSick?.SickName ?? "none"}, Предпочтения: не заданы");
        }
    }

    public void SetState(ClientState newState)
    {
        State = newState;
        Debug.Log($"Состояние клиента {clientName} изменено на {newState}");
        onStateChanged?.Invoke(this);
    }

    public void SetTargetChair(Transform chairBottomPoint)
    {
        targetChair = chairBottomPoint;
        Debug.Log($"Клиент {clientName} получил цель BottomPoint стула: {chairBottomPoint?.parent.name} на позиции {chairBottomPoint?.position}");
        SetState(ClientData.ClientState.OnOccupyChair);
    }

    [ContextMenu("Ожидать")]
    public void SendToChair()
    {
        if (State != ClientData.ClientState.Waiting)
        {
            Debug.LogWarning($"Клиент {clientName} не в состоянии Waiting для отправки на стул.");
            return;
        }

        Transform chair = GameManager.Instance.Chairs.FirstOrDefault(c => c.gameObject.activeInHierarchy && !occupiedChairs.Contains(c.Find("BottomPoint")));
        if (chair == null)
        {
            Debug.LogWarning($"Нет доступных стульев для клиента {clientName}.");
            return;
        }

        Transform bottomPoint = chair.Find("BottomPoint");
        if (bottomPoint == null)
        {
            Debug.LogWarning($"BottomPoint не найден для стула {chair.name}.");
            return;
        }

        occupiedChairs.Add(bottomPoint);
        Debug.Log($"Выбран BottomPoint стула {chair.name} для клиента {clientName}.");
        SetTargetChair(bottomPoint); // Используем SetTargetChair для установки состояния OnOccupyChair
    }

    [ContextMenu("Назначить сотрудницу")]
    public void AssignEmployee()
    {
        if (State != ClientData.ClientState.Waiting && State != ClientData.ClientState.OnChair)
        {
            Debug.LogWarning($"Клиент {clientName} не в состоянии Waiting или OnChair для назначения сотрудницы.");
            return;
        }

        if (selectedEmployee == null)
        {
            Debug.LogWarning($"Сотрудница не выбрана для клиента {clientName}.");
            return;
        }

        if (!EmployeeManager.Instance.AvailableEmployees.Contains(selectedEmployee))
        {
            Debug.LogWarning($"Сотрудница {selectedEmployee.Data.employeeName} не доступна для клиента {clientName}.");
            selectedEmployee = null;
            return;
        }

        bool isSpecialRace = selectedEmployee.Race == EmployeeDataSO.Race.Допельгангер || selectedEmployee.Race == EmployeeDataSO.Race.Суккуб || selectedEmployee.Race == EmployeeDataSO.Race.Ангел;

        if (!isSpecialRace)
        {
            if (!selectedEmployee.Skills.ContainsKey(RequestedService) && clientType != 4)
            {
                Debug.LogWarning($"Клиент {clientName} ожидал услугу {RequestedService}, сотрудница {selectedEmployee.Data.employeeName} не поддерживает эту услугу.");
                return;
            }
            if (clientType == 2 && selectedEmployee.BreastSize != preferredBreastSize && selectedEmployee.BodyType != preferredBodyType)
            {
                Debug.LogWarning($"Клиент {clientName} ожидал BreastSize: {preferredBreastSize} или BodyType: {preferredBodyType}, сотрудница {selectedEmployee.Data.employeeName} имеет BreastSize: {selectedEmployee.BreastSize}, BodyType: {selectedEmployee.BodyType}.");
                return;
            }
            if (clientType == 3 && selectedEmployee.Race != preferredRace)
            {
                Debug.LogWarning($"Клиент {clientName} ожидал Race: {preferredRace}, сотрудница {selectedEmployee.Data.employeeName} имеет Race: {selectedEmployee.Race}.");
                return;
            }
            if (clientType == 4 && selectedEmployee != SpecificEmployee)
            {
                Debug.LogWarning($"Клиент {clientName} ожидал сотрудницу {SpecificEmployee.Data.employeeName}, выбрана {selectedEmployee.Data.employeeName}.");
                return;
            }
        }

        Debug.Log($"Проверка соответствия для клиента {clientName} (Тип {clientType}): Услуга = {RequestedService}, BreastSize = {selectedEmployee.BreastSize}/{preferredBreastSize}, BodyType = {selectedEmployee.BodyType}/{preferredBodyType}, Race = {selectedEmployee.Race}/{preferredRace}, Выбрана сотрудница: {selectedEmployee.Data.employeeName}");
        float reward = EmployeeManager.Instance.AssignEmployee(this, selectedEmployee);
        if (clientGold >= reward)
        {
            clientGold -= reward;
            GameManager.Instance.AddGold(reward);
            SpecificEmployee = selectedEmployee; // Установить SpecificEmployee для передачи в CompleteService
            ClearChair();
            SetState(ClientData.ClientState.MovingToService);
            Debug.Log($"Заказ успешен: Награда = {reward}, Остаток золота клиента = {clientGold}.");
        }
        else
        {
            Debug.LogWarning($"У клиента {clientName} недостаточно золота. Требуется: {reward}, доступно: {clientGold}.");
        }
    }

    public void ClearChair()
    {
        if (targetChair != null)
        {
            occupiedChairs.Remove(targetChair);
            Debug.Log($"Стул {targetChair.parent.name} освобождён клиентом {clientName}.");
            targetChair = null;
        }
    }
}