using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class TrainingManager : MonoBehaviour
{
    [SerializeField] private GameObject trainingUIPrefab; // Prefab with TrainingIndicator
    private Employee _employee;
    private string _skill;
    private bool _success1, _success2, _success3;

    public Employee Employee { get => _employee; set => _employee = value; }
    public string Skill { get => _skill; set => _skill = value; }
    public bool Success1 { get => _success1; set => _success1 = value; }
    public bool Success2 { get => _success2; set => _success2 = value; }
    public bool Success3 { get => _success3; set => _success3 = value; }

    void Start()
    {
        if (GameStateManager.Instance != null)
        {
            Employee = GameStateManager.Instance.TrainingEmployee;
            Skill = GameStateManager.Instance.TrainingSkill;
        }
        StartCoroutine(RunTrainingSessions());
    }

    IEnumerator RunTrainingSessions()
    {
        yield return new WaitForSeconds(10f);
        for (int i = 0; i < 3; i++)
        {
            GameObject uiInstance = Instantiate(trainingUIPrefab, transform);
            TrainingIndicator ind = uiInstance.GetComponent<TrainingIndicator>();
            if (ind != null)
            {
                int session = i + 1;
                ind.OnComplete = () => {
                    bool success = ind.CurrentFill >= 1f;
                    switch (session)
                    {
                        case 1: Success1 = success; break;
                        case 2: Success2 = success; break;
                        case 3: Success3 = success; break;
                    }
                    Debug.Log($"Сотрудница {Employee.Data.employeeName} {Skill} сессия {session}: {(success ? "успешно" : "провал")}");
                    Destroy(uiInstance);
                };
            }
            yield return new WaitForSeconds(10f);
        }
        CompleteTrain();
    }

    [ContextMenu("CompleteTrain")]
    public void CompleteTrain()
    {
        if (Employee == null) return;
        float addProgress = Employee.Data.progressionSpeed * ((Success1 ? 1 : 0) + (Success2 ? 1 : 0) + (Success3 ? 1 : 0));
        var (level, progress) = Employee.Skills.ContainsKey(Skill) ? Employee.Skills[Skill] : (0, 0f);
        progress += addProgress;
        while (progress >= 100)
        {
            level += 1;
            if (level > 10)
            {
                level = 10;
                progress = 0;
                break;
            }
            progress -= 100;
        }
        Employee.Skills[Skill] = (level, progress);
        GameStateManager.Instance.ClearTrainingData();
        SceneManager.LoadScene("ManagmentScene");
    }
}