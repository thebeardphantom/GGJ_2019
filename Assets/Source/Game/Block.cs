using DG.Tweening;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Transform _anchor;

    [SerializeField]
    private Throne _activatedBy;

    [SerializeField]
    private Throne[] _orActivatedBy;

    #endregion

    #region Properties

    public Transform Anchor => _anchor;

    public bool IsOff { get; private set; }

    #endregion

    #region Methods

    private void Awake()
    {
        if (_activatedBy != null)
        {
            transform.localScale = Vector3.zero;
            IsOff = true;
            _activatedBy.TokenAddRemove += OnTokenAddRemove;
        }

        if (_orActivatedBy != null && _orActivatedBy.Length > 0)
        {
            IsOff = true;
            transform.localScale = Vector3.zero;
            foreach (var activatedBy in _orActivatedBy)
            {
                activatedBy.TokenAddRemove += OnTokenAddRemove;
            }
        }
    }

    private void OnTokenAddRemove(bool add)
    {
        transform.DOKill();
        transform.DOScale(add ? 1f : 0f, 1f);
        IsOff = !add;
    }

    #endregion
}