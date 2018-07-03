/*
 * Handles loading and unloading scenes. All scenes loaded additive onto MAIN. MAIN never unloaded.
 * Will also handle loading screen.
 * 
 * Amanda D. Barbadora
 */
using System.Collections.Generic;
using UnityEngine;

public class Scene_Manager : MonoSingleton<Scene_Manager>
{
    [SerializeField]
    List<Scenes> sceneQueue;

    [SerializeField]
    List<Scenes> scenesUsed;

	// Use this for initialization
	void Start ()
    {
        Subscribe();
        LoadScene(Scenes.GAMEPLAY);
	}
	
	void Subscribe()
    {
        if( Event_Manager.instance != null )
        {
            Event_Manager.instance.AddListener<SceneEvents.LoadScene>(OnLoadScene);
            Event_Manager.instance.AddListener<SceneEvents.GoBack>(OnBackScene);
        }
    }

    void OnBackScene( SceneEvents.GoBack @event )
    {
        if( sceneQueue.Count >=2 )
        {
            if( sceneQueue[sceneQueue.Count -1] != Scenes.MAIN )
            {

                LoadScene(sceneQueue[sceneQueue.Count - 2]);
                UnloadScene();
            }
        }

    }

    void OnLoadScene( SceneEvents.LoadScene @event )
    {
        UnloadScene();
        LoadScene(@event._sceneName);
    }

    void UnloadScene( )
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scenesUsed.IndexOf(sceneQueue[sceneQueue.Count -1]));
    }

    void LoadScene( Scenes sceneName )
    {
        if( sceneQueue.Count > 0 )
        {
            if (sceneQueue[sceneQueue.Count - 1] != sceneName)
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenesUsed.IndexOf(sceneName), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                UpdateSceneQueue(sceneName);
            }
            else
            {
                Debug.LogWarning("SCENE ALREADY LOADED >|");
            }
        }
        else
        {
            // Default Main Hub
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenesUsed.IndexOf(Scenes.GAMEPLAY), UnityEngine.SceneManagement.LoadSceneMode.Additive);
            UpdateSceneQueue(sceneName);
        }
        
    }

    void UpdateSceneQueue( Scenes sceneName )
    {
        if( sceneQueue.Count >= 8 )
        {
            sceneQueue.RemoveAt(sceneQueue.Count - 1);
        }
        sceneQueue.Add(sceneName);
    }
}
