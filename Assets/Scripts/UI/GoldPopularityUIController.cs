using UnityEngine;
using UnityEngine.UIElements;

public class GoldPopularityUIController : MonoBehaviour
{
    private Label goldLabel;
    private Label popularityLabel;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        goldLabel = root.Query<Label>("GoldLabel");
        popularityLabel = root.Query<Label>("PopularityLabel");

        if (goldLabel == null || popularityLabel == null)
        {
            Debug.LogError("GoldLabel or PopularityLabel not found in UI Document.");
            return;
        }

        UpdateLabels();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onStateChange.AddListener(UpdateLabels);
        }
        else
        {
            Debug.LogWarning("GameManager instance not found.");
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onStateChange.RemoveListener(UpdateLabels);
        }
    }

    private void UpdateLabels()
    {
        if (GameManager.Instance != null)
        {
            goldLabel.text = $"Золото: {GameManager.Instance.currentGold:F0}";
            popularityLabel.text = $"Популярность: {GameManager.Instance.currentPopularity:F0}";
        }
        else
        {
            Debug.LogWarning("GameManager instance not found.");
        }
    }
}