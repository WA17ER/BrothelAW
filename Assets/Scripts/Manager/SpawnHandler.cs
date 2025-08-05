using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform spawnPoint;
    private Coroutine spawnCoroutine;
    private bool isPaused = false;

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
            return;
        }
        if (gameManager == null || spawnPoint == null)
        {
            Debug.LogError("SpawnHandler: GameManager или spawnPoint не назначены.");
            enabled = false;
        }
    }

    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        isPaused = false;
        spawnCoroutine = StartCoroutine(SpawnClients());
    }

    public void PauseSpawning()
    {
        isPaused = true;
        Debug.Log("Спавн клиентов поставлен на паузу.");
    }

    public void ResumeSpawning()
    {
        isPaused = false;
        Debug.Log("Спавн клиентов возобновлён.");
    }

    private IEnumerator SpawnClients()
    {
        int totalClients = gameManager.TotalClients;
        int baseClients = gameManager.BaseVisitors;
        Dictionary<int, int> extraVisitors = gameManager.ExtraVisitors;
        int spawned = 0;
        float spawnInterval = gameManager.MaxDayDuration / Mathf.Max(totalClients, 1);

        // Спавн базовых клиентов (Type1)
        for (int i = 0; i < baseClients && spawned < totalClients; i++)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            List<GameObject> type1Prefabs = gameManager.ClientVisualModels[1];
            if (type1Prefabs.Count == 0)
            {
                Debug.LogError("SpawnHandler: ClientVisualModels[1] пустой.");
                break;
            }
            int index = Random.Range(0, type1Prefabs.Count);
            GameObject clientGO = Instantiate(type1Prefabs[index], spawnPoint.position, Quaternion.identity);
            ClientData client = clientGO.GetComponent<ClientData>();
            if (client == null)
            {
                Debug.LogError($"ClientData отсутствует на клиенте {clientGO.name}.");
                Destroy(clientGO);
                continue;
            }
            client.Data.isSick = Random.value < client.Data.sickChance / 100f;
            Debug.Log($"Клиент {clientGO.name} создан, болен: {client.Data.isSick}, тип: {client.Data.clientType}");
            gameManager.ClientPool.Add(client);
            gameManager.ClientsSpawnedToday++;
            spawned++;
            float delay = Random.Range(gameManager.MinSpawnDelay, spawnInterval);
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Спавн дополнительных клиентов (Type2–Type4)
        foreach (var extra in extraVisitors)
        {
            int type = extra.Key;
            int count = extra.Value;
            List<GameObject> extraPrefabs = gameManager.ClientVisualModels[type];
            if (extraPrefabs.Count == 0)
            {
                Debug.LogError($"SpawnHandler: ClientVisualModels[{type}] пустой.");
                continue;
            }
            for (int i = 0; i < count && spawned < totalClients; i++)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }
                int index = Random.Range(0, extraPrefabs.Count);
                GameObject clientGO = Instantiate(extraPrefabs[index], spawnPoint.position, Quaternion.identity);
                ClientData client = clientGO.GetComponent<ClientData>();
                if (client == null)
                {
                    Debug.LogError($"ClientData отсутствует на клиенте {clientGO.name}.");
                    Destroy(clientGO);
                    continue;
                }
                client.Data.isSick = Random.value < client.Data.sickChance / 100f;
                Debug.Log($"Клиент {clientGO.name} создан, болен: {client.Data.isSick}, тип: {client.Data.clientType}");
                gameManager.ClientPool.Add(client);
                gameManager.ClientsSpawnedToday++;
                spawned++;
                float delay = Random.Range(gameManager.MinSpawnDelay, spawnInterval);
                float elapsed = 0f;
                while (elapsed < delay)
                {
                    if (isPaused)
                    {
                        yield return null;
                        continue;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        Debug.Log($"Всего клиентов: {totalClients}, Тип 1: {baseClients}, Доп: {string.Join(", ", extraVisitors)}");
    }
}