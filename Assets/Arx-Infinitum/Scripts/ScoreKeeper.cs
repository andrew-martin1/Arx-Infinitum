using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScoreKeeper : MonoBehaviour
{

    public static int score { get; private set; }
    public float streakExpiryTime = 1;

    float lastEnemyKilledTime;
    int streakCount;

    void Start()
    {
        //BE CAREFUL: This static subscription remains over scenes.
        //That means that when we restart the game, the start method
        //will resubscribe and then OnEnemyKilled will get activated
        //twice per kill
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        if (Time.time < lastEnemyKilledTime + streakExpiryTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKilledTime = Time.time;

        score += 5 + (int)Mathf.Pow(2, streakCount);
    }

    //Unsubscribes on player death so that the static event is not
    //subscribed to twice when the game restarts
    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
