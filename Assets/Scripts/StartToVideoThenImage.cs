using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartToVideoThenImage : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public GameObject startButtonObject;
    public GameObject videoRawImageObject;
    public GameObject studyRoomImageObject;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoUrl = "https://w33lam.panel.uwe.ac.uk/Video/1_1.mp4";

    private bool hasFinishedIntro = false;

    private void Awake()
    {
        // 初始狀態
        if (videoRawImageObject) videoRawImageObject.SetActive(false);
        if (studyRoomImageObject) studyRoomImageObject.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnStartClicked()
    {
        if (hasFinishedIntro) return;

        if (startButtonObject) startButtonObject.SetActive(false);
        else if (startButton) startButton.gameObject.SetActive(false);

        if (videoRawImageObject) videoRawImageObject.SetActive(true);
        if (studyRoomImageObject) studyRoomImageObject.SetActive(false);

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (hasFinishedIntro) return;
        hasFinishedIntro = true;

        // 第一段播完 -> 出四按鈕畫面
        if (videoRawImageObject) videoRawImageObject.SetActive(false);
        if (studyRoomImageObject) studyRoomImageObject.SetActive(true);

        // ⭐最重要：之後唔再干涉其他影片
        this.enabled = false;
    }
}
