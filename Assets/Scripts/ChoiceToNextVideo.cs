using UnityEngine;
using UnityEngine.Video;

public class ChoiceToNextVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoRawImageObject;
    public GameObject studyRoomImageObject;

    [Header("URL")]
    public string nextUrl;

    public void PlayNext()
    {
        if (studyRoomImageObject) studyRoomImageObject.SetActive(false);
        if (videoRawImageObject) videoRawImageObject.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = nextUrl;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepared;
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        vp.Play();
    }
}
