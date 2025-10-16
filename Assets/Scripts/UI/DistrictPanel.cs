using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DistrictPanel : MonoBehaviour
{
    [SerializeField] private Image employeeImage;
    [SerializeField] private TMP_Text districtNameText;
    [SerializeField] private EmployeeMarketingPanel employeeMarketingPanel;

    void Start()
    {
        if (employeeImage == null || districtNameText == null || employeeMarketingPanel == null) return;
        var districtData = GetComponent<DistrictData>();
        if (districtData?.GetDistrictData() is DistrictDataSO data)
        {
            districtNameText.text = data.DistrictName ?? "Unnamed";
            var button = employeeImage.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => employeeMarketingPanel.OpenPanel(districtData));
        }
    }

    private void OnEmployeeImageClicked()
    {
        if (employeeMarketingPanel != null)
        {
            employeeMarketingPanel.OpenPanel(GetComponent<DistrictData>());
        }
    }
}