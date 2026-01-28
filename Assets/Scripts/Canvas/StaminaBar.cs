using UnityEngine;
using UnityEngine.UI;

public class StaminaBarHUD : MonoBehaviour
{
    public Image fillImage;

    public void UpdateStamina(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}

