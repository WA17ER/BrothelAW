using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DistrictPanel : MonoBehaviour
{
    [SerializeField] private Image employeeImage;
    [SerializeField] private TMP_Text districtNameText;
    [SerializeField] private EmployeeMarketingPanel employeeMarketingPanel;

    private void Start()
    {
        if (employeeImage == null || districtNameText == null || employeeMarketingPanel == null)
        {
            Debug.LogError("Не все поля инициализированы в DistrictPanel.");
            return;
        }
        DistrictData districtData = GetComponent<DistrictData>();
        if (districtData != null)
        {
            districtNameText.text = districtData.GetDistrictData().DistrictName;
            var button = employeeImage.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnEmployeeImageClicked);
            }
            else
            {
                Debug.LogError("Компонент Button не найден на employeeImage в DistrictPanel.");
            }
        }
        else
        {
            Debug.LogError("DistrictData не найден на DistrictPanel.");
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