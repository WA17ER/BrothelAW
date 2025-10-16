using UnityEngine;
using UnityEngine.UI;
public class TrainingIndicator : MonoBehaviour
{
    [SerializeField] private Image redIndicator; // RedIndicator
    [SerializeField] private Image greenZone; // GreenZone
    [SerializeField] private Image fillBar; // FillBar from RightBar
    [SerializeField] private float speedUp = 100;
    [SerializeField] private float speedDown = 10;
    [SerializeField] private float fillSpeed = 0.1f; // Speed for fill increase/decrease
    [SerializeField] private float greenZoneHeight = 70f; // Height for GreenZone
    [SerializeField] private float leftBarHeight = 200f; // Height of LeftBar
    private RectTransform redRect;
    private RectTransform greenRect;
    private bool isActive = false;
    private float currentFill = 0.5f;
    public float CurrentFill { get => currentFill; }
    public System.Action OnComplete;
    void Start()
    {
        redRect = redIndicator.rectTransform;
        greenRect = greenZone.rectTransform;
        if (fillBar) fillBar.fillAmount = 0.5f;
        currentFill = 0.5f;
        // Set GreenZone height
        greenRect.sizeDelta = new Vector2(greenRect.sizeDelta.x, greenZoneHeight);
        // Random GreenZone center Y: -100 + halfHeight to 100 - halfHeight
        float halfHeight = greenZoneHeight / 2f;
        float minCenterY = -leftBarHeight / 2f + halfHeight;
        float maxCenterY = leftBarHeight / 2f - halfHeight;
        float randomCenterY = Random.Range(minCenterY, maxCenterY);
        greenRect.anchoredPosition = new Vector2(0, randomCenterY);
        float randomY = Random.Range(-100f, 100f);
        redRect.anchoredPosition = new Vector2(0, randomY);
        Debug.Log($"Random Y: {randomY}, GreenZone center: {randomCenterY}");
        isActive = true;
    }
    void Update()
    {
        if (!isActive) return;
        float delta = Time.deltaTime;
        float newY = redRect.anchoredPosition.y;
        if (Input.GetMouseButton(0))
        {
            newY += speedUp * delta;
        }
        else
        {
            newY -= speedDown * delta;
        }
        redRect.anchoredPosition = new Vector2(0, Mathf.Clamp(newY, -100f, 100f));
        // Check if in GreenZone
        float redY = redRect.anchoredPosition.y;
        float greenY = greenRect.anchoredPosition.y;
        float halfHeight = greenRect.sizeDelta.y / 2f;
        bool inZone = redY >= greenY - halfHeight && redY <= greenY + halfHeight;
        if (fillBar)
        {
            if (inZone)
            {
                currentFill += fillSpeed * delta;
            }
            else
            {
                currentFill -= fillSpeed * delta;
            }
            currentFill = Mathf.Clamp01(currentFill);
            fillBar.fillAmount = currentFill;
            if (fillBar.fillAmount >= 1f)
            {
                Debug.Log("Success");
                CompleteIndicator();
            }
            if (fillBar.fillAmount <= 0f)
            {
                Debug.Log("Fail");
                CompleteIndicator();
            }
        }
    }
    void CompleteIndicator()
    {
        isActive = false;
        OnComplete?.Invoke();
    }
}