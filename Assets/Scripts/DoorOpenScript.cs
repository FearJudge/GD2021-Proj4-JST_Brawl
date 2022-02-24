using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenScript : MonoBehaviour
{
    public Transform door;
    public Vector3 closed;
    public Vector3 open;
    public bool isOpen = false;
    public LayerMask activeOn;
    float t = 0f;
    public AudioClip openSound = null;
    public float openSpeed = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) { return; }
        if ((1 << other.gameObject.layer & activeOn) != 0)
        {
            StartCoroutine(Open());
        }
    }

    public IEnumerator Open()
    {
        SoundPlayer.PlaySFX(openSound, 1f, 1f);
        isOpen = true;
        while ( t <= 1f)
        {
            if (!isOpen) { door.localRotation = Quaternion.Lerp(Quaternion.Euler(open), Quaternion.Euler(closed), t); }
            else { door.localRotation = Quaternion.Lerp(Quaternion.Euler(closed), Quaternion.Euler(open), t); }
            yield return new WaitForSeconds(0.02f);
            t += openSpeed / 100f;
        }

    }
}
