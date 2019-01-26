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
        if (other.transform.IsChildOf(GameController.Instance.Player.transform))
        {
            PlayerEnteredTriggerVolume();
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