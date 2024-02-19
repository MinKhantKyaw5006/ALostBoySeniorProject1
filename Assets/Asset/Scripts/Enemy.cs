using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;

    // Existing enemy states
    protected enum EnemyStates
    {
        Idle, // Added Idle state
        Crawler_Idle,
        Crawler_Flip,
        Following // Added Following state for when the zombie detects the player
    }

    protected EnemyStates currentEnemyState = EnemyStates.Idle;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
    }

    protected virtual void Update()
    {
        UpdateEnemyStates();

        if (health <= 0)
        {
            Destroy(gameObject);
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    protected void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }

    protected virtual void UpdateEnemyStates() { }

    protected void ChangeState(EnemyStates _newState)
    {
        currentEnemyState = _newState;
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}

