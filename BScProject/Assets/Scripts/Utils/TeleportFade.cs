using System.Collections;
using UnityEngine;

public class TeleportFade : MonoBehaviour
{

    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private Material _fadeMaterial;
    private Color _fadeColor;

    private void Start() 
    {
        _fadeColor = _fadeMaterial.color;
    }

    public IEnumerator FadeAndTeleport(float alphaIn, float alphaOut, Vector3 position)
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

        ExperimentManager.Instance.TeleportPlayer(position);
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

    public void FadeIn()
    {
        StartCoroutine(Fade(1, 0));
    }
}
