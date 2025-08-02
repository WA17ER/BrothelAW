using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        int totalClients = gameManager.TotalClients;
        if (totalClients <= 0)
        {
            Debug.Log("SpawnHandler: TotalClients равно 0, спавн не выполняется.");
            yield break;
        }

        List<(int type, GameObject prefab)> spawnList = new List<(int, GameObject)>();

        List<GameObject> type1Prefabs = gameManager.ClientVisualModels[1];
        if (type1Prefabs.Count < 3)
        {
            Debug.LogError($"SpawnHandler: clientType1Prefabs содержит {type1Prefabs.Count} префабов, требуется минимум 3.");
            yield break;
        }

        List<int> type1Positions = Enumerable.Range(0, gameManager.BaseVisitors).ToList();
        for (int i = 0; i < type1Prefabs.Count; i++)
        {
            int randomPosition = Random.Range(0, type1Positions.Count);
            spawnList.Add((1, type1Prefabs[i]));
            type1Positions.RemoveAt(randomPosition);
        }

        for (int i = type1Prefabs.Count; i < gameManager.BaseVisitors; i++)
        {
            spawnList.Add((1, type1Prefabs[Random.Range(0, type1Prefabs.Count)]));
        }

        foreach (var type in gameManager.ExtraVisitors)
        {
            if (!gameManager.ClientVisualModels.ContainsKey(type.Key) || gameManager.ClientVisualModels[type.Key].Count == 0)
            {
                Debug.LogError($"SpawnHandler: Нет префабов для типа {type.Key}.");
                continue;
            }
            for (int i = 0; i < type.Value; i++)
            {
                GameObject prefab = gameManager.ClientVisualModels[type.Key][Random.Range(0, gameManager.ClientVisualModels[type.Key].Count)];
                spawnList.Add((type.Key, prefab));
            }
        }

        spawnList = spawnList.OrderBy(x => Random.value).ToList();

        Dictionary<int, int> typeCounts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        Dictionary<GameObject, int> type1Distribution = type1Prefabs.ToDictionary(p => p, _ => 0);

        foreach (var (type, prefab) in spawnList)
        {
            typeCounts[type]++;
            if (type == 1)
            {
                type1Distribution[prefab]++;
            }
        }

        string distributionLog = $"Всего клиентов: {totalClients}, Тип 1: {typeCounts[1]}";
        if (typeCounts[1] > 0)
        {
            distributionLog += $", {string.Join(", ", type1Distribution.Select(kvp => $"{kvp.Key.name}: {kvp.Value}"))}";
        }
        if (typeCounts[2] > 0) distributionLog += $", Тип 2: {typeCounts[2]}";
        if (typeCounts[3] > 0) distributionLog += $", Тип 3: {typeCounts[3]}";
        if (typeCounts[4] > 0) distributionLog += $", Тип 4: {typeCounts[4]}";
        Debug.Log(distributionLog);

        float maxInterval = totalClients > 0 ? gameManager.MaxDayDuration / totalClients : gameManager.MinSpawnDelay;
        for (int i = 0; i < spawnList.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(gameManager.MinSpawnDelay, maxInterval));
            GameObject clientGO = Instantiate(spawnList[i].prefab, spawnPoint.position, Quaternion.identity);
            ClientRequest client = clientGO.GetComponent<ClientRequest>();
            client.clientLevel = spawnList[i].type;
            gameManager.ClientPool.Add(client);
            gameManager.ClientsSpawnedToday++;
            Debug.Log($"Клиент типа {spawnList[i].type}, префаб: {spawnList[i].prefab.name}, позиция: {spawnPoint.position}, всего: {gameManager.ClientsSpawnedToday}/{totalClients}");
        }
    }
}