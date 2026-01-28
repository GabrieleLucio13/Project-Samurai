using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Statistiche")]
    public int maxHealth = 100;
    protected int currentHealth;
    public EnemyUI healthBar;

    [Header("Movimento")]
    public float moveSpeed = 3f;
    protected Transform playerPosition;
    protected PlayerStatus playerStatus;
    protected Rigidbody rb;
    protected bool isDead;
    
    [Header("Riferimento")]
    [SerializeField] private Renderer debugRenderer;
    [SerializeField] private float damageColorDuration = 0.15f;
    private Color originalColor;
    private float damageDebugTimer;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerPosition = player.transform;
        playerStatus = player.GetComponent<PlayerStatus>();

        if (debugRenderer != null)
        originalColor = debugRenderer.material.color;
    }
    private void Update()
    {
        if (damageDebugTimer > 0f)
        {
            damageDebugTimer -= Time.deltaTime;

            if (damageDebugTimer <= 0f && debugRenderer != null)
                debugRenderer.material.color = originalColor;
        }
    }
    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        EnemyPhysicsUpdate();
    }
    protected abstract void EnemyPhysicsUpdate();
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
        
        PlayDamageEffect();

        if (currentHealth <= 0)
            Die();
    }
    protected virtual void PlayDamageEffect()
    {
        if (debugRenderer == null) return;

        damageDebugTimer = damageColorDuration;
        debugRenderer.material.color = Color.red;
    }
    public virtual void OnAttackHit(){}
    protected virtual void Die()
    {
        isDead = true;
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject);
    }

}
