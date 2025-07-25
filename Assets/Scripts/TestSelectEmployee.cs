using UnityEngine;

public class TestSelectEmployee : MonoBehaviour
{
    [SerializeField] private CustomerPreferences prefs;
    [SerializeField] private BaseEmployeeDataSO testEmployee; // Назначь в инспекторе (например, Anna)

    private void Awake()
    {
        if (prefs == null || testEmployee == null)
        {
            Debug.LogError("Missing references in TestSelectEmployee");
            enabled = false;
            return;
        }
        prefs.OnPreferencesGenerated += SelectEmployeeOnGenerated;
        Debug.Log("Subscribed to OnPreferencesGenerated"); // Временный лог
    }

    private void SelectEmployeeOnGenerated()
    {
        Debug.Log("Selecting employee"); // Временный лог
        prefs.SelectEmployeeManually(testEmployee); // Вызов ручного выбора после генерации запроса
    }

    private void OnDestroy()
    {
        if (prefs != null)
            prefs.OnPreferencesGenerated -= SelectEmployeeOnGenerated;
    }
}