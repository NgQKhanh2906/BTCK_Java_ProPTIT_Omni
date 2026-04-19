using System.Collections;
using UnityEngine;

public class FireTrap : TrapBase
{
    [SerializeField] private float fireDuration = 1f;
    [SerializeField] private float delay = 2f;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(FireLoop());
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