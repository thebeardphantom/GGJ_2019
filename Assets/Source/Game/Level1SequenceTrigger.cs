using DG.Tweening;
using RSG;
using UnityEngine;

public class Level1SequenceTrigger : PlayerTriggerVolume
{
    #region Fields

    [SerializeField]
    private Transform _token;

    [SerializeField]
    private Light _directionLight;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void PlayerEnteredTriggerVolume()
    {
        GameController.Instance.Player.Movement.WaitForState(PlayerMovement.State.Idle, BeginSequence);
    }

    private void BeginSequence()
    {
        var player = GameController.Instance.Player;
        player.transform.DOScale(0f, 1f)
            .ToPromise()
            .Then(
                tween =>
                {
                    var mainCamera = Camera.main;
                    var worldPoint = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 50f));
                    var lookRot = Quaternion.LookRotation(-mainCamera.transform.forward);
                    var rotateTween = _token.DORotateQuaternion(lookRot, 3f).ToUntypedPromise();
                    var moveTween = _token.DOMove(worldPoint, 3f).ToUntypedPromise();
                    var lightTween = _directionLight.DOShadowStrength(0f, 3f).ToUntypedPromise();
                    return Promise.All(moveTween, rotateTween, lightTween);
                })
            .Then(() => _token.DOScale(20f, 3f).ToPromise())
            .Then(tween => GameController.Instance.LoadNextMap())
            .Done();
    }

    #endregion
}