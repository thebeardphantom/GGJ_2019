using UnityEngine;
using UnityEngine.EventSystems;

public class DirectionalArrow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    #region Types

    public delegate void OnArrowPressed(DirectionalArrow arrow);

    public delegate void OnArrowReleased(DirectionalArrow arrow);

    #endregion

    #region Events

    public static event OnArrowPressed ArrowPressed;

    public static event OnArrowReleased ArrowReleased;

    #endregion

    #region Fields

    [SerializeField]
    private Direction _direction;

    #endregion

    #region Properties

    public Direction Direction => _direction;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 0.9f;
        ArrowPressed?.Invoke(this);
    }

    /// <inheritdoc />
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        ArrowReleased?.Invoke(this);
    }

    /// <inheritdoc />
    public void OnPointerClick(PointerEventData eventData)
    {

    }

    #endregion
}

public enum Direction
{
    UpLeft,
    UpRight,
    DownRight,
    DownLeft
}