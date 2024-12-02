using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void LoadSceneByID(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
