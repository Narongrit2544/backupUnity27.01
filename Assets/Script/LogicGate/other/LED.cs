using UnityEngine;

public class LED : MonoBehaviour
{
    [Header("Input ที่ใช้ควบคุม LED (ต้องเชื่อมต่อกับ Output ของ Gate เท่านั้น)")]
    public InputConnector input;  // รับค่าจาก InputConnector ของวงจรดิจิตอล

    [Header("วัตถุที่ต้องการควบคุมสีเพิ่มเติม")]
    public GameObject targetObject; // วัตถุที่ต้องการให้เปลี่ยนสีตาม LED

    private Renderer ledRenderer;
    private Renderer targetRenderer;
    private Light targetLight; // ใช้สำหรับแสดงแสง
    private Material targetMaterial; // เก็บ Material ของ targetObject

    // เพิ่ม property isOn เพื่อให้ QuizManager2 เรียกใช้งานได้
    public bool isOn
    {
        get { return input != null ? input.isOn : false; }
    }

    void Start()
    {
        ledRenderer = GetComponent<Renderer>();

        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();
            targetMaterial = targetRenderer.material; // ดึง Material มาใช้

            targetLight = targetObject.GetComponent<Light>();

            // ถ้า targetObject ไม่มี Light ให้เพิ่มเข้าไป
            if (targetLight == null)
            {
                targetLight = targetObject.AddComponent<Light>();
                targetLight.type = LightType.Point; // ใช้แสงแบบ Point Light
                targetLight.range = 2.5f; // กำหนดระยะของแสง
                targetLight.intensity = 0f; // เริ่มต้นที่ 0 (ปิดแสง)
                targetLight.color = Color.red; // ตั้งค่าให้แสงเป็นสีแดง
            }
        }
        
        // หมายเหตุ: อย่าเปลี่ยนแปลง input หรือชื่อต่าง ๆ ที่เชื่อมต่อกับวงจร
        // เพื่อให้แน่ใจว่า LED จะอัปเดตจากผลลัพธ์ของวงจรดิจิตอลเท่านั้น

        // อัปเดตสถานะของ LED ครั้งแรก (เรียกโดย ForceUpdateCircuit() ในระบบวงจร)
        UpdateState();
    }

    // ปิดการเรียก Update() อัตโนมัติ เพื่อป้องกันการรีเฟรชสถานะจากการเปลี่ยนแปลงโดยตรง
    // ให้ LED อัปเดตเฉพาะเมื่อวงจร (Gate) เรียก ForceUpdateCircuit() เพื่อควบคุมการคำนวณ
    void Update()
    {
        UpdateState();
    }
    
    // ฟังก์ชัน UpdateState() จะอัปเดตสถานะของ LED จาก input ที่คำนวณมาจากวงจรดิจิตอล
    public void UpdateState()
    {
        if (input != null)
        {
            // ใช้ input.isOn ในการคำนวณสถานะของ LED
            bool isActive = input.isOn;

            // เปลี่ยนสีของ LED ตามสถานะที่คำนวณได้
            if (ledRenderer != null)
            {
                ledRenderer.material.color = isActive ? Color.red : Color.gray;
            }

            // เปลี่ยนสีของ targetObject และเพิ่ม Emission ให้เรืองแสง
            if (targetRenderer != null)
            {
                targetRenderer.material.color = isActive ? Color.red : Color.gray;

                if (targetMaterial != null)
                {
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
            }

            // ควบคุมแสงของ targetObject
            if (targetLight != null)
            {
                targetLight.intensity = isActive ? 5f : 0f;
            }
        }
    }
}
