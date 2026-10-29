using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ClimbDemo.Editor
{
    /// <summary>Editor helpers for placing climb volumes in the standalone demo.</summary>
    public static class ClimbVolumeTools
    {
        [MenuItem("Tools/Climb Demo/Add Climb Volume %#k")]
        public static void AddClimbVolume()
        {
            Vector3 position = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero;

            GameObject go = new GameObject("ClimbVolume");
            Undo.RegisterCreatedObjectUndo(go, "Add Climb Volume");
            go.transform.position = position;

            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2f, 2f);
            box.isTrigger = true;

            ClimbVolume climb = go.AddComponent<ClimbVolume>();
            climb.useColliderTop = true;
            climb.blocksWalking = true;

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
            SceneView.RepaintAll();
        }

        [MenuItem("Tools/Climb Demo/Create Volume From Selected Sprite")]
        public static void AddFromSprite()
        {
            GameObject selected = Selection.activeGameObject;
            SpriteRenderer sr = selected != null ? selected.GetComponent<SpriteRenderer>() : null;
            if (sr == null)
            {
                EditorUtility.DisplayDialog("Climb Volume", "Select a GameObject with a SpriteRenderer.", "OK");
                return;
            }

            Bounds bounds = sr.bounds;
            GameObject go = new GameObject("ClimbVolume_" + selected.name);
            Undo.RegisterCreatedObjectUndo(go, "Add Climb Volume From Sprite");
            go.transform.position = bounds.center;

            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = bounds.size;
            box.isTrigger = true;

            ClimbVolume climb = go.AddComponent<ClimbVolume>();
            climb.useColliderTop = true;
            climb.blocksWalking = false;
            climb.landingInset = Mathf.Min(0.28f, bounds.size.x * 0.2f);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
            SceneView.RepaintAll();
        }
    }
}
