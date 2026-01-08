using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class AutoPlayWithClickAdvance : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string studyingUrl;
    public string nextUrl;

    [Header("UI")]
    public GameObject clickAdvanceUI;   // 你個 ClickAdvanceUI（整個panel）
    public Button advanceButton;        // AdvanceButton
    public Slider progressSlider;       // ProgressSlider

    [Header("Tuning")]
    public float pauseAtSeconds = 3f;
    public int requiredClicks = 25;

    int clickCount = 0;
    bool paused = false;
    bool started = false;

    void Awake()
    {
        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        if (progressSlider != null)
        {
            progressSlider.maxValue = requiredClicks;
            progressSlider.value = 0;
        }

        if (advanceButton != null)
        {
            advanceButton.onClick.RemoveAllListeners();
            advanceButton.onClick.AddListener(RegisterClick);
        }
    }

    void Start()
    {
        StartStudying(); // 一入scene就播 studying
    }

    public void StartStudying()
    {
        started = true;
        paused = false;
        clickCount = 0;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);
        if (progressSlider != null) progressSlider.value = 0;

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer 未assign！");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = studyingUrl;
        videoPlayer.Play();

        StopAllCoroutines();
        StartCoroutine(PauseAtPoint());
    }

    IEnumerator PauseAtPoint()
    {
        // 等到 video 真係開始播先計 time，避免 Web/URL 有 buffer 時間
        while (!videoPlayer.isPlaying)
            yield return null;

        while (videoPlayer.time < pauseAtSeconds)
            yield return null;

        videoPlayer.Pause();
        paused = true;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);
    }

    public void RegisterClick()
    {
        if (!started || !paused) return;

        clickCount++;
        if (progressSlider != null) progressSlider.value = clickCount;

        if (clickCount >= requiredClicks)
        {
            AdvanceToNextVideo();
        }
    }

    void AdvanceToNextVideo()
    {
        paused = false;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);
        clickCount = 0;
        if (progressSlider != null) progressSlider.value = 0;

        videoPlayer.Stop();
        videoPlayer.url = nextUrl;
        videoPlayer.Play();
    }
}
