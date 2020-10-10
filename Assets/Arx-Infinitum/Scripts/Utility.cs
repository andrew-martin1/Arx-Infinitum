using System.Collections;
using System.Collections.Generic;


/**
* Generic methods and algorithms for anything in our project to use
*/
public static class Utility
{
    /**
    * Shuffle an array according to the Fisher-Yates shuffle method
    */
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        //prng = pseudoRandomNumberGenerator
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++) //Ends one before the last element since the last element is already shuffled
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }
}
