using UnityEngine;

public class LED : MonoBehaviour
{
    [Header("Input ที่ใช้ควบคุม LED")]
    public InputConnector input;  // รับค่าจาก InputConnector

    [Header("วัตถุที่ต้องการควบคุมสีเพิ่มเติม")]
    public GameObject targetObject; // วัตถุที่ต้องการให้เปลี่ยนสีตาม LED

    private Renderer ledRenderer;
    private Renderer targetRenderer;
    private Light targetLight; // สำหรับแสดงแสง
    private Material targetMaterial; // เก็บ Material ของ targetObject

    void Start()
    {
        ledRenderer = GetComponent<Renderer>();

        // ถ้า input ยังไม่ถูกกำหนด ให้พยายามหาใน children
        if (input == null)
        {
            input = GetComponentInChildren<InputConnector>();
            if (input == null)
            {
                // ถ้ายังหาไม่เจอ ให้เพิ่ม Component ใหม่ลงใน GameObject นี้
                input = gameObject.AddComponent<InputConnector>();
            }
        }

        // ตั้งชื่อ Child Connector ให้ตรงกับรูปแบบที่ต้องการ เช่น "LED_1_IN"
        // โดยใช้ชื่อของ LED ที่เป็น parent แล้วต่อด้วย "_IN"
        if (input != null)
        {
            input.gameObject.name = $"{gameObject.name}_IN1";
            Debug.Log("LED InputConnector name set to: " + input.gameObject.name);
        }

        // ตรวจสอบ targetObject และเพิ่ม light หากไม่มี
        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetMaterial = targetRenderer.material;
            }
            targetLight = targetObject.GetComponent<Light>();
            if (targetLight == null)
            {
                targetLight = targetObject.AddComponent<Light>();
                targetLight.type = LightType.Point;
                targetLight.range = 2.5f;
                targetLight.intensity = 0f;
                targetLight.color = Color.red;
            }
        }

        UpdateState();
        // เรียก UpdateState อีกครั้งถ้าจำเป็น
        UpdateState();
    }

    void Update()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        if (input != null)
        {
            bool isActive = input.isOn;

            // เปลี่ยนสีของ LED
            if (ledRenderer != null)
            {
                ledRenderer.material.color = isActive ? Color.red : Color.gray;
            }

            // เปลี่ยนสีของ targetObject และปรับ Emission
            if (targetRenderer != null && targetMaterial != null)
            {
                targetRenderer.material.color = isActive ? Color.red : Color.gray;
                if (isActive)
                {
                    targetMaterial.EnableKeyword("_EMISSION");
                    targetMaterial.SetColor("_EmissionColor", Color.red * 2f);
                }
                else
                {
                    targetMaterial.DisableKeyword("_EMISSION");
                }
            }

            // ควบคุมแสง
            if (targetLight != null)
            {
                targetLight.intensity = isActive ? 5f : 0f;
            }
        }
    }
}
