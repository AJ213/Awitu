using UnityEngine;
using UnityEngine.UI;

public class Gun : Weapon
{
    [SerializeField] float manaCost = default;
    [SerializeField] GameObject impactEffect = null;
    [SerializeField] ParticleSystem muzzleFlash = null;
    [SerializeField] LayerMask viableSurfaces = default;
    Animator gunAnimator;

    public Camera cam = null;
    Mana playerMana = default;

    [SerializeField] Animator crosshairFlash = default;


    private void Start()
    {
        playerMana = Player.player.GetComponent<Mana>();
        gunAnimator = this.GetComponent<Animator>();
    }

    override protected void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            TryToAttack();
        }
    }

    override protected void Attack()
    {
        if(playerMana.CurrrentStat < manaCost)
        {
            return;
        }
        
        if(!PauseMenu.GameIsPaused)
        {
            shotSound.PlayOneShot(shotSound.clip);
            playerMana.CurrrentStat -= manaCost;
        }
        
        muzzleFlash.Play();
        gunAnimator.SetTrigger("Fire");
        

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range, viableSurfaces))
        {

            Health health = hit.transform.GetComponent<Health>();
            if(health != null)
            {
                health.CurrrentStat -= Damage;
                crosshairFlash.SetTrigger("Flash");
            }

            GameObject goImpact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal), this.transform);
            Destroy(goImpact, 1);
        }

    }
}
