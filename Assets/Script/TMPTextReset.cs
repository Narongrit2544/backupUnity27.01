using UnityEngine;
using TMPro;

public class TMPTextReset : MonoBehaviour
{
    [Header("Text ที่ต้องการรีเซ็ต")]
    public TMP_Text textToReset;

    [Header("ข้อความเริ่มต้น")]
    [TextArea]
    public string defaultText = "ข้อความเริ่มต้น";

    private void OnEnable()
    {
        if (textToReset != null)
        {
            textToReset.text = defaultText;
        }
    }
}
