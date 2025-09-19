using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityPanel : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private List<DistrictPanel> districtPanels;

    private void Awake()
    {
        if (buttonPanel == null || districtPanels == null)
        {
            Debug.LogError("Не все поля инициализированы в CityPanel.");
            return;
        }
    }

    private void Start()
    {
        UpdateDistrictPanelsVisibility(); // Проверка видимости панелей при старте
        gameObject.SetActive(false); // Деактивация после полной инициализации
    }

    private void OnEnable()
    {
        if (buttonPanel != null)
        {
            var confirmButton = buttonPanel.transform.Find("ConfirmButton")?.GetComponent<UnityEngine.UI.Button>();
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(ConfirmAssignments);
            }
            else
            {
                Debug.LogError("ConfirmButton не найден или не содержит компонент Button в buttonPanel.");
            }
            var cancelButton = buttonPanel.transform.Find("CancelButton")?.GetComponent<UnityEngine.UI.Button>();
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelAssignments);
            else
            {
                Debug.LogWarning("CancelButton не найден или не содержит компонент Button.");
            }
        }
    }

    private void OnDisable()
    {
        if (buttonPanel != null)
        {
            var confirmButton = buttonPanel.transform.Find("ConfirmButton")?.GetComponent<UnityEngine.UI.Button>();
            if (confirmButton != null) confirmButton.onClick.RemoveListener(ConfirmAssignments);
            var cancelButton = buttonPanel.transform.Find("CancelButton")?.GetComponent<UnityEngine.UI.Button>();
            if (cancelButton != null) cancelButton.onClick.RemoveListener(CancelAssignments);
        }
    }

    private void UpdateDistrictPanelsVisibility()
    {
        float globalPopularity = GameStateManager.Instance != null ? GameStateManager.Instance.Popularity : 0f;
        foreach (var districtPanel in districtPanels.Where(dp => dp != null && dp.GetComponent<DistrictData>() != null))
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            if (districtData != null && districtData.GetDistrictData() != null)
            {
                bool shouldBeActive = globalPopularity >= districtData.GetDistrictData().PopularityToUnlock;
                districtPanel.gameObject.SetActive(shouldBeActive);
                Debug.Log($"Район {districtData.GetDistrictData().DistrictName} видимость: {shouldBeActive}, Популярность: {globalPopularity} vs {districtData.GetDistrictData().PopularityToUnlock}");
            }
        }
    }

    private void ConfirmAssignments()
    {
        // Установка статуса Marketing и передача в activeEmployeeMap
        foreach (var districtPanel in districtPanels.Where(dp => dp != null && dp.GetComponent<DistrictData>() != null && dp.GetComponent<DistrictData>().GetDistrictData() != null))
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            var district = districtData.GetDistrictData();
            if (districtData.GetAssignedEmployee() != null)
            {
                DistrictManager.Instance.AssignEmployeeToDistrict(districtData.GetAssignedEmployee(), district);
                districtData.SetActiveEmployee(DistrictManager.Instance.GetActiveEmployee(district)); // Синхронизация с activeEmployeeMap
                districtData.SetAssignedEmployee(null);
                districtData.UpdateEmployeeImageInteractable(false);
                Debug.Log($"Назначена сотрудница {districtData.GetActiveEmployee()?.Data.employeeName} для района {district.DistrictName}.");
            }
        }

        // Логи для каждого района
        foreach (var districtPanel in districtPanels.Where(dp => dp != null && dp.GetComponent<DistrictData>() != null))
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            string employeeName = DistrictManager.Instance.GetActiveEmployee(districtData.GetDistrictData()) != null
                ? DistrictManager.Instance.GetActiveEmployee(districtData.GetDistrictData()).Data.employeeName
                : "None";
            Debug.Log($"Район {districtData.GetDistrictData().DistrictName} назначена сотрудница {employeeName}");
        }

        // Логирование содержимого marketingEmployees после закрытия
        string marketingNames = "Сотрудницы в marketingEmployees: ";
        foreach (var employee in EmployeeManager.Instance.MarketingEmployees)
        {
            marketingNames += employee?.Data.employeeName + ", ";
        }
        Debug.Log(marketingNames.TrimEnd(',', ' ') + (EmployeeManager.Instance.MarketingEmployees.Count == 0 ? " (пусто)" : ""));

        // Новый лог для availableEmployees
        string availableNames = "Список доступных сотрудниц для рекламы: ";
        foreach (var employee in EmployeeManager.Instance.AvailableEmployees)
        {
            availableNames += employee?.Data.employeeName + ", ";
        }
        Debug.Log(availableNames.TrimEnd(',', ' ') + (EmployeeManager.Instance.AvailableEmployees.Count == 0 ? " (пусто)" : ""));

        // Новый лог для marketingEmployees
        string marketingList = "Список назначенных сотрудниц для рекламы: ";
        foreach (var employee in EmployeeManager.Instance.MarketingEmployees)
        {
            marketingList += employee?.Data.employeeName + ", ";
        }
        Debug.Log(marketingList.TrimEnd(',', ' ') + (EmployeeManager.Instance.MarketingEmployees.Count == 0 ? " (пусто)" : ""));

        DistrictManager.Instance.ConfirmAssignments();
        DistrictManager.Instance.TransferPopularityToGameState();
        gameObject.SetActive(false);
        Debug.Log("Назначения подтверждены и панель закрыта.");
    }

    private void CancelAssignments()
    {
        foreach (var districtPanel in districtPanels.Where(dp => dp != null && dp.GetComponent<DistrictData>() != null && dp.GetComponent<DistrictData>().GetDistrictData() != null))
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            if (districtData.GetAssignedEmployee() != null)
            {
                districtData.ClearAssignment();
            }
        }
        gameObject.SetActive(false);
        Debug.Log("Назначения отменены и панель закрыта.");
    }
}