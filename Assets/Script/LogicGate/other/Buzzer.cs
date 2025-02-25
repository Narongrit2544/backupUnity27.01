using UnityEngine;

public class Buzzer : MonoBehaviour
{
    [Header("Input ที่ใช้ควบคุม Buzzer")]
    public InputConnector input; // รับค่าจาก InputConnector

    [Header("เสียงของ Buzzer")]
    public AudioSource buzzerSound; // เสียงที่จะเล่น

    void Start()
    {
        if (buzzerSound == null)
        {
            buzzerSound = GetComponent<AudioSource>();

            // ถ้าไม่มี AudioSource ให้เพิ่มอัตโนมัติ
            if (buzzerSound == null)
            {
                buzzerSound = gameObject.AddComponent<AudioSource>();
                buzzerSound.loop = true; // ให้เสียงเล่นซ้ำ
                buzzerSound.playOnAwake = false; // ไม่ให้เล่นอัตโนมัติ
            }
        }

        if (input != null)
        {
            input.gameObject.name = $"{gameObject.name}_IN1";
        }

        UpdateState();
    }

    void Update()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        if (input == null || buzzerSound == null) return;

        if (input.isOn)
        {
            if (!buzzerSound.isPlaying)
            {
                buzzerSound.Play();
            }
        }
        else
        {
            if (buzzerSound.isPlaying)
            {
                buzzerSound.Stop();
            }
        }
    }
}
