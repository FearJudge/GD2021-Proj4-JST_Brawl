using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDroplet : MonoBehaviour
{
    Transform currentTarget;

    public Transform root;
    public Rigidbody rb;

    public LayerMask activeOn;
    public GameObject hitSpark;
    public Vector2 bloodFill;
    public ParticleSystem ps;
    public Collider trigger;
    public float speed = 2f;

    public float maxDuration = 8f;
    public float suckInRange = 7f;
    public float suckInAt = 5.5f;
    public float rage = 0.04f;
    public GameObject blood;
    public MeshRenderer bloodrend;

    private void Awake()
    {
        trigger.enabled = false;
    }

    private void Update()
    {
        if (blood == null) { return; }
        if  (currentTarget != null) { root.position += (((currentTarget.position + Vector3.up*1.5f) - root.position) * Time.deltaTime * speed); }
        else { blood.transform.localPosition = new Vector3(Random.Range(-rage, rage), Random.Range(-rage, rage), Random.Range(-rage, rage)); }
        maxDuration -= Time.deltaTime;
        if (suckInAt >= maxDuration && currentTarget == null) { TrySuckIn(); }
        if (maxDuration < 0f) {
            DetachParticles();
            Destroy(root.gameObject);
        }
        if (maxDuration < 3f) { speed += Time.deltaTime * 10f; }
    }

    void TrySuckIn()
    {
        foreach (PlayerController p in PlayerController.players)
        {
            if (currentTarget != null && Random.Range(0f, 1f) >= 0.5f) { continue; }
            if (Mathf.Abs((p.transform.position - root.position).magnitude) < suckInRange) { currentTarget = p.transform; }
        }
        if (currentTarget == null) { suckInAt -= 0.7f; }
        else { trigger.enabled = true; rb.velocity = Vector3.zero; rb.isKinematic = true; rb.useGravity = false; }
    }

    public void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & activeOn) != 0 && maxDuration < suckInAt)
        {
            other.gameObject.transform.parent.gameObject.TryGetComponent(out PlayerController player);
            if (player == null) { return; }
            if (hitSpark != null) { Instantiate(hitSpark, other.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0), transform.rotation); }
            player.special.Value += Random.Range((int)bloodFill.x, (int)bloodFill.y);
            DetachParticles();
            Destroy(root.gameObject);
        }
    }

    public void DetachParticles()
    {
        blood.transform.parent = null;
        blood = null;
        ps.Stop();
        bloodrend.enabled = false;
    }
}
