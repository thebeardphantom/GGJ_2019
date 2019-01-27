using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorHelper
{
    static EditorHelper()
    {
        Shader.SetGlobalInt("_PlayMode", 0);
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }

    #region Methods

    [MenuItem("GameObject/Align Camera")]
    private static void AlignCamera()
    {
        var main = Camera.main;
        GetSceneView();
        var sceneView = GetSceneView();
        sceneView.AlignViewToObject(main.transform);
        sceneView.orthographic = true;
    }

    private static SceneView GetSceneView()
    {
        if (SceneView.currentDrawingSceneView != null)
        {
            return SceneView.currentDrawingSceneView;
        }

        if (SceneView.lastActiveSceneView != null)
        {
            return SceneView.lastActiveSceneView;
        }

        if (SceneView.sceneViews != null && SceneView.sceneViews.Count > 0)
        {
            return (SceneView)SceneView.sceneViews[0];
        }

        return null;
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
            {
                Shader.SetGlobalInt("_PlayMode", 0);
                break;
            }
            case PlayModeStateChange.ExitingEditMode:
            {
                Shader.SetGlobalInt("_PlayMode", 1);
                break;
            }
            case PlayModeStateChange.EnteredPlayMode:
            {
                Shader.SetGlobalInt("_PlayMode", 1);
                break;
            }
            case PlayModeStateChange.ExitingPlayMode:
            {
                Shader.SetGlobalInt("_PlayMode", 0);
                break;
            }
        }
    }

    #endregion
}