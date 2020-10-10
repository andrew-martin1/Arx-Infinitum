using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/**
* This editor script MUST be within an Editor folder to work!
*
* Changes the functionality of the editor to generate new maps when its being inspected.
*
* By extending Editor, we are given access to the functions that run the editor itself.
* We can then change the functionality of the editors Inspector tab by overriding the
* OnInspectorGUI() function, where we tell it to generate new maps if the item being inspected
* is typeof(MapGenerator).
*/
[CustomEditor(typeof(MapGenerator))] //Tells the editor what type of object it should be editing to activate
public class MapEditor : Editor
{
    /**
    * For customizing the inspector functionality
    */
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI(); //Do all the default stuff....?

        //Target is the object that the inspector is looking at
        MapGenerator map = target as MapGenerator;

        //If a value is changed in the inspector
        if (DrawDefaultInspector())
        {
            //Generates new maps
            map.GenerateMap();
        }

        //Creates a button, and if its clicked run the function
        if (GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
