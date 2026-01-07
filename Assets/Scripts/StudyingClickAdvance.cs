using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StudyingClickAdvance : MonoBehaviour
{
    [Header("Video Player (Controller B)")]
    public VideoPlayer videoPlayer;

    [Header("URLs")]
    public string studyingUrl = "https://w33lam.panel.uwe.ac.uk/Video/Studying.mp4";
    public string nextUrl     = "https://w33lam.panel.uwe.ac.uk/Video/2-1%20Fire_1.mp4";

    [Header("Pause point")]
    public double pauseAtSeconds = 3.0;

    [Header("Click-Advance UI")]
    public GameObject clickAdvanceUI;   // Panel
    public Button advanceButton;        // Button inside panel
    public Slider progressSlider;       // Slider 0..requiredClicks

    [Header("Tuning")]
    public int requiredClicks = 25;
    public float holdPlaySeconds = 0.12f; // if user stops clicking, pause after this

    // --- internal state ---
    private int clicks = 0;
    private bool prepared = false;
    private bool pauseTriggered = false;

    private bool isStudyingPhase = false; // IMPORTANT: replaces vp.url string compare

    private float lastClickTime = -999f;

    private double clipLength = 0;
    private double stepPerClick = 0;

    void Awake()
    {
        // Safety: don't auto-play on start
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;

            videoPlayer.Stop();
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
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        if (advanceButton != null)
            advanceButton.onClick.RemoveListener(OnAdvanceClicked);
    }

    // ✅ Call this from the "Keep Studying" button OnClick()
    public void StartStudyingSequence()
    {
        if (videoPlayer == null) return;

        // reset state
        clicks = 0;
        prepared = false;
        pauseTriggered = false;
        clipLength = 0;
        stepPerClick = 0;
        lastClickTime = -999f;

        isStudyingPhase = true;

        if (progressSlider != null)
        {
            progressSlider.maxValue = requiredClicks;
            progressSlider.value = 0;
        }

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        // load studying url
        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = studyingUrl;

        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        prepared = true;
        vp.Play();
    }

    void Update()
    {
        if (!prepared || videoPlayer == null) return;

        // get clip length when ready
        if (clipLength <= 0.1 && videoPlayer.length > 0.1)
        {
            clipLength = videoPlayer.length;
            stepPerClick = (clipLength - pauseAtSeconds) / requiredClicks;

            if (stepPerClick < 0.02) stepPerClick = 0.05; // safety
        }

        // 1) Pause at pauseAtSeconds and show click UI
        if (isStudyingPhase && !pauseTriggered && videoPlayer.isPlaying && videoPlayer.time >= pauseAtSeconds)
        {
            pauseTriggered = true;

            videoPlayer.Pause();
            videoPlayer.time = pauseAtSeconds; // snap exact pause point

            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);

            lastClickTime = -999f;
        }

        // 2) While in click-advance mode, if user stops clicking, pause again
        if (isStudyingPhase && pauseTriggered && clicks < requiredClicks)
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
        if (!isStudyingPhase) return;        // only during studying
        if (!pauseTriggered) return;         // not yet paused at 3s
        if (clicks >= requiredClicks) return;

        clicks++;
        lastClickTime = Time.time;

        if (progressSlider != null)
            progressSlider.value = clicks;

        // move time forward by a fixed step from pause point
        double targetTime = pauseAtSeconds + (clicks * stepPerClick);

        // clamp
        if (clipLength > 0.1)
            targetTime = System.Math.Min(targetTime, clipLength - 0.05);

        // jump there and play briefly (click fast => continues playing)
        videoPlayer.time = targetTime;
        videoPlayer.Play();

        // if finished clicks, hide UI and let the rest play out to the end
        if (clicks >= requiredClicks)
        {
            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);
            videoPlayer.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // ✅ no string-compare URL; use phase flag
        if (!isStudyingPhase) return;

        isStudyingPhase = false;

        // load next video
        vp.Stop();
        prepared = false;
        pauseTriggered = false;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        vp.source = VideoSource.Url;
        vp.url = nextUrl;
        vp.Prepare(); // will auto-play in OnPrepared()
    }
}
