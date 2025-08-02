using UnityEngine;

public class TestStateChangeHandler : MonoBehaviour
{
    public void OnGameStateChanged()
    {
        Debug.Log($"TestStateChangeHandler: Состояние игры изменено на объекте {name}.");
    }
}