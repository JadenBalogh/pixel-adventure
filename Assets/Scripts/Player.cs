using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Actor
{
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float attackCameraShake = 2f;
    [SerializeField] private float hitCameraShake = 10f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundedDist;
    [SerializeField] private LayerMask groundMask;

    [Header("Tools")]
    [SerializeField] private Item wallJumpTool;

    private bool isGrounded = true;
    private bool isAgainstWall = false;
    private bool isMoving = false;
    private bool isInputLocked = false;

    protected override void Update()
    {
        float horizontal = isInputLocked ? Rigidbody2D.velocity.x : Input.GetAxisRaw("Horizontal") * moveSpeed;
        float vertical = Rigidbody2D.velocity.y;

        isMoving = horizontal != 0;

        if (CanJump() && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
        {
            vertical = jumpForce;
        }

        if (canAttack && Input.GetButtonDown("Fire1"))
        {
            Animator2D.Play(attackAnim, false, true);

            StartCoroutine(AttackCooldown(attackCooldown));

            StartCoroutine(DelayAttack());
        }

        Rigidbody2D.velocity = Vector2.right * horizontal + Vector2.up * vertical;

        base.Update();
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.BoxCast(transform.position, new Vector2(0.4f, 1f), 0, Vector2.down, groundedDist, groundMask);
        isAgainstWall = Physics2D.OverlapBox(transform.position, new Vector2(1f, 0.6f), 0, groundMask);
    }

    private bool CanJump()
    {
        if (isInputLocked)
        {
            return false;
        }

        return isGrounded || (isAgainstWall && IsWallJumpUnlocked());
    }

    private bool IsWallJumpUnlocked()
    {
        return GameManager.InventorySystem.HasTool(wallJumpTool);
    }

    protected override bool IsMoving()
    {
        return isMoving;
    }

    public override void Die()
    {
        ExplodeSprite();

        StopAllCoroutines();
        isInputLocked = true;
        SpriteRenderer.enabled = false;

        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void ApplyKnockback(Vector2 force, float duration)
    {
        base.ApplyKnockback(force, duration);

        StartCoroutine(KnockbackTimer(duration));
    }

    private IEnumerator KnockbackTimer(float duration)
    {
        isInputLocked = true;
        yield return new WaitForSeconds(duration);
        isInputLocked = false;
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        CameraController.CameraShake(attackCameraShake);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, GetAttackDir(), attackDist, enemyLayer);

        if (hit && hit.collider.TryGetComponent<Actor>(out Actor target))
        {
            CameraController.CameraShake(hitCameraShake);
            Attack(target);
        }
    }

    public override Vector2 GetAttackDir()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - transform.position).normalized;
    }
}
