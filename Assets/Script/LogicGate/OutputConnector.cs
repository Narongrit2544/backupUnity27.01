using UnityEngine;
using System.Collections.Generic;

public class OutputConnector : MonoBehaviour
{
    [Header("สถานะของ Output (true/false)")]
    public bool isOn = false;

    public Transform outPoint;
    private List<InputConnector> connectedInputs = new List<InputConnector>();

    private WireManager wireManager;
    private Renderer renderer;
    private static int outputIDCounter = 0; // ใช้แยก ID ของแต่ละ Output
    public int outputID; // ระบุ ID ของแต่ละ OutputConnector

    void Start()
    {
        wireManager = FindObjectOfType<WireManager>();
        if (wireManager == null)
        {
            Debug.LogError("WireManager not found in the scene! Make sure WireManager is added.");
        }
        outputID = ++outputIDCounter; // ให้ ID ไม่ซ้ำกัน
        wireManager = FindObjectOfType<WireManager>();
        renderer = GetComponent<Renderer>();
        UpdateColor();
    }
    public bool IsConnected()
    {
        return connectedInputs.Count > 0; // ตรวจสอบว่ามี Input เชื่อมต่ออยู่หรือไม่
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Pinch"))
        {
            //Debug.Log("Pinch detected! Selecting output.");
            if (wireManager != null)
            {
                wireManager.SelectOutput(this);
            }
        }
    }

    void Update()
    {
        UpdateState();
    }

    public void AddConnection(InputConnector input)
    {
        if (input != null && !connectedInputs.Contains(input))
        {
            connectedInputs.Add(input);
            input.AddConnection(this);
        }
        UpdateState();
    }

    public void RemoveConnection(InputConnector input)
    {
        if (input != null && connectedInputs.Contains(input))
        {
            connectedInputs.Remove(input);
            input.RemoveConnection(this);
        }
        UpdateState();
    }

    public void UpdateState()
    {
        if (connectedInputs == null) return; // ป้องกันการเรียกใช้ null

        foreach (var input in connectedInputs)
        {
            if (input != null)
            {
                input.UpdateState();
            }
        }

        UpdateColor();

        if (wireManager != null)
        {
            wireManager.UpdateWireColor(this); // อัปเดตสีของสายไฟเมื่อสถานะเปลี่ยน
        }
        else
        {
            Debug.LogWarning("WireManager is null in OutputConnector!");
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