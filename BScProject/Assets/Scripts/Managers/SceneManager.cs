using System.Collections;
using Unity.VisualScripting;
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

    public void LoadExperimentScene(ExperimentData experiment, PathData selectedPath, Trail selectedTrail, AssessmentData assessment)
    {
        StartCoroutine(TransitionToScene("ExperimentScene"));
        _cachedExperiment = experiment;
        _cachedPath = selectedPath;
        _cacheTrail = selectedTrail;
        _cachedAssessmentData = assessment;
    }

    public void LoadStartScene(ExperimentState state)
    {
        _cachedExerpimentState = state;
        _cachedAssessmentData =  AssessmentManager.Instance.GetAssessment();
        ResourceManager.Instance.FreeRenderTextures();
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
            if (ExperimentManager.Instance.SetupExperiment(_cachedExperiment, _cachedPath, _cacheTrail.floor))
            {
                ExperimentManager.Instance.StartExperiment();
                AssessmentManager.Instance.PrepareAssessment(_cachedExperiment.id, _cachedPath, _cacheTrail.floor, _cachedAssessmentData);
            }
            else Debug.LogError($"Experiment could not be prepared.");
        }

        if (currentScene == "StartScene")
        {
            ExperimentUIManager experimentUI = FindObjectOfType<ExperimentUIManager>();
            experimentUI.LoadNextExperimentPanel(_cachedExperiment, _cachedExerpimentState, _cachedAssessmentData);
            // ExperimentManager.Instance.SetupExperimentScene();
        }
    }



}
