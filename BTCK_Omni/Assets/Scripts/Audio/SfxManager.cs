using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip walkSound;
    [Range(0f, 1f)] public float walkSoundVolume = 1f;

    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpSoundVolume = 1f;

    public AudioClip rollSound;
    [Range(0f, 1f)] public float rollSoundVolume = 1f;

    public AudioClip hurtSound;
    [Range(0f, 1f)] public float hurtSoundVolume = 1f;

    public AudioClip dieSound;
    [Range(0f, 1f)] public float dieSoundVolume = 1f;

    public AudioClip attack1Sound;
    [Range(0f, 1f)] public float attack1SoundVolume = 1f;

    public AudioClip attack2Sound;
    [Range(0f, 1f)] public float attack2SoundVolume = 1f;

    public AudioClip attack3Sound;
    [Range(0f, 1f)] public float attack3SoundVolume = 1f;

    public AudioClip airAttackSound;
    [Range(0f, 1f)] public float airAttackSoundVolume = 1f;

    public AudioClip specialAttackSound;
    [Range(0f, 1f)] public float specialAttackSoundVolume = 1f;

    [Header("Charge Sound")]
    public AudioClip chargeSound;
    [Range(0f, 1f)] public float chargeSoundVolume = 1f;
    [Header("Interactions")]
    public AudioClip chestOpenSound;
    [Range(0f, 1f)] public float chestOpenSoundVolume = 1f;

    public AudioClip itemPickupSound;
    [Range(0f, 1f)] public float itemPickupSoundVolume = 1f;

    private float GetVolume()
    {
        if (AudioManager.Instance != null)
        {
            return AudioManager.Instance.soundEffectsVolume;
        }

        return 1f;
    }

    public void PlayWalk()
    {
        if (walkSound != null)
        {
            audioSource.PlayOneShot(walkSound, GetVolume() * walkSoundVolume);
        }
    }

    public void PlayJump()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, GetVolume() * jumpSoundVolume);
        }
    }

    public void PlayRoll()
    {
        if (rollSound != null)
        {
            audioSource.PlayOneShot(rollSound, GetVolume() * rollSoundVolume);
        }
    }

    public void PlayHurt()
    {
        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound, GetVolume() * hurtSoundVolume);
        }
    }

    public void PlayDie()
    {
        if (dieSound != null)
        {
            audioSource.PlayOneShot(dieSound, GetVolume() * dieSoundVolume);
        }
    }

    public void PlayAttack1()
    {
        if (attack1Sound != null)
        {
            audioSource.PlayOneShot(attack1Sound, GetVolume() * attack1SoundVolume);
        }
    }

    public void PlayAttack2()
    {
        if (attack2Sound != null)
        {
            audioSource.PlayOneShot(attack2Sound, GetVolume() * attack2SoundVolume);
        }
    }

    public void PlayAttack3()
    {
        if (attack3Sound != null)
        {
            audioSource.PlayOneShot(attack3Sound, GetVolume() * attack3SoundVolume);
        }
    }

    public void PlayAirAttack()
    {
        if (airAttackSound != null)
        {
            audioSource.PlayOneShot(airAttackSound, GetVolume() * airAttackSoundVolume);
        }
    }

    public void PlaySpecialAttack()
    {
        if (specialAttackSound != null)
        {
            audioSource.PlayOneShot(specialAttackSound, GetVolume() * specialAttackSoundVolume);
        }
    }

    public void PlayChargeLoop()
    {
        if (chargeSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.volume = GetVolume() * chargeSoundVolume;
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
        if (chestOpenSound != null)
        {
            audioSource.PlayOneShot(chestOpenSound, GetVolume() * chestOpenSoundVolume);
        }
    }

    public void PlayItemPickup()
    {
        if (itemPickupSound != null)
        {
            audioSource.PlayOneShot(itemPickupSound, GetVolume() * itemPickupSoundVolume);
        }
    }
}