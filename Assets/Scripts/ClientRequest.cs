using System.Collections.Generic;
using UnityEngine;

public class ClientRequest : MonoBehaviour
{
    [SerializeField] private List<Employee> availableEmployees; // Для теста

    private string requestedService;
    private Dictionary<string, string> preferences = new Dictionary<string, string>(); // race, bodyType, breastSize
    private Employee specificEmployee;

    private CustomerMovement customerMovement;

    private void Awake()
    {
        customerMovement = GetComponent<CustomerMovement>();
        if (customerMovement == null)
        {
            Debug.LogError("ClientRequest: CustomerMovement не найден.");
            enabled = false;
        }
    }

    private void Start()
    {
        GenerateRequest();
    }

    private void GenerateRequest()
    {
        // Пример генерации, позже расширить по уровню
        requestedService = "Missionary"; // Random из пула
        // preferences.Add("race", "Elf"); etc.
        // specificEmployee = null or random
        Debug.Log($"Запрос клиента: Услуга - {requestedService}");
    }

    public bool IsMatch(Employee employee)
    {
        if (!employee.BaseSkills.Contains(requestedService)) return false;

        if (preferences.TryGetValue("race", out string reqRace) && employee.Race != reqRace) return false;
        if (preferences.TryGetValue("bodyType", out string reqBody) && !employee.BodyTypes.Contains(reqBody)) return false;
        if (preferences.TryGetValue("breastSize", out string reqBreast) && employee.BreastSize.ToString() != reqBreast) return false;

        if (specificEmployee != null && employee != specificEmployee) return false;

        return true;
    }

    [ContextMenu("Выбрать сотрудницу 0")]
    public void SelectEmployee0()
    {
        if (availableEmployees.Count > 0) SelectEmployee(availableEmployees[0]);
    }

    // Добавить больше для теста

    public void SelectEmployee(Employee employee)
    {
        var state = customerMovement.CurrentState;
        if (state == CustomerMovement.CustomerState.Waiting || state == CustomerMovement.CustomerState.OnChair)
        {
            if (IsMatch(employee))
            {
                Debug.Log("Запрос совпадает, отправка на услугу.");
                customerMovement.SendToService();
                // +экономика позже
            }
            else
            {
                Debug.Log("Запрос не совпадает, клиент уходит.");
                customerMovement.ForceExit();
            }
        }
        else
        {
            Debug.LogError("Нельзя выбрать: Недопустимое состояние.");
        }
    }
}