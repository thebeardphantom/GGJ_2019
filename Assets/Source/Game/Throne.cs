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
    private Block _teleportBlock;

    [SerializeField]
    private Block _endTeleportBlock;

    [SerializeField]
    private float _timer;

    [SerializeField]
    private bool _onlyMoveOnActivate;

    [SerializeField]
    private Transform _endPosition;

    private Coroutine _timerRoutine;
    private Coroutine _moveRoutine;

    #endregion

    #region Properties

    public bool HasToken { get; private set; }

    public bool IsMoving { get; private set; }

    public Block TeleportBlock
    {
        get
        {
            if(_endTeleportBlock == null)
            {
                return _teleportBlock;
            }
            else
            {
                var startBlockDist = Vector3.Distance(transform.position, _teleportBlock.transform.position);
                var endBlockDist = Vector3.Distance(transform.position, _endTeleportBlock.transform.position);
                return startBlockDist < endBlockDist ? _teleportBlock : _endTeleportBlock;
            }
        }
    }

    #endregion

    #region Methods

    public void AddRemoveToken(bool add)
    {
        if(!add && !_onlyMoveOnActivate)
        {
            GameController.Instance.SetTimerPlay(false);
        }
        if (!_onlyMoveOnActivate && _timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }
        if(!_onlyMoveOnActivate || _moveRoutine == null)
        {
            HasToken = add;
            _token.transform.DOKill();
            _token.transform.DOScale(add ? 0.25f : 0f, 0.5f);
            TokenAddRemove?.Invoke(add);
        }
        if (add && _timer > 0f)
        {
            _timerRoutine = StartCoroutine(TokenTimer());
        }

        if (add && _onlyMoveOnActivate)
        {
            _moveRoutine = StartCoroutine(MoveLoop());
        }
    }

    private void Awake()
    {
        _token.transform.localScale = Vector3.zero;
        if (_endPosition != null && !_onlyMoveOnActivate)
        {
            _moveRoutine = StartCoroutine(MoveLoop());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _moveRoutine = null;
    }

    private IEnumerator MoveLoop()
    {
        var home = transform.position;
        while (true)
        {
            if(!_onlyMoveOnActivate)
            {
                yield return new WaitForSeconds(3f);
            }
            IsMoving = true;
            yield return transform.DOMove(_endPosition.position, 5f).SetSpeedBased().WaitForCompletion();
            IsMoving = false;
            yield return new WaitForSeconds(3f);
            IsMoving = true;
            yield return transform.DOMove(home, 5f).SetSpeedBased().WaitForCompletion();
            IsMoving = false;
            if (_onlyMoveOnActivate)
            {
                HasToken = false;
                _token.transform.DOKill();
                _token.transform.DOScale(0f, 0.5f);
                TokenAddRemove?.Invoke(false);
                GameController.Instance.SetTimerPlay(false);
                break;
            }
        }

        _moveRoutine = null;
    }

    private IEnumerator TokenTimer()
    {
        GameController.Instance.SetTimerPlay(true);
        yield return new WaitForSeconds(_timer);
        _timerRoutine = null;
        GameController.Instance.SetTimerPlay(false);
        GameController.Instance.Player.TokenInteraction.RetrieveToken(true);
    }

    #endregion
}