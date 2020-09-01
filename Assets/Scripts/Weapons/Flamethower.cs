using UnityEngine;

public class Flamethower : Weapon
{
    [SerializeField] float manaCost = default;

    [SerializeField] ParticleSystem flamethrower = null;

    Mana playerMana = default;
    private void Start()
    {
        flamethrower.Stop();
        playerMana = Player.player.GetComponent<Mana>();
    }

    override protected void Update()
    {
        if (Input.GetButton("Fire2"))
        {
            TryToAttack();
        }
        else
        {
            flamethrower.Stop();
            shotSound.Stop();
        }
    }

    override protected void Attack()
    {
        if (playerMana.CurrrentStat < manaCost)
        {
            flamethrower.Stop();
            shotSound.Stop();
            return;
        }

        flamethrower.Play();
        playerMana.CurrrentStat -= manaCost;
        if (!shotSound.isPlaying && !PauseMenu.GameIsPaused)
        {
            shotSound.Play();
        }
    }
}

