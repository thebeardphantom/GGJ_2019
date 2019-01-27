public class PlayerWinTrigger : PlayerTriggerVolume
{
    /// <inheritdoc />
    protected override void PlayerEnteredTriggerVolume()
    {
        GameController.Instance.LoadNextMap();
    }
}