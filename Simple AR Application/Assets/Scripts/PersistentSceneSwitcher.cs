using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PersistentSceneSwitcher : MonoBehaviour
{
    public Button nextButton;
    public Button previousButton;

    private static PersistentSceneSwitcher instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        nextButton.onClick.AddListener(NextScene);
        previousButton.onClick.AddListener(PreviousScene);
        SetButtonVisibility(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetButtonVisibility(scene);
        StartCoroutine(ActivateARPlaneManagerWithDelay(0.5f));
    }

    IEnumerator ActivateARPlaneManagerWithDelay(float delay)
    {
        ARPlaneManager planeManager = FindObjectOfType<ARPlaneManager>();

        if (planeManager != null)
        {
            planeManager.enabled = false;
            yield return new WaitForSeconds(delay);
            planeManager.enabled = true;
        }
    }

    void SetButtonVisibility(Scene scene)
    {
        int currentSceneIndex = scene.buildIndex;
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        previousButton.gameObject.SetActive(currentSceneIndex != 0);
        nextButton.gameObject.SetActive(currentSceneIndex != lastSceneIndex);
    }

    void NextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }

    void PreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = (currentSceneIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(previousSceneIndex);
    }
}
