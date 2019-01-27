using DG.Tweening;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Transform _anchor;

    [SerializeField]
    private Throne _activatedBy;

    #endregion

    #region Properties

    public Transform Anchor => _anchor;

    #endregion

    #region Methods

    private void Awake()
    {
        if (_activatedBy != null)
        {
            transform.localScale = Vector3.zero;
            _activatedBy.TokenAddRemove += OnTokenAddRemove;
        }
    }

    private void OnTokenAddRemove(bool add)
    {
        transform.DOKill();
        transform.DOScale(add ? 1f : 0f, 1f);
    }

    #endregion
}