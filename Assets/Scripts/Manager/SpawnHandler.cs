using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    private bool isSpawning = false;
    private List<GameObject> clientPrefabs = new List<GameObject>();
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
        if (clientPrefabs.Count < 3)
        {
            Debug.LogError($"SpawnHandler: Недостаточно префабов Type1 ({clientPrefabs.Count}), требуется минимум 3 (Моряк, Строитель, Работяга).");
        }
        Debug.Log($"SpawnHandler: Получено префабов: Type1 = {clientPrefabs.Count}");
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
        yield return new WaitForSeconds(Random.Range(10f, 20f));

        List<GameObject> requiredPrefabs = new List<GameObject>(clientPrefabs);
        // Перемешиваем requiredPrefabs для случайного порядка первых трёх клиентов
        for (int i = requiredPrefabs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = requiredPrefabs[i];
            requiredPrefabs[i] = requiredPrefabs[j];
            requiredPrefabs[j] = temp;
        }

        int clientsSpawned = 0;

        // Спавн по одному экземпляру каждого префаба (до 3 клиентов)
        while (clientsSpawned < Mathf.Min(3, GameManager.Instance.BaseVisitors) && requiredPrefabs.Count > 0 && isSpawning)
        {
            GameObject clientPrefab = requiredPrefabs[0];
            requiredPrefabs.RemoveAt(0);

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
            clientsSpawned++;
            Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");

            if (clientsSpawned < GameManager.Instance.BaseVisitors)
            {
                yield return new WaitForSeconds(Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay));
            }
        }

        // Спавн оставшихся клиентов до baseVisitors
        while (clientsSpawned < GameManager.Instance.BaseVisitors && isSpawning)
        {
            int randomIndex = Random.Range(0, clientPrefabs.Count);
            GameObject clientPrefab = clientPrefabs[randomIndex];

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
            clientsSpawned++;
            Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");

            if (clientsSpawned < GameManager.Instance.BaseVisitors)
            {
                yield return new WaitForSeconds(Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay));
            }
        }

        isSpawning = false;
    }
}