using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Movement & Combat Sounds")]
    public AudioClip walkSound;
    [Range(0f, 3f)] public float vWalk = 1f;

    public AudioClip jumpSound;
    [Range(0f, 3f)] public float vJump = 1f;

    public AudioClip rollSound;
    [Range(0f, 3f)] public float vRoll = 1f;

    public AudioClip hurtSound;
    [Range(0f, 3f)] public float vHurt = 1f;

    public AudioClip dieSound;
    [Range(0f, 3f)] public float vDie = 1f;

    public AudioClip attack1Sound;
    [Range(0f, 3f)] public float vAtk1 = 1f;

    public AudioClip attack2Sound;
    [Range(0f, 3f)] public float vAtk2 = 1f;

    public AudioClip attack3Sound;
    [Range(0f, 3f)] public float vAtk3 = 1f;

    public AudioClip airAttackSound;
    [Range(0f, 3f)] public float vAirAtk = 1f;

    public AudioClip specialAttackSound;
    [Range(0f, 3f)] public float vSpcAtk = 1f;

    [Header("Charge Sound")]
    public AudioClip chargeSound;
    [Range(0f, 3f)] public float vCharge = 1f;

    [Header("Interactions")]
    public AudioClip chestOpenSound;
    [Range(0f, 3f)] public float vChest = 1f;

    public AudioClip itemPickupSound;
    [Range(0f, 3f)] public float vItem = 1f;

    private float GetVolume()
    {
        if (AudioManager.Instance != null)
        {
            return AudioManager.Instance.soundEffectsVolume;
        }

        return 1f;
    }

    private void Phat(AudioClip c, float v)
    {
        if (c != null)
        {
            audioSource.PlayOneShot(c, GetVolume() * v);
        }
    }

    public void PlayWalk()
    {
        Phat(walkSound, vWalk);
    }

    public void PlayJump()
    {
        Phat(jumpSound, vJump);
    }

    public void PlayRoll()
    {
        Phat(rollSound, vRoll);
    }

    public void PlayHurt()
    {
        Phat(hurtSound, vHurt);
    }

    public void PlayDie()
    {
        Phat(dieSound, vDie);
    }

    public void PlayAttack1()
    {
        Phat(attack1Sound, vAtk1);
    }

    public void PlayAttack2()
    {
        Phat(attack2Sound, vAtk2);
    }

    public void PlayAttack3()
    {
        Phat(attack3Sound, vAtk3);
    }

    public void PlayAirAttack()
    {
        Phat(airAttackSound, vAirAtk);
    }

    public void PlaySpecialAttack()
    {
        Phat(specialAttackSound, vSpcAtk);
    }

    public void PlayChargeLoop()
    {
        if (chargeSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.volume = GetVolume() * vCharge;
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
        Phat(chestOpenSound, vChest);
    }

    public void PlayItemPickup()
    {
        Phat(itemPickupSound, vItem);
    }
}