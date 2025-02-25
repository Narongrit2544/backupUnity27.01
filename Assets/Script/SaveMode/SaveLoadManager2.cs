using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SaveLoadManager2 : MonoBehaviour
{
    public float loadCooldown = 0.1f;
    private bool isLoading = false;
    private bool isSaving = false;
    [SerializeField]
    private string saveFileName = "saveFile.json"; // แสดงใน Inspector เพื่อแก้ไขชื่อไฟล์ได้
    
    [System.Serializable]
    public class DeviceData
    {
        public string deviceType;
        public string objectName;
        public Vector3 position;
        public Quaternion rotation;
        public bool state;
    }

    [System.Serializable]
    public class WireData
    {
        public string outputName;  // เช่น "ToggleSwitch_1_OUT1"
        public string inputName;   // เช่น "AndGate_1_IN2"
        public bool isConnected;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<DeviceData> devices = new List<DeviceData>();
        public List<WireData> wires = new List<WireData>();
    }

    public SpawnManager spawnManager;
    public WireManager wireManager;

    // รายชื่ออุปกรณ์ที่ต้องการจัดการ
    public string[] deviceTypes = new string[] {
        "AndGate", "OrGate", "NorGate", "NandGate", "XorGate", "XnorGate", "NotGate",
        "SevenSegment", "ToggleSwitch", "BinarySwitch", "Buzzer", "Clock", "JKFlipFlop", "LED"
    };

    private string saveFilePath;
    private Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> spawnPointMapping = new Dictionary<string, Transform>();

    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/" + saveFileName;

        if (spawnManager != null)
        {
            // แมป Prefab
            prefabMapping["AndGate"] = spawnManager.andGatePrefab;
            prefabMapping["OrGate"] = spawnManager.orGatePrefab;
            prefabMapping["NorGate"] = spawnManager.norGatePrefab;
            prefabMapping["NandGate"] = spawnManager.nandGatePrefab;
            prefabMapping["XorGate"] = spawnManager.xorGatePrefab;
            prefabMapping["XnorGate"] = spawnManager.xnorGatePrefab;
            prefabMapping["NotGate"] = spawnManager.notGatePrefab;
            prefabMapping["SevenSegment"] = spawnManager.sevenSegmentPrefab;
            prefabMapping["ToggleSwitch"] = spawnManager.toggleSwitchPrefab;
            prefabMapping["BinarySwitch"] = spawnManager.binarySwitchPrefab;
            prefabMapping["Buzzer"] = spawnManager.buzzerPrefab;
            prefabMapping["Clock"] = spawnManager.clockPrefab;
            prefabMapping["JKFlipFlop"] = spawnManager.jkFlipFlopPrefab;
            prefabMapping["LED"] = spawnManager.ledPrefab;

            // แมป SpawnPoint (หากต้องการ)
            spawnPointMapping["AndGate"] = spawnManager.andGateSpawnPoint;
            spawnPointMapping["OrGate"] = spawnManager.orGateSpawnPoint;
            spawnPointMapping["NorGate"] = spawnManager.norGateSpawnPoint;
            spawnPointMapping["NandGate"] = spawnManager.nandGateSpawnPoint;
            spawnPointMapping["XorGate"] = spawnManager.xorGateSpawnPoint;
            spawnPointMapping["XnorGate"] = spawnManager.xnorGateSpawnPoint;
            spawnPointMapping["NotGate"] = spawnManager.notGateSpawnPoint;
            spawnPointMapping["SevenSegment"] = spawnManager.sevenSegmentSpawnPoint;
            spawnPointMapping["ToggleSwitch"] = spawnManager.toggleSwitchSpawnPoint;
            spawnPointMapping["BinarySwitch"] = spawnManager.binarySwitchSpawnPoint;
            spawnPointMapping["Buzzer"] = spawnManager.buzzerSpawnPoint;
            spawnPointMapping["Clock"] = spawnManager.clockSpawnPoint;
            spawnPointMapping["JKFlipFlop"] = spawnManager.jkFlipFlopSpawnPoint;
            spawnPointMapping["LED"] = spawnManager.ledSpawnPoint;
        }
        else
        {
            Debug.LogError("SpawnManager not assigned in SaveLoadManager2!");
        }

        if (wireManager == null)
        {
            wireManager = FindObjectOfType<WireManager>();
            if (wireManager == null)
            {
                Debug.LogError("WireManager not found in scene!");
            }
        }
    }

    /// <summary>
    /// เรียก Save แบบหน่วงเวลา 3 วินาที
    /// </summary>
    public void Save()
    {
        if (isSaving)
        {
            Debug.LogWarning("Save operation is already in progress.");
            return;
        }
        StartCoroutine(DelayedSave());
    }

    private IEnumerator DelayedSave()
    {
        yield return new WaitForSeconds(3f);
        isSaving = true;

        SaveData saveData = new SaveData();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // เก็บข้อมูลอุปกรณ์หลัก (ชื่อไม่ประกอบด้วย _IN / _OUT)
        foreach (GameObject obj in allObjects)
        {
            foreach (string type in deviceTypes)
            {
                if (obj.name.StartsWith(type + "_") &&
                    !obj.name.Contains("_IN") && !obj.name.Contains("_OUT"))
                {
                    DeviceData data = new DeviceData();
                    data.deviceType = type;
                    data.objectName = obj.name;
                    data.position = obj.transform.position;
                    data.rotation = obj.transform.rotation;

                    // ถ้าเป็น ToggleSwitch ก็เซฟสถานะ toggle
                    var toggle = obj.GetComponent<ToggleSwitch>();
                    if (toggle != null)
                    {
                        data.state = toggle.isOn;
                    }
                    else
                    {
                        bool state = false;
                        var input = obj.GetComponent<InputConnector>();
                        var output = obj.GetComponent<OutputConnector>();
                        if (input != null) state = input.isOn;
                        else if (output != null) state = output.isOn;
                        data.state = state;
                    }
                    saveData.devices.Add(data);
                    break;
                }
            }
        }

        // เก็บข้อมูลสายไฟ โดยใช้ชื่อ Child Connector
        var wireDict = wireManager.GetWireConnections();
        Debug.Log("[Save] Number of wires in dictionary: " + wireDict.Count);

        foreach (var connection in wireDict)
        {
            if (connection.Key.Item1 != null && connection.Key.Item2 != null)
            {
                WireData wireData = new WireData();
                wireData.outputName = connection.Key.Item1.gameObject.name; // เช่น "AndGate_1_OUT1"
                wireData.inputName = connection.Key.Item2.gameObject.name; // เช่น "AndGate_2_IN1"
                wireData.isConnected = true;
                saveData.wires.Add(wireData);
            }
        }

        Debug.Log("[Save] Total devices saved: " + saveData.devices.Count);
        Debug.Log("[Save] Total wires saved: " + saveData.wires.Count);

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[Save] Saved data to " + saveFilePath);

        isSaving = false;
    }

    /// <summary>
    /// เรียก Load แบบหน่วงเวลา 3 วินาที
    /// </summary>
    public void Load()
    {
        if (isLoading)
        {
            Debug.LogWarning("Load operation is already in progress.");
            return;
        }
        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("Save file not found: " + saveFilePath);
            return;
        }
        isLoading = true;
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return Wait();

        string json = File.ReadAllText(saveFilePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[Load] Read file: {saveFilePath}");
        Debug.Log($"[Load] Devices in file: {saveData.devices.Count}, Wires in file: {saveData.wires.Count}");
        foreach (var w in saveData.wires)
        {
            Debug.Log($"[Load] wire => output={w.outputName}, input={w.inputName}, isConnected={w.isConnected}");
        }

        // ลบอุปกรณ์เก่า
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            foreach (string type in deviceTypes)
            {
                if (obj.name.StartsWith(type + "_"))
                {
                    Destroy(obj);
                    break;
                }
            }
        }
        yield return new WaitForSeconds(0.1f);

        // โหลดอุปกรณ์และสายไฟ
        yield return StartCoroutine(LoadSequence(saveData));
        isLoading = false;
    }

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
    }

    /// <summary>
    /// แก้ปัญหา “ชื่อซ้ำหรือไม่ตรงกัน” โดยตั้งชื่อ Child Connector ให้ตรงกับที่เซฟไว้
    /// แล้วบันทึกลง loadedObjects เพื่อ simulate pinch ได้
    /// </summary>
    private IEnumerator LoadSequence(SaveData saveData)
    {
        Dictionary<string, GameObject> loadedObjects = new Dictionary<string, GameObject>();

        // โหลดอุปกรณ์ทีละตัว
        foreach (DeviceData data in saveData.devices)
        {
            if (prefabMapping.ContainsKey(data.deviceType))
            {
                GameObject prefab = prefabMapping[data.deviceType];
                GameObject newObj = Instantiate(prefab, data.position, data.rotation);
                // ชื่ออุปกรณ์หลัก เช่น "ToggleSwitch_1" หรือ "AndGate_1"
                newObj.name = data.objectName;

                Debug.Log("[LoadSequence] Loaded device: " + data.objectName);

                // ตั้งค่า state พื้นฐาน
                var input = newObj.GetComponent<InputConnector>();
                var output = newObj.GetComponent<OutputConnector>();
                if (input != null)
                {
                    input.isOn = data.state;
                    input.UpdateState();
                }
                else if (output != null)
                {
                    output.isOn = data.state;
                    output.UpdateState();
                }

                // ถ้าเป็น ToggleSwitch ก็อาจมี pivot ให้หมุน
                var toggle = newObj.GetComponent<ToggleSwitch>();
                if (toggle != null)
                {
                    toggle.isOn = data.state;
                    if (toggle.pivot != null)
                    {
                        var rot = toggle.pivot.transform.localEulerAngles;
                        rot.y = toggle.isOn ? 15f : -15f;
                        toggle.pivot.transform.localEulerAngles = rot;
                    }
                }

                // **ส่วนแก้ไข**: ตั้งชื่อ Child Connector ตามที่ไฟล์ JSON ระบุ
                // ตัวอย่าง: JSON บอกว่า "ToggleSwitch_1_OUT"
                // คุณทราบว่าถ้าเป็น ToggleSwitch จะมี 1 outputConnector
                // ก็ให้ชื่อ = <objectName>_OUT (เช่น "ToggleSwitch_1_OUT")

                // หา OutputConnector ทั้งหมด
                var outConnectors = newObj.GetComponentsInChildren<OutputConnector>();
                // สมมติ ToggleSwitch มี 1 output
                // ตั้งชื่อเป็น <parentName>_OUT => "ToggleSwitch_1_OUT"
                if (outConnectors.Length > 0)
                {
                    outConnectors[0].gameObject.name = data.objectName + "_OUT";
                    loadedObjects[outConnectors[0].gameObject.name] = outConnectors[0].gameObject;
                }

                // ถ้าเป็น AndGate ที่มี 2 input => "AndGate_1_IN1" / "AndGate_1_IN2"
                // เช่นกัน หา InputConnector ทั้งหมด
                var inConnectors = newObj.GetComponentsInChildren<InputConnector>();
                // ตั้งชื่อ input ตัวแรกเป็น <parentName>_IN1, ตัวที่สองเป็น <parentName>_IN2
                for (int i = 0; i < inConnectors.Length; i++)
                {
                    string inName = data.objectName + "_IN" + (i + 1);
                    inConnectors[i].gameObject.name = inName;
                    loadedObjects[inName] = inConnectors[i].gameObject;
                }

                // บันทึกตัวหลักด้วย
                loadedObjects[data.objectName] = newObj;
            }
            else
            {
                Debug.LogWarning("[LoadSequence] No prefab mapping for device type: " + data.deviceType);
            }
            yield return new WaitForSeconds(loadCooldown);
        }

        // สร้างสายไฟด้วย simulate pinch
        foreach (WireData wire in saveData.wires)
        {
            if (wire.isConnected)
            {
                Debug.Log($"[LoadSequence] Processing wire: {wire.outputName} -> {wire.inputName}");
                if (loadedObjects.ContainsKey(wire.outputName) && loadedObjects.ContainsKey(wire.inputName))
                {
                    Debug.Log("[LoadSequence] Found devices for wire connection: " + wire.outputName + " -> " + wire.inputName);
                    GameObject outputObj = loadedObjects[wire.outputName];
                    GameObject inputObj = loadedObjects[wire.inputName];
                    OutputConnector outputCon = outputObj.GetComponent<OutputConnector>();
                    InputConnector inputCon = inputObj.GetComponent<InputConnector>();
                    if (outputCon != null && inputCon != null)
                    {

                        Debug.Log("[LoadSequence] Simulating pinch for wire: " + wire.outputName + " -> " + wire.inputName);
                        wireManager.SelectOutput(outputCon);
                        yield return new WaitForSeconds(0.05f);
                        wireManager.SelectInput(inputCon);
                        Debug.Log("[LoadSequence] Wire created for: " + wire.outputName + " -> " + wire.inputName);


                    }
                    else
                    {
                        Debug.LogWarning("[LoadSequence] Missing connector components for wire: " + wire.outputName + " -> " + wire.inputName);
                    }
                }
                else
                {
                    Debug.LogWarning("[LoadSequence] Could not find devices for wire connection: " + wire.outputName + " -> " + wire.inputName);
                }
            }
            else
            {
                Debug.Log($"[LoadSequence] Wire isConnected=false for: {wire.outputName} -> {wire.inputName}");
            }
            yield return new WaitForSeconds(loadCooldown);
        }
    }

}
