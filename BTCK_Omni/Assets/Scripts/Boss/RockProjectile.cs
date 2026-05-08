using UnityEngine;
using UnityEngine.Pool;

public class RockProjectile : MonoBehaviour
{
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float dmg = 20f;

    [Header("SFX")]
    public AudioClip shootSound;
    [Range(0f, 3f)] public float shootSoundVolume = 1f;

    public AudioClip hitSound;
    [Range(0f, 3f)] public float hitSoundVolume = 1f;

    private Vector2 moveDir;
    private float speed;
    private bool hasHit = false;

    private IObjectPool<RockProjectile> pool;
    public void SetPool(IObjectPool<RockProjectile> p) => pool = p;

    public void Setup(Vector2 dir, float s)
    {
        CancelInvoke();
        hasHit = false;

        moveDir = dir.normalized;
        speed = s;

        PlaySfx(shootSound, shootSoundVolume);

        Invoke("DestroyRock", 5f);
    }

    void Update()
    {
        if (hasHit) return;

        float dist = speed * Time.deltaTime;
        LayerMask mask = playerLayer | groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, dist, mask);

        if (hit.collider != null)
        {
            transform.position = hit.point;
            int l = hit.collider.gameObject.layer;

            if (((1 << l) & playerLayer) != 0)
            {
                PlayerBase p = hit.collider.GetComponent<PlayerBase>();

                if (p != null) p.TakeDamage(dmg, moveDir);

                hasHit = true;
                PlaySfx(hitSound, hitSoundVolume);
                DestroyRock();
            }
            else if (((1 << l) & groundLayer) != 0)
            {
                hasHit = true;
                PlaySfx(hitSound, hitSoundVolume);
                DestroyRock();
            }
        }
        else
        {
            transform.Translate(moveDir * dist, Space.World);
        }
    }

    private void PlaySfx(AudioClip clip, float volumeMultiplier)
    {
        if (clip != null)
        {
            float baseVolume = 1f;
            if (AudioManager.Instance != null)
            {
                baseVolume = AudioManager.Instance.soundEffectsVolume;
            }

            GameObject tempAudioHost = new GameObject("TempRockAudio");
            tempAudioHost.transform.position = transform.position;

            AudioSource tempSource = tempAudioHost.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = baseVolume * volumeMultiplier;
            tempSource.spatialBlend = 0f;

            if (AudioManager.Instance != null && AudioManager.Instance.mainMixer != null)
            {
                UnityEngine.Audio.AudioMixerGroup[] groups = AudioManager.Instance.mainMixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                {
                    tempSource.outputAudioMixerGroup = groups[0];
                }
            }

            tempSource.Play();
            Destroy(tempAudioHost, clip.length);
        }
    }

    void DestroyRock()
    {
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}