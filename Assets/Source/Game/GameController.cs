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

    private int _currentLevel;

    #endregion

    #region Properties

    public Throne[] Thrones { get; private set; } = new Throne[0];

    public PromiseTimer Timer { get; } = new PromiseTimer();

    public Player Player => _player;

    #endregion

    #region Methods

    public IPromise LoadMap(int level)
    {
        var returnPromise = new Promise();
        TweenFaderColor(1f, 0.5f)
            .ToPromise()
            .Then(tween => Timer.WaitFor(0.5f))
            .Then(
                () =>
                {
                    SceneManager.MoveGameObjectToScene(Player.gameObject, gameObject.scene);
                    AsyncOperation unloadOp = null;
                    if (_currentLevel >= 1)
                    {
                        unloadOp = SceneManager.UnloadSceneAsync(_currentLevel);
                    }

                    var loadOp = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
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
                    _currentLevel = level;
                    var scene = SceneManager.GetSceneByBuildIndex(level);
                    PostSceneLoad(scene);
                    TweenFaderColor(0f, 1f);
                });
        return returnPromise;
    }

    public IPromise LoadNextMap()
    {
        return LoadMap(_currentLevel + 1);
    }

    private void GetCurrentChapterLevel(out int chapter, out int level)
    {
        chapter = -1;
        level = -1;

        var scene = SceneManager.GetSceneByBuildIndex(_currentLevel);
        if (scene.name.Contains("-"))
        {
            var split = scene.name.Split('-');
            chapter = int.Parse(split[0]);
            level = int.Parse(split[1]);
        }
    }

    private void PostSceneLoad(Scene scene)
    {
        if (scene.buildIndex > 0)
        {
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(Player.gameObject, scene);
            Thrones = FindObjectsOfType<Throne>();
            Player.Respawn();
            GetCurrentChapterLevel(out var chapter, out var level);
            if(chapter > 1 || level > 1)
            {
                Player.TokenInteraction.SetUnlockedToken();
            }
        }
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
        LoadNextMap().Done();
        Shader.SetGlobalInt("_PlayMode", 1);
    }

    private void OnApplicationQuit()
    {
        Shader.SetGlobalInt("_PlayMode", 0);
    }

    private void Update()
    {
        Timer.Update(Time.deltaTime);
    }

    #endregion
}