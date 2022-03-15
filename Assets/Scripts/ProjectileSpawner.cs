using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public bool isEnemySpawnerInsteadSomehow = false;

    public int maxAmmo = 6;
    public int firedProjectiles = 3;
    public int firedExtraProjectiles = 0;

    public GameObject projectile;
    public int ammo = 6;
    public AudioClip noAmmoSound;
    public AudioClip yesAmmoSound;
    public Vector3 vel;
    public float lifeTime = 0.5f;
    public HurtBox hbData;

    public int lifeSteal = 0;
    public int ammoChange = 0;
    public int extraDamage = 0;
    public float extraStun = 0f;
    public Vector3 extraVelocity;
    public int extraCrit;

    const int MAXPROJECTILES = 10;

    private void OnEnable()
    {
        if (isEnemySpawnerInsteadSomehow) { EnemySpawn(); }
        else { ProjectileSpawn(); }
    }

    public void ProjectileSpawn()
    {
        if (ammo > maxAmmo) { ammo = maxAmmo; }
        if (ammo <= 0) { SoundPlayer.PlaySFX(noAmmoSound, 1f, 1f); return; }
        ammo += ammoChange;
        SoundPlayer.PlaySFX(yesAmmoSound, 1f, 1f);
        for (int a = 0; a < firedProjectiles + firedExtraProjectiles; a++)
        {
            if (a >= MAXPROJECTILES) { break; }
            GameObject proj = Instantiate(projectile);
            proj.transform.position = transform.position + (Vector3.down * a * 0.1f);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            HurtBox hb = proj.GetComponent<HurtBox>();
            Projectile ps = proj.GetComponent<Projectile>();
            int dirRight = (int)transform.right.x;
            rb.velocity = new Vector3((vel.x + extraVelocity.x) * dirRight, vel.y + extraVelocity.y - a, vel.z);
            hb.SetData(hbData);
            hb.damage += extraDamage;
            hb.crit += extraCrit;
            hb.lifeSteal += lifeSteal;
            hb.hitStun += extraStun;
            ps.lifetime = lifeTime;
            ps.BeginDeath();
        }
        SetStateToRegular();
        ammoChange = 0;
    }

    public void EnemySpawn()
    {
        SoundPlayer.PlaySFX(yesAmmoSound, 1f, 1f);
        for (int a = 0; a < firedProjectiles + firedExtraProjectiles; a++)
        {
            GameObject proj = Instantiate(projectile);
            proj.transform.position = transform.position + (vel * a);
        }
    }

    public void SetStateToRegular()
    {
        extraCrit = 0;
        extraDamage = 0;
        extraStun = 0f;
        lifeSteal = 0;
        extraVelocity = Vector3.zero;
        ammoChange = 0;
        firedExtraProjectiles = 0;
    }
}
