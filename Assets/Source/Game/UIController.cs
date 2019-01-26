using UnityEngine;

public class UIController : Singleton<UIController>
{
    #region Fields

    [SerializeField]
    private DirectionalArrow _upLeftArrow;

    [SerializeField]
    private DirectionalArrow _upRightArrow;

    [SerializeField]
    private DirectionalArrow _downRightArrow;

    [SerializeField]
    private DirectionalArrow _downLeftArrow;

    #endregion
}