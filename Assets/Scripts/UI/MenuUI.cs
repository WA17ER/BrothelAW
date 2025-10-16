using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text newGameButtonText;

    void Start()
    {
        var newGameBtn = newGameButtonText.GetComponent<Button>();
        newGameBtn?.onClick.AddListener(LoadNewGame);
    }

    void LoadNewGame()
    {
        SceneManager.LoadScene("ManagmentScene");
    }
}