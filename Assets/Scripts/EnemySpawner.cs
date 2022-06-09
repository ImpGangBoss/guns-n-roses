using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemiesPrefabs;
    [SerializeField] private List<BoxCollider> spawnZoneList;
    [SerializeField] private int gameCycles = 10;
    [SerializeField] private bool simulate = true;
    private string _key = "Cycle";
    private List<GameObject> _enemies;

    void Start()
    {
        if (!simulate)
            return;

        if (!enemiesPrefabs.Any())
        {
            Debug.LogError("There is no enemy to spawn");
            return;
        }

        if (!spawnZoneList.Any())
        {
            Debug.LogError("There is no zone to spawn");
            return;
        }

        _enemies = new List<GameObject>();

        if (!PlayerPrefs.HasKey(_key))
            PlayerPrefs.SetInt(_key, gameCycles);
        else if (PlayerPrefs.GetInt(_key) > 0)
            PlayerPrefs.SetInt(_key, PlayerPrefs.GetInt(_key) - 1);
        else
        {
            Debug.LogWarning("Simulation over. Game cycles simulated: " + gameCycles);
            PlayerPrefs.DeleteKey(_key);
            simulate = false;
            return;
        }

        SpawnEnemies();
    }

    void Update()
    {
        if (_enemies.Count(x => x.gameObject.activeInHierarchy) < 1 && simulate)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SpawnEnemies()
    {
        foreach (var enemyToSpawn in enemiesPrefabs)
        {
            var zone = spawnZoneList[Random.Range(0, spawnZoneList.Count)].bounds;
            Vector3 randomPosition = new Vector3(
                Random.Range(zone.min.x, zone.max.x),
                Random.Range(zone.min.y, zone.max.y),
                Random.Range(zone.min.z, zone.max.z));

            _enemies.Add(Instantiate(enemyToSpawn, randomPosition, Quaternion.identity));
        }
    }
}
