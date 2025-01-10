using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityToolbarExtender;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SceneLoaderToolbar
{
    private static bool loadAdditive = false;

    private static IEnumerable<Scene> openScenes {
        get {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                yield return SceneManager.GetSceneAt(i);
        }
    }

    // Add a menu button in the toolbar
    [InitializeOnLoadMethod]
    private static void AddToolbarButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        ProcessInputs();
        RenderGUI();
    }

    private static void ProcessInputs()
    {
        if (Event.current.type == EventType.KeyDown)
            loadAdditive = Event.current.keyCode == KeyCode.LeftShift || Event.current.keyCode == KeyCode.RightShift;
        else if (Event.current.type == EventType.KeyUp)
            loadAdditive = false;
    }

    private static void RenderGUI()
    {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(EditorGUIUtility.IconContent("SceneAsset Icon").image, EditorStyles.toolbarDropDown, GUILayout.Width(40)))
        {
            Debug.Log("loadAdditive = " + loadAdditive);
            DisplaySceneSelector();
        }
    }

    private static void DisplaySceneSelector()
    {
        // Get scenes from Build Settings
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToList();

        if (scenes.Count == 0)
        {
            EditorUtility.DisplayDialog("No Scenes", "No scenes are available in Build Settings.", "OK");
            return;
        }

        string[] loadedScenes = openScenes.Select(scene => scene.name)
            .ToArray();
        // Create a GenericMenu to display the scenes
        GenericMenu menu = new GenericMenu();
        foreach (var scenePath in scenes)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            bool isActiveScene = loadedScenes.Contains(sceneName);
            menu.AddItem(new GUIContent(sceneName), isActiveScene, () => OnSceneSelected(scenePath));
        }

        menu.ShowAsContext();
    }

    private static void OnSceneSelected(string scenePath)
    {
        // Check if the current scene is dirty and prompt to save
        if (openScenes.Any(scene => scene.isDirty))
        {
            bool saveCurrentScene = EditorUtility.DisplayDialog(
                "Unsaved Changes",
                "The current scene has unsaved changes. Do you want to save before loading a new scene?",
                "Save",
                "Don't Save"
            );

            if (saveCurrentScene)
            {
                if (!EditorSceneManager.SaveOpenScenes())
                {
                    Debug.LogError("Failed to save the current scene.");
                    return;
                }
            }
        }

        OpenSceneMode mode = loadAdditive ? OpenSceneMode.Additive : OpenSceneMode.Single;
        // Load the selected scene
        EditorSceneManager.OpenScene(scenePath, mode);
    }
}