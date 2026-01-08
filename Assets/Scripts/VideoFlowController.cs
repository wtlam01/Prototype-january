using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoFlowController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;

    [Header("Images")]
    public GameObject studyRoomImage;

    [Header("Click Advance UI")]
    public GameObject clickAdvanceUI;
    public Button advanceButton;
    public Slider progressSlider;

    [Header("URLs")]
    public string studyingUrl;
    public string nextUrl;

    [Header("Tuning")]
    public float pauseAtSeconds = 3f;
    public int requiredClicks = 25;

    private int clickCount = 0;
    private bool paused = false;



    // 🔘 Start / Keep Studying button
    public void StartStudyingSequence()
    {
        studyRoomImage.SetActive(false);
        videoRawImage.gameObject.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.url = studyingUrl;
        videoPlayer.Play();

        StartCoroutine(PauseAtPoint());
    }

    IEnumerator PauseAtPoint()
    {
        paused = false;

        while (videoPlayer.time < pauseAtSeconds)
        {
            yield return null;
        }

        videoPlayer.Pause();
        paused = true;
        clickAdvanceUI.SetActive(true);
    }

    // 🔘 Advance button
    public void RegisterClick()
    {
        if (!paused) return;

        clickCount++;
        progressSlider.value = clickCount;

        if (clickCount >= requiredClicks)
        {
            AdvanceToNextVideo();
        }
    }

    void AdvanceToNextVideo()
    {
        clickAdvanceUI.SetActive(false);
        clickCount = 0;
        progressSlider.value = 0;

        videoPlayer.url = nextUrl;
        videoPlayer.Play();
    }
}
