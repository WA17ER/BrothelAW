using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }
    private List<GameObject> clientType1Prefabs;
    private List<GameObject> clientType2Prefabs;
    private List<GameObject> clientType3Prefabs;
    private List<GameObject> clientType4Prefabs;
    private List<ClientDataSO> mixedVisitorList;
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    public UnityEvent<ClientData> OnClientSpawned; // Событие для уведомления о спавне
    private Transform spawnPoint;
    private int nextClientId = 1; // Счётчик для уникальных ID
    private List<ClientDataSO> availableVisitors;

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
        }
    }

    public void Initialize(List<GameObject> type1, List<GameObject> type2, List<GameObject> type3, List<GameObject> type4, List<ClientDataSO> visitors)
    {
        clientType1Prefabs = type1;
        clientType2Prefabs = type2;
        clientType3Prefabs = type3;
        clientType4Prefabs = type4;
        mixedVisitorList = visitors;
        availableVisitors = new List<ClientDataSO>(mixedVisitorList);
        isSpawning = false;
        spawnPoint = GameManager.Instance.SpawnPoint; // Изначальная привязка
        Debug.Log($"SpawnHandler initialized with spawnPoint: {spawnPoint?.name}, mixedVisitorList count: {mixedVisitorList.Count}, availableVisitors count: {availableVisitors.Count}");
    }

    public void UpdateSpawnPoint(Transform newSpawnPoint)
    {
        if (newSpawnPoint != null && spawnPoint != newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
            Debug.Log($"SpawnPoint updated to: {spawnPoint?.name}");
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            Debug.Log("StartSpawning: Запуск спавна клиентов.");
            spawnCoroutine = StartCoroutine(SpawnClients());
        }
        else
        {
            Debug.LogWarning("Спавн уже активен.");
        }
    }

    public void PauseSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnClients());
        }
    }

    private IEnumerator SpawnClients()
    {
        Debug.Log($"SpawnClients: Initial delay, isDayPaused = {GameManager.Instance.IsDayPaused}, RemainingTime = {GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)}");
        yield return new WaitForSeconds(Random.Range(10f, 20f));
        int totalClientsToSpawn = GameManager.Instance.TotalClients;
        Debug.Log($"SpawnClients: Начало спавна, TotalClients = {totalClientsToSpawn}, availableVisitors count: {availableVisitors.Count}");
        while (isSpawning && GameManager.Instance.ClientsSpawnedToday < totalClientsToSpawn)
        {
            Debug.Log($"SpawnClients: Loop check, isDayPaused = {GameManager.Instance.IsDayPaused}, RemainingTime = {GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)}");
            if (!GameManager.Instance.IsDayPaused && (GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)) > GameManager.Instance.MinRemainingTimeForLastClient)
            {
                Debug.Log($"Перед спавном: ClientsSpawnedToday = {GameManager.Instance.ClientsSpawnedToday}, TotalClients = {totalClientsToSpawn}, spawnPoint = {spawnPoint?.name}");
                if (spawnPoint == null)
                {
                    Debug.LogError("SpawnPoint is null, skipping spawn.");
                    yield break;
                }
                ClientDataSO clientSO = GetRandomClient();
                Debug.Log($"GetRandomClient returned: {clientSO?.name ?? "null"}");
                if (clientSO != null)
                {
                    int clientType = GameManager.Instance.GetClientTypeId(clientSO);
                    GameObject prefab = GetPrefabForType(clientType);
                    Debug.Log($"GetPrefabForType returned: {prefab?.name ?? "null"} for clientType {clientType}");
                    if (prefab != null)
                    {
                        GameObject clientGO = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                        ClientData clientData = clientGO.GetComponent<ClientData>();
                        CustomerMovement movement = clientGO.GetComponent<CustomerMovement>();
                        if (clientData != null && movement != null)
                        {
                            string baseName = clientData.clientName;
                            int nameIndex = 1;
                            string uniqueName = baseName;
                            while (GameManager.Instance.ClientPool.Any(c => c.clientName == uniqueName) ||
                                   ClientManager.Instance.AllClients.Any(c => c.clientName == uniqueName))
                            {
                                uniqueName = $"{baseName}_{nameIndex++}";
                            }
                            clientData.clientName = uniqueName;
                            Debug.Log($"Клиент спавнен с именем {clientData.clientName} (ID: {nextClientId}) из префаба {baseName}, уникальность проверена");

                            clientData.SetClientId(nextClientId++);

                            List<Employee> availableEmployees = EmployeeManager.Instance.AvailableEmployees;
                            Employee randomEmployee = availableEmployees != null && availableEmployees.Count > 0 ?
                                availableEmployees[Random.Range(0, availableEmployees.Count)] : null;
                            EmployeeDataSO employeeData = randomEmployee?.Data;

                            clientData.InitializeClientPreferences(clientType, randomEmployee);

                            clientData.SetState(ClientData.ClientState.MovingToRegister);
                            GameManager.Instance.ClientPool.Add(clientData);
                            GameManager.Instance.ClientsSpawnedToday++;
                            if (OnClientSpawned != null)
                            {
                                OnClientSpawned.Invoke(clientData);
                            }
                        }
                        else
                        {
                            Debug.LogError($"ClientData or CustomerMovement missing on {clientGO.name}.");
                            Destroy(clientGO);
                        }
                    }
                }
                float delay = Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay);
                Debug.Log($"Next spawn delay: {delay} seconds");
                yield return new WaitForSeconds(delay);
            }
            else
            {
                Debug.Log($"Spawn blocked: isDayPaused = {GameManager.Instance.IsDayPaused}, RemainingTime = {GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)}");
                yield return null;
            }
        }
        isSpawning = false;
        Debug.Log($"Спавн остановлен: достигнут лимит {totalClientsToSpawn} клиентов.");
    }

    private ClientDataSO GetRandomClient()
    {
        Debug.Log($"GetRandomClient: availableVisitors count = {availableVisitors.Count}, contains null: {availableVisitors.Any(x => x == null)}");
        if (availableVisitors.Count == 0)
        {
            Debug.LogError("No available visitors left.");
            return null;
        }
        int index = Random.Range(0, availableVisitors.Count);
        ClientDataSO client = availableVisitors[index];
        availableVisitors.RemoveAt(index);
        return client;
    }

    private GameObject GetPrefabForType(int type)
    {
        Debug.Log($"GetPrefabForType: Checking type {type}, clientType1Prefabs count = {clientType1Prefabs.Count}");
        switch (type)
        {
            case 1:
                return clientType1Prefabs[Random.Range(0, clientType1Prefabs.Count)];
            case 2:
                return clientType2Prefabs[Random.Range(0, clientType2Prefabs.Count)];
            case 3:
                return clientType3Prefabs[Random.Range(0, clientType3Prefabs.Count)];
            case 4:
                return clientType4Prefabs[Random.Range(0, clientType4Prefabs.Count)];
            default:
                Debug.LogWarning($"Некорректный тип клиента: {type}, возвращается null.");
                return null;
        }
    }

    public bool IsSpawning => isSpawning;
}