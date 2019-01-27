using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Throne : MonoBehaviour
{
    #region Types

    public delegate void OnTokenAddRemove(bool add);

    #endregion

    #region Events

    public event OnTokenAddRemove TokenAddRemove;

    #endregion

    #region Fields

    [SerializeField]
    private GameObject _token;

    [SerializeField]
    private float _timer;

    #endregion

    #region Properties

    public bool HasToken { get; private set; }

    #endregion

    #region Methods

    private void Awake()
    {
        _token.transform.localScale = Vector3.zero;
    }

    public void AddRemoveToken(bool add)
    {
        GameController.Instance.SetTimerPlay(false);
        StopAllCoroutines();
        HasToken = add;
        _token.transform.DOKill();
        _token.transform.DOScale(add ? 0.25f : 0f, 0.5f);
        TokenAddRemove?.Invoke(add);
        if (add && _timer > 0f)
        {
            StartCoroutine(TokenTimer());
        }
    }

    private IEnumerator TokenTimer()
    {
        GameController.Instance.SetTimerPlay(true);
        yield return new WaitForSeconds(_timer);
        GameController.Instance.SetTimerPlay(false);
        GameController.Instance.Player.TokenInteraction.RetrieveToken(true);
    }

    #endregion
}