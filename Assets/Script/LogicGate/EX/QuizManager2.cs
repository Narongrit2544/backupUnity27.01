using UnityEngine;
using System.Collections.Generic;

public class QuizManager2 : MonoBehaviour
{
    [System.Serializable]
    public class LogicTask
    {
        [Header("คำอธิบายโจทย์")]
        [TextArea(2, 5)]
        public string description;

        [Header("Gate ที่ต้องการให้มีในฉาก (และตรวจชื่อ)")]
        public bool requireAndGate;
        public bool requireOrGate;
        public bool requireNandGate;
        public bool requireNorGate;
        public bool requireXorGate;
        public bool requireXnorGate;
        public bool requireNotGate;

        [Header("Toggle Switch (สมมติ 4 ตัว)")]
        public ToggleSwitch[] toggleSwitches = new ToggleSwitch[4];

        [Header("ตัวเลขเป้าหมาย (Target Numbers) ที่ต้องทำให้ LED ติด")]
        public List<int> targetNumbers = new List<int>();

        [Header("LED ที่ต้องตรวจ")]
        public LED ledToCheck;

        [Header("คะแนนของโจทย์นี้")]
        public int score = 50; // คะแนนเต็มของข้อนี้
    }

    [Header("รายการโจทย์ทั้งหมด")]
    public List<LogicTask> tasks = new List<LogicTask>();

    [Header("คะแนนรวม (ดูได้ใน Inspector)")]
    public int totalScore;

    [Header("ข้อความแสดงผลการตรวจ")]
    public string resultMessage;

    // เรียกเมื่อกดปุ่มตรวจสอบ หรือจากสคริปต์อื่น
    public void CheckAllTasks()
    {
        int scoreAccumulated = 0;
        string messageBuilder = "";

        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask task = tasks[i];

            bool isToggleCorrect      = CheckToggleSwitches(task);
            bool isConnectionCorrect  = CheckConnections(task);
            bool isGatePresenceCorrect= CheckGatePresence(task);

            // ใหม่: ตรวจการตั้งชื่อ 
            bool isNamingCorrect      = CheckGateNaming(task);

            // สรุปว่าเป็น "ผ่าน" ถ้า 4 ส่วนนี้ถูกต้อง
            bool isTaskAllCorrect = (
                isToggleCorrect && isConnectionCorrect 
                && isGatePresenceCorrect && isNamingCorrect
            );

            // ตัวอย่างการคำนวณคะแนน
            int scoreThisTask = CalculateScore(
                task,
                isToggleCorrect,
                isConnectionCorrect,
                isGatePresenceCorrect,
                isNamingCorrect // เพิ่มตัวนี้
            );

            scoreAccumulated += scoreThisTask;

            messageBuilder += isTaskAllCorrect
                ? $"โจทย์ข้อที่ {i+1}: ถูกต้อง! +{scoreThisTask} คะแนน\n"
                : $"โจทย์ข้อที่ {i+1}: ยังไม่ถูกต้อง (ได้ {scoreThisTask} คะแนน)\n";
        }

