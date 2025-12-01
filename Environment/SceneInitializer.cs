using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        OptionManager.Instance?.OnSceneChanged(SceneManager.GetActiveScene().name);
    }
}
