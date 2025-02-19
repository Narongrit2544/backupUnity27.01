using UnityEngine;
using System.Collections.Generic;

public class QuizManager3 : MonoBehaviour
{
    // -----------------------------
    // 1) โครงสร้างของตารางความจริง (Truth Table)
    // -----------------------------
    [System.Serializable]
    public class TruthTableEntry
    {
        [Tooltip("ค่า input (0-63) ที่แทนสถานะของ 6 Toggle Switch (เมื่อแปลงเป็นเลขฐานสอง)")]
        public int input;

        [Tooltip("ผลที่คาดว่า LED ควรจะเป็น (true = ติด, false = ดับ)")]
        public bool expectedOutput;
    }

    // -----------------------------
    // 2) โครงสร้างของ LogicTask (โจทย์)
    // -----------------------------
    [System.Serializable]
    public class LogicTask
    {
        [Header("คำอธิบายโจทย์")]
        [TextArea(2, 5)]
        public string description;

        [Header("Toggle Switch (สมมติ 6 ตัว)")]
        public ToggleSwitch[] toggleSwitches = new ToggleSwitch[6];

        [Header("LED ที่ต้องตรวจ")]
        public LED ledToCheck;

        [Header("คะแนนของโจทย์นี้ (คะแนนเต็ม)")]
        public int score = 100;

        [Header("ตารางความจริง (Truth Table) สำหรับโจทย์นี้")]
        public List<TruthTableEntry> truthTableEntries = new List<TruthTableEntry>();
    }

    // -----------------------------
    // 3) ตัวแปรหลักของ QuizManager3
    // -----------------------------
    [Header("รายการโจทย์ทั้งหมด")]
    public List<LogicTask> tasks = new List<LogicTask>();

    [Header("คะแนนรวม (ดูได้ใน Inspector)")]
    public int totalScore;

    [Header("ข้อความแสดงผลการตรวจ")]
    [TextArea(4, 8)]
    public string resultMessage;

    // ใน Awake เราจะตรวจสอบและเติมตารางความจริงให้ครบ 64 รายการ หากยังไม่ถูกกำหนดใน Inspector
    void Awake()
    {
        foreach (LogicTask task in tasks)
        {
            if (task.truthTableEntries == null || task.truthTableEntries.Count == 0)
            {
                // เติมค่าเริ่มต้นสำหรับ 6 bit (0-63) โดย default expectedOutput = false
                for (int i = 0; i < 64; i++)
                {
                    TruthTableEntry entry = new TruthTableEntry();
                    entry.input = i;
                    entry.expectedOutput = false;
                    task.truthTableEntries.Add(entry);
                }
            }
        }
    }

    // -----------------------------
    // 4) ฟังก์ชันหลัก: ตรวจสอบทุกโจทย์
    // -----------------------------
    public void CheckAllTasks()
    {
        int scoreAccumulated = 0;
        string messageBuilder = "";

        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask task = tasks[i];
            Debug.Log($"[Task {i + 1}] เริ่มตรวจโจทย์: {task.description}");

            // 1) ตรวจ ToggleSwitch combinations (ในตัวอย่างนี้ ตั้งค่า default = true)
            bool toggleCorrect = true;
            string toggleError = "";

            // 2) ตรวจการเชื่อมต่อสายไฟผ่าน Gate
            (bool connectionCorrect, string connectionError) = CheckConnectionsWithError(task);

            // 3) ตรวจว่ามี Gate อย่างน้อย 1 ตัว (อะไรก็ได้)
            (bool hasGate, string gateError) = CheckAtLeastOneGate();

            // 4) ตรวจการคำนวณของวงจรตามตารางความจริง
            (bool truthTableCorrect, string truthTableError) = CheckTruthTableOutput(task);

            // สรุปความถูกต้องของทุกเงื่อนไข
            bool isTaskAllCorrect = toggleCorrect 
                                    && connectionCorrect 
                                    && hasGate
                                    && truthTableCorrect;

            // คำนวณคะแนน
            int scoreThisTask = CalculateScore(
                task,
                toggleCorrect,
                connectionCorrect,
                hasGate,
                truthTableCorrect
            );
            scoreAccumulated += scoreThisTask;

            // สร้างข้อความสรุป
            if (isTaskAllCorrect)
            {
                messageBuilder += $"[Task {i + 1}]: ถูกต้อง! +{scoreThisTask} คะแนน\n";
            }
            else
            {
                messageBuilder += $"[Task {i + 1}]: ยังไม่ถูกต้อง (ได้ {scoreThisTask} คะแนน)\n";
                messageBuilder += toggleError + connectionError + gateError + truthTableError + "\n";
            }
        }

