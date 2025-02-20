using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;   // อ้างอิงไปยัง AudioSource
    public AudioClip buttonClickSound; // ไฟล์เสียงคลิก

    // ฟังก์ชันสำหรับเล่นเสียงคลิก
    public void PlayButtonSound()
    {
        // PlayOneShot ใช้เล่นเสียงสั้น ๆ โดยไม่กระทบเสียงอื่นที่กำลังเล่น
        audioSource.PlayOneShot(buttonClickSound);
    }
}
