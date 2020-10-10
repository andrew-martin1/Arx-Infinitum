using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour
{

    public enum AudioChannel { MASTER, SFX, MUSIC };

    //Accessible but not settable
    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    //Used for making 2D sounds that dont have a position, ie level complete
    AudioSource sfx2DSource;
    //Having the audio source object lets you dynamically adjust the settings while audio plays, eg. volume
    //Using two audio sources, we can cross fade music when changing.
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance; //Make this a singleton, allowing it to be accessed statically.

    //We set our audio listener to follow around the player. This way the audio listener wont get deleted (this happens when its attatched to the player directly)
    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    private void Awake()
    {

        if (instance != null)
        {
            Destroy(gameObject); //If the singleton already exists, do not instantiate this one
        }
        else
        {
            instance = this; //Initialize singleton variable
            DontDestroyOnLoad(gameObject);


            library = GetComponent<SoundLibrary>();

            //Create two audio sources
            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform; //Put the new music sources as the child object of this for organization
            }

            GameObject newSfx2DSource = new GameObject("2D SFX Source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            sfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }

            //Load the players settings
            //If not existant, use default value of 1
            masterVolumePercent = PlayerPrefs.GetFloat("master vol", .2f);
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat("musiv vol", 1);
            PlayerPrefs.Save();
        }
    }

    private void Update()
    {
        if (playerT != null) //while the player is alive
        {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.MASTER:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.SFX:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.MUSIC:
                musicVolumePercent = volumePercent;
                break;
        }

        //Update the music sources with the new volume
        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        //Save the new volume settings as player settings
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("musiv vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //0,1,0,1...
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();
        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration; // 1/duration = speed of crossfade
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }

    //Plays a sound at a position. For sound effects
    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent); //Cant change volume durring sound, not ideal for music
        }
        else
        {
            print("Audio is null!");
        }
    }

    //Uses the sound library we made, which is a map of sound names to sounds
    public void PlaySound(string soundName, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    //Plays sounds in 2D rather than 3D space. Doesnt need a position.
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }
}

