using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientIndicatorUIController : MonoBehaviour
{
    [SerializeField] private GameObject clientIndicatorTemplate; // Assign ClientIndicatorTemplate prefab
    [SerializeField] private Transform containerTransform; // Assign ClientIndicatorContainer RectTransform

    private Dictionary<ClientData, GameObject> indicatorMap = new Dictionary<ClientData, GameObject>();

    private void Start()
    {
        containerTransform = transform; // If attached to ClientIndicatorContainer
        GameManager.Instance.onStateChange.AddListener(UpdateIndicators);
        UpdateIndicators();
    }

    private void OnDisable()
    {
        GameManager.Instance.onStateChange.RemoveListener(UpdateIndicators);
    }

    private void UpdateIndicators()
    {
        // Remove indicators for clients not in Waiting or OnOccupyChair
        List<ClientData> toRemove = new List<ClientData>();
        foreach (var pair in indicatorMap)
        {
            CustomerMovement movement = pair.Key.GetComponent<CustomerMovement>();
            if (movement != null && (movement.State != CustomerMovement.ClientState.Waiting && movement.State != CustomerMovement.ClientState.OnOccupyChair))
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
            else if (movement != null)
            {
                // Update progress bar
                Slider slider = pair.Value.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    slider.value = movement.waitTime / movement.maxWaitTime;
                }
                // Ensure correct size
                RectTransform rect = pair.Value.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(200, 150);
                }
            }
        }
        foreach (var key in toRemove)
        {
            indicatorMap.Remove(key);
        }

        // Add indicators for new clients in Waiting or OnOccupyChair
        foreach (var client in GameManager.Instance.ClientPool)
        {
            CustomerMovement movement = client.GetComponent<CustomerMovement>();
            if (movement != null && (movement.State == CustomerMovement.ClientState.Waiting || movement.State == CustomerMovement.ClientState.OnOccupyChair) && !indicatorMap.ContainsKey(client))
            {
                GameObject indicator = Instantiate(clientIndicatorTemplate, containerTransform);
                indicator.SetActive(true);
                RectTransform rect = indicator.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(200, 150);
                }
                Slider slider = indicator.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    slider.value = movement.waitTime / movement.maxWaitTime;
                }
                indicatorMap.Add(client, indicator);
            }
        }
    }
}