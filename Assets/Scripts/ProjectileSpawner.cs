using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public int maxAmmo = 6;

    public GameObject projectile;
    public int ammo = 6;
    public AudioClip noAmmoSound;
    public AudioClip yesAmmoSound;
    public Vector3 vel;
    public float lifeTime = 0.5f;
    public HurtBox hbData;

    public int ammoChange = 0;
    public int extraDamage = 0;
    public float extraStun = 0f;
    public Vector3 extraVelocity;
    public int extraCrit;

    private void OnEnable()
    {
        if (ammo > maxAmmo) { ammo = maxAmmo; }
        if (ammo <= 0) { SoundPlayer.PlaySFX(noAmmoSound, 1f, 1f); return; }
        ammo += ammoChange;
        SoundPlayer.PlaySFX(yesAmmoSound, 1f, 1f);
        GameObject proj = Instantiate(projectile);
        proj.transform.position = transform.position;
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        HurtBox hb = proj.GetComponent<HurtBox>();
        Projectile ps = proj.GetComponent<Projectile>();
        int dirRight = (int)transform.right.x;
        rb.velocity = new Vector3((vel.x + extraVelocity.x) * dirRight, vel.y + extraVelocity.y, vel.z);
        hb.SetData(hbData);
        hb.damage += extraDamage;
        hb.crit += extraCrit;
        hb.hitStun += extraStun;
        ps.lifetime = lifeTime;
        ps.BeginDeath();
        SetStateToRegular();
        ammoChange = 0;
    }

    public void SetStateToRegular()
    {
        extraCrit = 0;
        extraDamage = 0;
        extraStun = 0f;
        extraVelocity = Vector3.zero;
        ammoChange = 0;
    }
}
