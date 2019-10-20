using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class TileClass
{
    public int type;
    public int f;
    public int g;
    public int h;
    public int room;
    public TileClass parent;
    public Vector3Int coordinate;

    public TileClass()
    {

    }
}
public class Tile_Selector_Script : MonoBehaviour
{
    const int spawn = 2;
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
    public GameObject turnHandlerObj;
    public new GameObject camera;
    public TileBase floor_tile_asset;

    /// These are the three colors the square cursor can be. I attach an image to them in the same way that I would for the GameObjects above,
    /// except I drag from the Project window to the Inspector window.
    /// Currently, the cursor color stuff is not relevant because the "dragging to select" feature is not created yet.
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;
    public Sprite red_other_cursor;
    public Sprite yellow_other_cursor;
    public Sprite green_other_cursor;

    /// These are some references to components from the objects in the scene.
    public Tilemap tileMap;
    guiScript gui;
    //Player playerData;
    private SpriteRenderer spriteRenderer;
    public TileClass[,] world;
    Turn_Handler turnHandler;

    /// These are variables that I will be using in this script.
    /// If I want scripts on other GameObjects to use data from this script, it will likely be using one of these variables.
    /// As of now, the only one used in a different script is pendingMoves, which I use to display the number on the GUI.
    Vector3 mousePosition;
    readonly int zAxis = 0;
    public bool confirm = false;
    //Vector3 destination;
    float startTime;
    float totalDistance;
    //public Vector3Int start;

    Vector3 undefinedVec3 = new Vector3(-1, -1, 0);
    Vector3Int undefinedVec3Int = new Vector3Int(-1, -1, 0);

    /// This is a list of the cells of the currently selected tiles. They are (x, y, z) coordinates, but since this is a 2D map they all have the same Z value.
    //List<Vector3Int> path;
    private Vector3Int[] possibleTiles = new Vector3Int[4];
    List<Vector3Int> doors;
    List<Vector3Int> wires;
    Vector3Int[] spawns;
    int spawnAmount = 0;

    BoundsInt cellBounds;
    Vector3Int size;

