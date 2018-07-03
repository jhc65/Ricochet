/*
 * Contains events to load and unload specific scenes. All scenes are additive to MAIN.
 * 
 * Amanda D. Barbadora
 * 
 */

namespace SceneEvents
{
    /// <summary>
    /// Load this next scene, please.
    /// </summary>
    public struct LoadScene : iEvent
    {
        public readonly Scenes _sceneName;

        public LoadScene( Scenes sceneName )
        {
            _sceneName = sceneName;
        }
    }

    public struct GoBack : iEvent
    {

    }

}

