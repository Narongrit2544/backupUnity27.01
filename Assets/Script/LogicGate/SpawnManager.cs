using UnityEngine;
using System.Collections.Generic;
using TMPro; // สำหรับ TextMeshPro

public class SpawnManager : MonoBehaviour
{
    [Header("Prefab ของอุปกรณ์ต่างๆ")]
    public GameObject andGatePrefab;
    public GameObject orGatePrefab;
    public GameObject norGatePrefab;
    public GameObject nandGatePrefab;
    public GameObject xorGatePrefab;
    public GameObject xnorGatePrefab;
    public GameObject notGatePrefab;
    public GameObject sevenSegmentPrefab;
    public GameObject toggleSwitchPrefab;
    public GameObject binarySwitchPrefab;
    public GameObject buzzerPrefab;
    public GameObject clockPrefab;
    public GameObject jkFlipFlopPrefab;
    public GameObject ledPrefab;

    [Header("ตำแหน่งเกิดของ Object (แยกตามประเภท)")]
    public Transform andGateSpawnPoint;
    public Transform orGateSpawnPoint;
    public Transform norGateSpawnPoint;
    public Transform nandGateSpawnPoint;
    public Transform xorGateSpawnPoint;
    public Transform xnorGateSpawnPoint;
    public Transform notGateSpawnPoint;
    public Transform sevenSegmentSpawnPoint;
    public Transform toggleSwitchSpawnPoint;
    public Transform binarySwitchSpawnPoint;
    public Transform buzzerSpawnPoint;
    public Transform clockSpawnPoint;
    public Transform jkFlipFlopSpawnPoint;
    public Transform ledSpawnPoint;

    [Header("อ้างอิงไปยัง QuizManager2 (ถ้ามี)")]
    public QuizManager2 quizManager;

    // เก็บตัวนับ ID ของแต่ละประเภท
    private Dictionary<string, int> objectCount = new Dictionary<string, int>();

    private void Spawn(GameObject prefab, Transform spawnPoint, string baseName)
    {
        if (prefab != null && spawnPoint != null)
        {
            // สร้างวัตถุ
            GameObject newObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            // ถ้าไม่มีการนับของ baseName มาก่อน ให้เซ็ตเริ่มต้น
            if (!objectCount.ContainsKey(baseName))
            {
                objectCount[baseName] = 1;
            }

            // ตั้งชื่อวัตถุตาม baseName_ลำดับ
            newObj.name = $"{baseName}_{objectCount[baseName]}";
            objectCount[baseName]++;

            // กรณีพิเศษ: ถ้าเป็น ToggleSwitch -> ค้นหา TMP_Text หลายตัว แล้วตั้งค่าเฉพาะตัวแรก
            if (baseName == "ToggleSwitch")
            {
                TMP_Text[] textArray = newObj.GetComponentsInChildren<TMP_Text>();
                if (textArray != null && textArray.Length > 0)
                {
                    // ใช้ตัวแรกเสมอ (index 0)
                    TMP_Text label = textArray[0];
                    int currentID = objectCount[baseName] - 1; 
                    label.text = $"SW{currentID}"; 
                }
                else
                {
                    Debug.LogWarning("ToggleSwitch Spawned but no TMP_Text found in children!");
                }
            }

            // แจ้งไปยัง QuizManager2 (ถ้ามี)
            if (quizManager != null)
            {
                quizManager.NotifySpawnedObject(newObj);
            }
        }
        else
        {
            Debug.LogWarning($"Prefab หรือ Spawn Point ของ {baseName} ยังไม่ได้ตั้งค่า!");
        }
    }

    // เมธอดสำหรับ Spawn อุปกรณ์แต่ละชนิด
    public void SpawnAndGate()      { Spawn(andGatePrefab,      andGateSpawnPoint,      "AndGate"); }
    public void SpawnOrGate()       { Spawn(orGatePrefab,       orGateSpawnPoint,       "OrGate"); }
    public void SpawnNorGate()      { Spawn(norGatePrefab,      norGateSpawnPoint,      "NorGate"); }
    public void SpawnNandGate()     { Spawn(nandGatePrefab,     nandGateSpawnPoint,     "NandGate"); }
    public void SpawnXorGate()      { Spawn(xorGatePrefab,      xorGateSpawnPoint,      "XorGate"); }
    public void SpawnXnorGate()     { Spawn(xnorGatePrefab,     xnorGateSpawnPoint,     "XnorGate"); }
    public void SpawnNotGate()      { Spawn(notGatePrefab,      notGateSpawnPoint,      "NotGate"); }
    public void SpawnSevenSegment() { Spawn(sevenSegmentPrefab, sevenSegmentSpawnPoint, "SevenSegment"); }
    public void SpawnToggleSwitch() { Spawn(toggleSwitchPrefab, toggleSwitchSpawnPoint, "ToggleSwitch"); }
    public void SpawnBinarySwitch() { Spawn(binarySwitchPrefab, binarySwitchSpawnPoint, "BinarySwitch"); }
    public void SpawnBuzzer()       { Spawn(buzzerPrefab,       buzzerSpawnPoint,       "Buzzer"); }
    public void SpawnClock()        { Spawn(clockPrefab,        clockSpawnPoint,        "Clock"); }
    public void SpawnJKFlipFlop()   { Spawn(jkFlipFlopPrefab,   jkFlipFlopSpawnPoint,   "JKFlipFlop"); }
    public void SpawnLED()          { Spawn(ledPrefab,          ledSpawnPoint,          "LED"); }
}
