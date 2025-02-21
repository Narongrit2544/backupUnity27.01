using UnityEngine;

public class ManagementCanvas : MonoBehaviour
{
    [Header("เมนูหลักแต่ละหน้า")]
    public GameObject mainMenu;   // หน้าแรก
    public GameObject secondMenu; // หน้าที่ 2
    public GameObject modeMenu;   // หน้าที่ 3
    public GameObject saveMenu;   // หน้าที่ 4

    public GameObject Mannual;   // หน้าที่ 5

    [Header("Object ที่จะเปิดเมื่อกดปุ่มที่ 2 ใน Mode Menu")]
    public GameObject mySpecialObject;

    [Header("Object ที่จะเปิดเมื่อกดปุ่ม Simulator")]
    public GameObject simulatorObject1;
    public GameObject simulatorObject2;

    void Start()
    {
        ShowMainMenu(); // เริ่มต้นที่หน้าแรก
        simulatorObject1.SetActive(true);
        simulatorObject2.SetActive(true);
    }

    private void ResetAllMenus()
    {
        // รีเซ็ตค่าเริ่มต้นของแต่ละหน้า
        mainMenu.SetActive(false);
        secondMenu.SetActive(false);
        modeMenu.SetActive(false);
        saveMenu.SetActive(false);
        mySpecialObject.SetActive(false);
        Mannual.SetActive(false);

    }

    public void ShowMainMenu()
    {
        ResetAllMenus(); // รีเซ็ตก่อนเปิดหน้าใหม่
        mainMenu.SetActive(true);
    }

    public void ShowSecondMenu()
    {
        ResetAllMenus();
        secondMenu.SetActive(true);
    }

    public void ShowModeMenu()
    {
        ResetAllMenus();
        modeMenu.SetActive(true);
    }

    public void ShowSaveMenu()
    {
        ResetAllMenus();
        saveMenu.SetActive(true);
    }

     public void ShowManual()
    {
        ResetAllMenus();
        Mannual.SetActive(true);
    }

    //===================== ปุ่มต่าง ๆ ใน Mode Menu =====================//

    // ปุ่มแรก ยังไม่ต้องทำอะไร
    public void ModeMenuButton1()
    {
        Debug.Log("Mode Menu Button #1 Pressed (Do nothing yet)");
    }

    // ปุ่มที่ 2: เปิด mySpecialObject และปิดเมนูทั้งหมด
    public void ModeMenuButton2()
    {
        ResetAllMenus();
        mySpecialObject.SetActive(true);
        simulatorObject1.SetActive(true); // ปิด wall circuit 
        simulatorObject2.SetActive(false); //เปิด wall digital


    }

    // ปุ่มที่ 3: เปิด simulatorObject1 และ simulatorObject2 แล้วกลับไป Mode Menu
    public void ModeMenuButton3()
    {
        simulatorObject1.SetActive(false);
        simulatorObject2.SetActive(false);
        ShowModeMenu(); // กลับไปที่ Mode Menu
    }

    //===============================================================//

    public void ExitApplication()
    {
        Debug.Log("Exit button pressed!");
        Application.Quit();
    }
}
