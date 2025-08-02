using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ClientPrefabConfig
{
    public GameObject prefab;
    public int count;
}

public class SpawnHandler : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        if (gameManager == null || spawnPoint == null)
        {
            Debug.LogError("SpawnHandler: GameManager или SpawnPoint не назначены.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnClients());
    }

    private IEnumerator SpawnClients()
    {
        int totalClients = gameManager.BaseVisitors;
        if (totalClients <= 0)
        {
            Debug.Log("SpawnHandler: baseVisitors равно 0, спавн не выполняется.");
            yield break;
        }

        List<GameObject> prefabs = gameManager.ClientVisualModels[1];
        if (prefabs.Count < 3)
        {
            Debug.LogError($"SpawnHandler: clientType1Prefabs содержит {prefabs.Count} префабов, требуется минимум 3.");
            yield break;
        }

        List<int> positions = Enumerable.Range(0, totalClients).ToList();
        List<GameObject> spawnList = new List<GameObject>(new GameObject[totalClients]);

        for (int i = 0; i < prefabs.Count; i++)
        {
            int randomPosition = Random.Range(0, positions.Count);
            spawnList[positions[randomPosition]] = prefabs[i];
            positions.RemoveAt(randomPosition);
        }

        for (int i = 0; i < totalClients; i++)
        {
            if (spawnList[i] == null)
            {
                spawnList[i] = prefabs[Random.Range(0, prefabs.Count)];
            }
        }

        Dictionary<GameObject, int> distribution = new Dictionary<GameObject, int>();
        foreach (var prefab in prefabs)
        {
            distribution[prefab] = 0;
        }
        foreach (var prefab in spawnList)
        {
            distribution[prefab]++;
        }

        string distributionLog = $"Всего клиентов: {totalClients}, Тип 1: {totalClients}, ";
        foreach (var kvp in distribution)
        {
            distributionLog += $"{kvp.Key.name}: {kvp.Value}, ";
        }
        Debug.Log(distributionLog.TrimEnd(',', ' '));

        for (int i = 0; i < totalClients; i++)
        {
            yield return new WaitForSeconds(gameManager.MinSpawnDelay);
            GameObject clientGO = Instantiate(spawnList[i], spawnPoint.position, Quaternion.identity);
            ClientRequest client = clientGO.GetComponent<ClientRequest>();
            client.clientLevel = 1;
            gameManager.ClientPool.Add(client);
            gameManager.ClientsSpawnedToday++;
            Debug.Log($"Клиент типа 1, префаб: {spawnList[i].name} заспавнен, всего: {gameManager.ClientsSpawnedToday}/{totalClients}");
        }
    }
}