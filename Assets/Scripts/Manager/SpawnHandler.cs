using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public static SpawnHandler Instance { get; private set; }

    private bool isSpawning = false;
    private List<GameObject> clientType1Prefabs = new List<GameObject>();
    private List<GameObject> clientType2Prefabs = new List<GameObject>();
    private List<GameObject> clientType3Prefabs = new List<GameObject>();
    private List<GameObject> clientType4Prefabs = new List<GameObject>();
    private Dictionary<int, int> clientTypeCounts;
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

    public void Initialize(List<GameObject> clientType1Prefabs, List<GameObject> clientType2Prefabs, List<GameObject> clientType3Prefabs, List<GameObject> clientType4Prefabs, Dictionary<int, int> extraVisitors)
    {
        this.clientType1Prefabs = clientType1Prefabs;
        this.clientType2Prefabs = clientType2Prefabs;
        this.clientType3Prefabs = clientType3Prefabs;
        this.clientType4Prefabs = clientType4Prefabs;
        this.clientTypeCounts = new Dictionary<int, int>(extraVisitors);

        if (clientTypeCounts.ContainsKey(1) && clientTypeCounts[1] > 0 && clientType1Prefabs.Count < 3)
        {
            Debug.LogError($"SpawnHandler: Недостаточно префабов Type1 ({clientType1Prefabs.Count}), требуется минимум 3 (Моряк, Строитель, Работяга).");
        }
        Debug.Log($"SpawnHandler: Получено префабов: Type1 = {clientType1Prefabs.Count}, Type2 = {clientType2Prefabs.Count}, Type3 = {clientType3Prefabs.Count}, Type4 = {clientType4Prefabs.Count}");
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
        Debug.Log("Спавн клиентов приостановлен.");
    }

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnClients());
            Debug.Log("Спавн клиентов возобновлён.");
        }
    }

    private IEnumerator SpawnClients()
    {
        yield return new WaitForSeconds(Random.Range(10f, 20f));

        int clientsSpawned = 0;
        int totalClientsToSpawn = Mathf.Min(GameManager.Instance.TotalClients, GameManager.Instance.MaxClientsPerDay);

        // Спавн минимум одного экземпляра каждого префаба Type1, если есть клиенты Type1
        List<GameObject> requiredPrefabs = new List<GameObject>();
        if (clientTypeCounts.ContainsKey(1) && clientTypeCounts[1] > 0)
        {
            requiredPrefabs.AddRange(clientType1Prefabs);
            // Перемешиваем requiredPrefabs для случайного порядка первых трёх клиентов
            for (int i = requiredPrefabs.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                GameObject temp = requiredPrefabs[i];
                requiredPrefabs[i] = requiredPrefabs[j];
                requiredPrefabs[j] = temp;
            }
        }

        while (clientsSpawned < totalClientsToSpawn && requiredPrefabs.Count > 0 && isSpawning)
        {
            // Проверка времени для последнего клиента
            if (clientsSpawned == totalClientsToSpawn - 1 && (GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime) < GameManager.Instance.MinRemainingTimeForLastClient))
            {
                Debug.Log("Недостаточно времени для спавна последнего клиента.");
                yield break;
            }

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
            clientTypeCounts[1]--;
            Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");

            if (clientsSpawned < totalClientsToSpawn)
            {
                yield return new WaitForSeconds(Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay));
            }
        }

        // Спавн оставшихся клиентов по типам
        while (clientsSpawned < totalClientsToSpawn && isSpawning)
        {
            // Проверка времени для последнего клиента
            if (clientsSpawned == totalClientsToSpawn - 1 && (GameManager.Instance.MaxDayDuration - (Time.time - GameManager.Instance.DayStartTime) < GameManager.Instance.MinRemainingTimeForLastClient))
            {
                Debug.Log("Недостаточно времени для спавна последнего клиента.");
                yield break;
            }

            List<int> availableTypes = new List<int>();
            if (clientTypeCounts.ContainsKey(1) && clientTypeCounts[1] > 0) availableTypes.Add(1);
            if (clientTypeCounts.ContainsKey(2) && clientTypeCounts[2] > 0) availableTypes.Add(2);
            if (clientTypeCounts.ContainsKey(3) && clientTypeCounts[3] > 0) availableTypes.Add(3);
            if (clientTypeCounts.ContainsKey(4) && clientTypeCounts[4] > 0) availableTypes.Add(4);

            if (availableTypes.Count == 0)
            {
                Debug.Log("Нет доступных типов клиентов для спавна.");
                yield break;
            }

            int selectedType = availableTypes[Random.Range(0, availableTypes.Count)];
            List<GameObject> selectedPrefabs = null;
            switch (selectedType)
            {
                case 1: selectedPrefabs = clientType1Prefabs; break;
                case 2: selectedPrefabs = clientType2Prefabs; break;
                case 3: selectedPrefabs = clientType3Prefabs; break;
                case 4: selectedPrefabs = clientType4Prefabs; break;
            }

            if (selectedPrefabs == null || selectedPrefabs.Count == 0)
            {
                Debug.LogError($"Нет префабов для типа {selectedType}.");
                yield break;
            }

            GameObject clientPrefab = selectedPrefabs[Random.Range(0, selectedPrefabs.Count)];
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
            clientTypeCounts[selectedType]--;
            Debug.Log($"Клиент {clientGO.name} создан с болезнью {(client.ActiveSick != null ? client.ActiveSick.SickName : "none")}.");

            if (clientsSpawned < totalClientsToSpawn)
            {
                yield return new WaitForSeconds(Random.Range(GameManager.Instance.MinSpawnDelay, GameManager.Instance.MaxSpawnDelay));
            }
        }

        isSpawning = false;
    }
}