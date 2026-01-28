using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private WeaponController weaponController;
    private PlayerController player;

    private void Awake()
    {
        weaponController = GetComponentInParent<WeaponController>();
        player = GetComponentInParent<PlayerController>();
    }

     // PLAYER EVENTS
    public void Anim_AttackStart() => player.Anim_AttackStart();
    public void Anim_AttackEnd() => player.Anim_AttackEnd();
    public void Anim_ComboWindowOpen() => player.Anim_ComboWindowOpen();
    public void Anim_ComboWindowClose() => player.Anim_ComboWindowClose();
    public void Anim_DodgeEnd() => player.Anim_DodgeEnd();
    public void Anim_SheathEnd() => player.Anim_SheathEnd();
    public void Anim_DodgeIFrameEnd() => player.Anim_DodgeIFrameEnd();

    //WEAPON EVENTS
    public void EquipSword() => weaponController?.Equip();
    public void UnequipSword() => weaponController?.Unequip();
    public void SpawnSlash() => weaponController?.SpawnSlash();
    public void SpawnShuriken() => weaponController?.SpawnShuriken();


}


