using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{

    /// These are references to GameObjects in the Unity scene.
    /// I use them so that this script can interact with components on these game objects.
    /// When I declare a gameobject in this way, I have to open the GameObject that this script is attached to in Unity.
    /// Then, from the Hierarchy window I look for the GameObjects I want these three references below to target, and drag them into their respective areas in the Inspector window.
    /// Notice that I do not create one for the cursor GameObject - that is because this script is already attached to the cursor.
    /// I can directly refer to components from this GameObject, without needing to say what I am trying to reference.
    /// For example, in the Start() function, I type "spriteRenderer = GetComponent<SpriteRenderer>();".
    /// You can imagine there is an implicit "this" in front of GetComponent, like: "this.GetComponent<SpriteRenderer>();".
    /// However, if I want to get a component from the player GameObject, I have to type "player.GetComponent<NameOfComponent>();"
    public GameObject tileMapObj;
    public GameObject player;

    /// These are the three colors the square cursor can be. I attach an image to them in the same way that I would for the GameObjects above,
    /// except I drag from the Project window to the Inspector window.
    /// Currently, the cursor color stuff is not relevant because the "dragging to select" feature is not created yet.
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;

    /// These are some references to components from the objects in the scene.
    Tilemap tileMap;
    Player playerData;
    private SpriteRenderer spriteRenderer;
    //public int[,] world; /* Commenting this and all related lines out because we aren't using it at the moment but might in the future */

    /// These are variables that I will be using in this script.
    /// If I want scripts on other GameObjects to use data from this script, it will likely be using one of these variables.
    /// As of now, the only one used in a different script is pendingMoves, which I use to display the number on the GUI.
    Vector3 mousePosition;
    Vector3Int playerCell;
    float zAxis = 10;
    public float startTime;
    public bool nextTile = true;
    public bool started = false;
    private int index = 0;
    public float totalDistance;
    public bool confirm = false;
    public int pendingMoves = 0;

    /// This is a list of the cells of the currently selected tiles. They are (x, y, z) coordinates, but since this is a 2D map they all have the same Z value.
    List<Vector3Int> path;
    private Vector3Int[] possibleTiles = new Vector3Int[4];

    void Start()
    {
        /// Initializing the tileMap in this script by getting the tileMap component from the Tilemap object that "tileMapObj" is referencing.
        /// Initializing the spriteRenderer in this script by getting the SpriteRenderer component from this object.
        /// Initializing the playerData variable in this script by getting the Player component from the player object.
        /// In the future, we will have to be more particular about the names as there will be more types of players/characters.
        tileMap = tileMapObj.GetComponent<Tilemap>();
        //world = tileMapObj.GetComponent<TileMap>().world;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = yellow_cursor;
        playerData = player.GetComponent<Player>();

        /// Initializing the list by creating a new one.
        path = new List<Vector3Int>();

        playerCell = tileMap.WorldToCell(player.transform.position);
        tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileMap.SetColor(playerCell, Color.magenta);

        // Setting possible-to-select tiles as those surrounding the player, if the player has moves
        if (playerData.moves > 0)
        {
            HighlightNeighbors(tileMap.WorldToCell(player.transform.position));
        }
    }

    void Update()
    {
        /// Making the cursor follow the mouse, while snapping to the grid. The grid is offset by 0.5, because the Unity scene editor has its pivot point at the center,
        /// while the tiles we are using have their pivot points in one of the corners. The tiles are 1x1, so the distance from the center to the sides is 0.5.
        /// that is why I had to make the cursor snap by a 0.5 unit offset in the x and y direction via the RoundOffset() function.
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = zAxis;
        transform.position = new Vector3((RoundOffset(mousePosition.x)), (RoundOffset(mousePosition.y)), zAxis);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            /// This is just converting the Vector3 position of the cursor and player in the world to the cell position of
            /// the tile they are currently on when the player clicks their left mouse button.
            Vector3Int goal = tileMap.WorldToCell(transform.position);
            Vector3Int start;
            if (path.Count > 0)
            {
                start = path[path.Count - 1];
            }
            else
            {
                start = tileMap.WorldToCell(player.transform.position);
            }
            /// If this statement is true, it adds the tile to the path. See CheckTile() for more info.
            if (playerData.moves > 0 && CheckTile(start, goal) && IsNeighbor(start, goal))
            {
                UnhighlightOldNeighbors();
                /// Changes the tileflags to none so that we are able to change anything about the tile.
                /// Then, changiing the color to red, adds it to the path list, and adjusts the number of remaining moves appropriately.
                tileMap.SetTileFlags(goal, TileFlags.None);
                tileMap.SetColor(goal, Color.red);
                //spriteRenderer.sprite = green_cursor;
                path.Add(goal);
                --playerData.moves;
                ++pendingMoves;
                if (playerData.moves > 0)
                {
                    HighlightNeighbors(path[path.Count - 1]);
                }
            }
            if (start == goal)
            {
                spriteRenderer.sprite = green_cursor;
            }
            else
            {
                spriteRenderer.sprite = red_cursor;
            }
        }
        else
        {
            spriteRenderer.sprite = yellow_cursor;
        }

        if (Input.GetMouseButtonDown(1))
        {
            /// Does the opposite of above. It checks if the cell the cursor is on is the last tile you selected. If so, it removes it from the list.
            /// In the future, I would want the "last selected tile" to be a different color so that people aren't forced to memorize what the last tile they selected is.
            /// Then, if they remove that tile, the one before that will change color as it is the "new" last tile selected.
            Vector3Int coordinate = tileMap.WorldToCell(transform.position);
            if (path.Count > 0 && coordinate == path[path.Count - 1])
            {
                tileMap.SetColor(coordinate, Color.white);
                path.RemoveAt(path.Count - 1);
                playerData.moves++;
                pendingMoves--;
            }
        }

        /// "confirm" is set true by the GUI script when the button is pressed.
        /// "started" is true if the player is currently moving, false otherwise.
        /// All this does is change the red tiles to green tiles, and then stops trying to change them to green afterward.
        if (confirm && !started)
        {
            pendingMoves = 0;
            for (int i = 0; i < path.Count; i++)
            {
                tileMap.SetColor(path[i], Color.green);
            }
            started = true;
        }

        /// This begins player movement by moving the player one tile at a time.
        if (confirm && index < path.Count)
        {
            Vector3 destination = new Vector3(RoundOffset(path[index].x), RoundOffset(path[index].y), zAxis);
            if (nextTile)
            {
                startTime = Time.time;
                nextTile = false;
                totalDistance = Vector3.Distance(player.transform.position, destination);
            }
            float distanceCovered = Time.time - startTime;
            float fractionOfJourney = distanceCovered / totalDistance;
            player.transform.position = Vector3.Lerp(player.transform.position, destination, fractionOfJourney);
            if (player.transform.position == destination && index < path.Count)
            {
                nextTile = true;
                tileMap.SetColor(path[index], Color.white);
                ++index;
            }
            tileMap.SetTileFlags(playerCell, TileFlags.None);
            tileMap.SetColor(playerCell, Color.white);
        }

        /// This removes every tile from the path after the player has reached their destination, so that path may be used again.
        if (path.Count > 0 && index == path.Count)
        {
            playerCell = tileMap.WorldToCell(player.transform.position);
            tileMap.SetTileFlags(playerCell, TileFlags.None);
            tileMap.SetColor(playerCell, Color.magenta);
            /*if (index == path.Count)
            {
                playerData.moves = 0;
            }*/
            do
            {
                --index;
                path.RemoveAt(index);
            }
            while (path.Count > 0);
            confirm = false;
            started = false;
        }
    }

    /// Takes a float and rounds it to the nearest 0.5 decimal place.
    /// examples:
    /// 1.4 rounds to 1.5
    /// 0.9 rounds to 0.5
    /// 2.7 rounds to 2.5
    /// 9.9 rounds to 9.5
    /// 10.1 rounds to 10.5
    public float RoundOffset(float a)
    {
        int b = Mathf.RoundToInt(a);
        if (b > a)
        {
            return b - 0.5f;
        }
        else
        {
            return b + 0.5f;
        }

    }

    /// Checks if a tile is valid.
    /// To be valid... 
    /// * it must not be the same as the tile the player is currently on
    /// * it must be a tile (not null)
    /// * it must have "floor" in its name.
    /// * it must not already be in the path.
    /// * it must be a neighbor of the previous tile in the path.
    public bool CheckTile(Vector3Int start, Vector3Int goal)
    {
        TileBase tile = tileMap.GetTile(goal);
        if(goal != tileMap.WorldToCell(player.transform.position) && !path.Contains(goal))
        {
            if (tile != null && tile.name.Contains("floor"))
            {
                return true;
            }
        }
        return false;
    }

    /// Checks if one cell is above/below/left/right of another cell.
    public bool IsNeighbor(Vector3Int start, Vector3Int goal)
    {
        if (goal.x == start.x - 1 && goal.y == start.y ||
            goal.x == start.x + 1 && goal.y == start.y ||
            goal.y == start.y - 1 && goal.x == start.x ||
            goal.y == start.y + 1 && goal.x == start.x)
        {
            return true;
        }
        return false;
    }

    public void HighlightNeighbors(Vector3Int start)
    {
        Vector3Int left = new Vector3Int(start.x - 1, start.y, 0);
        Vector3Int right = new Vector3Int(start.x + 1, start.y, 0);
        Vector3Int above = new Vector3Int(start.x, start.y + 1, 0);
        Vector3Int below = new Vector3Int(start.x, start.y - 1, 0);
        if (CheckTile(start, left))
        {
            possibleTiles[0] = left;
            tileMap.SetTileFlags(left, TileFlags.None);
            tileMap.SetColor(left, Color.yellow);
        }
        if (CheckTile(start, right))
        {
            possibleTiles[1] = right;
            tileMap.SetTileFlags(right, TileFlags.None);
            tileMap.SetColor(right, Color.yellow);
        }
        if (CheckTile(start, above))
        {
            possibleTiles[2] = above;
            tileMap.SetTileFlags(above, TileFlags.None);
            tileMap.SetColor(above, Color.yellow);
        }
        if (CheckTile(start, below))
        {
            possibleTiles[3] = below;
            tileMap.SetTileFlags(below, TileFlags.None);
            tileMap.SetColor(below, Color.yellow);
        }
    }

    public void UnhighlightOldNeighbors()
    {
        Vector3Int undefinedV = new Vector3Int(-1, -1, -1);
        for(int i = 0; i < possibleTiles.Length; i++)
        {
            if (possibleTiles[i] != undefinedV)
            {
                tileMap.SetColor(possibleTiles[i], Color.white);
                possibleTiles[i] = undefinedV;
            }
        }
    }

    /// EVERYTHING BELOW IS UNUSED CURRENTLY. UNCOMMENTING WILL CAUSE ERRORS
    /*public bool InRange(Vector3Int start, Vector3Int goal)
    {
        Vector3Int leftTile = new Vector3Int(start.x - 1, start.y, start.z);
        Vector3Int rightTile = new Vector3Int(start.x + 1, start.y, start.z);
        Vector3Int upTile = new Vector3Int(start.x, start.y + 1, start.z);
        Vector3Int downTile = new Vector3Int(start.x, start.y - 1, start.z);

        if (CheckTile(tileMap.GetTile(leftTile)) && !viewedTiles.Contains(leftTile) && (compareTiles(leftTile, goal)))
        {

        }

    public bool InRange()
    {
        if (cursor.transform.position == player.transform.position)
        {
            return false;
        }

        Vector3Int playerCell = tileMap.WorldToCell(player.transform.position);
        Vector3Int goalCell = tileMap.WorldToCell(cursor.transform.position);

        Queue<Vector3Int> previousTileList = new Queue<Vector3Int>();
        HashSet<Vector3Int> viewedTiles = new HashSet<Vector3Int>();
        return search(previousTileList, viewedTiles, playerCell, goalCell, playerData.moves);
    }

    public bool search(Queue<Vector3Int> previousTileList, HashSet<Vector3Int> viewedTiles, Vector3Int start, Vector3Int goal, int moves)
    {
        moves--;
        Debug.Log("Current: " + start);
        previousTileList.Enqueue(start);
        viewedTiles.Add(start);
        Vector3Int leftTile = new Vector3Int(start.x - 1, start.y, start.z);
        Vector3Int rightTile = new Vector3Int(start.x + 1, start.y, start.z);
        Vector3Int upTile = new Vector3Int(start.x, start.y + 1, start.z);
        Vector3Int downTile = new Vector3Int(start.x, start.y - 1, start.z);
        if (CheckTile(tileMap.GetTile(leftTile)) && !viewedTiles.Contains(leftTile) && (compareTiles(leftTile, goal)))
        {
            previousTileList.Enqueue(leftTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(rightTile)) && !viewedTiles.Contains(rightTile) && (compareTiles(rightTile, goal)))
        {
            previousTileList.Enqueue(rightTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(upTile)) && !viewedTiles.Contains(upTile) && (compareTiles(upTile, goal)))
        {
            previousTileList.Enqueue(upTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(downTile)) && !viewedTiles.Contains(downTile) && (compareTiles(downTile, goal)))
        {
            previousTileList.Enqueue(downTile);
            return true;
        }

        if (((CheckTile(tileMap.GetTile(leftTile)) && moves > 0) && !viewedTiles.Contains(leftTile) && search(previousTileList, viewedTiles, leftTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(rightTile)) && moves > 0) && !viewedTiles.Contains(rightTile) && search(previousTileList, viewedTiles, rightTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(upTile)) && moves > 0) && !viewedTiles.Contains(upTile) && search(previousTileList, viewedTiles, upTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(downTile)) && moves > 0) && !viewedTiles.Contains(downTile) && search(previousTileList, viewedTiles, downTile, goal, moves)))
        {
            return true;
        }
        //Debug.Log("start after: " + start);
        //Debug.Log("DEQUEUING: " + previousTileList.Dequeue());
        return false;
    }

    public bool compareTiles(Vector3Int start, Vector3Int goal)
    {
        if (start.x >= 0 && start.x < world.GetLength(0) && start.y >= 0 && start.y < world.GetLength(1) && start.x == goal.x && start.y == goal.y)
        {
            return true;
        }
        return false;
    }*/
}