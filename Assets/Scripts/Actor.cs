using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private ItemDrop[] itemDrops;

    [Header("Combat")]
    [SerializeField] protected Item requiredTool;
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float attackCooldown = 0.5f;
    [SerializeField] protected float knockbackForce = 3f;
    [SerializeField] protected float knockbackDuration = 0.8f;
    [SerializeField] protected float attackDist = 1.5f;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float pixelExplosionStrength = 1f;

    [Header("Animation")]
    [SerializeField] private bool isAnimated = false;
    [SerializeField] protected Animation2D idleAnim;
    [SerializeField] protected Animation2D moveAnim;
    [SerializeField] protected Animation2D attackAnim;

    private int health;
    protected bool canAttack = true;

    public Animator2D Animator2D { get; private set; }
    public Rigidbody2D Rigidbody2D { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    private void Awake()
    {
        Animator2D = GetComponent<Animator2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        health = maxHealth;
    }

    protected virtual void Update()
    {
        if (!isAnimated)
        {
            return;
        }

        if (Mathf.Abs(GetMoveDir().x) > 0.1f)
        {
            Animator2D.Play(moveAnim, true);

            SpriteRenderer.flipX = GetMoveDir().x < 0;
        }
        else
        {
            Animator2D.Play(idleAnim, true);
        }
    }

    public void Attack(Actor other)
    {
        if (other.requiredTool != null && !GameManager.InventorySystem.HasTool(other.requiredTool))
        {
            GameManager.InventorySystem.AddEventLog("Missing required tool (" + other.requiredTool.displayName + ")");
            return;
        }

        Vector2 hitDir = (other.transform.position - transform.position).normalized;
        other.ApplyKnockback(hitDir * knockbackForce, knockbackDuration);
        other.TakeDamage(damage);
    }

    public virtual void ApplyKnockback(Vector2 force, float duration)
    {
        Rigidbody2D.AddForce(force, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        ExplodeSprite();

        foreach (ItemDrop itemDrop in itemDrops)
        {
            int count = Random.Range(itemDrop.minCount, itemDrop.maxCount);
            GameManager.InventorySystem.AddItem(itemDrop.item, count);
        }

        Destroy(gameObject);
    }

    protected virtual bool IsMoving()
    {
        return Mathf.Abs(GetMoveDir().x) > 0.01f;
    }

    public virtual Vector2 GetMoveDir()
    {
        return Rigidbody2D.velocity.normalized;
    }

    public virtual Vector2 GetAttackDir()
    {
        return Vector2.right;
    }

    protected IEnumerator AttackCooldown(float cooldown)
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    protected void ExplodeSprite()
    {
        Texture2D tex = SpriteRenderer.sprite.texture;

        Vector2 pos = transform.position;
        Vector2 scale = transform.localScale;
        Vector2 origin = pos - scale / 2f;

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                Color col = SpriteRenderer.sprite.texture.GetPixel(i, j);

                if (col.a > Mathf.Epsilon)
                {
                    Vector2 pixelPos = origin + new Vector2(i / 16f, j / 16f);
                    GameObject pixel = Instantiate(GameManager.PixelPrefab, pixelPos, Quaternion.identity);

                    float randForceUp = Random.Range(0.5f, 1.5f) * pixelExplosionStrength;
                    float randForceSide = Random.Range(-1f, 1f) * pixelExplosionStrength;
                    pixel.GetComponent<Rigidbody2D>().AddForce(new Vector2(randForceSide, randForceUp), ForceMode2D.Impulse);
                }
            }
        }
    }

    [System.Serializable]
    private class ItemDrop
    {
        public Item item;
        public int minCount = 0;
        public int maxCount = 1;
    }
}
