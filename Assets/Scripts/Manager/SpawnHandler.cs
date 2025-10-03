using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }
    private List<ClientDataSO> mixedVisitorList; // Теперь ClientDataSO
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    public UnityEvent<ClientData> OnClientSpawned;
    private Transform spawnPoint;
    private int nextClientId = 1;
    private List<ClientDataSO> availableVisitors; // ClientDataSO

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

    public void Initialize(List<ClientDataSO> totalVisitors) // Только totalClientList
    {
        mixedVisitorList = totalVisitors;
        availableVisitors = new List<ClientDataSO>(mixedVisitorList);
        isSpawning = false;
        spawnPoint = GameManager.Instance.SpawnPoint;
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
        yield return new WaitForSeconds(Random.Range(10f, 20f));
        int totalClientsToSpawn = GameManager.Instance.TotalClients;
        while (isSpawning && GameManager.Instance.ClientsSpawnedToday < totalClientsToSpawn)
        {
            if (!GameManager.Instance.IsDayPaused && (GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime)) > GameManager.Instance.MinRemainingTimeForLastClient)
            {
                if (spawnPoint == null) yield break;
                ClientDataSO clientSO = GetRandomClient();
                if (clientSO != null && clientSO.Prefab != null)
                {
                    var clientGO = Instantiate(clientSO.Prefab, spawnPoint.position, Quaternion.identity);
                    var clientData = clientGO.GetComponent<ClientData>();
                    var movement = clientGO.GetComponent<CustomerMovement>();
                    if (clientData != null && movement != null)
                    {
                        clientData.clientName = GetUniqueName(clientData.clientName);
                        clientData.SetClientId(nextClientId++);
                        clientData.ClientDataSO = clientSO;
                        clientData.InitializeClientPreferences();
                        clientData.SetState(ClientData.ClientState.MovingToRegister);
                        GameManager.Instance.ClientPool.Add(clientData);
                        GameManager.Instance.ClientsSpawnedToday++;
                        OnClientSpawned?.Invoke(clientData);
                    }
                    else
                    {
                        Destroy(clientGO);
                    }
                }
                yield return new WaitForSeconds(Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay));
            }
            else
            {
                yield return null;
            }
        }
        isSpawning = false;
    }

    private string GetUniqueName(string baseName)
    {
        var usedNames = GameManager.Instance.ClientPool.Select(c => c.clientName).Concat(ClientManager.Instance.AllClients.Select(c => c.clientName));
        string uniqueName = baseName;
        int nameIndex = 1;
        while (usedNames.Contains(uniqueName))
        {
            uniqueName = $"{baseName}_{nameIndex++}";
        }
        return uniqueName;
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

    // Убрано GetPrefabForType

    public bool IsSpawning => isSpawning;
}