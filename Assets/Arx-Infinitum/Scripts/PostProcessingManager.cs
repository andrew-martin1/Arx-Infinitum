using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


[RequireComponent(typeof(PostProcessVolume))]
public class PostProcessingManager : MonoBehaviour
{
    public float minVignetteIntensity = .3f;
    public float maxVignetteIntensity = .5f;

    public static PostProcessingManager instance;
    PostProcessVolume volume;
    Vignette vignette;
    Grain grain;
    Spawner spawner;
    float hitsToKillPlayer;
    float playerStartingHealth;
    float enemyDamage;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); //If the singleton already exists, do not instantiate this one
        }
        else
        {
            instance = this; //Initialize singleton variable
                             //DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        volume = GetComponent<PostProcessVolume>();
        spawner = FindObjectOfType<Spawner>();
        playerStartingHealth = FindObjectOfType<Player>().startingHealth;
        spawner.OnNewWave += OnNewWave;
    }

    void OnNewWave(int wave)
    {
        //Reset the vignette since the players health is restored to 100
        SetVignetteIntensity(minVignetteIntensity);

        hitsToKillPlayer = spawner.waves[wave - 1].hitsToKillPlayer;
        enemyDamage = Mathf.Ceil(playerStartingHealth / hitsToKillPlayer);

        if (hitsToKillPlayer <= 1)
        {
            enableGrain(); //If the enemies 1 hit kill the player, enable grain
        }
    }

    public void PlayerHealthChanged(float health, float startingHealth)
    {
        //Set the vignette to be more intense
        float maxIntensityChange = maxVignetteIntensity - minVignetteIntensity;
        SetVignetteIntensity(((1 - (health / startingHealth)) * maxIntensityChange) + minVignetteIntensity);

        //If the player is one hit from death, enable grain
        if (health - enemyDamage == 0)
        {
            enableGrain();
        }
    }

    void SetVignetteIntensity(float intensity)
    {
        StopCoroutine("animateVignetteChange");
        StartCoroutine("animateVignetteChange", intensity);
    }

    void enableGrain()
    {
        if (volume.profile.TryGetSettings<Grain>(out grain))
        {
            grain.enabled.value = true;
        }
        else
        {
            print("Grain does not exist on volume!");
        }
    }

    IEnumerator animateVignetteChange(float newIntensity)
    {
        if (volume.profile.TryGetSettings<Vignette>(out vignette))
        {
            float animationSeconds = 1f;
            float oldIntensity = vignette.intensity.value;
            float percent = 0;
            while (percent <= 1)
            {
                percent += Time.deltaTime / animationSeconds;
                vignette.intensity.value = Mathf.Lerp(oldIntensity, newIntensity, percent);
                yield return null;
            }
        }
        else
        {
            print("Vignette does not exist on volume!");
        }
    }

}
