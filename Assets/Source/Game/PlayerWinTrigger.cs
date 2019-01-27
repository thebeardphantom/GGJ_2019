using DG.Tweening;

public class PlayerWinTrigger : PlayerTriggerVolume
{
    #region Methods

    /// <inheritdoc />
    protected override void PlayerEnteredTriggerVolume()
    {
        var player = GameController.Instance.Player;
        player.transform.DOScale(0f, 1f)
            .ToPromise()
            .Then(tween =>
            {
                return GameController.Instance.LoadNextMap();
            })
            .Done();
    }

    #endregion
}