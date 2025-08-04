using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        if (gameManager == null || spawnPoint == null)
        {
            Debug.LogError("SpawnHandler: GameManager или spawnPoint не назначены.");
            return;
        }
        StartCoroutine(SpawnClients());
    }

    private IEnumerator SpawnClients()
    {
        int totalClients = gameManager.TotalClients;
        int baseClients = gameManager.BaseVisitors;
        Dictionary<int, int> extraVisitors = gameManager.ExtraVisitors;
        int spawned = 0;
        float spawnInterval = gameManager.MaxDayDuration / Mathf.Max(totalClients, 1); // Деление дня на общее число клиентов

        // Спавн базовых клиентов (Type1)
        for (int i = 0; i < baseClients && spawned < totalClients; i++)
        {
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
            yield return new WaitForSeconds(delay);
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
                yield return new WaitForSeconds(delay);
            }
        }

        Debug.Log($"Всего клиентов: {totalClients}, Тип 1: {baseClients}, Доп: {string.Join(", ", extraVisitors)}");
    }
}