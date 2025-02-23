using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class DeviceState
{
    public string prefabName;
    public int pegX;
    public int pegY;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public float positionX;
    public float positionY;
    public float positionZ;
    
    // ฟิลด์เพิ่มเติมสำหรับบันทึกค่าเฉพาะของอุปกรณ์
    public double resistance;
    public double batteryVoltage;
}

[System.Serializable]
public class SaveData
{
    public List<DeviceState> deviceStates;
}

public class SaveManager : MonoBehaviour
{
    public string fileName = "savegame.json";
    public GameObject[] availablePrefabs;
    private int numSpawned = 1;
    private CircuitLab circuitLab;

    void Start()
    {
        GameObject labObj = GameObject.Find("CircuitLab");
        if (labObj)
        {
            circuitLab = labObj.GetComponent<CircuitLab>();
        }
        else
        {
            Debug.LogError("ไม่พบ GameObject 'CircuitLab'");
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.deviceStates = new List<DeviceState>();

        List<PlacedComponent> placedComponents = circuitLab.GetPlacedComponents();
        foreach (PlacedComponent comp in placedComponents)
        {
            if (comp.GameObject.activeInHierarchy)
            {
                DeviceState state = new DeviceState();
                state.prefabName = comp.GameObject.tag;
                state.pegX = comp.Start.x;
                state.pegY = comp.Start.y;
                state.rotationX = comp.GameObject.transform.localEulerAngles.x;
                state.rotationY = comp.GameObject.transform.localEulerAngles.y;
                state.rotationZ = comp.GameObject.transform.localEulerAngles.z;
                state.positionX = comp.GameObject.transform.localPosition.x;
                state.positionY = comp.GameObject.transform.localPosition.y;
                state.positionZ = comp.GameObject.transform.localPosition.z;

                // บันทึกค่าความต้านทาน ถ้าอุปกรณ์นั้นมี IResistor
                var resistor = comp.GameObject.GetComponent<IResistor>();
                if (resistor != null)
                {
                    state.resistance = resistor.Resistance;
                }
                // บันทึกค่าแรงดัน ถ้าอุปกรณ์นั้นมี IBattery
                var battery = comp.GameObject.GetComponent<IBattery>();
                if (battery != null)
                {
                    state.batteryVoltage = battery.BatteryVoltage;
                }

                data.deviceStates.Add(state);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
        Debug.Log("บันทึกข้อมูลสำเร็จ: " + path);
    }

    public IEnumerator ResetAndWait()
    {
        circuitLab.Reset();
        yield return new WaitForSeconds(3f);
    }

    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            StartCoroutine(ResetAndLoad(data));
        }
        else
        {
            Debug.LogWarning("ไม่พบไฟล์ save: " + path);
        }
    }

    private IEnumerator ResetAndLoad(SaveData data)
    {
        yield return StartCoroutine(ResetAndWait());
        yield return StartCoroutine(LoadGameSequentially(data));
    }

    private IEnumerator LoadGameSequentially(SaveData data)
    {
        numSpawned = 1;

        foreach (DeviceState state in data.deviceStates)
        {
            GameObject prefab = GetPrefabByIdentifier(state.prefabName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = "Component" + state.prefabName + numSpawned++;

                string pegName = "Peg_" + state.pegY + "_" + state.pegX;
                GameObject pegObj = GameObject.Find(pegName);
                if (pegObj != null)
                {
                    instance.transform.parent = pegObj.transform;
                    instance.transform.localPosition = new Vector3(state.positionX, state.positionY, state.positionZ);
                    instance.transform.localEulerAngles = new Vector3(state.rotationX, state.rotationY, state.rotationZ);

                    Debug.Log($"[LOAD] อุปกรณ์: {instance.name}, Position: {instance.transform.position}, LocalPosition: {instance.transform.localPosition}, Peg: {pegObj.name}");

                    Rigidbody rb = instance.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = instance.AddComponent<Rigidbody>();
                    }
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    PegMgr pegMgr = pegObj.GetComponent<PegMgr>();
                    if (pegMgr != null)
                    {
                        pegMgr.RegisterComponent(instance);
                        Debug.Log($"[LOAD] RegisterComponent เรียกใช้สำเร็จ: {instance.name} กับ {pegObj.name}");
                        
                        yield return new WaitForEndOfFrame();

                        rb.isKinematic = true;
                        rb.useGravity = false;
                        Physics.Simulate(0.1f);
                    }
                    else
                    {
                        Debug.LogError($"[ERROR] ไม่พบ PegMgr บน Peg: {pegObj.name}");
                    }

                    // ตั้งค่าแรงดันของแบตเตอรี่ ถ้าอุปกรณ์นั้นเป็น IBattery
                    var battery = instance.GetComponent<IBattery>();
                    if (battery != null)
                    {
                        battery.BatteryVoltage = state.batteryVoltage;
                    }

                    // ตั้งค่าความต้านทาน ถ้าอุปกรณ์นั้นเป็น Bulb, Motor หรือ Flute
                    var bulb = instance.GetComponent<Bulb>();
                    if (bulb != null)
                    {
                        bulb.SetResistance(state.resistance);
                    }
                    else
                    {
                        var motor = instance.GetComponent<Motor>();
                        if (motor != null)
                        {
                            motor.SetResistance(state.resistance);
                        }
                        else
                        {
                            var flute = instance.GetComponent<Flute>();
                            if (flute != null)
                            {
                                flute.Resistance = state.resistance;
                            }
                        }
                    }

                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    Debug.LogWarning($"ไม่พบ Peg: {pegName}");
                }
            }
            else
            {
                Debug.LogWarning($"ไม่พบ Prefab สำหรับ Tag: {state.prefabName}");
            }
        }

        Debug.Log("โหลดข้อมูลสำเร็จ");
    }

    private GameObject GetPrefabByIdentifier(string identifier)
    {
        foreach (GameObject prefab in availablePrefabs)
        {
            if (prefab.CompareTag(identifier))
                return prefab;
        }
        return null;
    }
}
