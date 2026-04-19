using UnityEngine;

public class ArcherController : PlayerBase
{
    [Header("Shoot Setup")]
    [SerializeField] private Arrow arrowPf;
    [SerializeField] private Arrow atk3TriggerPf;
    [SerializeField] private Transform firePt;
    [SerializeField] private Beam beamPf;

    private float manaCostAttack2 = 1f;
    private float manaCostAttack3 = 20f;
    private float manaCostSpecial = 70f;

    private float damageAttack1 = 20f;
    private float damageAttack2 = 30f;
    private float damageAttack3 = 50f;
    private float damageAirAttack = 35f;
    private float damageSpecialAttack = 100f;

    private KeyCode keyAttack2 = KeyCode.J;
    private KeyCode keyAttack3 = KeyCode.U;
    private KeyCode keySpecialAttack = KeyCode.I;

    private static string ANIMATION_ATTACK2 = "Atk2";
    private static string ANIMATION_ATTACK3 = "Atk3";
    private static string ANIMATION_SPECIAL = "SpAtk";

    private bool hasUsedAirAttack2;
    private int localJumpCount;
    private int autoMaxJumps = 1;

    protected override void Awake()
    {
        base.Awake();

        System.Reflection.FieldInfo field = typeof(PlayerBase).GetField("maxJumps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            autoMaxJumps = (int)field.GetValue(this);
        }
    }

    protected override void OnUpdate()
    {
        if (isGrounded)
        {
            hasUsedAirAttack2 = false;
            localJumpCount = 0;
        }

        if (Input.GetKeyDown(keyJump) && !isAttacking && !isDefending && !isRolling)
        {
            if (isGrounded || localJumpCount < autoMaxJumps)
            {
                localJumpCount++;
                hasUsedAirAttack2 = false;
            }
        }

        if (isRolling || isDefending || isAttacking)
        {
            return;
        }

        HandleSpecialAttack();
        HandleAttack2();
        HandleAttack3();
    }

    private void HandleSpecialAttack()
    {
        if (Input.GetKeyDown(keySpecialAttack) && isGrounded)
        {
            if (currentMana >= manaCostSpecial)
            {
                RestoreMana(-manaCostSpecial);
                isAttacking = true;
                anim.SetTrigger(ANIMATION_SPECIAL);
                SetVelocityX(0);
            }
        }
    }

    private void HandleAttack2()
    {
        if (Input.GetKeyDown(keyAttack2))
        {
            if (isGrounded)
            {
                if (currentMana >= manaCostAttack2)
                {
                    RestoreMana(-manaCostAttack2);
                    isAttacking = true;
                    anim.SetTrigger(ANIMATION_ATTACK2);
                    SetVelocityX(0);
                }
            }
            else if (!hasUsedAirAttack2)
            {
                hasUsedAirAttack2 = true;
                isAttacking = true;
                anim.SetTrigger(GameConfig.ANIM_COL_AIRATTACK);
            }
        }
    }

    private void HandleAttack3()
    {
        if (Input.GetKeyDown(keyAttack3) && isGrounded)
        {
            if (currentMana >= manaCostAttack3)
            {
                RestoreMana(-manaCostAttack3);
                isAttacking = true;
                anim.SetTrigger(ANIMATION_ATTACK3);
                SetVelocityX(0);
            }
        }
    }

    protected override void Attack(bool hasUsedAirAttack)
    {
        if (hasUsedAirAttack)
        {
            return;
        }

        isAttacking = true;
        SetVelocityX(0);
        anim.SetTrigger(GameConfig.ANIM_COL_ATTACK);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void ShootAtk1()
    {
        SpawnProjectile(arrowPf, damageAttack1, 0, 0f, false);
    }

    public void ShootAtk2()
    {
        SpawnProjectile(arrowPf, damageAttack2, 1, 0f, false);
    }

    public void ShootAtk3()
    {
        SpawnProjectile(atk3TriggerPf, damageAttack3, 2, 0f, true);
    }

    public void ShootAir()
    {
        SpawnProjectile(arrowPf, damageAirAttack, 1, -45f, false);
    }

    public void ShootSp()
    {
        if (!beamPf || !firePt)
        {
            return;
        }

        Beam beam = Instantiate(beamPf, firePt.position, Quaternion.identity);
        beam.Setup(facingDir, damageSpecialAttack);
    }

    private void SpawnProjectile(Arrow prefab, float damage, int type, float angle = 0f, bool isAttack3 = false)
    {
        if (!prefab || !firePt)
        {
            return;
        }

        Arrow arrow = Instantiate(prefab, firePt.position, Quaternion.identity);
        arrow.Setup(facingDir, damage, type, angle, isAttack3);
    }
}