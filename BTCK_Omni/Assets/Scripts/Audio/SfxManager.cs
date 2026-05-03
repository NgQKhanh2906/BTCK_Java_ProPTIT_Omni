using UnityEngine;

public class SfxManager : MonoBehaviour
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

    [Header("Charge Sound")]
    public AudioClip chargeSound;

    [Header("Interactions")]
    public AudioClip chestOpenSound;
    public AudioClip itemPickupSound;

    private float GetVolume()
    {
        if (AudioManager.instance != null)
        {
            return AudioManager.instance.soundEffectsVolume;
        }
        return 1f;
    }

    public void PlayWalk()
    {
        if (walkSound != null)
        {
            audioSource.PlayOneShot(walkSound, GetVolume());
        }
    }

    public void PlayJump()
    {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, GetVolume());
        }
    }

    public void PlayRoll()
    {
        if (rollSound != null)
        {
            audioSource.PlayOneShot(rollSound, GetVolume());
        }
    }

    public void PlayHurt()
    {
        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound, GetVolume());
        }
    }

    public void PlayDie()
    {
        if (dieSound != null)
        {
            audioSource.PlayOneShot(dieSound, GetVolume());
        }
    }

    public void PlayAttack1()
    {
        if (attack1Sound != null)
        {
            audioSource.PlayOneShot(attack1Sound, GetVolume());
        }
    }

    public void PlayAttack2()
    {
        if (attack2Sound != null)
        {
            audioSource.PlayOneShot(attack2Sound, GetVolume());
        }
    }

    public void PlayAttack3()
    {
        if (attack3Sound != null)
        {
            audioSource.PlayOneShot(attack3Sound, GetVolume());
        }
    }

    public void PlayAirAttack()
    {
        if (airAttackSound != null)
        {
            audioSource.PlayOneShot(airAttackSound, GetVolume());
        }
    }

    public void PlaySpecialAttack()
    {
        if (specialAttackSound != null)
        {
            audioSource.PlayOneShot(specialAttackSound, GetVolume());
        }
    }

    public void PlayChargeLoop()
    {
        if (chargeSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.volume = GetVolume();
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
            audioSource.PlayOneShot(chestOpenSound, GetVolume());
        }
    }

    public void PlayItemPickup()
    {
        if (itemPickupSound != null)
        {
            audioSource.PlayOneShot(itemPickupSound, GetVolume());
        }
    }
}