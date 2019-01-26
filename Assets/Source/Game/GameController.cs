using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    #region Fields

    [SerializeField]
    private Player _player;

    #endregion

    #region Properties

    public int CurrentChapter { get; private set; } = -1;

    public int CurrentLevel { get; private set; } = -1;

    public Player Player => _player;

    #endregion

    #region Methods

    private static int GetMapBuildIndex(int chapter, int level)
    {
        return SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/Maps/{chapter}-{level}.unity");
    }

    public AsyncOperation LoadMap(int chapter, int level)
    {
        AsyncOperation unloadOp = null;
        if (CurrentChapter >= 0 && CurrentLevel >= 0)
        {
            var unloadIndex = GetMapBuildIndex(CurrentChapter, CurrentLevel);
            unloadOp = SceneManager.UnloadSceneAsync(unloadIndex);
        }

        var loadIndex = GetMapBuildIndex(chapter, level);
        var loadOp = SceneManager.LoadSceneAsync(loadIndex, LoadSceneMode.Additive);
        if (unloadOp != null)
        {
            loadOp.allowSceneActivation = false;
            unloadOp.completed += op =>
            {
                loadOp.allowSceneActivation = true;
            };
        }

        loadOp.completed += op =>
        {
            var scene = SceneManager.GetSceneByBuildIndex(loadIndex);
            SceneManager.SetActiveScene(scene);
            Player.Respawn();
        };

        CurrentChapter = chapter;
        CurrentLevel = level;
        return loadOp;
    }

    public AsyncOperation LoadNextMap()
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

    private void Awake()
    {
        LoadMap(1, 1);
    }

    #endregion
}