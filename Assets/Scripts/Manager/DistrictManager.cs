using System.Collections.Generic;
using UnityEngine;

public class DistrictManager : MonoBehaviour
{
    public static DistrictManager Instance { get; private set; }
    [SerializeField] private List<DistrictDataSO> districts;
    private Dictionary<DistrictDataSO, Employee> activeEmployeeMap = new Dictionary<DistrictDataSO, Employee>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        foreach (var district in districts)
        {
            district.EmployeePreferences.InitializePreferences(); // Инициализация предпочтений
            if (!activeEmployeeMap.ContainsKey(district))
            {
                activeEmployeeMap[district] = null;
            }
        }
    }

    public void AssignEmployeeToDistrict(Employee employee, DistrictDataSO district)
    {
        if (employee == null || district == null) return;
        if (employee.GetState() != Employee.EmployeeState.Available)
        {
            Debug.LogWarning($"Сотрудница {employee.Data.employeeName} не доступна для назначения в район {district.DistrictName}.");
            return;
        }
        employee.SetState(Employee.EmployeeState.Marketing);
        EmployeeManager.Instance.MoveEmployeeToList(employee);
        activeEmployeeMap[district] = employee;
        Debug.Log($"Сотрудница {employee.Data.employeeName} назначена на рекламу в район {district.DistrictName}.");
    }

    public void ConfirmAssignments()
    {
        foreach (var district in districts)
        {
            if (activeEmployeeMap.ContainsKey(district) && activeEmployeeMap[district] != null)
            {
                Debug.Log($"Подтверждено назначение для района {district.DistrictName}.");
            }
        }
    }

    public float CalculatePopularityGain()
    {
        float totalGain = 0f;
        foreach (var district in districts)
        {
            float districtGain = 0f;
            foreach (var employee in EmployeeManager.Instance.MarketingEmployees)
            {
                if (MatchesPreferences(employee, district) && activeEmployeeMap[district] != null)
                {
                    float gain = CalculateEmployeeEffectiveness(employee, district);
                    districtGain += gain;
                    Debug.Log($"Прирост популярности от {employee.Data.employeeName} в {district.DistrictName}: {gain}");
                }
            }
            district.PreliminaryPopularity = districtGain; // Устанавливаем прирост без коэффициентов
            totalGain += districtGain;
        }
        return totalGain; // Возвращаем общий прирост
    }

    public void ApplyPreliminaryPopularity()
    {
        foreach (var district in districts)
        {
            district.DistrictPopularity += district.PreliminaryPopularity;
            Debug.Log($"Применена предварительная популярность для {district.DistrictName}: +{district.PreliminaryPopularity}, итого {district.DistrictPopularity}");
            district.PreliminaryPopularity = 0; // Сбрасываем после применения
        }
    }

    private bool MatchesPreferences(Employee employee, DistrictDataSO district)
    {
        var prefs = district.EmployeePreferences;
        bool matches = prefs.ChestSize.Exists(p => p.Size == employee.Data.breastSize) &&
                      prefs.BodyType.Exists(p => p.Type == employee.Data.bodyType) &&
                      prefs.RacePreference.Exists(p => p.Race == employee.Data.race);
        return matches;
    }

    private float CalculateEmployeeEffectiveness(Employee employee, DistrictDataSO district)
    {
        var prefs = district.EmployeePreferences;
        float effectiveness = prefs.ChestSize.Find(p => p.Size == employee.Data.breastSize).Value +
                             prefs.BodyType.Find(p => p.Type == employee.Data.bodyType).Value +
                             prefs.RacePreference.Find(p => p.Race == employee.Data.race).Value;
        return effectiveness; // Убираем коэффициент 0.1
    }

    public void UpdateDistrictPopularity()
    {
        foreach (var district in districts)
        {
            // Логика обновления популярности (здесь можно добавить ограничения)
            Debug.Log($"Популярность района {district.DistrictName} обновлена до {district.DistrictPopularity}.");
        }
    }

    public void UpdateClientTypes(GameManager gameManager)
    {
        foreach (var district in districts)
        {
            foreach (var clientTypeWithMultiplier in district.ClientTypes)
            {
                var clientType = clientTypeWithMultiplier.ClientType;
                int clientTypeId = gameManager.GetClientTypeId(clientType);
                if (clientTypeId > 0 && gameManager.ExtraVisitors.ContainsKey(clientTypeId))
                {
                    int additionalClients = Mathf.FloorToInt(district.DistrictPopularity / 50);
                    gameManager.ExtraVisitors[clientTypeId] = Mathf.Min(gameManager.ExtraVisitors[clientTypeId] + additionalClients, gameManager.GetMaxClientsForType(clientTypeId));
                    Debug.Log($"Добавлено {additionalClients} клиентов типа {clientTypeId} из района {district.DistrictName}.");
                }
            }
        }
    }

    public List<DistrictDataSO> GetDistricts()
    {
        return districts;
    }

    public void CalculateTotalPreliminaryPopularity()
    {
        float total = 0f;
        foreach (var district in districts)
        {
            total += district.PreliminaryPopularity;
            Debug.Log($"Суммирование PreliminaryPopularity для {district.DistrictName}: {district.PreliminaryPopularity}, текущий total: {total}");
        }
        Debug.Log($"Общая предварительная популярность: {total}");
    }

    public void TransferPopularityToGameState()
    {
        CalculateTotalPreliminaryPopularity();
        float totalPopularity = 0f;
        foreach (var district in districts)
        {
            totalPopularity += district.PreliminaryPopularity;
        }
        GameStateManager.Instance.SetTemporaryPopularity(totalPopularity);
        Debug.Log($"Передана временная популярность в GameStateManager: {totalPopularity}");
    }

    [ContextMenu("ApplyPreliminaryPopularityOnly")]
    public void ApplyPreliminaryPopularityOnly()
    {
        foreach (var district in districts)
        {
            if (activeEmployeeMap.ContainsKey(district) && activeEmployeeMap[district] != null)
            {
                float totalPop = district.PreliminaryPopularity;
                district.DistrictPopularity += totalPop;
                Debug.Log($"Применена популярность для {district.DistrictName}: +{totalPop}, итого {district.DistrictPopularity}");
            }
        }
    }

    [ContextMenu("ClearActiveEmployees")]
    public void ClearActiveEmployees()
    {
        foreach (var district in districts)
        {
            if (activeEmployeeMap.ContainsKey(district) && activeEmployeeMap[district] != null)
            {
                activeEmployeeMap[district].SetState(Employee.EmployeeState.Available);
                EmployeeManager.Instance.MoveEmployeeToList(activeEmployeeMap[district]);
                activeEmployeeMap[district] = null;
                Debug.Log($"Очищен activeEmployee и переведена в Available для {district.DistrictName}.");
            }
            district.PreliminaryPopularity = 0;
        }
    }

    [ContextMenu("GenerateDistrictExtraVisitors")]
    public void GenerateDistrictExtraVisitors()
    {
        var tempVisitors = new List<ClientDataSO>();
        foreach (var district in districts)
        {
            int baseClients = Mathf.FloorToInt(district.DistrictPopularity / 10); // Базовое количество клиентов за 10 популярности
            foreach (var clientTypeWithMultiplier in district.ClientTypes)
            {
                var clientType = clientTypeWithMultiplier.ClientType;
                int multiplier = clientTypeWithMultiplier.Multiplier;
                int clientsToAdd = Mathf.FloorToInt(district.DistrictPopularity / multiplier); // Количество клиентов для каждого типа
                for (int j = 0; j < clientsToAdd; j++)
                {
                    tempVisitors.Add(clientType);
                }
                Debug.Log($"Для района {district.DistrictName} добавлено {clientsToAdd} клиентов типа {clientType.name} (популярность: {district.DistrictPopularity}, множитель: {multiplier}).");
            }
        }
        Debug.Log($"Список tempVisitors заполнен, общее количество: {tempVisitors.Count}");
    }

    public Employee GetActiveEmployee(DistrictDataSO district)
    {
        return activeEmployeeMap.ContainsKey(district) ? activeEmployeeMap[district] : null;
    }
}