/*using UnityEngine;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    private Dictionary<GameObject, float> customerTimers = new Dictionary<GameObject, float>();
    private static CustomerManager instance;

    public static CustomerManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterCustomer(GameObject customer)
    {
        if (!customerTimers.ContainsKey(customer) && customer != null)
        {
            customerTimers.Add(customer, 0f);
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement != null)
            {
                movement.OnServiceStarted += OnServiceStartedHandler;
                Debug.Log($"Customer {customer.name} registered and subscribed to OnServiceStarted");
            }
            else
            {
                Debug.LogError("CustomerMovement not found on registered customer");
            }
        }
        else
        {
            Debug.LogWarning("Customer already registered or null");
        }
    }

    private void OnServiceStartedHandler(float time)
    {
        GameObject customer = null;
        foreach (var kvp in customerTimers)
        {
            if (kvp.Value == 0f) // Предполагаем, что только тот клиент, чей таймер не запущен
            {
                customer = kvp.Key;
                break;
            }
        }
        if (customer != null && customerTimers.ContainsKey(customer))
        {
            customerTimers[customer] = time;
            Debug.Log($"Service timer started for {customer.name}, time: {time}");
        }
        else
        {
            Debug.LogWarning("No valid customer found for timer start");
        }
    }

    private void Update()
    {
        foreach (var customer in new List<GameObject>(customerTimers.Keys))
        {
            if (customerTimers.ContainsKey(customer) && customerTimers[customer] > 0)
            {
                customerTimers[customer] -= Time.deltaTime;
                Debug.Log($"Service timer for {customer.name}: {customerTimers[customer]}");
                if (customerTimers[customer] <= 0)
                {
                    var movement = customer.GetComponent<CustomerMovement>();
                    if (movement != null)
                    {
                        movement.ActivateAndExit();
                        Debug.Log($"Service completed for {customer.name}, activating and exiting");
                        movement.OnServiceStarted -= OnServiceStartedHandler; // Отписка
                    }
                    customerTimers.Remove(customer);
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var customer in customerTimers.Keys)
        {
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement != null)
            {
                movement.OnServiceStarted -= OnServiceStartedHandler;
            }
        }
    }
}*/