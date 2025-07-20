using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChairManager : MonoBehaviour
{
    [SerializeField] private List<Chair> chairList;
    private Queue<Chair> freeChairs = new Queue<Chair>();

    private void Awake()
    {
        if (chairList == null || chairList.Count == 0)
        {
            Debug.LogError($"{gameObject.name}: ChairList is empty or not assigned.");
            enabled = false;
            return;
        }

        // »нициализаци€ очереди свободных стульев
        freeChairs = new Queue<Chair>(chairList.Where(chair => chair.CurrentState == Chair.ChairState.Free));
    }

    public Chair GetFreeChair()
    {
        // ¬озвращаем первый свободный стул из очереди
        while (freeChairs.Count > 0)
        {
            var chair = freeChairs.Dequeue();
            if (chair.CurrentState == Chair.ChairState.Free)
            {
                chair.SetState(Chair.ChairState.OnOccupation);
                return chair;
            }
        }

        // ѕровер€ем список стульев, если очередь пуста
        var freeChair = chairList.FirstOrDefault(chair => chair.CurrentState == Chair.ChairState.Free);
        if (freeChair != null)
        {
            freeChair.SetState(Chair.ChairState.OnOccupation);
            return freeChair;
        }

        return null; // Ќет свободных стульев
    }

    public void ReturnChair(Chair chair)
    {
        if (chair != null && chair.CurrentState != Chair.ChairState.Free)
        {
            chair.SetState(Chair.ChairState.Free);
            freeChairs.Enqueue(chair);
        }
    }
}