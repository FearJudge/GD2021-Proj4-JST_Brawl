using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeathSpawner : MonoBehaviour
{
    int indexOfEnables = 0;
    [System.Serializable]
    public class SpawnSet
    {
        public string name = "DeathSpawnObject";
        public float chanceToDrop;
        public GameObject dropItem;
        public bool enabled = true;
        public int[] disableOtherDrops = new int[0];
    }
    public SpawnSet[] spawns;
    [SerializeField] int triggers = 1;

    public void OnDestroy()
    {
        SpawnStuff();
    }

    public void SpawnStuff()
    {
        if (triggers <= 0) { return; }
        triggers--;
        for (int a = 0; a < spawns.Length; a++)
        {
            if (!spawns[a].enabled) { continue; }
            float randomVal = Random.Range(0f, 1f);
            for (int b = 0; b < spawns[a].disableOtherDrops.Length; b++)
            {
                spawns[spawns[a].disableOtherDrops[b]].enabled = false;
            }
            Instantiate(spawns[a].dropItem, transform.position, spawns[a].dropItem.transform.rotation);
        }
    }

    public void EnableNext()
    {
        if (indexOfEnables >= spawns.Length - 1) { return; }
        spawns[indexOfEnables].enabled = true;
        indexOfEnables++;
    }
}
