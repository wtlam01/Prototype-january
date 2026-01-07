using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartToVideoThenImage : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public GameObject startButtonObject; // optional: same as button gameObject
    public GameObject videoRawImageObject;
    public GameObject studyRoomImageObject;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoUrl = "https://w33lam.panel.uwe.ac.uk/Video/1_1.mp4";

    private void Awake()
    {
        // Initial state
        if (videoRawImageObject != null) videoRawImageObject.SetActive(false);
        if (studyRoomImageObject != null) studyRoomImageObject.SetActive(false);

        // Hook button
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        // When video finishes
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        // Clean up
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnStartClicked()
    {
        // Hide start UI
        if (startButtonObject != null) startButtonObject.SetActive(false);
        else if (startButton != null) startButton.gameObject.SetActive(false);

        // Show video display
        if (videoRawImageObject != null) videoRawImageObject.SetActive(true);

        // Prepare and play
        if (videoPlayer != null)
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoUrl;

            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnPrepared;
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Hide video view
        if (videoRawImageObject != null) videoRawImageObject.SetActive(false);

        // Show final image
        if (studyRoomImageObject != null) studyRoomImageObject.SetActive(true);
    }
}
