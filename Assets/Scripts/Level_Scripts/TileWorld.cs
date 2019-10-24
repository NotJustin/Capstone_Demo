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
    public TileBase tileBase;
    public Vector3Int cell;
    public Vector3 position;

    public TileClass()
    {

    }
}

public class TileRoom
{
    public TileClass[,] tiles;
    public int number;
    public Tilemap map;
    public Tilemap world;
    int roomSize = 7;
    public int startX, startY;
    public TileRoom up, down, left, right;
    int x, y;
    public TileBase empty_tile_asset;
    public TileRoom(Tilemap _world, Tilemap _map, int _number, int _x, int _y)
    {
        world = _world;
        map = _map;
        number = _number;
        tiles = new TileClass[roomSize, roomSize];
        GenerateTileList(map, x, y);
        up = null;
        down = null;
        left = null;
        right = null;
        x = _x;
        y = _y;
    }
    TileClass AddTile(int _number, int x, int startX, int y, int startY)
    {
        TileClass tile = new TileClass();
        tile.room = number;
        tile.cell = new Vector3Int(x + startX + 1, y + startY + 1, 0);
        tile.position = map.CellToWorld(tile.cell);
        tile.tileBase = map.GetTile(tile.cell);
        return tile;
    }
    public void GenerateTileList(Tilemap _map, int _startX, int _startY)
    {
        startX = _startX;
        startY = _startY;
        int spawn = 2;
        int floor = 0;
        int win = 1;
        int wall = -1;
        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                tiles[x, y] = AddTile(number, x, startX, y, startY);
                if (tiles[x, y].tileBase != null)
                {
                    if (tiles[x, y].tileBase.name.Contains("spawn"))
                    {
                        tiles[x, y].type = spawn;
                    }
                    else if (tiles[x, y].tileBase.name.Contains("win"))
                    {
                        tiles[x, y].type = win;
                    }
                    else if (tiles[x, y].tileBase.name.Contains("floor") || tiles[x, y].tileBase.name.Contains("wire"))
                    {
                        tiles[x, y].type = floor;
                    }
                    else if (tiles[x, y].tileBase.name.Contains("wall") || tiles[x, y].tileBase.name.Contains("barrier"))
                    {
                        tiles[x, y].type = wall;
                    }
                }
                else
                {
                    // no tile
                    tiles[x, y].type = wall;
                }
            }
        }
    }
    public void HideRoom()
    {
        foreach (TileClass tile in tiles)
        {
            world.SetTile(tile.cell, empty_tile_asset);
        }
        world.RefreshAllTiles();
    }
    public void ShowRoom()
    {
        foreach (TileClass tile in tiles)
        {
            world.SetTile(tile.cell, tile.tileBase);
        }
        world.RefreshAllTiles();
    }
}

