using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// QuizManager.cs (ฉบับใหม่ที่มี NotifySpawnedObject)
/// สำหรับตรวจสอบโจทย์การต่อวงจรลอจิก (LogicTask)
/// โดยการตรวจสอบจะคล้ายกับ QuizManager2 แต่ใช้โครงสร้างโจทย์แบบใน QuizManager เดิม
/// พร้อมรองรับการแจ้งเมื่อมีวัตถุถูก Spawn
/// และเพิ่มการตรวจสอบ ToggleSwitch (SW) และ LED ในโจทย์ด้วย
/// </summary>
public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class LogicTask
    {
        [Header("คำอธิบายโจทย์")]
        [TextArea(2, 5)]
        public string description;    

        [Header("รายการ AndGate / OrGate / LogicGates ที่ต้องตรวจสอบ")]
        public List<AndGate>  andGatesToCheck;
        public List<OrGate>   orGatesToCheck;
        public List<NandGate> nandGatesToCheck;
        public List<NorGate>  norGatesToCheck;
        public List<XorGate>  xorGatesToCheck;
        public List<XnorGate> xnorGatesToCheck;
        public List<NotGate>  notGatesToCheck;

        [Header("รายการ ToggleSwitch (SW) ที่ต้องตรวจสอบ")]
        public List<ToggleSwitch> toggleSwitchesToCheck;

        [Header("รายการ LED ที่ต้องตรวจสอบ")]
        public List<LED> ledsToCheck;

        [Header("เงื่อนไขค่าความถูกต้อง (คาดหวัง)")]
        public bool expectedResultForAllGates;

        [Header("คะแนนในข้อนี้")]
        public int score;
    }

    [Header("รายการโจทย์ทั้งหมด")]
    public List<LogicTask> tasks = new List<LogicTask>();

    [Header("คะแนนรวม (ดูได้ใน Inspector)")]
    public int totalScore;

    [Header("ข้อความแสดงผลการตรวจ")]
    [TextArea(4, 8)]
    public string resultMessage;

    // -----------------------------
    // 1) ฟังก์ชันหลัก: ตรวจสอบโจทย์ทั้งหมด
    // -----------------------------
    public void CheckAllTasks()
    {
        // บังคับให้อัปเดตสถานะของวงจรก่อนตรวจสอบ
        ForceUpdateCircuit();

        int scoreAccumulated = 0;
        string messageBuilder = "";

        // วนลูปตรวจสอบโจทย์แต่ละข้อ
        for (int i = 0; i < tasks.Count; i++)
        {
            LogicTask currentTask = tasks[i];
            bool gateCorrect = true;
            string gateError = "";

            // ตรวจสอบ Gate แต่ละประเภท (7 ประเภท)
            // 1) AndGate
            foreach (AndGate andGate in currentTask.andGatesToCheck)
            {
                if (andGate == null || andGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"AndGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (andGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"AndGate {andGate.gameObject.name} มีค่า {andGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 2) OrGate
            foreach (OrGate orGate in currentTask.orGatesToCheck)
            {
                if (orGate == null || orGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"OrGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (orGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"OrGate {orGate.gameObject.name} มีค่า {orGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 3) NandGate
            foreach (NandGate nandGate in currentTask.nandGatesToCheck)
            {
                if (nandGate == null || nandGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"NandGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (nandGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"NandGate {nandGate.gameObject.name} มีค่า {nandGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 4) NorGate
            foreach (NorGate norGate in currentTask.norGatesToCheck)
            {
                if (norGate == null || norGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"NorGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (norGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"NorGate {norGate.gameObject.name} มีค่า {norGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 5) XorGate
            foreach (XorGate xorGate in currentTask.xorGatesToCheck)
            {
                if (xorGate == null || xorGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"XorGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (xorGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"XorGate {xorGate.gameObject.name} มีค่า {xorGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 6) XnorGate
            foreach (XnorGate xnorGate in currentTask.xnorGatesToCheck)
            {
                if (xnorGate == null || xnorGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"XnorGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (xnorGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"XnorGate {xnorGate.gameObject.name} มีค่า {xnorGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }
            // 7) NotGate
            foreach (NotGate notGate in currentTask.notGatesToCheck)
            {
                if (notGate == null || notGate.output == null)
                {
                    gateCorrect = false;
                    gateError += $"NotGate ไม่ถูกผูกในโจทย์\n";
                    continue;
                }
                if (notGate.output.isOn != currentTask.expectedResultForAllGates)
                {
                    gateCorrect = false;
                    gateError += $"NotGate {notGate.gameObject.name} มีค่า {notGate.output.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                }
            }

            // ตรวจสอบ ToggleSwitch (SW)
            bool switchCorrect = true;
            string switchError = "";
            if (currentTask.toggleSwitchesToCheck != null)
            {
                foreach (ToggleSwitch sw in currentTask.toggleSwitchesToCheck)
                {
                    if (sw == null)
                    {
                        switchCorrect = false;
                        switchError += "ToggleSwitch ไม่ถูกผูกในโจทย์ (SW)\n";
                        continue;
                    }
                    if (sw.isOn != currentTask.expectedResultForAllGates)
                    {
                        switchCorrect = false;
                        switchError += $"ToggleSwitch {sw.gameObject.name} มีค่า {sw.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                    }
                }
            }
            else
            {
                // ถ้าไม่มีการกำหนดรายการ SW ให้ถือว่าไม่ถูกต้อง
                switchCorrect = false;
                switchError += "ไม่พบรายการ ToggleSwitch ในโจทย์\n";
            }

            // ตรวจสอบ LED
            bool ledCorrect = true;
            string ledError = "";
            if (currentTask.ledsToCheck != null)
            {
                foreach (LED led in currentTask.ledsToCheck)
                {
                    if (led == null || led.input == null)
                    {
                        ledCorrect = false;
                        ledError += "LED ไม่ถูกผูกในโจทย์\n";
                        continue;
                    }
                    if (led.input.isOn != currentTask.expectedResultForAllGates)
                    {
                        ledCorrect = false;
                        ledError += $"LED {led.gameObject.name} มีค่า {led.input.isOn} แต่คาด {currentTask.expectedResultForAllGates}\n";
                    }
                }
            }
            else
            {
                ledCorrect = false;
                ledError += "ไม่พบรายการ LED ในโจทย์\n";
            }

            // รวมผลการตรวจสอบของ Gate, ToggleSwitch และ LED
            bool isTaskAllCorrect = gateCorrect && switchCorrect && ledCorrect;

            // คำนวณคะแนน (ตัวอย่าง: ให้ 70 คะแนนสำหรับ Gate, 10 สำหรับ SW และ 10 สำหรับ LED = 90 คะแนนรวม)
            int scoreThisTask = 0;
            if (gateCorrect) scoreThisTask += 70;
            if (switchCorrect) scoreThisTask += 10;
            if (ledCorrect) scoreThisTask += 10;
            // ตัดคะแนนเกิน score ของโจทย์
            scoreThisTask = Mathf.Clamp(scoreThisTask, 0, currentTask.score);
            scoreAccumulated += scoreThisTask;

            // สร้างข้อความสรุป
            if (isTaskAllCorrect)
            {
                messageBuilder += $"โจทย์ข้อที่ {i + 1}: ถูกต้อง! +{scoreThisTask} คะแนน\n";
            }
            else
            {
                messageBuilder += $"โจทย์ข้อที่ {i + 1}: ยังไม่ถูกต้อง (ได้ {scoreThisTask} คะแนน)\n";
                messageBuilder += gateError + switchError + ledError + "\n";
            }
        }

        totalScore = scoreAccumulated;
        resultMessage = $"คะแนนรวม: {scoreAccumulated} / {GetMaxScore()}\n\nรายละเอียด:\n{messageBuilder}";
        Debug.Log(resultMessage);
    }

    // -----------------------------
    // 2) ฟังก์ชันบังคับให้อัปเดตสถานะของวงจรทั้งหมด
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
    // 3) ฟังก์ชันหาคะแนนเต็มรวม
    // -----------------------------
    int GetMaxScore()
    {
        int maxScore = 0;
        foreach (var task in tasks)
        {
            maxScore += task.score;
        }
        return maxScore;
    }

    // -----------------------------
    // 4) ฟังก์ชันสำหรับรับแจ้งเมื่อมีการ Spawn วัตถุใหม่ (ถ้าต้องการ)
    // -----------------------------
    public void NotifySpawnedObject(GameObject spawnedObj)
    {
        Debug.Log($"[QuizManager] Spawned: {spawnedObj.name}");

        if (tasks.Count == 0) return; // ถ้าไม่มี tasks ไม่ต้องทำอะไร

        // ตรวจสอบว่าเป็น Gate, ToggleSwitch หรือ LED แล้วเพิ่มลงใน tasks[0]
        AndGate andGate = spawnedObj.GetComponent<AndGate>();
        if (andGate != null)
        {
            tasks[0].andGatesToCheck.Add(andGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {andGate.name} ลงใน andGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        OrGate orGate = spawnedObj.GetComponent<OrGate>();
        if (orGate != null)
        {
            tasks[0].orGatesToCheck.Add(orGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {orGate.name} ลงใน orGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        NandGate nandGate = spawnedObj.GetComponent<NandGate>();
        if (nandGate != null)
        {
            tasks[0].nandGatesToCheck.Add(nandGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {nandGate.name} ลงใน nandGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        NorGate norGate = spawnedObj.GetComponent<NorGate>();
        if (norGate != null)
        {
            tasks[0].norGatesToCheck.Add(norGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {norGate.name} ลงใน norGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        XorGate xorGate = spawnedObj.GetComponent<XorGate>();
        if (xorGate != null)
        {
            tasks[0].xorGatesToCheck.Add(xorGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {xorGate.name} ลงใน xorGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        XnorGate xnorGate = spawnedObj.GetComponent<XnorGate>();
        if (xnorGate != null)
        {
            tasks[0].xnorGatesToCheck.Add(xnorGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {xnorGate.name} ลงใน xnorGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        NotGate notGate = spawnedObj.GetComponent<NotGate>();
        if (notGate != null)
        {
            tasks[0].notGatesToCheck.Add(notGate);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {notGate.name} ลงใน notGatesToCheck ของโจทย์ข้อ 1");
            return;
        }

        ToggleSwitch sw = spawnedObj.GetComponent<ToggleSwitch>();
        if (sw != null)
        {
            if (tasks[0].toggleSwitchesToCheck == null)
                tasks[0].toggleSwitchesToCheck = new List<ToggleSwitch>();
            tasks[0].toggleSwitchesToCheck.Add(sw);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {sw.gameObject.name} ลงใน toggleSwitchesToCheck ของโจทย์ข้อ 1");
            return;
        }

        LED led = spawnedObj.GetComponent<LED>();
        if (led != null)
        {
            if (tasks[0].ledsToCheck == null)
                tasks[0].ledsToCheck = new List<LED>();
            tasks[0].ledsToCheck.Add(led);
            Debug.Log($"NotifySpawnedObject: เพิ่ม {led.gameObject.name} ลงใน ledsToCheck ของโจทย์ข้อ 1");
            return;
        }

        Debug.Log($"NotifySpawnedObject: {spawnedObj.name} ไม่ใช่วัตถุที่ตรวจในโจทย์ข้อ 1");
    }
}
