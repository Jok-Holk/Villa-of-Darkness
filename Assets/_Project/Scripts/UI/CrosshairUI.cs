using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private Image dotIcon;       // dot nhỏ mặc định
    [SerializeField] private Image handIcon;      // icon bàn tay

    private void Awake()
    {
        SetHandIcon(false);
    }

    public void SetHandIcon(bool showHand)
    {
        dotIcon.gameObject.SetActive(!showHand);
        handIcon.gameObject.SetActive(showHand);
    }
}