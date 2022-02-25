using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public int maxAllowed = 1;
    public float spawnDelay = 2f;
    public List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        EncounterManager.AllCleared += Activate;
    }

    private void OnDestroy()
    {
        EncounterManager.AllCleared -= Activate;
    }

    public void Activate()
    {
        StartCoroutine(SpewEnemy(spawnDelay));
    }

    IEnumerator SpewEnemy(float delay = 2f)
    {
        while (gameObject != null)
        {
            yield return new WaitForSeconds(delay);
            if (spawnedEnemies.Count < maxAllowed) { spawnedEnemies.Add(Instantiate(enemy, transform.position, transform.rotation)); }
            else { CheckDeaths(); }
            yield return null;
        }
    }

    void CheckDeaths()
    {
        for (int a = spawnedEnemies.Count - 1; a >= 0; a--)
        {
            if (spawnedEnemies[a] == null) { spawnedEnemies.RemoveAt(a); }
        }
    }
}
