using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerBootStop : MonoBehaviour
{
    public VideoPlayer vp;

    void Awake()
    {
        if (vp == null) vp = GetComponent<VideoPlayer>();
        if (vp == null) return;

        vp.playOnAwake = false;
        vp.Stop();
        vp.time = 0;
    }
}

public class StudyingClickAdvance : MonoBehaviour
{
    [Header("Video Player (Controller B)")]
    public VideoPlayer videoPlayer;

    [Header("URLs")]
    public string studyingUrl = "https://w33lam.panel.uwe.ac.uk/Video/Studying.mp4";
    public string nextUrl = "https://w33lam.panel.uwe.ac.uk/Video/2-1%20Fire_1.mp4";

    [Header("Pause point")]
    public double pauseAtSeconds = 3.0;

    [Header("Click-Advance UI")]
    public GameObject clickAdvanceUI;
    public Button advanceButton;
    public Slider progressSlider;

    [Header("Tuning")]
    public int requiredClicks = 25;
    public float holdPlaySeconds = 0.12f;

    private int clicks = 0;
    private bool prepared = false;
    private bool pauseTriggered = false;
    private bool isStudyingPhase = false;

    private float lastClickTime = -999f;

    private double clipLength = 0;
    private double stepPerClick = 0;

    void Awake()
    {
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

    public void StartStudyingSequence()
    {
        if (videoPlayer == null) return;

        if (!videoPlayer.isActiveAndEnabled)
        {
            Debug.LogError("VideoPlayer is disabled or GameObject inactive. Enable it before Prepare().");
            return;
        }

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

        if (clipLength <= 0.1 && videoPlayer.length > 0.1)
        {
            clipLength = videoPlayer.length;
            stepPerClick = (clipLength - pauseAtSeconds) / requiredClicks;
            if (stepPerClick < 0.02) stepPerClick = 0.05;
        }

        if (isStudyingPhase && !pauseTriggered && videoPlayer.isPlaying && videoPlayer.time >= pauseAtSeconds)
        {
            pauseTriggered = true;

            videoPlayer.Pause();
            videoPlayer.time = pauseAtSeconds;

            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);
            lastClickTime = -999f;
        }

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
        if (!isStudyingPhase) return;
        if (!pauseTriggered) return;
        if (clicks >= requiredClicks) return;

        clicks++;
        lastClickTime = Time.time;

        if (progressSlider != null)
            progressSlider.value = clicks;

        if (clipLength <= 0.1 || stepPerClick <= 0.001)
        {
            stepPerClick = holdPlaySeconds; // fallback
        }

        double targetTime = pauseAtSeconds + (clicks * stepPerClick);

        if (clipLength > 0.1)
            targetTime = System.Math.Min(targetTime, clipLength - 0.05);

        videoPlayer.time = targetTime;
        videoPlayer.Play();

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

        vp.Stop();
        prepared = false;
        pauseTriggered = false;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        vp.source = VideoSource.Url;
        vp.url = nextUrl;
        vp.Prepare();
    }
}
