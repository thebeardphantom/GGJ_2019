using DG.Tweening;
using RSG;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    #region Fields

    [SerializeField]
    private Player _player;

    [SerializeField]
    private PostProcessProfile _vfxProfile;

    #endregion

    #region Properties

    public int CurrentChapter { get; private set; } = -1;

    public int CurrentLevel { get; private set; } = -1;

    public PromiseTimer Timer { get; } = new PromiseTimer();

    public Player Player => _player;

    #endregion

    #region Methods

    private static int GetMapBuildIndex(int chapter, int level)
    {
        return SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/Maps/{chapter}-{level}.unity");
    }

    public IPromise LoadMap(int chapter, int level)
    {
        var returnPromise = new Promise();
        var loadIndex = GetMapBuildIndex(chapter, level);
        TweenFaderColor(1f, 0.5f)
            .ToPromise()
            .Then(tween => Timer.WaitFor(0.5f))
            .Then(
                () =>
                {
                    SceneManager.MoveGameObjectToScene(Player.gameObject, gameObject.scene);
                    AsyncOperation unloadOp = null;
                    if (CurrentChapter >= 0 && CurrentLevel >= 0)
                    {
                        var unloadIndex = GetMapBuildIndex(CurrentChapter, CurrentLevel);
                        unloadOp = SceneManager.UnloadSceneAsync(unloadIndex);
                    }

                    var loadOp = SceneManager.LoadSceneAsync(loadIndex, LoadSceneMode.Additive);
                    if (unloadOp != null)
                    {
                        loadOp.allowSceneActivation = false;
                        unloadOp.completed += op =>
                        {
                            loadOp.allowSceneActivation = true;
                        };
                    }

                    return loadOp.ToPromise();
                })
            .Then(
                loadOp =>
                {
                    var scene = SceneManager.GetSceneByBuildIndex(loadIndex);
                    SceneManager.SetActiveScene(scene);
                    SceneManager.MoveGameObjectToScene(Player.gameObject, scene);
                    TweenFaderColor(0f, 1f);
                    Player.Respawn();
                    CurrentChapter = chapter;
                    CurrentLevel = level;
                });
        return returnPromise;
    }

    public IPromise LoadNextMap()
    {
        var nextChapter = CurrentChapter;
        var nextLevel = CurrentLevel + 1;
        var sceneIndex = GetMapBuildIndex(nextChapter, nextLevel);
        if (sceneIndex < 0)
        {
            nextChapter = CurrentChapter + 1;
            nextLevel = 1;
        }

        return LoadMap(nextChapter, nextLevel);
    }

    private Tween TweenFaderColor(float alpha, float duration)
    {
        var fader = _vfxProfile.GetSetting<Fader>();
        var startColor = fader.Color.value;
        var endColor = startColor;
        endColor.a = alpha;

        var tweenValue = 0f;
        var tween = DOTween.To(
            () => tweenValue,
            v =>
            {
                tweenValue = v;
                fader.Color.Interp(startColor, endColor, tweenValue);
            },
            1f,
            duration);
        tween.SetTarget(_vfxProfile);
        return tween;
    }

    private void Awake()
    {
        LoadMap(1, 1);
    }

    private void Update()
    {
        Timer.Update(Time.deltaTime);
    }

    #endregion
}