using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ClimbDemo.Editor
{
    public static class ClimbDemoSceneBuilder
    {
        [MenuItem("Climb Demo/Create Demo Scene")]
        public static void CreateScene()
        {
            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            const string path = "Assets/Scenes/ClimbDemo.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            Debug.Log("CLIMB DEMO SCENE CREATED " + path);
        }
    }
}