public class TileWorld : MonoBehaviour
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
    public GameObject tileMapWorldObj;
    public GameObject turnHandlerObj;
    public GameObject cursorObj;
    public GameObject room_1;
    public GameObject room_2;
    public GameObject room_3;
    public GameObject room_4;
    public GameObject room_5;
    public GameObject room_6;
    public new GameObject camera;
    public TileBase floor_tile_asset;
    public TileBase wall_tile_asset;

    /// These are the three colors the square cursor can be. I attach an image to them in the same way that I would for the GameObjects above,
    /// except I drag from the Project window to the Inspector window.
    /// Currently, the cursor color stuff is not relevant because the "dragging to select" feature is not created yet.

    /// These are some references to components from the objects in the scene.
    public Tilemap world;
    guiScript gui;
    public Cursor cursor;
    Turn_Handler turnHandler;

    /// These are variables that I will be using in this script.
    /// If I want scripts on other GameObjects to use data from this script, it will likely be using one of these variables.
    /// As of now, the only one used in a different script is pendingMoves, which I use to display the number on the GUI.
    readonly int zAxis = 0;

    Vector3Int undefinedVec3Int = new Vector3Int(-1, -1, 0);

    /// This is a list of the cells of the currently selected tiles. They are (x, y, z) coordinates, but since this is a 2D map they all have the same Z value.
    private Vector3Int[] possibleTiles = new Vector3Int[4];
    List<Vector3Int> doors;
    List<Vector3Int> wires;
    Vector3Int[] spawns;
    int spawnAmount = 0;

    int roomCount;
    TileRoom firstRoom;

    public void GenerateWalls(TileRoom room)
    {
        int y = room.startY;
        int x = room.startX;
        Vector3Int cell;

        int doorCount = Random.Range(1, 4);
        while (y < room.startY + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                world.SetTile(cell, wall_tile_asset);
            }
            y++;
        }
        y--;
        while (x < room.startX + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                world.SetTile(cell, wall_tile_asset);
            }
            x++;
        }
        x--;
        while (y > room.startY - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                world.SetTile(cell, wall_tile_asset);
            }
            y--;
        }
        y++;
        while (x > room.startX - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                world.SetTile(cell, wall_tile_asset);
            }
            x--;
        }
        x++;

    }

    public TileRoom AddRoom(GameObject room, int x, int y)
    {
        Instantiate(room, transform);
        roomCount++;
        return new TileRoom(world, room.GetComponent<Tilemap>(), roomCount, x, y);
    }

    void Awake()
    {
        /// Getting components from other objects to refer to them in this script
        gui = camera.GetComponent<guiScript>();
        cursor = cursorObj.GetComponent<Cursor>();
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        firstRoom = AddRoom(room_1, 0, 0);
        GenerateWalls(firstRoom);
        firstRoom.ShowRoom();
        world = tileMapWorldObj.GetComponent<Tilemap>();
        turnHandler.Initialize();
        world.CompressBounds();
    }

    void Start()
    {

        /// Initializing lists/arrays by creating new ones.
        doors = new List<Vector3Int>();
        wires = new List<Vector3Int>();
        spawns = new Vector3Int[turnHandler.players.transform.childCount];

        /// Search the world for every tile that is a spawn tile, and 
        int spawn = 2;
        for (int x = 0; x < firstRoom.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < firstRoom.tiles.GetLength(1); y++)
            {
                if (spawnAmount < spawns.GetLength(0) && firstRoom.tiles[x, y].type == spawn)
                {
                    spawns[spawnAmount] = new Vector3Int(x + 1, y + 1, zAxis);
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
            Transform child = turnHandler.players.transform.GetChild(i);
            child.transform.position = scrambledSpawns[i];
            Player player = child.gameObject.GetComponent<Player>();
            player.AdjustStartingPosition();
            player.start = scrambledSpawns[i];
        }
        Vector3Int playerCell = world.WorldToCell(turnHandler.activePlayer.transform.position);
        world.SetTileFlags(playerCell, TileFlags.None);
        world.SetColor(playerCell, Color.magenta);
        if (turnHandler.activePlayer.moves > 0)
        {
            HighlightNeighbors(playerCell);
        }
    }

    void Update()
    {
       /* if (tileMap.GetTile(tileMap.WorldToCell(turnHandler.activePlayer.transform.position)).name.Contains("win"))
        {
            SceneManager.LoadScene("End_Scene", LoadSceneMode.Single);
        }*/
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
        TileBase goalTile = world.GetTile(goal);
        TileBase startTile = world.GetTile(start);

        if (goal == world.WorldToCell(turnHandler.activePlayer.transform.position) || turnHandler.activePlayer.path.Contains(goal))
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
            world.SetTileFlags(left, TileFlags.None);
            world.SetColor(left, color);
        }
        if (CheckTile(start, right))
        {
            possibleTiles[1] = right;
            world.SetTileFlags(right, TileFlags.None);
            world.SetColor(right, color);
        }
        if (CheckTile(start, above))
        {
            possibleTiles[2] = above;
            world.SetTileFlags(above, TileFlags.None);
            world.SetColor(above, color);
        }
        if (CheckTile(start, below))
        {
            possibleTiles[3] = below;
            world.SetTileFlags(below, TileFlags.None);
            world.SetColor(below, color);
        }
    }

    public void UnhighlightOldNeighbors()
    {
        for(int i = 0; i < possibleTiles.Length; i++)
        {
            if (possibleTiles[i] != undefinedVec3Int)
            {
                world.SetColor(possibleTiles[i], Color.white);
                possibleTiles[i] = undefinedVec3Int;
            }
        }
    }

    public void OpenDoors(Vector3Int start)
    {
        TileBase startTile = world.GetTile(start);
        Vector3Int prev = undefinedVec3Int;
        if (startTile.name.Contains("key"))
        {
            DoorSearch(start, prev);
            foreach (Vector3Int door in doors)
            {
                world.SetTile(door, floor_tile_asset);
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
        TileBase tileStart = world.GetTile(start);
        TileBase tileLeft = world.GetTile(left);
        TileBase tileRight = world.GetTile(right);
        TileBase tileAbove = world.GetTile(above);
        TileBase tileBelow = world.GetTile(below);

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
        TileBase tileLeft = world.GetTile(left);
        TileBase tileRight = world.GetTile(right);
        TileBase tileAbove = world.GetTile(above);
        TileBase tileBelow = world.GetTile(below);

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
}