using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private Route route; // Ссылка на Route
    [SerializeField] private CustomerMenuUI customerMenuUI; // Ссылка на UI
    private NavMeshAgent agent; // NavMeshAgent

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name} requires NavMeshAgent.");
            Destroy(gameObject);
            return;
        }

        if (route == null)
        {
            Debug.LogError($"{gameObject.name} is missing Route component.");
            Destroy(gameObject);
            return;
        }

        if (customerMenuUI == null)
        {
            Debug.LogError($"{gameObject.name} is missing CustomerMenuUI component.");
            Destroy(gameObject);
            return;
        }

        // Принудительно Y=0
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    void Update()
    {
        // Принудительная коррекция Y
        if (Mathf.Abs(transform.position.y) > 0.001f)
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }

        // Получаем данные от Route
        var (targetPosition, shouldMove, shouldDestroy, currentState) = route.RouteHandler(transform.position);

        if (shouldDestroy)
        {
            Debug.Log($"{gameObject.name} destroying.");
            Destroy(gameObject);
            return;
        }

        if (shouldMove)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.isStopped = true;
            transform.position = targetPosition;
        }

        Debug.Log($"{gameObject.name} state: {currentState}, moving to: {targetPosition}, shouldMove: {shouldMove}, Y-position: {transform.position.y}");
    }

    void OnMouseDown()
    {
        var (_, _, _, currentState) = route.RouteHandler(transform.position);
        if (currentState == Route.State.Waiting || currentState == Route.State.OnWaitingSpot)
        {
            customerMenuUI.Initialize(route, this); // Открываем UI
        }
    }
}