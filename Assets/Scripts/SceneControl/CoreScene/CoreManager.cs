using System.Collections;
using UnityEngine;
public class CoreManager : MonoBehaviour
{
    void Start()
    {
        SceneController.SCENE_CONTROLLER
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}