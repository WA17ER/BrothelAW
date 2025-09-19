using UnityEngine;
using UnityEngine.EventSystems;

public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            if (EventSystem.current != null && EventSystem.current.gameObject != gameObject)
            {
                Destroy(EventSystem.current.gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}