using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    public TMP_Text shurikenText;

    public void UpdateAmmo(int current, int max)
    {
        shurikenText.text = $"{current}";
    }
}
