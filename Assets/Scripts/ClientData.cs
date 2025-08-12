using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    public string clienName;
    public string RequestedService { get; set; }
    public SicknessSO ActiveSick { get; set; }
    public Employee SpecificEmployee { get; set; }
    public List<Employee> availableEmployees = new List<Employee>();

    private void Awake()
    {
        InitializeClientPreferences();
    }

    public void InitializeClientPreferences()
    {
        RequestedService = EmployeeManager.Instance.GetRandomService();
        availableEmployees.AddRange(EmployeeManager.Instance.AvailableEmployees);
        if (Random.value < 0.3f)
        {
            ActiveSick = null; // Placeholder until sickness data is available
        }
        Debug.Log($"Клиент {name} инициализирован: Услуга {RequestedService}, Болезнь {ActiveSick?.SickName ?? "none"}.");
    }
}