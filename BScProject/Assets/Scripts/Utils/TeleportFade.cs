using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportFade : MonoBehaviour
{

    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private Material _fadeMaterial;
    private Color _fadeColor;

    private void Start() 
    {
        _fadeColor = _fadeMaterial.color;
    }

    public IEnumerator FadeAndTeleport(float alphaIn, float alphaOut, Transform target)
    {
        float time = 0;

        while (time <= _fadeDuration)
        {
            Color color = _fadeColor;
            color.a = Mathf.Lerp(alphaIn, alphaOut, time / _fadeDuration);
            _fadeMaterial.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _fadeMaterial.color = finalColor;

        ExperimentManager.Instance.TeleportPlayer(target);
        FadeIn();
            
    }

    private IEnumerator Fade(float alphaIn, float alphaOut)
    {
        float time = 0;

        while (time <= _fadeDuration)
        {
            Color color = _fadeColor;
            color.a = Mathf.Lerp(alphaIn, alphaOut, time / _fadeDuration);
            _fadeMaterial.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _fadeMaterial.color = finalColor;


    }
    private IEnumerator FadeSceneSwitch(float alphaIn, float alphaOut, string SceneName)
    {
        float time = 0;

        while (time <= _fadeDuration)
        {
            Color color = _fadeColor;
            color.a = Mathf.Lerp(alphaIn, alphaOut, time / _fadeDuration);
            _fadeMaterial.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _fadeMaterial.color = finalColor;

        
        SceneManager.LoadSceneAsync(SceneName);

    }

    public void FadeIn()
    {
        StartCoroutine(Fade(1, 0));
    }

    public void FadeOutScene(string sceneName)
    {
        StartCoroutine(FadeSceneSwitch(0, 1, sceneName));
    }
}
