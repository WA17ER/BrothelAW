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
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnClients());
    }

    private IEnumerator SpawnClients()
    {
        int totalClients = gameManager.TotalClients;
        float maxDelay = (gameManager.MaxDayDuration - gameManager.MinSpawnDelay * (totalClients - 1)) / totalClients;

        while (gameManager.ClientsSpawnedToday < totalClients)
        {
            float delay = Random.Range(gameManager.MinSpawnDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            int clientType = ChooseClientType();
            List<GameObject> prefabs = gameManager.ClientVisualModels[clientType];
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            GameObject client = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            client.GetComponent<ClientRequest>().clientLevel = clientType; // Исправлено
            gameManager.ClientPool.Add(client.GetComponent<ClientRequest>());
            gameManager.ClientsSpawnedToday++;
            Debug.Log($"Клиент типа {clientType} заспавнен, всего: {gameManager.ClientsSpawnedToday}/{totalClients}");
        }
    }

    private int ChooseClientType()
    {
        if (gameManager.ClientsSpawnedToday < gameManager.BaseVisitors)
        {
            return 1;
        }

        List<int> availableTypes = new List<int> { 1 };
        if (gameManager.ExtraVisitors.ContainsKey(2)) availableTypes.Add(2);
        if (gameManager.ExtraVisitors.ContainsKey(3)) availableTypes.Add(3);
        if (gameManager.ExtraVisitors.ContainsKey(4)) availableTypes.Add(4);

        return availableTypes[Random.Range(0, availableTypes.Count)];
    }
}