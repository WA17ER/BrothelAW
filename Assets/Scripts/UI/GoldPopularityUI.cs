using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoldPopularityUI : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text popularityText;

    void Start()
    {
        GameManager.Instance.onStateChange.AddListener(UpdateUI);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (goldText != null)
            goldText.text = "Золото: " + GameManager.Instance.CurrentGold;
        if (popularityText != null)
            popularityText.text = "Популярность: " + GameManager.Instance.CurrentPopularity;
    }
}