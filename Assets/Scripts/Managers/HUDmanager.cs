using UnityEngine;
public class HUDmanager : MonoBehaviour
{
    public PlayerStatus playerStatus;
    public HealthBarHUD healthHUD;
    public StaminaBarHUD staminaHUD;
    public AmmoCounter ammo_counter;
    void Start()
    {
        playerStatus.OnHealthChanged += healthHUD.UpdateHealth;
        playerStatus.OnStaminaChanged += staminaHUD.UpdateStamina;
        playerStatus.OnShurikenChanged += ammo_counter.UpdateAmmo;
    }

}
    
