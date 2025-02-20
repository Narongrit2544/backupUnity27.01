using UnityEngine;

public class MuteManager : MonoBehaviour
{
    // อ้างอิง Sprite Renderer สองตัว
    public SpriteRenderer targetSpriteRenderer1;
    public SpriteRenderer targetSpriteRenderer2;

    // อ้างอิง AudioSource ที่ใช้เล่นเพลง Background
    public AudioSource backgroundMusic;

    // ตัวแปรตรวจสอบสถานะปัจจุบันว่าแสดงตัวไหนอยู่
    private bool isShowingSprite1 = true;

    private void Start()
    {
        // กำหนดค่าเริ่มต้นให้แสดง Sprite1, ซ่อน Sprite2, และเปิดเสียง
        targetSpriteRenderer1.enabled = true;
        targetSpriteRenderer2.enabled = false;
        backgroundMusic.mute = false;
        
        // ยืนยันสถานะว่าตอนเริ่มคือแสดง Sprite1
        isShowingSprite1 = true;
    }

    public void ToggleSprite()
    {
        if (isShowingSprite1)
        {
            // ซ่อนตัวแรก
            targetSpriteRenderer1.enabled = false;
            // แสดงตัวสอง
            targetSpriteRenderer2.enabled = true;
            
            // Mute เพลง
            backgroundMusic.mute = true;
        }
        else
        {
            // แสดงตัวแรก
            targetSpriteRenderer1.enabled = true;
            // ซ่อนตัวสอง
            targetSpriteRenderer2.enabled = false;
            
            // Unmute เพลง
            backgroundMusic.mute = false;
        }

        // สลับสถานะ
        isShowingSprite1 = !isShowingSprite1;
    }
}
