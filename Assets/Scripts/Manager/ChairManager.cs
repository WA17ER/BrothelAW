using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChairManager : MonoBehaviour
{
    public static ChairManager Instance { get; private set; }

    [Header("Chairs")]
    [SerializeField] private List<Chair> chairList;
    private Queue<Chair> freeChairs = new Queue<Chair>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (chairList == null || chairList.Count == 0)
        {
            Debug.LogError($"{gameObject.name}: ChairList is empty or not assigned.");
            enabled = false;
            return;
        }

        // Удаляем null элементы и стулья без компонента Chair
        chairList.RemoveAll(chair => chair == null || chair.gameObject == null || chair.GetComponent<Chair>() == null);
        if (chairList.Count == 0)
        {
            Debug.LogError($"{gameObject.name}: No valid chairs in chairList after cleanup.");
            enabled = false;
            return;
        }

        Debug.Log($"ChairManager Awake: chairList.Count = {chairList.Count}");
        foreach (var chair in chairList)
        {
            if (chair == null)
            {
                Debug.LogError("Null chair found in chairList after cleanup.");
                continue;
            }
            Debug.Log($"Chair {chair.name} state: {chair.CurrentState}");
            if (chair.CurrentState == Chair.ChairState.Free)
            {
                freeChairs.Enqueue(chair);
            }
        }
        Debug.Log($"freeChairs.Count after Awake: {freeChairs.Count}");
    }

    public Chair GetFreeChair()
    {
        Debug.Log($"GetFreeChair called, freeChairs.Count = {freeChairs.Count}, chairList.Count = {chairList.Count}");

        while (freeChairs.Count > 0)
        {
            var chair = freeChairs.Dequeue();
            if (chair == null || chair.gameObject == null || chair.GetComponent<Chair>() == null)
            {
                Debug.LogError($"Invalid chair in freeChairs: {chair?.name ?? "null"}");
                continue;
            }
            Debug.Log($"Dequeued chair {chair.name} state: {chair.CurrentState}");
            if (chair.CurrentState == Chair.ChairState.Free)
            {
                chair.SetState(Chair.ChairState.OnOccupation);
                Debug.Log($"Assigned chair {chair.name}, new state: {chair.CurrentState}");
                return chair;
            }
        }

        Debug.Log("Checking chairList for free chairs.");
        var freeChair = chairList.FirstOrDefault(chair => chair != null && chair.gameObject != null && chair.GetComponent<Chair>() != null && chair.CurrentState == Chair.ChairState.Free);
        if (freeChair != null)
        {
            freeChair.SetState(Chair.ChairState.OnOccupation);
            Debug.Log($"Assigned chair from chairList {freeChair.name}, new state: {freeChair.CurrentState}");
            return freeChair;
        }

        Debug.Log("No free chair found.");
        return null;
    }

    public void ReturnChair(Chair chair)
    {
        if (chair != null && chair.gameObject != null && chair.GetComponent<Chair>() != null && chair.CurrentState != Chair.ChairState.Free)
        {
            chair.SetState(Chair.ChairState.Free);
            freeChairs.Enqueue(chair);
            Debug.Log($"Chair returned: {chair.name}, state: {chair.CurrentState}, freeChairs.Count = {freeChairs.Count}");
        }
        else
        {
            Debug.LogWarning($"Cannot return chair: {chair?.name ?? "null"}, invalid or already free.");
        }
    }
}