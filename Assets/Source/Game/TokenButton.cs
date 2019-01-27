using UnityEngine;
using UnityEngine.EventSystems;

public class TokenButton : MonoBehaviour, IPointerClickHandler
{
    #region Methods

    /// <inheritdoc />
    public void OnPointerClick(PointerEventData eventData)
    {
        var player = GameController.Instance.Player;
        if (player.TokenInteraction.CanUseToken && player.TokenInteraction.UnlockedToken)
        {
            if (player.TokenInteraction.HoldingToken)
            {
                player.TokenInteraction.PlaceToken();
            }
            else
            {
                player.TokenInteraction.RetrieveToken(false);
            }
        }
    }

    #endregion
}