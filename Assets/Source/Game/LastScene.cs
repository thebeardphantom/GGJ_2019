using UnityEngine;

public class LastScene : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Canvas _canvas;

    #endregion

    #region Methods

    private void Awake()
    {
        _canvas.worldCamera = GameController.Instance.UICamera;
    }

    #endregion
}