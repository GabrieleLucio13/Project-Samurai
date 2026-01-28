using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Image fillImage;
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float targetFill = currentHealth / maxHealth;
        fillImage.fillAmount = targetFill;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.forward);
    }
}