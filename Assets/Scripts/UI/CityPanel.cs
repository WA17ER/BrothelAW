using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityPanel : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private List<DistrictPanel> districtPanels;
    private Button confirmButton, cancelButton;

    void Awake()
    {
        if (buttonPanel == null || districtPanels == null)
        {
            Debug.LogError("Не все поля инициализированы в CityPanel.");
            return;
        }
    }

    void Start()
    {
        UpdateDistrictPanelsVisibility();
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        confirmButton ??= buttonPanel.transform.Find("ConfirmButton")?.GetComponent<Button>();
        cancelButton ??= buttonPanel.transform.Find("CancelButton")?.GetComponent<Button>();
        confirmButton?.onClick.AddListener(ConfirmAssignments);
        cancelButton?.onClick.AddListener(CancelAssignments);
    }

    void OnDisable()
    {
        confirmButton?.onClick.RemoveListener(ConfirmAssignments);
        cancelButton?.onClick.RemoveListener(CancelAssignments);
    }

    void UpdateDistrictPanelsVisibility()
    {
        float globalPopularity = GameStateManager.Instance?.Popularity ?? 0f;
        foreach (var districtPanel in districtPanels.Where(dp => dp?.GetComponent<DistrictData>() != null))
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            if (districtData?.GetDistrictData() is DistrictDataSO district)
            {
                bool shouldBeActive = globalPopularity >= district.PopularityToUnlock;
                districtPanel.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    void ConfirmAssignments()
    {
        var validPanels = districtPanels.Where(dp => dp?.GetComponent<DistrictData>() != null && dp.GetComponent<DistrictData>().GetDistrictData() != null);
        foreach (var districtPanel in validPanels)
        {
            var districtData = districtPanel.GetComponent<DistrictData>();
            var district = districtData.GetDistrictData();
            if (districtData.GetAssignedEmployee() != null)
            {
                DistrictManager.Instance.AssignEmployeeToDistrict(districtData.GetAssignedEmployee(), district);
                districtData.SetActiveEmployee(DistrictManager.Instance.GetActiveEmployee(district));
                districtData.SetAssignedEmployee(null);
                districtData.UpdateEmployeeImageInteractable(false);
            }
        }
        LogDistrictAndEmployeeLists(validPanels);
        DistrictManager.Instance.ConfirmAssignments();
        DistrictManager.Instance.TransferPopularityToGameState();
        gameObject.SetActive(false);
    }

    void LogDistrictAndEmployeeLists(IEnumerable<DistrictPanel> panels)
    {
        var assignments = panels.Select(dp =>
        {
            var districtData = dp.GetComponent<DistrictData>();
            var employeeName = DistrictManager.Instance.GetActiveEmployee(districtData.GetDistrictData())?.Data.employeeName ?? "None";
            return $"Район {districtData.GetDistrictData().DistrictName} назначена сотрудница {employeeName}";
        });
        Debug.Log(string.Join("\n", assignments));
        var marketingNames = string.Join(", ", EmployeeManager.Instance.MarketingEmployees.Select(e => e?.Data.employeeName ?? "null")) + (EmployeeManager.Instance.MarketingEmployees.Count == 0 ? " (пусто)" : "");
        var availableNames = string.Join(", ", EmployeeManager.Instance.AvailableEmployees.Select(e => e?.Data.employeeName ?? "null")) + (EmployeeManager.Instance.AvailableEmployees.Count == 0 ? " (пусто)" : "");
        Debug.Log($"Сотрудницы в marketingEmployees: {marketingNames}");
        Debug.Log($"Список доступных сотрудниц для рекламы: {availableNames}");
    }

    void CancelAssignments()
    {
        var validPanels = districtPanels.Where(dp => dp?.GetComponent<DistrictData>() != null && dp.GetComponent<DistrictData>().GetDistrictData() != null);
        foreach (var districtPanel in validPanels)
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