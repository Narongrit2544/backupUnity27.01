using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicTracks; // เก็บเพลงทั้งหมด
    private int currentTrackIndex = 0;

    void Start()
    {
        if (musicTracks.Length > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        // ตรวจสอบว่าเพลงปัจจุบันเล่นจบหรือยัง
        if (!audioSource.isPlaying)
        {
            NextTrack(); // เปลี่ยนไปเพลงถัดไป
        }
    }

    void PlayTrack(int index)
    {
        if (index >= 0 && index < musicTracks.Length)
        {
            audioSource.clip = musicTracks[index];
            audioSource.Play();
        }
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length; // วนซ้ำเมื่อถึงเพลงสุดท้าย
        PlayTrack(currentTrackIndex);
    }
}
