using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Weapon
{
    [SerializeField] ParticleSystem laser = null;
    [SerializeField]
    private void Start()
    {
        target = Player.player.transform;
        laser.Stop();
        shotSound.Stop();
    }

    override protected void Attack()
    {
        if (target == null || Vector3.Distance(this.transform.position, target.position) > range)
        {
            laser.Stop();
            shotSound.Stop();
            return;
        }

        laser.gameObject.transform.LookAt(target.position);

        laser.Play();
        if (!shotSound.isPlaying && !PauseMenu.GameIsPaused)
        {
            shotSound.Play();
        }
    }


}
