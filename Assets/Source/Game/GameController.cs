using System.IO;
using DG.Tweening;
using RSG;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    #region Fields

    [SerializeField]
    private Camera _uiCamera;

    [SerializeField]
    private Color[] _chapterColors;

    [SerializeField]
    private Player _player;

    [SerializeField]
    private PostProcessProfile _vfxProfile;

    [SerializeField]
    private AudioSource _timer;

    [SerializeField]
    private AudioSource _timerStop;

    [SerializeField]
    private AudioSource _timerStart;

    [SerializeField]
    private AudioSource _levelComplete;

    private int _currentLevel;

    #endregion

    #region Properties

    public Throne[] Thrones { get; private set; } = new Throne[0];

    public PromiseTimer Timer { get; } = new PromiseTimer();

    public bool PlayerBeatLevel { get; set; }

    public Player Player => _player;

    public Camera UICamera => _uiCamera;

    #endregion

    #region Methods

    private int GetChapter(int buildIndex)
    {
        if (buildIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            return _chapterColors.Length - 1;
        }
        var path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        path = Path.GetFileNameWithoutExtension(path);
        return int.Parse(path[0].ToString());
    }

    public IPromise LoadMap(int level)
    {
        Player.gameObject.SetActive(false);
        Player.TokenInteraction.RetrieveToken(true);
        Player.Movement.PlayerState = PlayerMovement.State.Idle;
        SetTimerPlay(false);
        var returnPromise = new Promise();

        if (_currentLevel > 0)
        {
            _levelComplete.Play();
        }

        Thrones = new Throne[0];
        TweenFaderColor(level, 1f, 0.5f)
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
                    PlayerBeatLevel = false;
                    _currentLevel = level;
                    var scene = SceneManager.GetSceneByBuildIndex(level);
                    PostSceneLoad(scene);
                    TweenFaderColor(level, 0f, 1f);
                });
        return returnPromise;
    }

    public IPromise LoadNextMap()
    {
        return LoadMap(_currentLevel + 1);
    }

    public void SetTimerPlay(bool isPlaying)
    {
        if (isPlaying)
        {
            _timerStart.Play();
            _timer.Play();
        }
        else if (_timer.isPlaying)
        {
            _timer.Stop();
            _timerStop.Play();
        }
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
            if (chapter > 1 || level > 1)
            {
                Player.TokenInteraction.SetUnlockedToken();
            }
        }
    }

    private Tween TweenFaderColor(int level, float alpha, float duration)
    {
        var fader = _vfxProfile.GetSetting<Fader>();
        var startColor = _chapterColors[GetChapter(level)];
        startColor.a = 1f - alpha;
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