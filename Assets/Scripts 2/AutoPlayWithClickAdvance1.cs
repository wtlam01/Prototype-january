using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AutoPlayWithClickAdvance1 : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("URLs")]
    public string studyingUrl;
    public string nextUrl;

    [Header("UI")]
    public GameObject clickAdvanceUI;
    public Button advanceButton;
    public Slider progressSlider;

    [Header("Tuning")]
    public double pauseAtSeconds = 3.0;
    public int requiredClicks = 25;
    public float holdPlaySeconds = 0.12f;   // 每click播放幾耐（有「推進感」）
    public double minStepSeconds = 0.08;    // 每click最少前進幾多秒（避免太細睇唔出）

    int clicks = 0;
    bool paused = false;
    bool isStudying = false;
    double stepPerClick = 0;

    void Awake()
    {
        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredClicks;
            progressSlider.value = 0;

            // 令 slider 做 progress bar，唔可以拖
            progressSlider.interactable = false;
        }

        if (advanceButton != null)
        {
            advanceButton.onClick.RemoveAllListeners();
            advanceButton.onClick.AddListener(OnAdvanceClicked);
        }
    }

    void Start()
    {
        StartStudying();
    }

    public void StartStudying()
    {
        if (videoPlayer == null) return;

        clicks = 0;
        paused = false;
        isStudying = true;

        if (progressSlider != null) progressSlider.value = 0;
        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);

        videoPlayer.Stop();
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = studyingUrl;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPreparedStudying;
    }

    void OnPreparedStudying(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPreparedStudying;
        vp.Play();

        // 計每click前進幾多秒：用剩餘長度平均分25份
        double len = vp.length;
        if (len > pauseAtSeconds + 0.2)
        {
            stepPerClick = (len - pauseAtSeconds) / requiredClicks;
            if (stepPerClick < minStepSeconds) stepPerClick = minStepSeconds;
        }
        else
        {
            stepPerClick = minStepSeconds;
        }

        StartCoroutine(PauseAtTime());
    }

    IEnumerator PauseAtTime()
    {
        // 等到 3 秒
        while (videoPlayer != null && videoPlayer.isPlaying && videoPlayer.time < pauseAtSeconds)
            yield return null;

        if (!isStudying) yield break;

        // 停住 + 顯示 UI
        videoPlayer.Pause();
        videoPlayer.time = pauseAtSeconds;
        paused = true;

        if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);
    }

    void OnAdvanceClicked()
    {
        if (!isStudying || !paused || videoPlayer == null) return;

        clicks++;
        if (progressSlider != null) progressSlider.value = clicks;

        // 先推進時間，再短播一下，再停返（你會見到「有前進」）
        double target = pauseAtSeconds + (clicks * stepPerClick);
        if (videoPlayer.length > 0.1)
            target = System.Math.Min(target, videoPlayer.length - 0.05);

        StopAllCoroutines(); // 防止重疊 coroutine
        StartCoroutine(PlayTinyStep(target));

        if (clicks >= requiredClicks)
        {
            // 完成：隱藏 UI，直接播到完，完咗就去下一條
            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(false);
            paused = false;
            StartCoroutine(PlayToEndThenNext());
        }
    }

    IEnumerator PlayTinyStep(double targetTime)
    {
        videoPlayer.time = targetTime;
        videoPlayer.Play();

        yield return new WaitForSeconds(holdPlaySeconds);

        if (isStudying && clicks < requiredClicks)
        {
            videoPlayer.Pause();
            paused = true;
            if (clickAdvanceUI != null) clickAdvanceUI.SetActive(true);
        }
    }

    IEnumerator PlayToEndThenNext()
    {
        // 讓 studying 繼續播
        videoPlayer.Play();

        // 等到播完（或接近播完）
        while (videoPlayer != null && videoPlayer.isPlaying && videoPlayer.time < videoPlayer.length - 0.05)
            yield return null;

        // 下一條
        isStudying = false;
        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = nextUrl;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPreparedNext;
    }

    void OnPreparedNext(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPreparedNext;
        vp.Play();
    }
}
