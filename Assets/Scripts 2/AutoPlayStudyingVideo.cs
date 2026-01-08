using UnityEngine;
using UnityEngine.Video;

public class AutoPlayStudyingVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string url;

    void Start()
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.Play();
    }
}