    void Awake()
    {
        int roomX = 8;
        int roomY = 8;
        tileMap = tileMapObj.GetComponent<Tilemap>();
        tileMap.CompressBounds();
        cellBounds = tileMap.cellBounds;
        world = new TileClass[tileMap.cellBounds.size.x, tileMap.cellBounds.size.y];
        for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                world[x, y] = new TileClass();
                world[x, y].coordinate = new Vector3Int(x, y, zAxis);
                TileBase tile = tileMap.GetTile(world[x, y].coordinate);
                int horizontalIndex = x / roomX + 1;
                int verticalIndex = y / roomY + 1;
                int horizontalOffset = (x / roomX) * (world.GetLength(1) / roomX);
                world[x, y].room = horizontalIndex + verticalIndex + horizontalOffset - 1;
                if (x % roomX == 0 || y % roomY == 0)
                {
                    world[x, y].room = 0;
                }
                if (!(tile == null))
                {
                    if (tile.name.Contains("spawn"))
                    {
                        world[x, y].type = 2;
                    }
                    else if(tile.name.Contains("win"))
                    {
                        world[x, y].type = 1; 
                    }
                    else if (tile.name.Contains("floor") || tile.name.Contains("wire"))
                    {
                        world[x, y].type = 0;
                    }
                }
                else
                {
                    // wall or no tile
                    world[x, y].type = -1;
                }
            }
        }
    }

    void Start()
    {
        /// Getting components from other objects to refer to them in this script
        gui = camera.GetComponent<guiScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();

        /// Initializing lists/arrays by creating new ones.
        //path = new List<Vector3Int>();
        doors = new List<Vector3Int>();
        wires = new List<Vector3Int>();
        spawns = new Vector3Int[turnHandler.players.transform.childCount];

        /// Search the world for every tile that is a spawn tile, and 
        for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                if (spawnAmount < spawns.GetLength(0) && world[x, y].type == spawn)
                {
                    spawns[spawnAmount] = new Vector3Int(x, y, zAxis);
                    spawnAmount++;
                }
            }
        }

        int arrayLength = spawns.GetLength(0);
        
        Vector3Int[] scrambledSpawns = new Vector3Int[spawns.GetLength(0)];

        for (int i = 0; i < spawns.GetLength(0); i++)
        {
            int j = Random.Range(0, arrayLength);
            scrambledSpawns[i] = spawns[j];
            while (j + 1 < arrayLength)
            {
                spawns[j] = spawns[j + 1];
                j++;
            }
            arrayLength--;
        }

        for (int i = 0; i < turnHandler.players.transform.childCount; i++)
        {
            turnHandler.players.transform.GetChild(i).transform.position = scrambledSpawns[i];
        }
    }

    void Update()
    {
        if (tileMap.GetTile(tileMap.WorldToCell(turnHandler.activePlayer.transform.position)).name.Contains("win"))
        {
            SceneManager.LoadScene("End_Scene", LoadSceneMode.Single);
        }
        if (gui.mode == 0)
        {
            CursorFollowMouse();
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                //Debug.Log("Player start: " + tileMap.WorldToCell(turnHandler.activePlayer.start));
                /// This is just converting the Vector3 position of the cursor and player in the world to the cell position of
                /// the tile they are currently on when the player clicks their left mouse button.
                Vector3Int goal = tileMap.WorldToCell(transform.position);
                /// If this statement is true, it adds the tile to the path. See CheckTile() for more info.
                if (turnHandler.activePlayer.moves > 0 && CheckTile(turnHandler.activePlayer.start, goal) && IsNeighbor(turnHandler.activePlayer.start, goal))
                {
                    turnHandler.activePlayer.AddTileToPath(goal);
                }
                if (turnHandler.activePlayer.start == goal)
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
                turnHandler.activePlayer.RemoveTileFromPath(coordinate);
            }

            /// "confirm" is set true by the GUI script when the button is pressed.
            /// "moving" is true if the player is currently moving, false otherwise.
            /// This begins player movement by moving the player one tile at a time.
            if (turnHandler.activePlayer.pendingMoves > 0 && confirm && turnHandler.activePlayer.path.Count > 0)
            {
                turnHandler.activePlayer.MovePlayer();
            }
        }
        else
        {
            HideCursor();
        }
    }

    /// Making the cursor follow the mouse, while snapping to the grid. The grid is offset by 0.5, because the Unity scene editor has its pivot point at the center,
    /// while the tiles we are using have their pivot points in one of the corners. The tiles are 1x1, so the distance from the center to the sides is 0.5.
    /// that is why I had to make the cursor snap by a 0.5 unit offset in the x and y direction via the RoundOffset() function.
    void CursorFollowMouse()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = zAxis;
        transform.position = new Vector3((RoundOffset(mousePosition.x)), (RoundOffset(mousePosition.y)), zAxis);
    }

    void HideCursor()
    {
        spriteRenderer.sprite = null;
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
        TileBase goalTile = tileMap.GetTile(goal);
        TileBase startTile = tileMap.GetTile(start);

        if (goal == tileMap.WorldToCell(turnHandler.activePlayer.transform.position) || turnHandler.activePlayer.path.Contains(goal))
        {
            return false;
        }
        else if (goalTile == null || goalTile.name.Contains("wall") || goalTile.name.Contains("door"))
        {
            return false;
        }
        else if (startTile.name.Contains("barrier"))
        {
            if (startTile.name.Contains("left") && goal.x == start.x - 1)
            {
                return false;
            }
            else if (startTile.name.Contains("bottom") && goal.y == start.y - 1)
            {
                return false;
            }
            else if (startTile.name.Contains("right") && goal.x == start.x + 1)
            {
                return false;
            }
            else if (startTile.name.Contains("top") && goal.y == start.y + 1)
            {
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
                return false;
            }
            else if (goalTile.name.Contains("left") && goal.x == start.x + 1)
            {
                return false;
            }
            else if (goalTile.name.Contains("bottom") && goal.y == start.y + 1)
            {
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
        Color color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        if (CheckTile(start, left))
        {
            possibleTiles[0] = left;
            tileMap.SetTileFlags(left, TileFlags.None);
            tileMap.SetColor(left, color);
        }
        if (CheckTile(start, right))
        {
            possibleTiles[1] = right;
            tileMap.SetTileFlags(right, TileFlags.None);
            tileMap.SetColor(right, color);
        }
        if (CheckTile(start, above))
        {
            possibleTiles[2] = above;
            tileMap.SetTileFlags(above, TileFlags.None);
            tileMap.SetColor(above, color);
        }
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


    public int GetHeuristic(TileClass start, TileClass goal)
    {
        return Mathf.Abs(start.coordinate.x - goal.coordinate.x) + Mathf.Abs(start.coordinate.y - goal.coordinate.y);
    }

    public bool UpdateF(TileClass current, List<TileClass> open, List<TileClass> closed, TileClass goal)
    {
        TileClass previous = closed[closed.Count - 1];
        if (!closed.Contains(current) && (current.type == 0 || current.type == 1 || current.type == 2))
        {
            if(!open.Contains(current))
            {
                current.parent = previous;
                current.g = 1 + previous.g;
                current.h = GetHeuristic(current, goal);
                current.f = current.g + current.h;
                return true;
            }
            else if (current.g > 1 + previous.g)
            {
                current.parent = previous;
                current.g = 1 + previous.g;
                current.f = current.g + current.h;
                open.Remove(current);
                return true;
            }
            return false;
        }
        return false;
    }

    public bool BuildPathAStar(TileClass start, TileClass goal)
    {
        List<TileClass> open = new List<TileClass>();
        List<TileClass> closed = new List<TileClass>();
        open.Add(start);
        start.parent = null;
        start.g = 0;
        bool foundGoal = false;
        List<TileClass> adjacent = new List<TileClass>();
        int count = 0;
        while (open.Count > 0)
        {
            if (open[0] == goal)
            {
                foundGoal = true;
                break;
            }
            closed.Add(open[0]);
            open.RemoveAt(0);

            if (!(tileMap.GetTile(new Vector3Int(closed[closed.Count - 1].coordinate.x - 1, closed[closed.Count - 1].coordinate.y, zAxis)) == null))
            {
                adjacent.Add(world[closed[closed.Count - 1].coordinate.x - 1, closed[closed.Count - 1].coordinate.y]);
            }
            if (!(tileMap.GetTile(new Vector3Int(closed[closed.Count - 1].coordinate.x + 1, closed[closed.Count - 1].coordinate.y, zAxis)) == null))
            {
                adjacent.Add(world[closed[closed.Count - 1].coordinate.x + 1, closed[closed.Count - 1].coordinate.y]);
            }
            if (!(tileMap.GetTile(new Vector3Int(closed[closed.Count - 1].coordinate.x, closed[closed.Count - 1].coordinate.y + 1, zAxis)) == null))
            {
                adjacent.Add(world[closed[closed.Count - 1].coordinate.x, closed[closed.Count - 1].coordinate.y + 1]);
            }
            if (!(tileMap.GetTile(new Vector3Int(closed[closed.Count - 1].coordinate.x, closed[closed.Count - 1].coordinate.y - 1, zAxis)) == null))
            {
                adjacent.Add(world[closed[closed.Count - 1].coordinate.x, closed[closed.Count - 1].coordinate.y - 1]);
            }
            foreach(TileClass tile in adjacent)
            {
                if (count == 0)
                {
                    open.Add(tile);
                    count++;
                }
                else if (UpdateF(tile, open, closed, goal))
                {
                    bool added = false;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if (tile.f < open[i].f)
                        {
                            open.Insert(i, tile);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        open.Add(tile);
                    }
                }
            }
            adjacent.Clear();
        }
        if (foundGoal)
        {
            return true;
        }
        return false;
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