using System.Linq;
using UnityEngine;

public class PlayerTokenInteraction : MonoBehaviour
{
    #region Types

    public delegate void OnTokenUnlocked();

    public delegate void OnCanUseTokenChanged(bool canUseToken);

    public delegate void OnHoldingTokenChanged(bool holding);

    #endregion

    #region Events

    public event OnTokenUnlocked TokenUnlocked;

    public event OnCanUseTokenChanged CanUseTokenChanged;

    public event OnHoldingTokenChanged HoldingTokenChanged;

    #endregion

    #region Properties

    public bool UnlockedToken { get; private set; }

    public bool HoldingToken { get; private set; }

    public bool CanUseToken { get; private set; }

    #endregion

    #region Methods

    public void SetUnlockedToken()
    {
        UnlockedToken = true;
        TokenUnlocked?.Invoke();
        HoldingToken = true;
        HoldingTokenChanged?.Invoke(true);
    }

    public void SetCanUseToken(bool canUseToken)
    {
        if (canUseToken != CanUseToken)
        {
            CanUseToken = canUseToken;
            CanUseTokenChanged?.Invoke(canUseToken);
        }
    }

    public void PlaceToken()
    {
        if (HoldingToken)
        {
            var throne = GetNearbyThrone();
            throne.AddRemoveToken(true);
            HoldingToken = false;
            HoldingTokenChanged?.Invoke(false);
        }
    }

    public void RetrieveToken(bool force)
    {
        if (force)
        {
            var throne = GameController.Instance.Thrones.FirstOrDefault(t => t.HasToken);
            throne.AddRemoveToken(false);
            HoldingToken = true;
            HoldingTokenChanged?.Invoke(true);
        }
        else if (!HoldingToken)
        {
            var throne = GetNearbyThrone();
            if (throne.HasToken)
            {
                throne.AddRemoveToken(false);
                HoldingToken = true;
                HoldingTokenChanged?.Invoke(true);
            }
        }
    }

    private void Update()
    {
        var throne = GetNearbyThrone();
        SetCanUseToken(throne != null);
    }

    private Throne GetNearbyThrone()
    {
        foreach (var throne in GameController.Instance.Thrones)
        {
            if (Vector3.Distance(transform.position, throne.transform.position) < 2f)
            {
                return throne;
            }
        }

        return null;
    }

    #endregion
}