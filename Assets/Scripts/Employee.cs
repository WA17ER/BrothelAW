using UnityEngine;

public class Employee : MonoBehaviour
{
    [SerializeField] private BaseEmployeeDataSO employeeData; // —сылка на данные сотрудницы
    [SerializeField] private EmployeeManager employeeManager; // —сылка на EmployeeManager

    public BaseEmployeeDataSO Data => employeeData;

    private void Awake()
    {
        if (employeeData == null)
        {
            Debug.LogError($"{gameObject.name}: EmployeeData is not assigned.");
            enabled = false;
            return;
        }

        if (employeeManager == null)
        {
            Debug.LogError($"{gameObject.name}: EmployeeManager is not assigned.");
            enabled = false;
            return;
        }

        // –егистраци€ в EmployeeManager
        employeeManager.RegisterEmployee(this);
    }
}