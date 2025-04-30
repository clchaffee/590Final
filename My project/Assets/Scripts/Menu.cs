using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static Menu Instance;
    
    public string selectedScene = "WindingTrack";
    
    RaceManager raceManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        raceManager = RaceManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextSceneOthers()
    {
        raceManager.isRacingSelf = false;
        SceneManager.LoadScene(selectedScene);
    }
    public void NextSceneSelf()
    {
        raceManager.isRacingSelf = true; 
        SceneManager.LoadScene(selectedScene);
    }
    public void SetSelectedScene(string sceneName)
    {
        selectedScene = sceneName;
    }
}
