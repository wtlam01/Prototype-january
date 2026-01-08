using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StudyingClickAdvance : MonoBehaviour
{
    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("URLs")]
    [SerializeField] private string studyingUrl = "https://w33lam.panel.uwe.ac.uk/Video/Studying.mp4";
    [SerializeField] private string nextUrl = "https://w33lam.panel.uwe.ac.uk/Video/2-1%20Fire_1.mp4";

    [Header("Pause point (seconds)")]
    [SerializeField] private double pauseAtSeconds = 3.0;

    [Header("Click-Advance UI")]
    [SerializeField] private GameObject clickAdvanceUI;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Slider progressSlider;

    [Header("Tuning")]
    [SerializeField] private int requiredClicks = 25;

    // 每 click 之後影片短暫播放幾耐（感覺像“推進一下”）
    [SerializeField] private float holdPlaySeconds = 0.12f;

    // -------------------- runtime --------------------
    private int clicks = 0;
    private bool isStudyingPhase = false;
    private bool prepared = false;
    private bool pauseTriggered = false;

    private float lastClickTime = -999f;

    private double clipLength = 0;
    private double stepPerClick = 0;

    private void Awake()
    {
        // Auto-find if not assigned
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // Boot-stop: avoid auto play
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.Stop();
            videoPlayer.time = 0;

            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        if (advanceButton != null)
        {
            advanceButton.onClick.RemoveAllListeners();
            advanceButton.onClick.AddListener(OnAdvanceClicked);
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredClicks;
            progressSlider.value = 0;

            // Make it a progress bar (not draggable)
            progressSlider.interactable = false;
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        if (advanceButton != null)
            advanceButton.onClick.RemoveListener(OnAdvanceClicked);
    }

    // Call this when user enters Studying phase (or on scene start)
    public void StartStudyingSequence()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("StudyingClickAdvance: videoPlayer is null.");
            return;
        }

        // Reset state
        clicks = 0;
        prepared = false;
        pauseTriggered = false;
        isStudyingPhase = true;

        clipLength = 0;
        stepPerClick = 0;
        lastClickTime = -999f;

        if (progressSlider != null)
        {
            progressSlider.maxValue = requiredClicks;
            progressSlider.value = 0;
        }

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        // Prepare studying video
        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = studyingUrl;
        videoPlayer.time = 0;

        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        prepared = true;
        vp.Play();
    }

    private void Update()
    {
        if (!isStudyingPhase || !prepared || videoPlayer == null) return;

        // Cache clip length once available
        if (clipLength <= 0.1 && videoPlayer.length > 0.1)
        {
            clipLength = videoPlayer.length;

            // spread remaining duration after pause point across clicks
            stepPerClick = (clipLength - pauseAtSeconds) / requiredClicks;

            // safety fallback if video is too short / weird length
            if (stepPerClick < 0.02) stepPerClick = 0.05;
        }

        // Trigger pause at target time
        if (!pauseTriggered && videoPlayer.isPlaying && videoPlayer.time >= pauseAtSeconds)
        {
            pauseTriggered = true;

            videoPlayer.Pause();
            videoPlayer.time = pauseAtSeconds;

            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);
            lastClickTime = -999f;
        }

        // After each click, we allow a tiny play window, then pause again
        if (pauseTriggered && clicks < requiredClicks)
        {
            if (videoPlayer.isPlaying && Time.time - lastClickTime > holdPlaySeconds)
            {
                videoPlayer.Pause();
            }
        }
    }

    private void OnAdvanceClicked()
    {
        if (videoPlayer == null) return;
        if (!isStudyingPhase) return;
        if (!pauseTriggered) return;
        if (clicks >= requiredClicks) return;

        clicks++;
        lastClickTime = Time.time;

        if (progressSlider != null)
            progressSlider.value = clicks;

        // If length not known yet, fallback to small step
        double step = stepPerClick;
        if (step <= 0.001) step = holdPlaySeconds;

        double targetTime = pauseAtSeconds + (clicks * step);

        if (clipLength > 0.1)
            targetTime = System.Math.Min(targetTime, clipLength - 0.05);

        // Jump and briefly play
        videoPlayer.time = targetTime;
        videoPlayer.Play();

        // Finished clicking -> hide UI and keep playing normally
        if (clicks >= requiredClicks)
        {
            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);
            videoPlayer.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!isStudyingPhase) return;

        isStudyingPhase = false;
        prepared = false;
        pauseTriggered = false;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        // Switch to next video
        vp.Stop();
        vp.source = VideoSource.Url;
        vp.url = nextUrl;
        vp.time = 0;

        vp.Prepare(); // will auto play in OnPrepared
    }
}
