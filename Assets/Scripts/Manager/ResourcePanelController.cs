using TMPro;
using UnityEngine;

public class ResourcePanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text popularityText;

    void Start()
    {
        UpdateDisplay();
    }

    private void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (goldText != null && popularityText != null && GameStateManager.Instance != null)
        {
            goldText.text = "Золото: " + GameStateManager.Instance.Gold.ToString("F0");
            popularityText.text = "Популярность: " + GameStateManager.Instance.Popularity.ToString("F0");
        }
    }
}