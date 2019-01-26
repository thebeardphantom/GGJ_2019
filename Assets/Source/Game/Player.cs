using UnityEngine;

public class Player : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private float _respawnHeight;

    #endregion

    #region Properties

    public PlayerMovement Movement => _movement;

    #endregion

    #region Methods

    public void Respawn()
    {
        transform.localScale = Vector3.one;
        var respawnBlock = GameObject.FindWithTag("Respawn");
        var position = respawnBlock.transform.position;
        position.y = _respawnHeight;
        Movement.Teleport(position);
        Movement.LerpToPosition(respawnBlock.transform.position);
    }

    #endregion
}