using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EmployeeManager : MonoBehaviour
{
    [SerializeField] private List<Employee> employees = new List<Employee>(); // Список всех сотрудниц в сцене
    private static EmployeeManager instance;

    public static EmployeeManager Instance => instance;

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

        if (employees == null || employees.Count == 0)
        {
            Debug.LogError("EmployeeManager: No employees assigned.");
            enabled = false;
        }
    }

    public void RegisterEmployee(Employee employee)
    {
        if (!employees.Contains(employee))
        {
            employees.Add(employee);
        }
    }

    public List<BaseEmployeeDataSO> GetAvailableEmployees()
    {
        return employees
            .Where(emp => emp.Data.CurrentState == BaseEmployeeDataSO.EmployeeState.Free)
            .Select(emp => emp.Data)
            .ToList();
    }

    public void SelectEmployee(BaseEmployeeDataSO employee, CustomerPreferences customer)
    {
        if (employee.CurrentState != BaseEmployeeDataSO.EmployeeState.Free)
            return;

        employee.SetState(BaseEmployeeDataSO.EmployeeState.Working);
        customer.SelectEmployee(employee);
    }

    public void CompleteService(BaseEmployeeDataSO employee, string service, int staminaCost = 10, float skillProgress = 10f)
    {
        if (employee.CurrentState == BaseEmployeeDataSO.EmployeeState.Working)
        {
            employee.UpdateSkillProgress(service, skillProgress);
            employee.DecreaseStamina(staminaCost + (employee.Disease != null ? employee.Disease.StaminaPenalty : 0));
            employee.SetState(BaseEmployeeDataSO.EmployeeState.Free);

            // Проверка на заражение после услуги
            if (Random.value < 0.05f) // 5% шанс
            {
                TryInfectEmployee(employee);
            }
        }
    }

    private void TryInfectEmployee(BaseEmployeeDataSO employee)
    {
        BaseSickSO[] diseases = Resources.LoadAll<BaseSickSO>("Diseases");
        if (diseases.Length == 0) return;

        BaseSickSO selectedDisease = diseases[Random.Range(0, diseases.Length)];
        if (!selectedDisease.ImmuneRaces.Contains(employee.Race))
        {
            employee.SetDisease(selectedDisease);
        }
    }
}