using UnityEngine;

public class CanvasSwitcher2 : MonoBehaviour
{
    [Header("Pages / Panels ที่ต้องการสลับ")]
    public GameObject[] pages; // เก็บหน้า (Panel, Image, ฯลฯ) ที่ต้องการโชว์ทีละหน้า

    // หน้าปัจจุบัน (Index เริ่มต้นเป็น 0)
    private int currentPageIndex = 0;

    // เรียกครั้งแรกเมื่อตัวสคริปต์ถูกเปิดใช้งาน (หรือ GameObject ถูก SetActive(true))
    void OnEnable()
    {
        if (pages.Length > 0)
        {
            ShowPage(0);
        }
    }

    // เรียกเมื่อกดปุ่ม "ถัดไป" (Next)
    public void OnClickNext()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex = (currentPageIndex + 1) % pages.Length;
        pages[currentPageIndex].SetActive(true);
    }

    // เรียกเมื่อกดปุ่ม "ก่อนหน้า" (Previous)
    public void OnClickPrev()
    {
        pages[currentPageIndex].SetActive(false);
        currentPageIndex = (currentPageIndex - 1 + pages.Length) % pages.Length;
        pages[currentPageIndex].SetActive(true);
    }

    // ฟังก์ชันภายในสำหรับแสดงหน้าใดหน้าหนึ่งโดยตรง
    private void ShowPage(int pageIndex)
    {
        // ปิดทุกหน้า
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
        // เปิดเฉพาะหน้าที่ระบุ
        pages[pageIndex].SetActive(true);

        currentPageIndex = pageIndex;
    }
}
