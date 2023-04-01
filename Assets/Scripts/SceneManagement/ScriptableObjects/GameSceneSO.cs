using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// This class is a base class which contains what is common to all game scenes (Locations, Menus, Managers)
/// </summary>
public class GameSceneSO : DescriptionBaseSO
{
    public GameSceneType sceneType;
    public AssetReference sceneReference; // used at runtime to lod the scene from the right AssetBundle

    public enum GameSceneType
    {
        // Playable Scenes
        Location, // SceneSelector tool will also load PersistentManagers and Gameplay
        Menu, // SceneSelector tool will also load Gameplay

        // Special Scenes
        Initialisation,
        PersistentManagers,
        Gameplay,

        // WIP scenes that don't need to be played
        Art
    }
}
