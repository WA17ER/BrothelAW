using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    private bool isSpawning = false;
    private List<GameObject> clientPrefabs = new List<GameObject>();
    private int currentPrefabIndex = 0;
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
        Debug.Log($"SpawnHandler: Получено префабов: Всего = {clientPrefabs.Count}");
        currentPrefabIndex = 0;
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
        while (GameManager.Instance.ClientsSpawnedToday < GameManager.Instance.MaxClientsPerDay)
        {
            if (clientPrefabs.Count == 0)
            {
                Debug.LogError("Нет доступных префабов клиентов для спавна.");
                yield break;
            }

            GameObject clientPrefab = clientPrefabs[currentPrefabIndex];
            GameObject clientGO = Instantiate(clientPrefab, GameManager.Instance.SpawnPoint.position, Quaternion.identity);
            ClientData client = clientGO.GetComponent<ClientData>();
            if (client == null)
            {
                Debug.LogError($"ClientData отсутствует на префабе клиента {clientGO.name}.");
                Destroy(clientGO);
                yield break;
            }

            client.InitializeClientPreferences();
            GameManager.Instance.ClientPool.Add(client);
            GameManager.Instance.ClientsSpawnedToday++;
            Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");

            currentPrefabIndex = (currentPrefabIndex + 1) % clientPrefabs.Count;

            yield return new WaitForSeconds(GameManager.Instance.MinSpawnDelay);
        }
        isSpawning = false;
    }
}