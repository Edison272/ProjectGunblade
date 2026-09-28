using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    // Start is called before the first frame update
    void Start()
    {
        int curr_high_score = PlayerPrefs.GetInt("Highscore", 0);
        ScoreText.text = "Most Zones Captured: " + curr_high_score;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Mission");
        //SceneManager.LoadScene("CoreLevel", LoadSceneMode.Additive);


        // SceneController.SCENE_CONTROLLER
        //     .NewTransition()
        //     .Load(SceneDatabase.Slots.Level, SceneDatabase.Scenes.Level)
        //     .Unload(SceneDatabase.Slots.Menu)
        //     .WithOverlay()
        //     .WithClearUnusedAssets()
        //     .Perform();
    }

    public void Settings()
    {
        
    }
}