        totalScore = scoreAccumulated;
        resultMessage = $"คะแนนรวม: {scoreAccumulated} / {GetMaxScore()}\n\nรายละเอียด:\n{messageBuilder}";
        Debug.Log(resultMessage);
    }

    // -----------------------------
    // 5) บังคับอัปเดตวงจร
    // -----------------------------
    void ForceUpdateCircuit()
    {
        LED[] leds = FindObjectsOfType<LED>();
        foreach (LED led in leds)
            led.UpdateState();

        AndGate[] ands = FindObjectsOfType<AndGate>();
        foreach (AndGate a in ands)
            a.UpdateState();

        OrGate[] ors = FindObjectsOfType<OrGate>();
        foreach (OrGate o in ors)
            o.UpdateState();

        NandGate[] nands = FindObjectsOfType<NandGate>();
        foreach (NandGate n in nands)
            n.UpdateState();

        NorGate[] nors = FindObjectsOfType<NorGate>();
        foreach (NorGate n in nors)
            n.UpdateState();

        XorGate[] xors = FindObjectsOfType<XorGate>();
        foreach (XorGate x in xors)
            x.UpdateState();

        XnorGate[] xnors = FindObjectsOfType<XnorGate>();
        foreach (XnorGate x in xnors)
            x.UpdateState();

        NotGate[] nots = FindObjectsOfType<NotGate>();
        foreach (NotGate n in nots)
            n.UpdateState();
    }

    // -----------------------------
    // 6) ตรวจการเชื่อมต่อสายไฟ (DFS)
    // -----------------------------
    (bool, string) CheckConnectionsWithError(LogicTask task)
    {
        if (task.ledToCheck == null)
            return (false, "CheckConnections: LED ไม่ถูกผูกใน Task\n");

        WireManager[] wireManagers = FindObjectsOfType<WireManager>();

        bool overall = true;
        string error = "";

        for (int i = 0; i < task.toggleSwitches.Length; i++)
        {
            ToggleSwitch toggle = task.toggleSwitches[i];
            if (toggle == null)
            {
                error += "CheckConnections: ToggleSwitch ไม่ถูกผูกใน Task\n";
                overall = false;
                continue;
            }

            bool connected = IsToggleSwitchConnected(task.ledToCheck, toggle, wireManagers);
            if (!connected)
            {
                error += $"CheckConnections: {toggle.gameObject.name} ไม่เชื่อมต่อกับ LED ผ่าน Gate\n";
                overall = false;
            }
        }

        if (overall)
            return (true, "CheckConnections: สายไฟเชื่อมต่อผ่าน Gate ถูกต้อง\n");
        else
            return (false, error);
    }

    bool IsToggleSwitchConnected(LED led, ToggleSwitch toggle, WireManager[] wireManagers)
    {
        if (led == null || led.input == null)
            return false;

        var discoveredEdges = new HashSet<(OutputConnector, InputConnector)>();
        var discoveredGates = new HashSet<GameObject>();
        var discoveredToggles = new HashSet<GameObject>();

        Stack<PathState> stack = new Stack<PathState>();
        HashSet<PathState> visited = new HashSet<PathState>();

        PathState start = new PathState(led.input, false);
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            PathState current = stack.Pop();

            foreach (var wm in wireManagers)
            {
                foreach (var conn in wm.GetWireConnections())
                {
                    OutputConnector outConn = conn.Key.Item1;
                    InputConnector inConn = conn.Key.Item2;

                    if (inConn == current.input)
                    {
                        discoveredEdges.Add(conn.Key);
                        GameObject outObj = outConn.gameObject;

                        bool isGate = HasAnyGateScriptInParentOrSelf(outObj);
                        bool newFoundGate = current.foundGate || isGate;

                        ToggleSwitch ts = outObj.GetComponentInParent<ToggleSwitch>();
                        if (ts != null)
                        {
                            discoveredToggles.Add(ts.gameObject);
                            if (ts == toggle && newFoundGate)
                                return true;
                        }
                        else if (isGate)
                        {
                            List<InputConnector> gateInputs = GetAllGateInputs(outObj);
                            foreach (var gi in gateInputs)
                            {
                                PathState nextState = new PathState(gi, newFoundGate);
                                if (!visited.Contains(nextState))
                                {
                                    visited.Add(nextState);
                                    stack.Push(nextState);
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    class PathState
    {
        public InputConnector input;
        public bool foundGate;

        public PathState(InputConnector inp, bool gateFound)
        {
            input = inp;
            foundGate = gateFound;
        }

        public override bool Equals(object obj)
        {
            PathState other = obj as PathState;
            if (other == null) return false;
            return input == other.input && foundGate == other.foundGate;
        }

        public override int GetHashCode()
        {
            int h1 = (input != null) ? input.GetHashCode() : 0;
            int h2 = foundGate.GetHashCode();
            return h1 ^ h2;
        }
    }

    bool HasAnyGateScriptInParentOrSelf(GameObject child)
    {
        return (child.GetComponentInParent<AndGate>() != null ||
                child.GetComponentInParent<OrGate>() != null ||
                child.GetComponentInParent<NandGate>() != null ||
                child.GetComponentInParent<NorGate>() != null ||
                child.GetComponentInParent<XorGate>() != null ||
                child.GetComponentInParent<XnorGate>() != null ||
                child.GetComponentInParent<NotGate>() != null);
    }

    List<InputConnector> GetAllGateInputs(GameObject gateObj)
    {
        List<InputConnector> inputs = new List<InputConnector>();

        AndGate ag = gateObj.GetComponentInParent<AndGate>();
        if (ag != null && ag.inputs != null)
            inputs.AddRange(ag.inputs);

        OrGate og = gateObj.GetComponentInParent<OrGate>();
        if (og != null && og.inputs != null)
            inputs.AddRange(og.inputs);

        NandGate ng = gateObj.GetComponentInParent<NandGate>();
        if (ng != null && ng.inputs != null)
            inputs.AddRange(ng.inputs);

        NorGate nog = gateObj.GetComponentInParent<NorGate>();
        if (nog != null && nog.inputs != null)
            inputs.AddRange(nog.inputs);

        XorGate xg = gateObj.GetComponentInParent<XorGate>();
        if (xg != null && xg.inputs != null)
            inputs.AddRange(xg.inputs);

        XnorGate xng = gateObj.GetComponentInParent<XnorGate>();
        if (xng != null && xng.inputs != null)
            inputs.AddRange(xng.inputs);

        NotGate ntg = gateObj.GetComponentInParent<NotGate>();
        if (ntg != null && ntg.input != null)
            inputs.Add(ntg.input);

        return inputs;
    }

    // -----------------------------
    // 7) ตรวจว่ามี Gate อย่างน้อย 1 ตัว
    // -----------------------------
    (bool, string) CheckAtLeastOneGate()
    {
        int totalGateCount = 0;
        totalGateCount += FindObjectsOfType<AndGate>().Length;
        totalGateCount += FindObjectsOfType<OrGate>().Length;
        totalGateCount += FindObjectsOfType<NandGate>().Length;
        totalGateCount += FindObjectsOfType<NorGate>().Length;
        totalGateCount += FindObjectsOfType<XorGate>().Length;
        totalGateCount += FindObjectsOfType<XnorGate>().Length;
        totalGateCount += FindObjectsOfType<NotGate>().Length;

        if (totalGateCount > 0)
        {
            return (true, "พบ Gate อย่างน้อย 1 ตัวในฉาก\n");
        }
        else
        {
            return (false, "ไม่พบ Gate ใด ๆ ในฉาก\n");
        }
    }

    // -----------------------------
    // 8) ตรวจตารางความจริงของโจทย์
    // -----------------------------
    public (bool, string) CheckTruthTableOutput(LogicTask task)
    {
        if (task.toggleSwitches == null || task.toggleSwitches.Length != 6)
            return (false, "CheckTruthTableOutput: ต้องมี ToggleSwitch 6 ตัว\n");
        if (task.ledToCheck == null)
            return (false, "CheckTruthTableOutput: LED ไม่ถูกผูกใน Task\n");

        bool allPassed = true;
        string errorMsg = "";

        // ใช้ตารางความจริงจาก task.truthTableEntries
        foreach (var entry in task.truthTableEntries)
        {
            int combo = entry.input;
            // ตั้งค่าสถานะของ ToggleSwitch ตามบิตของ combo (6 bit)
            for (int i = 0; i < 6; i++)
            {
                bool isOn = ((combo >> i) & 1) == 1;
                task.toggleSwitches[i].SetState(isOn);
            }

            // บังคับให้อัปเดตสถานะของวงจร
            ForceUpdateCircuit();

            // อ่านสถานะของ LED
            bool ledState = task.ledToCheck.isOn;

            // เทียบกับ expectedOutput
            if (ledState != entry.expectedOutput)
            {
                allPassed = false;
                string binaryStr = System.Convert.ToString(combo, 2).PadLeft(6, '0');
                errorMsg += $"Combo {combo} (Toggle: {binaryStr}) -> คาด {entry.expectedOutput} แต่ได้ {ledState}\n";
            }
        }

        if (allPassed)
            return (true, "CheckTruthTableOutput: Output ตรงตามตารางความจริงทั้งหมด\n");
        else
            return (false, errorMsg);
    }

    // -----------------------------
    // 9) คำนวณคะแนน (เน้น Truth Table มากกว่า)
    // -----------------------------
    int CalculateScore(LogicTask task,
                       bool isToggleCorrect,
                       bool isConnectionCorrect,
                       bool hasGate,
                       bool isTruthTableCorrect)
    {
        int scoreSum = 0;

        // น้ำหนักคะแนนใหม่
        // Toggle Switch Correct     = 10 คะแนน
        // Connection Correct        = 10 คะแนน
        // Has Gate (>=1)            = 10 คะแนน
        // Truth Table Correct       = 70 คะแนน
        if (isToggleCorrect)       scoreSum += 10;
        if (isConnectionCorrect)   scoreSum += 10;
        if (hasGate)               scoreSum += 10;
        if (isTruthTableCorrect)   scoreSum += 70;

        // ตัดคะแนนเกิน score ของโจทย์
        scoreSum = Mathf.Clamp(scoreSum, 0, task.score);
        Debug.Log($"CalculateScore: คะแนนย่อย = {scoreSum}");
        return scoreSum;
    }

    // -----------------------------
    // 10) หาคะแนนเต็มรวม
    // -----------------------------
    public int GetMaxScore()
    {
        int maxScore = 0;
        foreach (var task in tasks)
        {
            maxScore += task.score;
        }
        return maxScore;
    }

    // -----------------------------
    // 11) ฟังก์ชันผูกวัตถุที่ Spawn ใหม่ (ตัวอย่าง)
    // -----------------------------
    public void NotifySpawnedObject(GameObject spawnedObj)
    {
        Debug.Log($"[QuizManager3] Spawned: {spawnedObj.name}");

        LED newLED = spawnedObj.GetComponent<LED>();
        if (newLED != null)
        {
            if (tasks.Count > 0)
            {
                tasks[0].ledToCheck = newLED;
                Debug.Log($"NotifySpawnedObject: กำหนด {newLED.name} เป็น ledToCheck ของโจทย์ข้อ 1");
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
                        Debug.Log($"NotifySpawnedObject: กำหนด {newToggle.name} เป็น toggleSwitches[{i}] ของโจทย์ข้อ 1");
                        break;
                    }
                }
            }
            return;
        }

        AndGate andGate = spawnedObj.GetComponent<AndGate>();
        if (andGate != null)
        {
            Debug.Log($"NotifySpawnedObject: Spawned AndGate: {andGate.name}");
        }
    }
}
