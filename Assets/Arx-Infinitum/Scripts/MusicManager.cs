using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(new Scene(), new LoadSceneMode());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.instance.PlayMusic(mainTheme, 2);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        //If the scene has changed, play the music
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            Invoke("PlayMusic", .2f);
            //We use invoke instead of calling directly since OnLevelWasLoaded will be called before the object is destroyed when changing scenes
            //By invoking with a small timer, we can guarantee that the object which is destroyed will not call play music and we wont get doubles
        }
    }
    void PlayMusic()
    {
        AudioClip clipToPlay = null;
        if (sceneName == "Menu")
        {
            clipToPlay = menuTheme;
        }
        else if (sceneName == "Game")
        {
            clipToPlay = mainTheme;
        }

        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length); //Loop it when its done
        }
    }
}
