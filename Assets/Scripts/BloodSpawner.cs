using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSpawner : MonoBehaviour
{
    public GameObject bloodPrefab;

    [System.Serializable] public struct BloodRules
    {
        public float chance;
        public Vector2 velocityRange;
        public Vector2 angleRange;
    }

    [SerializeField] public BloodRules[] bloodArray;

    private void Awake()
    {
        TriggerBlood();
    }

    void TriggerBlood()
    {
        for (int a = 0; a < bloodArray.Length; a++)
        {
            if (Random.Range(0f, 1f) > bloodArray[a].chance) { continue; }
            GameObject bloody = Instantiate(bloodPrefab);
            bloody.transform.position = gameObject.transform.position;
            Rigidbody rb = bloody.GetComponent<Rigidbody>();
            rb.AddForce(Quaternion.AngleAxis(Random.Range(bloodArray[a].angleRange.x, bloodArray[a].angleRange.y), transform.forward) * (Vector3.up * Random.Range(bloodArray[a].velocityRange.x, bloodArray[a].velocityRange.y)), ForceMode.Impulse);
        }
    }
}
