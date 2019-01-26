using System.Collections;
using DG.Tweening;
using UnityEngine;

[ExecuteAlways]
public class PlayerMovement : MonoBehaviour
{
    #region Fields

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

    public void LerpToPosition(Vector3 position)
    {
        position = CorrectPosition(position, true);
        transform.DOMove(position, 3f).SetEase(Ease.InOutQuad);
    }

    public void Teleport(Vector3 position)
    {
        transform.position = CorrectPosition(position, false);
        ResetVelocity();
        _lastValidNode = FindNode(transform.position);
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
        StartCoroutine(PressArrowRoutine(arrow));
    }

    private void OnArrowReleased(DirectionalArrow arrow)
    {
        StopAllCoroutines();

        var position = transform.position;
        var nextNode = FindNode(position);
        nextNode = nextNode ?? _lastValidNode;
        position = nextNode.Value.point;
        position.x = nextNode.Value.transform.position.x;
        position.z = nextNode.Value.transform.position.z;
        position = CorrectPosition(position, false);
        transform.DOMove(position, 0.25f).SetEase(Ease.OutQuad);
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