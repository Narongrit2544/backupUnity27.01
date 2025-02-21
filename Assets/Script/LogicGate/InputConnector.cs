using UnityEngine;
using System.Collections.Generic;

public class InputConnector : MonoBehaviour
{
    [Header("สถานะของ Input (true/false)")]
    public bool isOn = false; // สถานะของ Input (ค่าเริ่มต้น false)

    public Transform inPoint; // จุดรับข้อมูล (ต้องตั้งค่าใน Inspector)

    private List<NotGate> notGates = new List<NotGate>(); // เก็บ Not Gates ที่เชื่อมต่ออยู่
    private List<AndGate> andGates = new List<AndGate>(); // เก็บ And Gates ที่เชื่อมต่ออยู่
    private List<OrGate> orGates = new List<OrGate>(); // เก็บ Or Gates ที่เชื่อมต่ออยู่
    private List<NorGate> norGates = new List<NorGate>(); // เก็บ Nor Gates ที่เชื่อมต่ออยู่
    private List<XorGate> xorGates = new List<XorGate>(); // เก็บ Xor Gates ที่เชื่อมต่ออยู่
    private List<NandGate> nandGates = new List<NandGate>(); // เก็บ Nand Gates ที่เชื่อมต่ออยู่
    private List<XnorGate> xnorGates = new List<XnorGate>(); // เก็บ XnorGate ที่เชื่อมต่ออยู่

    private WireManager wireManager;
    private Renderer renderer;

    private List<OutputConnector> connectedOutputs = new List<OutputConnector>(); // เก็บ Output ที่เชื่อมต่ออยู่

    void Start()
    {
        wireManager = FindObjectOfType<WireManager>();
        renderer = GetComponent<Renderer>();
        UpdateColor();
    }

    //void OnMouseDown()
    //{
    //    if (wireManager != null)
    //    {
    //        wireManager.SelectInput(this);
    //    }
    //}
    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Collided with: " + other.gameObject.name);

        if (other.gameObject.name.Contains("Pinch"))
        {
            //Debug.Log("Pinch detected! Selecting input.");
            if (wireManager != null)
            {
                wireManager.RemoveWire(this); // ลบสายไฟเก่าก่อน
                wireManager.SelectInput(this);
            }
        }
    }
    void Update()
    {
        UpdateState();
    }
    public bool IsConnected()
    {
        return connectedOutputs.Count > 0; // ตรวจสอบว่ามี Output เชื่อมต่ออยู่หรือไม่
    }

    public void AddConnection(OutputConnector output)
    {
        if (!connectedOutputs.Contains(output))
        {
            connectedOutputs.Add(output);
            UpdateState();
        }
    }
    public void RemoveConnection(OutputConnector output)
    {
        if (connectedOutputs.Contains(output))
        {
            connectedOutputs.Remove(output);
            UpdateState(); // เรียก UpdateState() หลังจากลบสายไฟ
        }
    }

    public void ClearConnections()
    {
        connectedOutputs.Clear();
        isOn = false;
        //Debug.Log($"Input {gameObject.name} cleared connections. isOn = {isOn}");
        UpdateState();
    }

    public void AddNotGate(NotGate notGate)
    {
        if (!notGates.Contains(notGate))
        {
            notGates.Add(notGate);
        }
    }

    public void AddAndGate(AndGate andGate)
    {
        if (!andGates.Contains(andGate))
        {
            andGates.Add(andGate);
        }
    }

    public void AddOrGate(OrGate orGate)
    {
        if (!orGates.Contains(orGate))
        {
            orGates.Add(orGate);
        }
    }

    public void AddNorGate(NorGate norGate)
    {
        if (!norGates.Contains(norGate))
        {
            norGates.Add(norGate);
        }
    }

    public void AddXorGate(XorGate xorGate)
    {
        if (!xorGates.Contains(xorGate))
        {
            xorGates.Add(xorGate);
        }
    }

    public void AddNandGate(NandGate nandGate)
    {
        if (!nandGates.Contains(nandGate))
        {
            nandGates.Add(nandGate);
        }
    }

    public void AddXnorGate(XnorGate xnorGate)
    {
        if (!xnorGates.Contains(xnorGate))
        {
            xnorGates.Add(xnorGate);
        }
    }

    // ฟังก์ชัน UpdateState() ที่ปรับปรุงใหม่
    public void UpdateState()
    {
        // ถ้าไม่มี Output เชื่อมต่อ, ตั้งค่า isOn เป็น false แล้วอัปเดตส่วนที่เกี่ยวข้อง
        if (connectedOutputs.Count == 0)
        {
            if (isOn != false)
            {
                isOn = false;
                // อัปเดตให้ SevenSegmentDisplay
                SevenSegmentDisplay display = FindObjectOfType<SevenSegmentDisplay>();
                if (display != null)
                {
                    int binaryValue = display.GetCurrentValue();
                    display.UpdateDisplay(binaryValue);
                }
                // อัปเดต LED และ Buzzer ถ้ามี
                LED led = GetComponentInChildren<LED>();
                if (led != null) led.UpdateState();
                Buzzer buzzer = GetComponentInChildren<Buzzer>();
                if (buzzer != null) buzzer.UpdateState();

                // อัปเดตทุก Logic Gates ที่เชื่อมต่ออยู่
                foreach (var gate in notGates) gate.UpdateState();
                foreach (var gate in andGates) gate.UpdateState();
                foreach (var gate in orGates) gate.UpdateState();
                foreach (var gate in norGates) gate.UpdateState();
                foreach (var gate in xorGates) gate.UpdateState();
                foreach (var gate in nandGates) gate.UpdateState();
                foreach (var gate in xnorGates) gate.UpdateState();
                UpdateColor();
            }
            return;
        }

        // ถ้ามีการเชื่อมต่อ, คำนวณค่าใหม่จากสถานะของ Output ที่เชื่อมต่อ
        bool newState = false;
        foreach (var output in connectedOutputs)
        {
            if (output.isOn)
            {
                newState = true;
                break;
            }
        }
        if (isOn != newState)
        {
            isOn = newState;
            // อัปเดตให้ SevenSegmentDisplay
            SevenSegmentDisplay display = FindObjectOfType<SevenSegmentDisplay>();
            if (display != null)
            {
                int binaryValue = display.GetCurrentValue();
                display.UpdateDisplay(binaryValue);
            }
            // อัปเดต LED และ Buzzer ถ้ามี
            LED led = GetComponentInChildren<LED>();
            if (led != null) led.UpdateState();
            Buzzer buzzer = GetComponentInChildren<Buzzer>();
            if (buzzer != null) buzzer.UpdateState();

            // อัปเดตทุก Logic Gates ที่เชื่อมต่ออยู่
            foreach (var gate in notGates) gate.UpdateState();
            foreach (var gate in andGates) gate.UpdateState();
            foreach (var gate in orGates) gate.UpdateState();
            foreach (var gate in norGates) gate.UpdateState();
            foreach (var gate in xorGates) gate.UpdateState();
            foreach (var gate in nandGates) gate.UpdateState();
            foreach (var gate in xnorGates) gate.UpdateState();
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (renderer != null)
        {
            renderer.material.color = isOn ? Color.green : Color.red;
        }
    }
}
