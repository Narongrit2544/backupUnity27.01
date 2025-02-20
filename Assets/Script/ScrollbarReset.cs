using UnityEngine;
using UnityEngine.UI;

public class ScrollbarReset : MonoBehaviour
{
    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void OnEnable()
    {
        // ตั้งค่า value ตามที่ต้องการ (0 = ล่างสุด, 1 = บนสุด ในกรณี Vertical)
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
