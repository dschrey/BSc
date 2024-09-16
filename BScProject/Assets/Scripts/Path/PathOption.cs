using UnityEngine;
using UnityEngine.UI;

public class PathOption : MonoBehaviour
{
    public PathData PathData;

    [SerializeField] private Image _image;

    private void OnEnable() 
    {
       if (_image != null)
       {
            _image.sprite = PathData.pathTexture;
       }
    }
}