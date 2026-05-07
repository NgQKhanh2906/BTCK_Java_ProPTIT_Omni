using System.Collections;
using UnityEngine;

public class FireTrap : TrapBase
{
    [SerializeField] private float fireDuration = 1f;
    [SerializeField] private float delay = 2f;

    private Animator anim;
    private Coroutine fireCoroutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
        }
        fireCoroutine = StartCoroutine(FireLoop());
    }

    private void OnDisable()
    {
        if (anim != null)
        {
            anim.SetBool("Fire", false);
        }
    }

    private IEnumerator FireLoop()
    {
        while (true)
        {
            anim.SetBool("Fire", true);
            yield return new WaitForSeconds(fireDuration);
            anim.SetBool("Fire", false);
            yield return new WaitForSeconds(delay);
        }
    }
}