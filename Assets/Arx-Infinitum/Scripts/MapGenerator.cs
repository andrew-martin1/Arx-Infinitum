using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform mapFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords; //We use a queue so that every time we get a random coordinate, we move it to the back of the queue.
    Queue<Coord> shuffledOpenTileCoords;

    Transform[,] tileMap;

    public Map[] maps;
    public int mapIndex;
    Map currentMap;

    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1; //array starts at 0
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        //GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.5f, currentMap.mapSize.y * tileSize); //Set box collider for floor (changed to mapFloor)

        //Generating Coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //Create map holder object
        string holderName = "Generated Map"; //The name of the empty gameObject to store tiles under
        if (transform.Find(holderName)) //Find the holder game object in the children of this object
        {
            DestroyImmediate(transform.Find(holderName).gameObject); //DestroyImmediate is used because we're calling it from the editor
        }

        Transform mapHolder = new GameObject(holderName).transform; //Create a new map holder after destroying the last one
        mapHolder.parent = transform; //Set the parent of the object to this objects transform

        //Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                //To calculate the leftmost edge, we do -currentMap.mapSize.x / 2
                //This puts the tile at the center of that position. We actually want the edge to be at the leftmost edge,
                //so we shift by 0.5
                Vector3 tilePosition = CoordToPosition(x, y);
                //For the euler angle, Vector3.right is the X axis
                //Multiply by 90 to set the rotation on X to 90
                //This way the tile faces up
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                //Set all scale dimensions to the percent of outline.
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder; //Add to holder
                tileMap[x, y] = newTile;
            }
        }

        //Spawning obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; //keeps track of what tiles are occupied by obstacles 

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords); //By default, this will be all tiles and we will remove each tile when we generate the obstacle

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();

            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            //Makes sure the map is fully accessible. We cant spawn a tile in the center, because that is the origin
            //from which we determine if things are accessible.
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                //Instantiate the obstacles
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial); //Create a new material from the obstacles current material
                float colourPercent = (float)randomCoord.y / currentMap.mapSize.y; //Gives us a gradient
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord); // Remove coord from open tiles
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        //Make a queue of shuffled open tiles
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //Create the navmesh mask
        //See notes for explanation of the formula for the position
        //By using Vector3.right or Vector3.left, we determine what direction to go in when we multiply it by the formula we made
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize; //Stretch it out to fill the area between the map edge and max map edge

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        //In this case, because the floor is rotated by 90 degrees, what we see as the Z axis is actually the objects Y axis
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
    }

    /**
    * Determines if the obstacles are blocking any paths in the map
    *
    * Uses a floodfill algorithm to count the number of empty tiles from the center of the map.
    * If the number of tiles the floodfill finds does not equal the actual number of tiles without
    * obstacles, then we return false.
    */
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        //Keeps track of what tiles we have already checked
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            //Go through all of the adjacent tiles
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    //Make sure we dont check the diagonals
                    if (x == 0 || y == 0)
                    {
                        //Make sure the coordinate is within our map
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            //Make sure we havent checked the tile and that its not an obstacle tile
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY)); //Look at this tile's neighbours
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        //How many tiles should there be?
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    //Converts a coordinate on the tile map to a position in 3d space
    Vector3 CoordToPosition(int x, int y)
    {
        //To calculate the leftmost edge, we do -currentMap.mapSize.x / 2
        //This puts the tile at the center of that position. We actually want the edge to be at the leftmost edge,
        //so we shift by 0.5
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    //Opposite of CoordToPosition() formula (inverse)
    public Transform GetTileFromPosition(Vector3 position)
    {
        //Inverse of CoordToPosition()
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f); //WARNING: In 3D for top down, Z is what we would consider to be Y
                                                                                           //Lock X and Y to the bounds of the tileMap coordinates
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    //Gets a random coordinate by returning the next item in the random coord queue
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue(); //Pop the random coord
        shuffledTileCoords.Enqueue(randomCoord); //Requeue  that coord to the end of the queue
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue(); //Pop the random coord
        shuffledTileCoords.Enqueue(randomCoord); //Requeue  that coord to the end of the queue
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public override bool Equals(object c2)
        {
            if (!(c2 is Coord cord))
                return false;

            return this.x == ((Coord)c2).x && this.y == ((Coord)c2).y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() * 17 + y.GetHashCode();
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {

        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

    }
}

