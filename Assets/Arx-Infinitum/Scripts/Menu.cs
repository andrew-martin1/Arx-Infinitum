using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    int activeScreenResIndex; //Keeps track of the last screen res index when we exit fullscreen

    private void Start()
    {
        //Load player prefs
        activeScreenResIndex = PlayerPrefs.GetInt("Screen Res Index");
        bool isFullscreen = (PlayerPrefs.GetInt("Fullscreen") == 1);

        //Get audio managers player prefs
        //volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        //volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        //volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

        //this is the stupidest for loop i have ever seen in my life
        //it just sets resolutionToggles[activeScreenResIndex].isOn
        //with the added benefit of range checking i guess
        //im losing brain cells thinking about this one
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = i == activeScreenResIndex; //When I is the active screen resolution index, we turn that one on
        }

        fullscreenToggle.isOn = isFullscreen; //Calls the set fullscreen method for us
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }
    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }
    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn)
        {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("Screen Res Index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }
    //Audio manager saves the volumes for us, we dont need to store these to player prefs here
    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.MASTER);
    }
    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.MUSIC);
    }
    public void SetSFXVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.SFX);
    }
    public void SetFullscreen(bool isFullscreen)
    {
        //Force the resolution to be native resolution because MacOSX says so
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullscreen; //Prevent the resolution from being changed if in fullscreen
        }

        if (isFullscreen)
        {
            //Get all the possible resolutions, and set it to the biggest one (last one in array)
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
        {
            //Revert back to the last screen resolution when fullscreen is turned off
            SetScreenResolution(activeScreenResIndex);
        }
        PlayerPrefs.SetInt("Fullscreen", ((isFullscreen) ? 1 : 0));
        PlayerPrefs.Save();
    }
}

