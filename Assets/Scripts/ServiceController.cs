using System.Collections;
using UnityEngine;

public class ServiceController : MonoBehaviour
{
    public void EnterService(CustomerMovement customer)
    {
        Debug.Log($"Клиент {customer.name} начал услугу.");
        customer.Visual.gameObject.SetActive(false);
        StartCoroutine(ServiceTimer(5f, customer));
    }

    private IEnumerator ServiceTimer(float time, CustomerMovement customer)
    {
        yield return new WaitForSeconds(time);
        customer.Visual.gameObject.SetActive(true);
        Debug.Log($"Клиент {customer.name} закончил услугу.");
        customer.ExitService();
    }
}