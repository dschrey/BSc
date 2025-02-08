using System.Collections;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    private ExperimentData _cachedExperiment;
    private Trail _cacheTrail;
    private PathData _cachedPath;

    private ExperimentState _cachedExerpimentState;
    private AssessmentData _cachedAssessmentData;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void LoadExperimentScene(ExperimentData experiment, PathData selectedPath, Trail selectedTrail)
    {
        StartCoroutine(TransitionToScene("ExperimentScene"));
        _cachedExperiment = experiment;
        _cachedPath = selectedPath;
        _cacheTrail = selectedTrail;

    }

    public void LoadStartScene(ExperimentState state)
    {
        _cachedExerpimentState = state;
        _cachedAssessmentData =  AssessmentManager.Instance.GetAssessment();
        StartCoroutine(TransitionToScene("StartScene"));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        // XROrigin in current scene
        XRFadeTransition currentScene = FindObjectOfType<XRFadeTransition>();
        yield return StartCoroutine(currentScene.Fade(0, 1));

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // XROrigin in target scene
        XRFadeTransition targetScene = FindObjectOfType<XRFadeTransition>();
        yield return StartCoroutine(targetScene.Fade(1, 0));

        SceneSetupFunctions();
    }


    private void SceneSetupFunctions()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "ExperimentScene")
        {
            ResourceManager.Instance.InitializeRenderObjects();
            ExperimentManager.Instance.SetupExperimentScene();
            ExperimentManager.Instance.PrepareExperiment(_cachedExperiment, _cachedPath);
            AssessmentManager.Instance.CreateNewAssessment(_cachedExperiment.id, _cachedPath, _cacheTrail.floor);
        }

        if (currentScene == "StartScene")
        {
            ExperimentUIManager experimentUI = FindObjectOfType<ExperimentUIManager>();
            experimentUI.LoadNextExperimentPanel(_cachedExperiment, _cachedExerpimentState, _cachedAssessmentData);
            ExperimentManager.Instance.SetupExperimentScene();
        }
    }



}
