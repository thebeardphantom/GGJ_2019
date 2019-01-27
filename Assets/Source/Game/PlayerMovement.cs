using System;
using System.Collections;
using DG.Tweening;
using RSG;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Types

    public enum State
    {
        Idle,
        PlayerControl,
        ScriptControl
    }

    #endregion

    #region Fields

    [NonSerialized]
    public State PlayerState;

    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _yOffset;

    [SerializeField]
    private float _velocityInterpolateSpeed;

    [SerializeField]
    private float _moveInterpolateSpeed;

    private NodeHit _lastValidNode;

    private Vector3 _lastPosition;

    private Vector3 _apparentVelocity;

    private Vector3 _targetPosition;
    private Coroutine _arrowRoutine;

    #endregion

    #region Methods

    public IPromise LerpToPosition(Vector3 position, float speed = 3f, bool speedBased = false, bool ignoreHeightCheck = false)
    {
        PlayerState = State.ScriptControl;
        position = CorrectPosition(position, true, ignoreHeightCheck);
        return transform.DOMove(position, speed)
            .SetSpeedBased(speedBased)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .ToPromise()
            .Then(
                tween =>
                {
                    PlayerState = State.Idle;
                    UpdateTargetPosition(transform.position);
                });
    }

    public void Teleport(Vector3 position)
    {
        _targetPosition = CorrectPosition(position, false);
        transform.position = _targetPosition;
        ResetVelocity();
        _lastValidNode = FindNode(transform.position);
    }

    public void WaitForState(State state, Action onState)
    {
        if (PlayerState == state)
        {
            onState();
        }
        else
        {
            StartCoroutine(WaitForStateRoutine(state, onState));
        }
    }

    public void TeleportToBlock(Block block)
    {
        Time.timeScale = 0f;
        transform.DOScale(0.25f, 0.125f)
            .SetUpdate(true)
            .ToUntypedPromise()
            .Then(() => LerpToPosition(block.Anchor.position, 10f, true, true))
            .Then(() => transform.DOScale(1f, 0.125f))
            .Done(
                () =>
                {
                    Time.timeScale = 1f;
                });
    }

    private NodeHit FindNode(Vector3 raycastStart, bool ignoreHeightCheck = false)
    {
        var startY = transform.position.y;
        raycastStart.y += 500f;
        var mask = LayerMask.GetMask("LevelGeometry");
        var ray = new Ray(raycastStart, Vector3.down);
        var hits = Physics.RaycastAll(ray, 1000f, mask);
        foreach (var hit in hits)
        {
            if (ignoreHeightCheck || hit.point.y <= startY)
            {
                Debug.DrawLine(ray.origin, hit.point);
                return new NodeHit(hit);
            }
        }

        Debug.DrawLine(ray.origin, ray.GetPoint(1000f));
        return null;
    }

    private IEnumerator WaitForStateRoutine(State state, Action onState)
    {
        while (PlayerState != state)
        {
            yield return null;
        }

        onState();
    }

    private void Awake()
    {
        _lastValidNode = FindNode(transform.position);
    }

    private void OnEnable()
    {
        DirectionalArrow.ArrowPressed += OnArrowPressed;
        DirectionalArrow.ArrowReleased += OnArrowReleased;
    }

    private void OnDisable()
    {
        DirectionalArrow.ArrowPressed -= OnArrowPressed;
        DirectionalArrow.ArrowReleased -= OnArrowReleased;
    }

    private void OnArrowPressed(DirectionalArrow arrow)
    {
        if (PlayerState != State.ScriptControl)
        {
            PlayerState = State.PlayerControl;
            _arrowRoutine = StartCoroutine(PressArrowRoutine(arrow));
        }
    }

    private void OnArrowReleased(DirectionalArrow arrow)
    {
        if (_arrowRoutine != null)
        {
            StopCoroutine(_arrowRoutine);
            _arrowRoutine = null;
        }

        if (PlayerState == State.PlayerControl)
        {
            var position = transform.position;
            var nextNode = FindNode(position);
            nextNode = nextNode ?? _lastValidNode;
            position = nextNode.Anchor;
            position = CorrectPosition(position, false);
            _targetPosition = position;
            PlayerState = State.Idle;
        }
    }

    private IEnumerator PressArrowRoutine(DirectionalArrow arrow)
    {
        var move = Vector3.zero;
        switch (arrow.Direction)
        {
            case Direction.UpLeft:
            {
                move = Vector3.back;
                break;
            }
            case Direction.UpRight:
            {
                move = Vector3.left;
                break;
            }
            case Direction.DownRight:
            {
                move = Vector3.forward;
                break;
            }
            case Direction.DownLeft:
            {
                move = Vector3.right;
                break;
            }
        }

        while (true)
        {
            if (PlayerState != State.PlayerControl)
            {
                break;
            }

            var newPosition = transform.position + move * Time.deltaTime * _speed;
            UpdateTargetPosition(newPosition);

            yield return null;
        }
    }

    private void UpdateTargetPosition(Vector3 newPosition)
    {
        var newNode = FindNode(newPosition);
        if (newNode != null)
        {
            _lastValidNode = newNode;
            newPosition = CorrectPosition(newNode.Hit, false);
            _targetPosition = newPosition;
        }
    }

    private void Update()
    {
        Shader.SetGlobalVector("_PlayerPosition", transform.position);

        if (PlayerState != State.ScriptControl)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                _targetPosition,
                _moveInterpolateSpeed * Time.deltaTime);
        }

        var targetVelocity = transform.position - _lastPosition;
        _apparentVelocity = Vector3.Lerp(_apparentVelocity, targetVelocity, Time.deltaTime * _velocityInterpolateSpeed);
        Shader.SetGlobalVector("_PlayerVelocity", _apparentVelocity);

        _lastPosition = transform.position;

        if (_lastValidNode != null && _lastValidNode.Block.IsOff)
        {
            GameController.Instance.Player.Respawn();
        }
    }

    private void ResetVelocity()
    {
        _lastPosition = transform.position;
        _apparentVelocity = Vector3.zero;
    }

    private Vector3 CorrectPosition(Vector3 position, bool raycast, bool ignoreHeightCheck = false)
    {
        if (raycast)
        {
            var node = FindNode(position, ignoreHeightCheck);
            position = node.Anchor;
        }

        position.y += _yOffset;
        return position;
    }

    #endregion
}

public class NodeHit
{
    #region Fields

    public Transform Node;

    public Vector3 Hit;

    public Vector3 Anchor;

    public Block Block;

    #endregion

    public NodeHit(RaycastHit hit)
    {
        Hit = hit.point;
        Node = hit.transform;
        Block = hit.transform.GetComponent<Block>();
        if (Block == null)
        {
            Debug.LogError($"{hit.transform.name} has no block!");
        }
        else
        {
            Anchor = Block.Anchor.position;
        }
    }
}