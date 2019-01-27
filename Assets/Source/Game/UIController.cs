using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    #region Fields

    [SerializeField]
    private DirectionalArrow _upLeftArrow;

    [SerializeField]
    private DirectionalArrow _upRightArrow;

    [SerializeField]
    private DirectionalArrow _downRightArrow;

    [SerializeField]
    private DirectionalArrow _downLeftArrow;

    [SerializeField]
    private Button _close;

    [SerializeField]
    private Animator _controlsAnimator;

    #endregion

    #region Methods

    private void Awake()
    {
        GameController.Instance.Player.TokenInteraction.TokenUnlocked += OnTokenUnlocked;
        GameController.Instance.Player.TokenInteraction.CanUseTokenChanged += OnCanUseTokenChanged;
        GameController.Instance.Player.TokenInteraction.HoldingTokenChanged += OnHoldingTokenChanged;
        _close.onClick.AddListener(OnCloseButton);
    }

    private void OnCloseButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnHoldingTokenChanged(bool holding)
    {
        _controlsAnimator.SetBool("HoldingToken", holding);
    }

    private void OnCanUseTokenChanged(bool canUseToken)
    {
        _controlsAnimator.SetBool("CanUseToken", canUseToken);
    }

    private void OnTokenUnlocked()
    {
        _controlsAnimator.SetBool("TokenUnlocked", true);
    }

    #endregion
}