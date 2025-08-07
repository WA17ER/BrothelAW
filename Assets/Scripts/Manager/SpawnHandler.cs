using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    private bool isSpawning = false;
    private List<GameObject> clientPrefabs = new List<GameObject>();
    private float spawnTimer = 0f;
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

    public void Initialize(List<GameObject> clientType1Prefabs, List<GameObject> clientType2Prefabs, List<GameObject> clientType3Prefabs, List<GameObject> clientType4Prefabs)
    {
        clientPrefabs.Clear();
        clientPrefabs.AddRange(clientType1Prefabs);
        clientPrefabs.AddRange(clientType2Prefabs);
        clientPrefabs.AddRange(clientType3Prefabs);
        clientPrefabs.AddRange(clientType4Prefabs);
    }

    public void StartSpawning()
    {
        if (isSpawning)
        {
            Debug.LogWarning("Спавн уже активен.");
            return;
        }
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnClients());
    }

    public void PauseSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
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
        while (isSpawning && GameManager.Instance.ClientsSpawnedToday < GameManager.Instance.TotalClients)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnClient();
                spawnTimer = GameManager.Instance.MinSpawnDelay;
            }
            yield return null;
        }
        isSpawning = false;
    }

    private void SpawnClient()
    {
        if (GameManager.Instance.ClientsSpawnedToday >= GameManager.Instance.TotalClients)
        {
            Debug.Log("Достигнут лимит клиентов на день.");
            return;
        }

        if (clientPrefabs.Count == 0)
        {
            Debug.LogError("Нет доступных префабов клиентов для спавна.");
            return;
        }

        GameObject clientPrefab = clientPrefabs[Random.Range(0, clientPrefabs.Count)];
        GameObject clientGO = Instantiate(clientPrefab, GameManager.Instance.SpawnPoint.position, Quaternion.identity);
        ClientData client = clientGO.GetComponent<ClientData>();
        if (client == null)
        {
            Debug.LogError($"ClientData отсутствует на префабе клиента {clientGO.name}.");
            Destroy(clientGO);
            return;
        }

        client.InitializeClientPreferences();
        GameManager.Instance.ClientPool.Add(client);
        GameManager.Instance.ClientsSpawnedToday++;
        Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");
    }
}