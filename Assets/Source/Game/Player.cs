using UnityEngine;

public class Player : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private PlayerMovement _movement;

    [SerializeField]
    private PlayerTokenInteraction _tokenInteraction;

    [SerializeField]
    private float _respawnHeight;

    #endregion

    #region Properties

    public PlayerMovement Movement => _movement;

    public PlayerTokenInteraction TokenInteraction => _tokenInteraction;

    #endregion

    #region Methods

    public void Respawn()
    {
        transform.localScale = Vector3.one;
        var respawnBlock = GameObject.FindWithTag("Respawn");
        var position = respawnBlock.transform.position;
        position.y = _respawnHeight;
        TokenInteraction.RetrieveToken(true);
        Movement.Teleport(position);
        gameObject.SetActive(true);
        Movement.LerpToPosition(respawnBlock.transform.position);
    }

    #endregion
}