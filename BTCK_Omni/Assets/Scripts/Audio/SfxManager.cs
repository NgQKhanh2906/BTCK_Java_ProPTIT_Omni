using UnityEngine;

public class SfxManager : Singleton<SfxManager>
{
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip rollSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    public AudioClip attack1Sound;
    public AudioClip attack2Sound;
    public AudioClip attack3Sound;
    public AudioClip airAttackSound;
    public AudioClip specialAttackSound;

    [Header("Charge Sound")] public AudioClip chargeSound;
    [Header("Interactions")] public AudioClip chestOpenSound;
    public AudioClip itemPickupSound;

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, 1f);
        }
    }

    public void PlayWalk() 
    {
        PlaySfx(walkSound);
    }
    public void PlayJump() 
    { 
        PlaySfx(jumpSound);
    }
    public void PlayRoll() 
    {
        PlaySfx(rollSound); 
    }
    public void PlayHurt() 
    { 
        PlaySfx(hurtSound);
    }
    public void PlayDie() 
    { 
        PlaySfx(dieSound);
    }
    public void PlayAttack1() 
    { 
        PlaySfx(attack1Sound); 
    }
    public void PlayAttack2() 
    { 
        PlaySfx(attack2Sound);
    }
    public void PlayAttack3() 
    { 
        PlaySfx(attack3Sound); 
    }
    public void PlayAirAttack() 
    { 
        PlaySfx(airAttackSound); 
    }
    public void PlaySpecialAttack() 
    { 
        PlaySfx(specialAttackSound);
    }

    public void PlayChargeLoop()
    {
        if (chargeSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopChargeLoop()
    {
        if (audioSource.clip == chargeSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    public void PlayChestOpen() 
    { 
        PlaySfx(chestOpenSound); 
    }
    public void PlayItemPickup() 
    { 
        PlaySfx(itemPickupSound); 
    }
}