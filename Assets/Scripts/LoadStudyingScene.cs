using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadStudyingScene : MonoBehaviour
{
    public void GoToStudyingScene()
    {
        SceneManager.LoadScene("StudyingScene");
    }
}
