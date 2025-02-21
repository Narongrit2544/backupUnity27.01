using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Clock : MonoBehaviour
{
    [Header("ตั้งค่าความถี่ของ Clock (วินาทีต่อรอบ)")]
    public float interval = 1.0f; // Default 1 วินาทีต่อรอบ

    [Header("สถานะของ Clock (true/false)")]
    public bool isOn = false; // ค่าปัจจุบันของ Clock

    public OutputConnector output; // Output ที่เชื่อมต่อกับวงจร

    [Header("UI สำหรับการตั้งค่า")]
    public GameObject canvas; // UI Canvas ที่ใช้กรอกค่า
    public Slider intervalSlider; // Slider สำหรับปรับค่า interval
    public TMP_Text displayText; // แสดงค่าที่ตั้งไว้

    [Header("เสียง")]
    public AudioClip toggleCanvasSound; // เสียงที่เล่นเมื่อเปิด/ปิด Canvas
    private AudioSource audioSource;    // สำหรับเล่นเสียง

    private Coroutine clockCoroutine; // เก็บ Coroutine ของ Clock
    private bool isCanvasOpen = false; // เช็คสถานะของ Canvas

    private void Start()
    {
        // ตรวจสอบหรือเพิ่ม AudioSource ให้กับ GameObject นี้
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (canvas != null)
        {
            canvas.SetActive(false); // ปิด Canvas ตอนเริ่ม
        }

        if (intervalSlider != null)
        {
            intervalSlider.minValue = 0.1f;
            intervalSlider.maxValue = 5.0f;
            intervalSlider.value = interval;
            intervalSlider.onValueChanged.AddListener(delegate { UpdateIntervalFromSlider(); });
        }

        if (displayText != null)
        {
            displayText.text = "1.0s";
        }

        if (output != null)
        {
            output.isOn = isOn;
            output.UpdateState();
            output.gameObject.name = $"{gameObject.name}_OUT";
        }

        StartClock();
    }

    private void StartClock()
    {
        if (clockCoroutine != null)
        {
            StopCoroutine(clockCoroutine);
        }
        clockCoroutine = StartCoroutine(ToggleClock());
    }

    private IEnumerator ToggleClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            isOn = !isOn;
            if (output != null)
            {
                output.isOn = isOn;
                output.UpdateState();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Pinch"))
        {
            ToggleCanvas();
        }
    }

    private void ToggleCanvas()
    {
        isCanvasOpen = !isCanvasOpen;
        if (canvas != null)
        {
            canvas.SetActive(isCanvasOpen);
            // เล่นเสียงเปิด/ปิด canvas เมื่อมีการเปลี่ยนสถานะ
            if (toggleCanvasSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(toggleCanvasSound);
            }
            if (isCanvasOpen && intervalSlider != null)
            {
                intervalSlider.value = interval;
            }
        }
    }

    private void UpdateIntervalFromSlider()
    {
        interval = intervalSlider.value;
        displayText.text = interval.ToString("0.0") + "s";
        StartClock();
    }
}
