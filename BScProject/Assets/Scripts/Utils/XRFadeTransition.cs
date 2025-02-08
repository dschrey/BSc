using System.Collections;
using UnityEngine;

public class XRFadeTransition : MonoBehaviour
{

    [SerializeField] private Material _fadeMaterial;
    private Color _fadeColor;

    private void Start() 
    {
        _fadeColor = _fadeMaterial.color;
    }

    public IEnumerator FadeAndTeleport(Transform destination)
    {
        yield return StartCoroutine(Fade(0, 1));
        ExperimentManager.Instance.TeleportPlayer(destination);
        yield return StartCoroutine(Fade(1, 0));
            
    }

    public IEnumerator Fade(float alphaIn, float alphaOut)
    {
        float time = 0;
        float fadeDuration = DataManager.Instance.Settings.TransitionDuration / 2;

        while (time <= fadeDuration)
        {
            Color color = _fadeColor;
            color.a = Mathf.Lerp(alphaIn, alphaOut, time / fadeDuration);
            _fadeMaterial.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _fadeMaterial.color = finalColor;
    }
}
