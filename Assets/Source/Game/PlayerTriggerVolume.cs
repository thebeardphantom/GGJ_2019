using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public abstract class PlayerTriggerVolume : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Collider _trigger;

    #endregion

    #region Methods

    protected abstract void PlayerEnteredTriggerVolume();

    private void OnTriggerEnter(Collider other)
    {
        if (GameController.Instance.PlayerBeatLevel)
        {
            return;
        }
        if (other.transform.IsChildOf(GameController.Instance.Player.transform))
        {
            Debug.Log("Player entered win trigger");
            var player = GameController.Instance.Player;
            player.Movement.WaitForState(PlayerMovement.State.Idle, () =>
            {
                player.transform.DOKill();
                player.Movement.PlayerState = PlayerMovement.State.ScriptControl;
                PlayerEnteredTriggerVolume();
            });
        }
    }

    private void Awake()
    {
        _trigger.isTrigger = true;
    }

    private void OnValidate()
    {
        if (_trigger != null)
        {
            _trigger.isTrigger = true;
        }
    }

    #endregion
}