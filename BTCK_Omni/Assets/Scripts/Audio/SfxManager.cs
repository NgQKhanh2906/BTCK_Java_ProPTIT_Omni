using UnityEngine;

public class SfxManager : Singleton<SfxManager>
{
    public AudioSource audioSource;

    [Header("Movement & Action")]
    public AudioClip walkSound;
    [Range(0f, 1f)] public float walkVolume = 1f;

    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 1f;

    public AudioClip rollSound;
    [Range(0f, 1f)] public float rollVolume = 1f;

    [Header("Combat")]
    public AudioClip hurtSound;
    [Range(0f, 1f)] public float hurtVolume = 1f;

    public AudioClip dieSound;
    [Range(0f, 1f)] public float dieVolume = 1f;

    public AudioClip attack1Sound;
    [Range(0f, 1f)] public float attack1Volume = 1f;

    public AudioClip attack2Sound;
    [Range(0f, 1f)] public float attack2Volume = 1f;

    public AudioClip attack3Sound;
    [Range(0f, 1f)] public float attack3Volume = 1f;

    public AudioClip airAttackSound;
    [Range(0f, 1f)] public float airAttackVolume = 1f;

    public AudioClip specialAttackSound;
    [Range(0f, 1f)] public float specialAttackVolume = 1f;

    [Header("Charge Sound")]
    public AudioClip chargeSound;
    [Range(0f, 1f)] public float chargeVolume = 1f;

    [Header("Interactions")]
    public AudioClip chestOpenSound;
    [Range(0f, 1f)] public float chestOpenVolume = 1f;

    public AudioClip itemPickupSound;
    [Range(0f, 1f)] public float itemPickupVolume = 1f;

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayWalk()
    {
        PlaySfx(walkSound, walkVolume);
    }

    public void PlayJump()
    {
        PlaySfx(jumpSound, jumpVolume);
    }

    public void PlayRoll()
    {
        PlaySfx(rollSound, rollVolume);
    }

    public void PlayHurt()
    {
        PlaySfx(hurtSound, hurtVolume);
    }

    public void PlayDie()
    {
        PlaySfx(dieSound, dieVolume);
    }

    public void PlayAttack1()
    {
        PlaySfx(attack1Sound, attack1Volume);
    }

    public void PlayAttack2()
    {
        PlaySfx(attack2Sound, attack2Volume);
    }

    public void PlayAttack3()
    {
        PlaySfx(attack3Sound, attack3Volume);
    }

    public void PlayAirAttack()
    {
        PlaySfx(airAttackSound, airAttackVolume);
    }

    public void PlaySpecialAttack()
    {
        PlaySfx(specialAttackSound, specialAttackVolume);
    }

    public void PlayChargeLoop()
    {
        if (chargeSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.volume = chargeVolume;
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
        PlaySfx(chestOpenSound, chestOpenVolume);
    }

    public void PlayItemPickup()
    {
        PlaySfx(itemPickupSound, itemPickupVolume);
    }
}