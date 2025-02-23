using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegMgr : MonoBehaviour, IPeg
{
    // Public members set in Unity Object Inspector
    public AudioSource clickSound;
    public float clickStartTime = 0f;
    public float pegInterval = 0.1f;

    // Offset from the center of each short and long component
    float shortPositionOffset = -2.5f;
    float longPositionOffset = -5.0f;

    // Height of each placed component above the breadboard
    float componentHeight = 0.5f;

    bool isOccupied = false;
    GameObject clone = null;
    GameObject original = null;
    CircuitComponent originalScript = null;
    // เพิ่มตัวแปรสำหรับเก็บความยาวของอุปกรณ์ (1 = short, 2 = long)
    private int currentComponentLength = 1;

    void Start()
    {
        // เรียก Component ของ clickSound (ไม่จำเป็นต้องเก็บค่าไว้)
        clickSound.GetComponent<AudioSource>();
    }
    public void RegisterComponent(GameObject component)
    {
        original = component;
        originalScript = component.GetComponent<CircuitComponent>();
        isOccupied = true;
    }

    IEnumerator PlaySound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.Play();
    }

    void Update()
    {
        if (original != null)
        {
            Debug.Log($"[UPDATE] Peg: {this.name}, อุปกรณ์ที่เชื่อมอยู่: {original.name}");
        }

        // ถ้ามี original ที่อ้างอิงอยู่ และมันถูกปล่อย (ไม่ถูกจับอยู่)
        if (original && !originalScript.IsHeld)
        {
            // ถ้าอุปกรณ์ถูกวางแล้ว (IsPlaced == true) ให้ลบ clone ทิ้ง
            if (originalScript.IsPlaced)
            {
                DestroyClone();
                original = null;
                return;
            }

            // คำนวณตำแหน่ง Peg จากฟังก์ชัน GetCoordinates()
            Point start = GetCoordinates();
            // กำหนด parent ของ original ให้เป็น Peg นี้
            original.transform.parent = transform;
            original.GetComponent<Rigidbody>().useGravity = false;
            original.GetComponent<Rigidbody>().isKinematic = true; // ทำให้ไม่โดน Physics

            // เปิดใช้งาน collider ให้จับได้
            original.GetComponent<BoxCollider>().enabled = true;

            // คำนวณ LockRotation และรับค่า end (ตำแหน่ง peg ปลายของอุปกรณ์)
            Point end = LockRotation(original, original);

            // คำนวณ Direction จากตำแหน่ง start กับ end
            Direction direction = GetDirection(start, end);
            // แจ้งให้ CircuitComponent รู้ว่าอุปกรณ์ถูกวางที่ Peg ตำแหน่ง start พร้อม Direction
            originalScript.Place(start, direction);

            // หลังจากวางแล้ว ให้ลบ clone
            DestroyClone();

            // เล่นเสียงคลิก
            StartCoroutine(PlaySound(clickSound, clickStartTime));

            // เพิ่มอุปกรณ์ลงในบอร์ด (CircuitLab.AddComponent จะสร้าง PlacedComponent)
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            if (script != null)
            {
                script.AddComponent(original, start, end);

                // หลังจาก AddComponent ให้ค้นหา PlacedComponent ที่เพิ่งถูกสร้างขึ้นมา
                // เพื่อกำหนดค่า Direction กับ ComponentLength
                CircuitLab circuitLab = lab.GetComponent<CircuitLab>();
                PlacedComponent placed = circuitLab.GetPlacedComponents().Find(x => x.GameObject == original);
                if (placed != null)
                {
                    placed.Direction = direction;
                    placed.ComponentLength = currentComponentLength;
                }
            }

            original = null;
        }

        // ถ้ามี clone อยู่ ให้เรียก LockRotation เพื่อให้ rotation เปลี่ยนตามการหมุนของผู้ใช้
        if (clone)
        {
            LockRotation(clone, original);
        }
    }

    public void Reset()
    {
        isOccupied = false;
        clone = null;
        original = null;
        originalScript = null;
    }

    Point GetCoordinates()
    {
        // สมมุติว่าชื่อของ Peg มีรูปแบบ "Peg_row_col"
        string name = transform.name.Substring(4); // ตัด "Peg_"
        int row = int.Parse(name.Substring(0, name.IndexOf('_')));
        int col = int.Parse(name.Substring(name.IndexOf('_') + 1));
        return new Point(col, row);
    }

    Point LockRotation(GameObject target, GameObject reference)
    {
        float offset = shortPositionOffset;
        int pegOffset = 1;
        // ตรวจสอบว่าชื่อของ reference มี "LongWire" หรือไม่
        if (reference != null && reference.transform.name.Contains("LongWire"))
        {
            offset = longPositionOffset;
            pegOffset = 2;
        }

        // คำนวณตำแหน่งของ Peg ปัจจุบัน
        Point coords = GetCoordinates();
        Point end = new Point(coords.x, coords.y);

        // หาชื่อและตำแหน่งของ Peg ที่อยู่รอบ ๆ
        string north = "Peg_" + (coords.y + pegOffset) + "_" + coords.x;
        Point ptNorth = new Point(coords.x, coords.y + pegOffset);
        string south = "Peg_" + (coords.y - pegOffset) + "_" + coords.x;
        Point ptSouth = new Point(coords.x, coords.y - pegOffset);
        string east = "Peg_" + coords.y + "_" + (coords.x + pegOffset);
        Point ptEast = new Point(coords.x + pegOffset, coords.y);
        string west = "Peg_" + coords.y + "_" + (coords.x - pegOffset);
        Point ptWest = new Point(coords.x - pegOffset, coords.y);

        // ดึง CircuitLab เพื่อเช็ค slot ว่าง
        var lab = GameObject.Find("CircuitLab").gameObject;
        var script = lab.GetComponent<ICircuitLab>();
        Point start = GetCoordinates();
        List<string> freeNeighbors = new List<string>();
        Point[] neighbors = { ptNorth, ptSouth, ptEast, ptWest };
        string[] neighborNames = { north, south, east, west };
        for (int i = 0; i < 4; i++)
        {
            if (script.IsSlotFree(start, neighbors[i], pegOffset))
            {
                freeNeighbors.Add(neighborNames[i]);
            }
        }

        // หาชื่อของ Peg ที่ใกล้เคียงที่สุดกับปลายสาย
        string closest = GetClosestNeighbor(reference, freeNeighbors);

        // ตั้งค่า rotation และตำแหน่งของ target ตาม Peg ที่เลือก
        var rotation = target.transform.localEulerAngles;
        var position = target.transform.localPosition;
        rotation.x = -90;
        rotation.y = 0;
        position.y = componentHeight;
        if (closest == north)
        {
            rotation.z = 0;
            position.x = 0;
            position.z = -offset;
            end.y += pegOffset;
        }
        else if (closest == east)
        {
            rotation.z = 90;
            position.x = -offset;
            position.z = 0;
            end.x += pegOffset;
        }
        else if (closest == south)
        {
            rotation.z = 180;
            position.x = 0;
            position.z = offset;
            end.y -= pegOffset;
        }
        else // west
        {
            rotation.z = 270;
            position.x = offset;
            position.z = 0;
            end.x -= pegOffset;
        }
        target.transform.localEulerAngles = rotation;
        target.transform.localPosition = position;

        return end;
    }

    string GetClosestNeighbor(GameObject clone, List<string> names)
    {
        string closest = names[0];
        GameObject closestNeighbor = null;
        float min = 999;
        // สมมุติว่ามี child ที่ชื่อ "WireEnd2" เพื่อใช้หาตำแหน่งปลายสาย
        var endpoint = clone.transform.Find("WireEnd2");

        foreach (string name in names)
        {
            GameObject neighbor = GameObject.Find(name);
            if (neighbor)
            {
                float nextDistance = Vector3.Distance(neighbor.transform.position, endpoint.transform.position);
                if (nextDistance < min)
                {
                    min = nextDistance;
                    closest = name;
                    closestNeighbor = neighbor;
                }
            }
        }
        return closest;
    }

    Direction GetDirection(Point start, Point end)
    {
        if (end.y > start.y)
            return Direction.North;
        else if (end.y < start.y)
            return Direction.South;
        else if (end.x > start.x)
            return Direction.East;
        else
            return Direction.West;
    }

    void OnTriggerEnter(Collider other)
    {
        // เฉพาะกับ SphereCollider เท่านั้น
        if (other.GetType() != typeof(SphereCollider))
        {
            return;
        }

        if (!isOccupied && other.name.StartsWith("Component"))
        {
            // กำหนดความยาวของอุปกรณ์
            currentComponentLength = 1;
            if (other.transform.name.Contains("LongWire"))
            {
                currentComponentLength = 2;
            }

            // ตรวจสอบว่า Peg นี้มี slot ว่างหรือไม่
            var lab = GameObject.Find("CircuitLab").gameObject;
            var script = lab.GetComponent<ICircuitLab>();
            if (script == null)
            {
                return;
            }
            Point start = GetCoordinates();
            int freeSlots = script.GetFreeComponentSlots(start, currentComponentLength);

            if (freeSlots == 0)
            {
                return;
            }

            // จดจำวัตถุดิบที่จะถูก clone
            original = other.gameObject;
            originalScript = original.GetComponent<CircuitComponent>();

            // สร้าง clone ของวัตถุ
            clone = Instantiate(other.gameObject);
            clone.GetComponent<Rigidbody>().detectCollisions = false;
            var cloneScript = clone.GetComponent<CircuitComponent>();
            cloneScript.IsClone = true;

            // ปิด gravity และตั้งค่า Rigidbody ให้ไม่กระทบ Physics
            clone.GetComponent<Rigidbody>().useGravity = false;
            clone.GetComponent<Rigidbody>().isKinematic = false;

            // วาง clone ลงใน Peg นี้
            clone.transform.parent = transform;
            isOccupied = true;

            Debug.Log($"[TRIGGER ENTER] Peg: {this.name}, อุปกรณ์: {original.name} เริ่มการเชื่อมต่อ");

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetType() != typeof(SphereCollider))
        {
            return;
        }

        if (other.name.StartsWith("Component"))
        {
            if (original)
            {
                Debug.Log($"[TRIGGER EXIT] Peg: {this.name}, อุปกรณ์: {original.name} หลุดออกจาก Peg");
                original.transform.parent = null;
                original = null;
            }
            DestroyClone();
        }
    }

    void DestroyClone()
    {
        if (isOccupied)
        {
            Destroy(clone);
            clone = null;
            isOccupied = false;
        }
    }
}
