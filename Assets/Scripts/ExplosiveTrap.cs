using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : Actor
{
    [Header("Trap")]
    [SerializeField] private float triggerDelay = 3f;
    [SerializeField] private float flashInterval = 0.25f;
    [SerializeField] private float detectRadius = 1.5f;

    private bool isTriggered = false;
    private float triggerTimer = 0f;
    private float flashTimer = 0f;

    protected override void Update()
    {
        base.Update();

        if (!isTriggered)
        {
            return;
        }

        triggerTimer += Time.deltaTime;
        flashTimer += Time.deltaTime;

        if (flashTimer >= flashInterval)
        {
            flashTimer = 0f;
            SpriteRenderer.enabled = !SpriteRenderer.enabled;
        }

        if (triggerTimer >= triggerDelay)
        {
            triggerTimer = 0f;
            Explode();
        }
    }

    protected void FixedUpdate()
    {
        if (!isTriggered && Physics2D.OverlapCircle(transform.position, detectRadius, enemyLayer))
        {
            isTriggered = true;
        }
    }

    private void Explode()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, attackDist, enemyLayer);

        foreach (Collider2D target in targets)
        {
            if (target.TryGetComponent<Actor>(out Actor actor))
            {
                Attack(actor);
            }
        }

        Die();
    }
}
