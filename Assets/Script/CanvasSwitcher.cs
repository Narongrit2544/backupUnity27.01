using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Canvas ของแต่ละ EX")]
    public GameObject ex1Canvas;
    public GameObject ex2Canvas;
    public GameObject ex3Canvas;


    // เรียกเมื่อกดปุ่ม EX1
    public void OnClickEx1()
    {
        ex1Canvas.SetActive(true);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(false);

    }

    // เรียกเมื่อกดปุ่ม EX2
    public void OnClickEx2()
    {
        ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(true);
        ex3Canvas.SetActive(false);

    }

    // เรียกเมื่อกดปุ่ม EX3
    public void OnClickEx3()
    {
        ex1Canvas.SetActive(false);
        ex2Canvas.SetActive(false);
        ex3Canvas.SetActive(true);

    }
}
