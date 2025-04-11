using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;

    private void Start()
    {
        double startTime = AudioSettings.dspTime;
        introSource.PlayScheduled(startTime);

        // Calculate precise duration based on sample count
        double exactIntroDuration = 
            (double)introSource.clip.samples / introSource.clip.frequency;

        double loopStartTime = startTime + exactIntroDuration;

        // Now schedule loop track
        loopSource.PlayScheduled(loopStartTime);
    }
}
