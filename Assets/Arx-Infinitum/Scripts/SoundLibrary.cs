using System.Collections;
using System.Collections.Generic; //allows us to use dictionaries
using UnityEngine;


public class SoundLibrary : MonoBehaviour
{

    public SoundGroup[] soundGroups; //sound groups to add to our dictionary from unity inspector
    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>(); //Maps each sound group ID to a audio clip array

    private void Awake()
    {
        //Fill in the dictionary
        foreach (SoundGroup soundGroup in soundGroups)
        {
            groupDictionary.Add(soundGroup.groupId, soundGroup.group);
        }
    }

    //If the name matches multiple sound effects, it will return a random one from that group
    //eg. Impact randomly returns Impact_01, Impact_02, or Impact_03
    public AudioClip GetClipFromName(string name)
    {
        if (groupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds = groupDictionary[name]; //Get the dictionary's sound array
            return sounds[Random.Range(0, sounds.Length)]; //Return a random sound
        }
        return null; //If we cant find a value in the dictionary
    }

    //Class for data to be filled in easily from the Unity inspector
    [System.Serializable]
    public class SoundGroup
    {
        public string groupId;
        public AudioClip[] group;
    }
}

