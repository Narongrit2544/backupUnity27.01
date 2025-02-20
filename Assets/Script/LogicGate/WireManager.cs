using UnityEngine;
using System.Collections.Generic;

public class WireManager : MonoBehaviour
{
    public GameObject wirePrefab; // Prefab สำหรับสายไฟ

    // สำหรับเล่นเสียงคลิก
    public AudioSource clickAudioSource;  // กำหนดใน Inspector ให้เป็น GameObject ที่มี AudioSource
    public AudioClip clickSound;          // ไฟล์เสียงคลิกที่นำเข้ามาในโปรเจค
    private OutputConnector firstOutput = null; // เก็บ Output ที่ถูกคลิกตัวแรก
    private Dictionary<(OutputConnector, InputConnector), GameObject> wireConnections = new Dictionary<(OutputConnector, InputConnector), GameObject>(); // เก็บสายไฟที่เชื่อมต่อกัน

    void Update()
    {
        foreach (var connection in wireConnections)
        {
            OutputConnector start = connection.Key.Item1;
            InputConnector end = connection.Key.Item2;
            GameObject wire = connection.Value;

            if (wire != null && start != null && end != null)
            {
                LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, start.outPoint.position);
                    lineRenderer.SetPosition(1, end.inPoint.position);
                    lineRenderer.startColor = start.isOn ? Color.red : Color.black;
                    lineRenderer.endColor = end.isOn ? Color.red : Color.black;
                }
            }
        }
    }

    public void SelectOutput(OutputConnector output)
    {
        //Debug.Log("SelectOutput called");
        // เล่นเสียงเมื่อเลือก Output
        if (clickAudioSource != null && clickSound != null)
        {
            clickAudioSource.PlayOneShot(clickSound);
        }
        if (firstOutput == null)
        {
            firstOutput = output;
        }
    }

    public void SelectInput(InputConnector input)
    {

        // เล่นเสียงเมื่อเลือก Input
        if (clickAudioSource != null && clickSound != null)
        {
            clickAudioSource.PlayOneShot(clickSound);
        }

        if (firstOutput != null)
        {
            RemoveWire(input);
            CreateWire(firstOutput, input);
            firstOutput = null;
        }
    }
    public Dictionary<(OutputConnector, InputConnector), GameObject> GetWireConnections()
    {
        return wireConnections;
    }

    private void CreateWire(OutputConnector output, InputConnector input)
    {
        if (!wireConnections.ContainsKey((output, input)))
        {
            GameObject wire = Instantiate(wirePrefab);
            LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, output.outPoint.position);
                lineRenderer.SetPosition(1, input.inPoint.position);
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;

                Color wireColor = output.isOn ? Color.green : Color.red;
                lineRenderer.startColor = wireColor;
                lineRenderer.endColor = wireColor;
            }

            wireConnections[(output, input)] = wire;
            output.AddConnection(input);

            output.UpdateState();
            input.UpdateState();
        }
    }

    public void UpdateWireColor(OutputConnector output)
    {
        foreach (var connection in wireConnections)
        {
            if (connection.Key.Item1 == output)
            {
                LineRenderer lineRenderer = connection.Value.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    Color wireColor = output.isOn ? Color.green : Color.red;
                    lineRenderer.startColor = wireColor;
                    lineRenderer.endColor = wireColor;
                }
            }
        }
    }

    public void RemoveWire(InputConnector input)
    {
        List<(OutputConnector, InputConnector)> connectionsToRemove = new List<(OutputConnector, InputConnector)>();

        foreach (var connection in wireConnections)
        {
            if (connection.Key.Item2 == input)
            {
                connectionsToRemove.Add(connection.Key);
            }
        }

        foreach (var key in connectionsToRemove)
        {
            OutputConnector output = key.Item1;
            output.RemoveConnection(input);
            input.ClearConnections();
            Destroy(wireConnections[key]);
            wireConnections.Remove(key);
        }

        input.UpdateState();
    }
}
