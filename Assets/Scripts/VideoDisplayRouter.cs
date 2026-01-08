using UnityEngine;
using UnityEngine.UI;

public class VideoDisplayRouter : MonoBehaviour
{
    public RawImage target;
    public RenderTexture rtA;
    public RenderTexture rtB;

    void Awake()
    {
        if (target == null) target = GetComponent<RawImage>();
        ShowA();
    }

    public void ShowA()
    {
        if (target != null) target.texture = rtA;
    }

    public void ShowB()
    {
        if (target != null) target.texture = rtB;
    }
}
