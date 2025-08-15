using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    private List<GameObject> clientType1Prefabs;
    private List<GameObject> clientType2Prefabs;
    private List<GameObject> clientType3Prefabs;
    private List<GameObject> clientType4Prefabs;
    private Dictionary<int, int> extraVisitors;
    private bool isSpawning;
    private Coroutine spawnCoroutine;

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

    public void Initialize(List<GameObject> type1, List<GameObject> type2, List<GameObject> type3, List<GameObject> type4, Dictionary<int, int> visitors)
    {
        clientType1Prefabs = type1;
        clientType2Prefabs = type2;
        clientType3Prefabs = type3;
        clientType4Prefabs = type4;
        extraVisitors = visitors;
        isSpawning = false;
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
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
        yield return new WaitForSeconds(Random.Range(10f, 20f));

        int totalClientsToSpawn = GameManager.Instance.TotalClients;
        while (isSpawning && GameManager.Instance.ClientsSpawnedToday < totalClientsToSpawn)
        {
            if (!GameManager.Instance.IsDayPaused && (GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)) > GameManager.Instance.MinRemainingTimeForLastClient)
            {
                Debug.Log($"Перед спавном: ClientsSpawnedToday = {GameManager.Instance.ClientsSpawnedToday}, TotalClients = {totalClientsToSpawn}");
                int clientType = GetClientType();
                GameObject prefab = GetPrefabForType(clientType);
                if (prefab != null)
                {
                    GameObject clientGO = Instantiate(prefab, GameManager.Instance.SpawnPoint.position, Quaternion.identity);
                    ClientData clientData = clientGO.GetComponent<ClientData>();
                    CustomerMovement movement = clientGO.GetComponent<CustomerMovement>();
                    if (clientData != null && movement != null)
                    {
                        clientData.clientName = $"Client_{GameManager.Instance.ClientsSpawnedToday + 1}";
                        clientData.InitializeClientPreferences(clientType);
                        clientData.SetState(ClientData.ClientState.MovingToRegister);
                        GameManager.Instance.ClientPool.Add(clientData);
                        GameManager.Instance.ClientsSpawnedToday++;
                        Debug.Log($"Клиент {clientData.clientName} типа {clientType} заспавнен.");
                    }
                    else
                    {
                        Debug.LogError($"ClientData or CustomerMovement missing on {clientGO.name}.");
                        Destroy(clientGO);
                    }
                }
                float delay = Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay);
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
        }
        isSpawning = false;
        Debug.Log($"Спавн остановлен: достигнут лимит {totalClientsToSpawn} клиентов.");
    }

    private int GetClientType()
    {
        List<int> availableTypes = new List<int>();
        foreach (var pair in extraVisitors)
        {
            if (pair.Value > 0)
            {
                availableTypes.Add(pair.Key);
            }
        }
        if (availableTypes.Count == 0)
        {
            Debug.Log("Нет доступных типов клиентов, возвращается тип 1.");
            return 1;
        }
        int selectedType = availableTypes[Random.Range(0, availableTypes.Count)];
        extraVisitors[selectedType]--;
        Debug.Log($"Выбран тип клиента: {selectedType}, осталось: {extraVisitors[selectedType]}");
        return selectedType;
    }

    private GameObject GetPrefabForType(int type)
    {
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
}