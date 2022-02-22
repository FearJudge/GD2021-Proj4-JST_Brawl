using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager manager;

    [System.Serializable]
    public class Spawn
    {
        bool spawned = false;
        public float encounterSpawnAt = 0f;
        public GameObject enemy;
        public Transform spawnLocation;
        public uint upgradeId = 0;

        public GameObject SpawnEnemy(float currentWaveDuration)
        {
            spawned = true;
            GameObject newEnemy = Instantiate(enemy);
            newEnemy.transform.position = spawnLocation.position;
            newEnemy.GetComponent<EnemyAI>().SetUpgradeId(upgradeId);
            return newEnemy;
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool waitUntilDestroyed = true;
        public float waitAfterPrevious = 0f;
        public float minimumTimeForNext = 12f;
        public float maximumTimeForNext = 0f;
        public Spawn[] enemies;
    }

    [System.Serializable]
    public class Encounter
    {
        public int songId;
        public EncounterTrigger encounterStart;
        public Vector3 cameraSet;
        public Wave[] waves;

        public void BeginEncounter()
        {
            encounterStart.trigger.enabled = false;
            SoundPlayer.PlayBGM(songId, 0.8f, 1f);
        }
    }

    public Encounter[] encounterListings = new Encounter[0];
    public CameraFollower mainCam;
    int waveInd = 0;
    float durFromWave = 0f;
    float waveMin = 0f;
    float waveMax = 0f;
    bool waitForNextWave = false;
    bool waitForKilled = false;
    int currentEnemyCap = 0;
    public Wave[] currentWaves = new Wave[0];
    public List<GameObject> enemies = new List<GameObject>();

    void Start()
    {
        manager = this;
        EncounterTrigger.PlayerReached += StartEncounter;
    }

    private void OnDestroy()
    {
        manager = null;
    }

    void FixedUpdate()
    {
        AdvanceWaves();
    }

    void StartEncounter(EncounterTrigger instance)
    {
        waveInd = 0;
        WaveInternalsClear();
        for (int a = 0; a < encounterListings.Length; a++)
        {
            if (encounterListings[a].encounterStart == instance)
            {
                encounterListings[a].BeginEncounter();
                mainCam.SetEncounterCamera(encounterListings[a].cameraSet);
                currentWaves = encounterListings[a].waves;
            }
        }
    }

    void AdvanceWaves()
    {
        if (currentWaves.Length == 0) { return; }
        durFromWave += Time.fixedDeltaTime;
        if (!waitForNextWave)
        {
            if (durFromWave < currentWaves[waveInd].waitAfterPrevious) { return; }
            waitForKilled = currentWaves[waveInd].waitUntilDestroyed;
            waveMin = currentWaves[waveInd].minimumTimeForNext;
            waveMax = currentWaves[waveInd].maximumTimeForNext;
            for (int a = 0; a < currentWaves[waveInd].enemies.Length; a++)
            {
                StartCoroutine(SpawnEnemy(waveInd, a));
                currentEnemyCap++;
            }
            waitForNextWave = true;
        }
        int enemyCount = enemies.Count;
        if ((durFromWave >= waveMax && waveMax >= waveMin) || (!waitForKilled && durFromWave >= waveMin) || (enemyCount == 0 && durFromWave >= waveMin))
        { NextWave(); }
    }

    IEnumerator SpawnEnemy(int wave, int enemyN)
    {
        yield return new WaitForSeconds(currentWaves[wave].enemies[enemyN].encounterSpawnAt);
        GameObject enemySpawn = currentWaves[wave].enemies[enemyN].SpawnEnemy(durFromWave);
        if (enemySpawn != null) { enemies.Add(enemySpawn); }
    }

    void WaveInternalsClear()
    {
        durFromWave = 0f; waitForNextWave = false; waitForKilled = false;
    }

    void NextWave()
    {
        waveInd++;
        if (waveInd >= currentWaves.Length) { EndEncounter(); }
        WaveInternalsClear();
    }

    void EndEncounter()
    {
        currentWaves = new Wave[0];
        mainCam.EndEncounterCamera();
    }

    public static void IDied(GameObject enemy)
    {
        manager.enemies.Remove(enemy);
    }
}