        totalScore = scoreAccumulated;
        resultMessage = $"คะแนนรวม: {scoreAccumulated} / {GetMaxScore()}\n\nรายละเอียด:\n{messageBuilder}";
        Debug.Log(resultMessage);
    }

    // 1) ตรวจ Toggle Switch → LED
    bool CheckToggleSwitches(LogicTask task)
    {
        if (task.ledToCheck == null || task.toggleSwitches == null || task.toggleSwitches.Length == 0)
        {
            return true;
        }

        int switchValue = 0;
        for (int i = 0; i < task.toggleSwitches.Length; i++)
        {
            if (task.toggleSwitches[i] != null && task.toggleSwitches[i].isOn)
            {
                switchValue |= (1 << i);
            }
        }

        bool isLEDOn = task.ledToCheck.input.isOn;
        bool shouldLEDBeOn = task.targetNumbers.Contains(switchValue);

        if (isLEDOn && !shouldLEDBeOn) return false;
        if (!isLEDOn && shouldLEDBeOn) return false;

        return true;
    }

    // 2) ตรวจการเชื่อมต่อสายไฟ
    bool CheckConnections(LogicTask task)
    {
        if (task.ledToCheck == null) return true;

        bool isConnected = false;
        WireManager[] wireManagers = FindObjectsOfType<WireManager>();

        foreach (var wireManager in wireManagers)
        {
            foreach (var connection in wireManager.GetWireConnections())
            {
                var output = connection.Key.Item1;
                var input  = connection.Key.Item2;
                if (input == task.ledToCheck.input)
                {
                    isConnected = true;
                    break;
                }
            }
            if (isConnected) break;
        }

        return isConnected;
    }

    // 3) ตรวจว่าถ้ามี requireGate = true ต้องมี Gate นั้น ๆ ใน Scene
    bool CheckGatePresence(LogicTask task)
    {
        bool needAnyGate = (task.requireAndGate || task.requireOrGate || task.requireNandGate
                            || task.requireNorGate || task.requireXorGate || task.requireXnorGate
                            || task.requireNotGate);

        if (!needAnyGate) return true;

        bool foundGate = false;

        if (task.requireAndGate && FindObjectsOfType<AndGate>().Length > 0) foundGate = true;
        if (task.requireOrGate && FindObjectsOfType<OrGate>().Length > 0) foundGate = true;
        if (task.requireNandGate && FindObjectsOfType<NandGate>().Length > 0) foundGate = true;
        if (task.requireNorGate && FindObjectsOfType<NorGate>().Length > 0) foundGate = true;
        if (task.requireXorGate && FindObjectsOfType<XorGate>().Length > 0) foundGate = true;
        if (task.requireXnorGate && FindObjectsOfType<XnorGate>().Length > 0) foundGate = true;
        if (task.requireNotGate && FindObjectsOfType<NotGate>().Length > 0) foundGate = true;

        return foundGate;
    }

    // 4) ฟังก์ชันใหม่: ตรวจสอบชื่อวัตถุ (Naming) ว่า Gate แต่ละตัวชื่อถูกต้องหรือไม่
    bool CheckGateNaming(LogicTask task)
    {
        // ถ้าไม่ได้ require อะไร ก็ไม่ต้องตรวจชื่อ
        bool needAnyGate = (task.requireAndGate || task.requireOrGate || task.requireNandGate
                            || task.requireNorGate || task.requireXorGate || task.requireXnorGate
                            || task.requireNotGate);

        if (!needAnyGate) return true;

        // สมมติว่าเราต้องการเช็คว่าถ้ามี AndGate → ชื่อควรขึ้นต้นด้วย "AndGate_"
        // ถ้ามี OrGate → ชื่อควรขึ้นต้น "OrGate_"
        // เป็นต้น
        bool namingOK = true;

        if (task.requireAndGate)
        {
            AndGate[] ands = FindObjectsOfType<AndGate>();
            foreach (var g in ands)
            {
                if (!g.name.StartsWith("AndGate_"))
                {
                    Debug.LogWarning($"พบ AndGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ AndGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireOrGate)
        {
            OrGate[] ors = FindObjectsOfType<OrGate>();
            foreach (var g in ors)
            {
                if (!g.name.StartsWith("OrGate_"))
                {
                    Debug.LogWarning($"พบ OrGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ OrGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireNandGate)
        {
            NandGate[] nands = FindObjectsOfType<NandGate>();
            foreach (var g in nands)
            {
                if (!g.name.StartsWith("NandGate_"))
                {
                    Debug.LogWarning($"พบ NandGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ NandGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireNorGate)
        {
            NorGate[] nors = FindObjectsOfType<NorGate>();
            foreach (var g in nors)
            {
                if (!g.name.StartsWith("NorGate_"))
                {
                    Debug.LogWarning($"พบ NorGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ NorGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireXorGate)
        {
            XorGate[] xors = FindObjectsOfType<XorGate>();
            foreach (var g in xors)
            {
                if (!g.name.StartsWith("XorGate_"))
                {
                    Debug.LogWarning($"พบ XorGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ XorGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireXnorGate)
        {
            XnorGate[] xnors = FindObjectsOfType<XnorGate>();
            foreach (var g in xnors)
            {
                if (!g.name.StartsWith("XnorGate_"))
                {
                    Debug.LogWarning($"พบ XnorGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ XnorGate_");
                    namingOK = false;
                }
            }
        }

        if (task.requireNotGate)
        {
            NotGate[] nots = FindObjectsOfType<NotGate>();
            foreach (var g in nots)
            {
                if (!g.name.StartsWith("NotGate_"))
                {
                    Debug.LogWarning($"พบ NotGate ชื่อ {g.name} ซึ่งไม่ตรงรูปแบบ NotGate_");
                    namingOK = false;
                }
            }
        }

        return namingOK;
    }


    // 5) คำนวณคะแนน
    int CalculateScore(
        LogicTask task,
        bool isToggleCorrect,
        bool isConnectionCorrect,
        bool isGatePresenceCorrect,
        bool isNamingCorrect
    )
    {
        int scoreSum = 0;

        // สมมติสัดส่วนคะแนน
        if (isToggleCorrect)        scoreSum += 30; else scoreSum -= 10;
        if (isConnectionCorrect)    scoreSum += 20; else scoreSum -= 10;
        if (isGatePresenceCorrect)  scoreSum += 15; else scoreSum -= 5;
        if (isNamingCorrect)        scoreSum += 15; else scoreSum -= 5;

        // จำกัดไม่เกินคะแนนเต็ม
        scoreSum = Mathf.Clamp(scoreSum, 0, task.score);

        return scoreSum;
    }

    // หาคะแนนสูงสุด
    public int GetMaxScore()
    {
        int maxScore = 0;
        foreach (var task in tasks)
        {
            maxScore += task.score;
        }
        return maxScore;
    }

    // กรณีใช้งาน SpawnManager
    public void NotifySpawnedObject(GameObject spawnedObj)
    {
        Debug.Log($"[QuizManager2] Spawned: {spawnedObj.name}");
        // ตัวอย่างผูก LED/ToggleSwitch ให้ tasks[0] อัตโนมัติ
        LED newLED = spawnedObj.GetComponent<LED>();
        if (newLED != null)
        {
            if (tasks.Count > 0)
            {
                tasks[0].ledToCheck = newLED;
                Debug.Log($"กำหนด {newLED.name} เป็น ledToCheck ของโจทย์ข้อ 1");
            }
            return;
        }

        ToggleSwitch newToggle = spawnedObj.GetComponent<ToggleSwitch>();
        if (newToggle != null)
        {
            if (tasks.Count > 0)
            {
                for (int i = 0; i < tasks[0].toggleSwitches.Length; i++)
                {
                    if (tasks[0].toggleSwitches[i] == null)
                    {
                        tasks[0].toggleSwitches[i] = newToggle;
                        Debug.Log($"กำหนด {newToggle.name} เป็น toggleSwitches[{i}] ของโจทย์ข้อ 1");
                        break;
                    }
                }
            }
            return;
        }

        // ถ้าเป็น Gate (AndGate/OrGate...) ก็แค่ Log
        AndGate andGate = spawnedObj.GetComponent<AndGate>();
        if (andGate != null)
        {
            Debug.Log($"Spawned AndGate: {andGate.name}");
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 40), "ตรวจสอบโจทย์"))
        {
            CheckAllTasks();
        }
        GUI.Label(new Rect(10, 60, 600, 300), resultMessage);
    }
#endif
}
