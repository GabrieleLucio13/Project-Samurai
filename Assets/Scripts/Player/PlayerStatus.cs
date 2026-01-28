using UnityEngine;
using System;

public class PlayerStatus : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 30f;
    [SerializeField] private float staminaRegenDelay = 1.5f; 
    
    [Header("Shuriken")]
    [SerializeField] private int maxShuriken = 10;  
    
    public float health;
    public float stamina;
    private float regenTimer;
    private bool regenBlock;
    public int currentShuriken;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<int, int> OnShurikenChanged;
    public bool IsDead = false;
    void Start()
    {
        health = maxHealth;
        stamina = maxStamina;
        currentShuriken = maxShuriken;

        NotifyAll();
    }
    void NotifyAll()
    {
        OnHealthChanged?.Invoke(health, maxHealth);
        OnStaminaChanged?.Invoke(stamina, maxStamina);
        OnShurikenChanged?.Invoke(currentShuriken, maxShuriken);
    }
    void Update()
    {
        HandleStaminaRegen();
    }
    public void heal()
    {
       if (IsDead) return; 
    }
    public void TakeDamage(float value)
    {
        if (IsDead) return;

        health = Mathf.Clamp(health - value, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0)
        {
            IsDead = true;
        }
    }
    public bool TryUseStamina(float value)
    {
        if (stamina < value)
            return false;

        stamina -= value;
        regenTimer = staminaRegenDelay;

        OnStaminaChanged?.Invoke(stamina, maxStamina);
        return true;
    }
    public bool TryUseShuriken()
    {
        if (currentShuriken <= 0)
            return false;
        
        currentShuriken--;
        OnShurikenChanged?.Invoke(currentShuriken, maxShuriken);
        
        return true;
    }
    private void HandleStaminaRegen()
    {
        if (stamina >= maxStamina || regenBlock)
            return;

        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        stamina += staminaRegenRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        OnStaminaChanged?.Invoke(stamina, maxStamina);
    }
    public void SetRegenBlocked(bool value)
    {
        regenBlock = value;
    }

    
}
