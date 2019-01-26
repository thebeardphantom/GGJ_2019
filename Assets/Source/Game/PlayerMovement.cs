using System;
using System.Collections;
using DG.Tweening;
using RSG;
using UnityEngine;

[ExecuteAlways]
public class PlayerMovement : MonoBehaviour
{
    #region Types

    public enum State
    {
        Idle,
        PlayerControl,
        ScriptControl,
        SequenceControl
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

    private RaycastHit? _lastValidNode;

    private Vector3 _lastPosition;

    private Vector3 _velocity;

    #endregion

    #region Methods

    private static RaycastHit? FindNode(Vector3 raycastStart)
    {
        RaycastHit hit;
        raycastStart.y = 500f;
        var mask = LayerMask.GetMask("LevelGeometry");
        var ray = new Ray(raycastStart, Vector3.down);
        if (Physics.Raycast(ray, out hit, 1000f, mask))
        {
            Debug.DrawLine(ray.origin, hit.point);
            return hit;
        }

        Debug.DrawLine(ray.origin, ray.GetPoint(1000f));
        return null;
    }

    public IPromise LerpToPosition(Vector3 position)
    {
        PlayerState = State.SequenceControl;
        position = CorrectPosition(position, true);
        DOTween.Kill(transform);
        return transform.DOMove(position, 3f).SetEase(Ease.InOutQuad)
            .ToPromise()
            .Then(
                tween =>
                {
                    PlayerState = State.Idle;
                });
    }

    public void Teleport(Vector3 position)
    {
        DOTween.Kill(transform);
        transform.position = CorrectPosition(position, false);
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
        if (PlayerState != State.SequenceControl)
        {
            DOTween.Kill(transform);
            PlayerState = State.PlayerControl;
            StartCoroutine(PressArrowRoutine(arrow));
        }
    }

    private void OnArrowReleased(DirectionalArrow arrow)
    {
        StopAllCoroutines();
        if (PlayerState == State.PlayerControl)
        {
            PlayerState = State.ScriptControl;

            var position = transform.position;
            var nextNode = FindNode(position);
            nextNode = nextNode ?? _lastValidNode;
            position = nextNode.Value.point;
            position.x = nextNode.Value.transform.position.x;
            position.z = nextNode.Value.transform.position.z;
            position = CorrectPosition(position, false);
            DOTween.Kill(transform);
            transform.DOMove(position, 0.25f)
                .SetEase(Ease.OutQuad)
                .OnComplete(
                    () =>
                    {
                        PlayerState = State.Idle;
                    });
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
            var newNode = FindNode(newPosition);
            if (newNode != null)
            {
                _lastValidNode = newNode;
                newPosition = CorrectPosition(newNode.Value.point, false);
                transform.position = newPosition;
            }

            yield return null;
        }
    }

    private void Update()
    {
        Shader.SetGlobalVector("_PlayerPosition", transform.position);

        var targetVelocity = transform.position - _lastPosition;
        _lastPosition = transform.position;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * _velocityInterpolateSpeed);
        Shader.SetGlobalVector("_PlayerVelocity", _velocity);

        // TODO: Hook up diretional scaling?
        //Shader.SetGlobalVector("_PlayerLook", transform.forward);
        //if (!Application.isPlaying) { }
    }

    private void ResetVelocity()
    {
        _lastPosition = transform.position;
        _velocity = Vector3.zero;
    }

    private Vector3 CorrectPosition(Vector3 position, bool raycast)
    {
        if (raycast)
        {
            var node = FindNode(position);
            position = node.Value.point;
        }

        position.y += _yOffset;
        return position;
    }

    #endregion
}