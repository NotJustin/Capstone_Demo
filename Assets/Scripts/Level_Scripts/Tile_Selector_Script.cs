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
    public new GameObject camera;
    public TileBase floor_tile_asset;

    /// These are the three colors the square cursor can be. I attach an image to them in the same way that I would for the GameObjects above,
    /// except I drag from the Project window to the Inspector window.
    /// Currently, the cursor color stuff is not relevant because the "dragging to select" feature is not created yet.
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;

    /// These are some references to components from the objects in the scene.
    public Tilemap tileMap;
    guiScript gui;
    //Player playerData;
    private SpriteRenderer spriteRenderer;
    public int[,] world; /* Commenting this and all related lines out because we aren't using it at the moment but might in the future */

    /// These are variables that I will be using in this script.
    /// If I want scripts on other GameObjects to use data from this script, it will likely be using one of these variables.
    /// As of now, the only one used in a different script is pendingMoves, which I use to display the number on the GUI.
    Vector3 mousePosition;
    public Vector3Int playerCell;
    static int zAxis = 0;
    public bool started = false;
    public bool confirm = false;
    public int pendingMoves = 0;
    bool moving = false;
    Vector3 destination;
    float startTime;
    float totalDistance;
    public Vector3Int start;

    Vector3 undefinedVec3 = new Vector3(-1, -1, 0);
    Vector3Int undefinedVec3Int = new Vector3Int(-1, -1, 0);

    /// This is a list of the cells of the currently selected tiles. They are (x, y, z) coordinates, but since this is a 2D map they all have the same Z value.
    List<Vector3Int> path;
    private Vector3Int[] possibleTiles = new Vector3Int[4];
    List<Vector3Int> doors;
    List<Vector3Int> wires;
    Vector3Int[] spawns;
    int spawnAmount = 0;

    void Start()
    {
        /// Initializing the tileMap in this script by getting the tileMap component from the Tilemap object that "tileMapObj" is referencing.
        /// Initializing the spriteRenderer in this script by getting the SpriteRenderer component from this object.
        /// Initializing the playerData variable in this script by getting the Player component from the player object.
        /// In the future, we will have to be more particular about the names as there will be more types of players/characters.
        tileMap = tileMapObj.GetComponent<Tilemap>();
        gui = camera.GetComponent<guiScript>();
        world = tileMapObj.GetComponent<TileMap>().world;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = yellow_cursor;

        /// Initializing the list by creating a new one.
        path = new List<Vector3Int>();
        doors = new List<Vector3Int>();
        wires = new List<Vector3Int>();
        spawns = new Vector3Int[gui.players.transform.childCount];

        for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                //Debug.Log(world[x, y]);
                if (spawnAmount < spawns.GetLength(0) && world[x, y] == 2)
                {
                    spawns[spawnAmount] = new Vector3Int(x, y, zAxis);
                    Debug.Log(spawns[spawnAmount]);
                    spawnAmount++;
                }
            }
        }

        for (int i = 0; i < gui.players.transform.childCount; i++)
        {
            Debug.Log(gui.players.transform.GetChild(i));
            Debug.Log(spawns[i]);
            gui.players.transform.GetChild(i).transform.position = spawns[i];
        }

        playerCell = tileMap.WorldToCell(gui.playerData.transform.position);
        tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileMap.SetColor(playerCell, Color.magenta);

        // Setting possible-to-select tiles as those surrounding the player, if the player has moves
        if (gui.playerData.moves > 0)
        {
            HighlightNeighbors(playerCell);
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

        if (Input.GetMouseButtonDown(0)/* || Input.GetMouseButton(0)*/)
        {
            /// This is just converting the Vector3 position of the cursor and player in the world to the cell position of
            /// the tile they are currently on when the player clicks their left mouse button.
            Vector3Int goal = tileMap.WorldToCell(transform.position);
            /// If this statement is true, it adds the tile to the path. See CheckTile() for more info.
            if (gui.playerData.moves > 0 && CheckTile(start, goal) && IsNeighbor(start, goal))
            {
                start = goal;
                UnhighlightOldNeighbors();
                /// Changes the tileflags to none so that we are able to change anything about the tile.
                /// Then, changiing the color to red, adds it to the path list, and adjusts the number of remaining moves appropriately.
                tileMap.SetTileFlags(goal, TileFlags.None);
                Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                tileMap.SetColor(goal, color);
                //spriteRenderer.sprite = green_cursor;
                path.Add(goal);
                --gui.playerData.moves;
                ++pendingMoves;
                if (gui.playerData.moves > 0)
                {
                    HighlightNeighbors(start);
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
                UnhighlightOldNeighbors();
                tileMap.SetColor(coordinate, Color.white);
                path.RemoveAt(path.Count - 1);
                gui.playerData.moves++;
                pendingMoves--;
                if(path.Count > 0)
                {
                    start = path[path.Count - 1];
                    HighlightNeighbors(path[path.Count - 1]);
                }
                else
                {
                    start = playerCell;
                    HighlightNeighbors(playerCell);
                }
            }
        }

        /// "confirm" is set true by the GUI script when the button is pressed.
        /// "started" is true if the player is currently moving, false otherwise.
        /// Setting up player movement.
        if (pendingMoves > 0 && confirm && !started)
        {
            pendingMoves = 0;
            for (int i = 0; i < path.Count; i++)
            {
                Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                tileMap.SetColor(path[i], color);
            }
            started = true;
            moving = true;
            destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
            startTime = Time.time;
            Debug.Log(gui.playerData.transform.position + " " + destination);
            totalDistance = Vector3.Distance(gui.playerData.transform.position, destination);
            tileMap.SetTileFlags(playerCell, TileFlags.None);
            tileMap.SetColor(playerCell, Color.white);
        }

        /// This begins player movement by moving the player one tile at a time.
        if (moving && confirm && path.Count > 0)
        {
            UnhighlightOldNeighbors();
            gui.playerData.animator.Play("walking");
            float distanceCovered, fractionOfJourney;
            if (gui.playerData.transform.position != destination)
            {
                distanceCovered = (Time.time - startTime) * gui.playerData.playerSpeed;
                fractionOfJourney = distanceCovered / totalDistance;
                Debug.Log(totalDistance);
                gui.playerData.transform.position = Vector3.Lerp(gui.playerData.transform.position, destination, fractionOfJourney);
            }
            if (gui.playerData.transform.position == destination)
            {
                OpenDoors(path[0]);
                tileMap.SetTileFlags(path[0], TileFlags.None);
                tileMap.SetColor(path[0], Color.white);
                path.RemoveAt(0);
                if (path.Count > 0)
                {
                    startTime = Time.time;
                    destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
                    totalDistance = Vector3.Distance(gui.playerData.transform.position, destination);
                }
                else
                {
                    gui.playerData.animator.Play("idle");
                    moving = false;
                    playerCell = tileMap.WorldToCell(gui.playerData.transform.position);
                    tileMap.SetTileFlags(playerCell, TileFlags.None);
                    tileMap.SetColor(playerCell, Color.magenta);
                    confirm = false;
                    started = false;
                    if (gui.playerData.moves > 0)
                    {
                        HighlightNeighbors(playerCell);
                    }
                }
            }
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
        //Debug.Log(tileMap.GetTile(undefinedVec3Int));
        TileBase goalTile = tileMap.GetTile(goal);
        TileBase startTile = tileMap.GetTile(start);
        //Debug.Log(startTile.name);
        //Debug.Log(goalTile.name);
        //Debug.Log("goal: " + goal+ "| start: " + start);
        if (goal == tileMap.WorldToCell(gui.playerData.transform.position) || path.Contains(goal))
        {
            return false;
        }
        else if (goalTile == null || goalTile.name.Contains("wall") || goalTile.name.Contains("door"))
        {
            //Debug.Log("wall/door/null");
            return false;
        }
        else if (startTile.name.Contains("barrier"))
        {
            if (startTile.name.Contains("left") && goal.x == start.x - 1)
            {
                //Debug.Log("left");
                return false;
            }
            else if (startTile.name.Contains("bottom") && goal.y == start.y - 1)
            {
                //Debug.Log("bottom");
                return false;
            }
            else if (startTile.name.Contains("right") && goal.x == start.x + 1)
            {
                //Debug.Log("right");
                return false;
            }
            else if (startTile.name.Contains("top") && goal.y == start.y + 1)
            {
                //Debug.Log("top");
                return false;
            }
        }
        else if (goalTile.name.Contains("barrier"))
        {
            if (goalTile.name.Contains("right") && goal.x == start.x - 1)
            {
                return false;
            }
            else if (goalTile.name.Contains("top") && goal.y == start.y - 1)
            {
                //Debug.Log("bottom");
                return false;
            }
            else if (goalTile.name.Contains("left") && goal.x == start.x + 1)
            {
                //Debug.Log("right");
                return false;
            }
            else if (goalTile.name.Contains("bottom") && goal.y == start.y + 1)
            {
                //Debug.Log("top");
                return false;
            }
        }
        return true;
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
        //Debug.Log("checking left");
        Color color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        if (CheckTile(start, left))
        {
            possibleTiles[0] = left;
            tileMap.SetTileFlags(left, TileFlags.None);
            tileMap.SetColor(left, color);
        }
        //Debug.Log("checking right");
        if (CheckTile(start, right))
        {
            possibleTiles[1] = right;
            tileMap.SetTileFlags(right, TileFlags.None);
            tileMap.SetColor(right, color);
        }
        //Debug.Log("checking top");
        if (CheckTile(start, above))
        {
            possibleTiles[2] = above;
            tileMap.SetTileFlags(above, TileFlags.None);
            tileMap.SetColor(above, color);
        }
        //Debug.Log("checking bottom");
        if (CheckTile(start, below))
        {
            possibleTiles[3] = below;
            tileMap.SetTileFlags(below, TileFlags.None);
            tileMap.SetColor(below, color);
        }
    }

    public void UnhighlightOldNeighbors()
    {
        for(int i = 0; i < possibleTiles.Length; i++)
        {
            if (possibleTiles[i] != undefinedVec3Int)
            {
                tileMap.SetColor(possibleTiles[i], Color.white);
                possibleTiles[i] = undefinedVec3Int;
            }
        }
    }

    public void ClearPath()
    {
        UnhighlightOldNeighbors();
        pendingMoves = 0;
        playerCell = tileMap.WorldToCell(gui.playerData.transform.position);
        tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileMap.SetColor(playerCell, Color.white);
        foreach (Vector3Int coordinate in path)
        {
            tileMap.SetTileFlags(coordinate, TileFlags.None);
            tileMap.SetColor(coordinate, Color.white);
            gui.playerData.moves++;
        }
        path.Clear();
    }

    public void OpenDoors(Vector3Int start)
    {
        TileBase startTile = tileMap.GetTile(start);
        Vector3Int prev = undefinedVec3Int;
        if (startTile.name.Contains("key"))
        {
            DoorSearch(start, prev);
            foreach (Vector3Int door in doors)
            {
                tileMap.SetTile(door, floor_tile_asset);
            }
            wires.Clear();
            doors.Clear();
        }
    }

    public void DoorSearch(Vector3Int start, Vector3Int prev)
    {
        Vector3Int left = new Vector3Int(start.x - 1, start.y, 0);
        Vector3Int right = new Vector3Int(start.x + 1, start.y, 0);
        Vector3Int above = new Vector3Int(start.x, start.y + 1, 0);
        Vector3Int below = new Vector3Int(start.x, start.y - 1, 0);
        TileBase tileStart = tileMap.GetTile(start);
        TileBase tileLeft = tileMap.GetTile(left);
        TileBase tileRight = tileMap.GetTile(right);
        TileBase tileAbove = tileMap.GetTile(above);
        TileBase tileBelow = tileMap.GetTile(below);

        wires.Add(start);

        if (prev == undefinedVec3Int || !wires.Contains(left))
        {
            if (tileLeft.name.Contains("door") && tileStart.name.Contains("left"))
            {
                ChangeDoors(left, prev);
                return;
            }
            else if (tileLeft.name.Contains("wire") && (tileStart.name.Contains("left") || tileStart.name.Contains("key")) && tileLeft.name.Contains("right"))
            {
                prev = start;
                DoorSearch(left, prev);
                return;
            }
        }
        if (prev == undefinedVec3Int || !wires.Contains(right))
        {
            if (tileRight.name.Contains("door") && tileStart.name.Contains("right"))
            {
                ChangeDoors(right, prev);
                return;
            }
            else if (tileRight.name.Contains("wire") && (tileStart.name.Contains("right") || tileStart.name.Contains("key")) && tileRight.name.Contains("left"))
            {
                prev = start;
                DoorSearch(right, prev);
                return;
            }
        }
        if (prev == undefinedVec3Int || !wires.Contains(above))
        {
            if (tileAbove.name.Contains("door") && tileStart.name.Contains("top"))
            {
                ChangeDoors(above, prev);
                return;
            }
            else if (tileAbove.name.Contains("wire") && (tileStart.name.Contains("top") || tileStart.name.Contains("key")) && tileAbove.name.Contains("bottom"))
            {
                prev = start;
                DoorSearch(above, prev);
                return;
            }
        }
        if (prev == undefinedVec3Int || !wires.Contains(below))
        {
            if (tileBelow.name.Contains("door") && tileStart.name.Contains("bottom"))
            {
                ChangeDoors(below, prev);
                return;
            }
            else if (tileBelow.name.Contains("wire") && (tileStart.name.Contains("bottom") || tileStart.name.Contains("key")) && tileBelow.name.Contains("top"))
            {
                prev = start;
                DoorSearch(below, prev);
                return;
            }
        }
    }

    public void ChangeDoors(Vector3Int start, Vector3Int prev)
    {
        Vector3Int left = new Vector3Int(start.x - 1, start.y, 0);
        Vector3Int right = new Vector3Int(start.x + 1, start.y, 0);
        Vector3Int above = new Vector3Int(start.x, start.y + 1, 0);
        Vector3Int below = new Vector3Int(start.x, start.y - 1, 0);
        TileBase tileLeft = tileMap.GetTile(left);
        TileBase tileRight = tileMap.GetTile(right);
        TileBase tileAbove = tileMap.GetTile(above);
        TileBase tileBelow = tileMap.GetTile(below);

        doors.Add(start);
        
        if (left != prev && tileLeft != null && !doors.Contains(left) && tileLeft.name.Contains("door"))
        {
            prev = start;
            ChangeDoors(left, prev);
        }
        if (right != prev && tileRight != null && !doors.Contains(right) && tileRight.name.Contains("door"))
        {
            prev = start;
            ChangeDoors(right, prev);
        }
        if (above != prev && tileAbove != null && !doors.Contains(above) && tileAbove.name.Contains("door"))
        {
            prev = start;
            ChangeDoors(above, prev);
        }
        if (below != prev && tileBelow != null && !doors.Contains(below) && tileBelow.name.Contains("door"))
        {
            prev = start;
            ChangeDoors(below, prev);
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