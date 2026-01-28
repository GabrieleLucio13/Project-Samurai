using UnityEngine;
using UnityEngine.UI;

public class HealthBarHUD : MonoBehaviour
{
    public Image fillImage;
    public float speed = 5f;

    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }


}
