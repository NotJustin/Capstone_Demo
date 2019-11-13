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
    public int playerCount;
    public Tilemap map;
    public Tilemap world;
    public int roomSize = 7;
    public int startX, startY;
    public TileRoom up, down, left, right;
    int x, y;
    public TileBase empty_tile_asset;
    public List<GameObject> enemies;
    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;
    public TileRoom(Tilemap _world, Tilemap _map, int _number, int _x, int _y)
    {
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        world = _world;
        map = _map;
        number = _number;
        tiles = new TileClass[roomSize, roomSize];
        startX = _x;
        startY = _y;
        x = _x;
        y = _y;
        GenerateTileList(map, x, y);
        up = null;
        down = null;
        left = null;
        right = null;
        playerCount = 0;
        enemies = new List<GameObject>();
    }
    TileClass AddTile(int _number, int x, int y)
    {
        TileClass tile = new TileClass();
        tile.room = number;
        tile.cell = new Vector3Int(x + startX + 1, y + startY + 1, 0);
        tile.tileBase = map.GetTile(new Vector3Int(x + 1, y + 1, 0));
        return tile;
    }
    public void GenerateTileList(Tilemap _map, int _startX, int _startY)
    {
        startX = _startX;
        startY = _startY;
        int spawn = 2, floor = 0, win = 1, wall = -1, enemyOne = 31, enemyTwo = 32, enemyThree = 33, enemyFour = 34;
        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                tiles[x, y] = AddTile(number, x, y);
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
                    else if (tiles[x, y].tileBase.name.Contains("enemy"))
                    {
                        if (tiles[x, y].tileBase.name.Contains("1"))
                        {
                            tiles[x, y].type = enemyOne;
                        }
                        else if(tiles[x, y].tileBase.name.Contains("2"))
                        {
                            tiles[x, y].type = enemyTwo;
                        }
                        else if (tiles[x, y].tileBase.name.Contains("3"))
                        {
                            tiles[x, y].type = enemyThree;
                        }
                        else if (tiles[x, y].tileBase.name.Contains("4"))
                        {
                            tiles[x, y].type = enemyFour;
                        }
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
            Vector3Int newCell = new Vector3Int(tile.cell.x, tile.cell.y, 0);
            world.SetColor(newCell, new Color(1, 1, 1, 0.1f));
        }
        foreach (GameObject enemy in enemies)
        {
            IEnemy newEnemy = turnHandler.FetchEnemyType(enemy);
            DespawnedEnemy despawnedEnemy = new DespawnedEnemy(enemy.tag, newEnemy.health);
            turnHandler.despawnedEnemies.Add(despawnedEnemy);
            turnHandler.enemyList.Remove(enemy);
        }
    }
    public void ShowRoom()
    {
        foreach (TileClass tile in tiles)
        {
            Vector3Int newCell = new Vector3Int(tile.cell.x, tile.cell.y, 0);
            world.SetTile(newCell, tile.tileBase);
            world.SetColor(newCell, new Color(1, 1, 1, 1));
            tile.position = new Vector3(tile.cell.x, tile.cell.y, 0);
        }
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
    public TileBase door_tile_asset;
    public TileBase empty_tile_asset;

    /// These are the three colors the square cursor can be. I attach an image to them in the same way that I would for the GameObjects above,
    /// except I drag from the Project window to the Inspector window.
    /// Currently, the cursor color stuff is not relevant because the "dragging to select" feature is not created yet.

    /// These are some references to components from the objects in the scene.
    public Tilemap world;
    public Tilemap highlighter;
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

    int roomCount = 0;
    TileRoom firstRoom;
    public List<TileRoom> rooms;

    public void GenerateWalls(TileRoom room)
    {
        int doorMin = 1, doorMax = 4, wallsLeft = 4, doorCount = Random.Range(doorMin, doorMax + 1), doorCountStart = doorCount;

        Debug.Log("initial door count: " + doorCountStart);

        if (world.GetTile(new Vector3Int(room.startX, room.startY + 1, zAxis)) == null)
        {
            doorCount = GenerateLeftWall(room, room.startX, room.startY, wallsLeft, doorCount);
        }
        wallsLeft--;
        if (world.GetTile(new Vector3Int(room.startX + 1, room.startY + 8, zAxis)) == null)
        {
            doorCount = GenerateUpWall(room, room.startX, room.startY + 8, wallsLeft, doorCount);
        }
        wallsLeft--;
        if (world.GetTile(new Vector3Int(room.startX + 8, room.startY + 1, zAxis)) == null)
        {
            doorCount = GenerateRightWall(room, room.startX + 8, room.startY + 8, wallsLeft, doorCount);
        }
        wallsLeft--;
        if (world.GetTile(new Vector3Int(room.startX + 1, room.startY, zAxis)) == null)
        {
            int test = doorCountStart > doorCount ? GenerateDownWall(room, room.startX + 8, room.startY, wallsLeft, doorCount) : GenerateDownWall(room, room.startX + 8, room.startY, wallsLeft, doorCount + 1);
        }
    }

    public bool HasGenerator(TileRoom room)
    {
        for (int x = 0; x < room.roomSize; x++)
        {
            for (int y = 0; y < room.roomSize; y++)
            {
                if (room.tiles[x, y].tileBase.name.Contains("key"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GenerateLeftWall(TileRoom room, int x, int y, int wallsLeft, int doorCount)
    {
        Vector3Int cell;
        int doorChance = Random.Range(doorCount, wallsLeft + 1);
        int chanceTile = Random.Range(room.startY + 1, room.startY + 8);
        bool createdDoor = false;
        while (y < room.startY + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (doorCount > 0 && !createdDoor && doorChance >= wallsLeft && y == chanceTile)
                {
                    if (HasGenerator(room))
                    {
                        world.SetTile(cell, door_tile_asset);
                    }
                    else
                    {
                        world.SetTile(cell, floor_tile_asset);
                    }
                    createdDoor = true;
                    doorCount--;
                }
                else
                {
                    world.SetTile(cell, wall_tile_asset);
                }
            }
            y++;
        }
        return doorCount;
    }

    public int GenerateUpWall(TileRoom room, int x, int y, int wallsLeft, int doorCount)
    {
        Vector3Int cell;
        int doorChance = Random.Range(doorCount, wallsLeft + 1);
        int chanceTile = Random.Range(room.startX + 1, room.startX + 8);
        bool createdDoor = false;
        while (x < room.startX + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (doorCount > 0 && !createdDoor && doorChance >= wallsLeft && x == chanceTile)
                {
                    if (HasGenerator(room))
                    {
                        world.SetTile(cell, door_tile_asset);
                    }
                    else
                    {
                        world.SetTile(cell, floor_tile_asset);
                    }
                    createdDoor = true;
                    doorCount--;
                }
                else
                {
                    world.SetTile(cell, wall_tile_asset);
                }
            }
            x++;
        }
        return doorCount;
    }

    public int GenerateRightWall(TileRoom room, int x, int y, int wallsLeft, int doorCount)
    {
        Vector3Int cell;
        int doorChance = Random.Range(doorCount, wallsLeft + 1);
        int chanceTile = Random.Range(room.startY + 1, room.startY + 8);
        bool createdDoor = false;
        while (y > room.startY - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (doorCount > 0 && !createdDoor && doorChance >= wallsLeft && y == chanceTile)
                {
                    if (HasGenerator(room))
                    {
                        world.SetTile(cell, door_tile_asset);
                    }
                    else
                    {
                        world.SetTile(cell, floor_tile_asset);
                    }
                    createdDoor = true;
                    doorCount--;
                }
                else
                {
                    world.SetTile(cell, wall_tile_asset);
                }
            }
            y--;
        }
        return doorCount;
    }

    public int GenerateDownWall(TileRoom room, int x, int y, int wallsLeft, int doorCount)
    {
        Vector3Int cell;
        int doorChance = Random.Range(doorCount, wallsLeft + 1);
        int chanceTile = Random.Range(room.startX + 1, room.startX + 8);
        bool createdDoor = false;
        while (x > room.startX - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (doorCount > 0 && !createdDoor && doorChance >= wallsLeft && x == chanceTile)
                {
                    if (HasGenerator(room))
                    {
                        world.SetTile(cell, door_tile_asset);
                    }
                    else
                    {
                        world.SetTile(cell, floor_tile_asset);
                    }
                    createdDoor = true;
                }
                else
                {
                    world.SetTile(cell, wall_tile_asset);
                }
            }
            x--;
        }
        return doorCount;
    }

    public TileRoom AddRoom(GameObject room, int x, int y)
    {
        roomCount++;
        TileRoom newRoom = new TileRoom(world, room.GetComponent<Tilemap>(), roomCount, x, y);
        GenerateWalls(newRoom);
        newRoom.ShowRoom();
        rooms.Add(newRoom);
        return newRoom;
    }

    void Awake()
    {
        /// Getting components from other objects to refer to them in this script
        gui = camera.GetComponent<guiScript>();
        cursor = cursorObj.GetComponent<Cursor>();
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        rooms = new List<TileRoom>();
        firstRoom = AddRoom(room_1, 0, 0);
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
        firstRoom.playerCount = spawns.Length;

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
            player.UpdateRoom();
            player.start = scrambledSpawns[i];
        }
    }

    bool addingRoom = false;

    Vector3 activePlayerPos;

    void Update()
    {
        //Debug.Log(world);
        /* if (tileMap.GetTile(tileMap.WorldToCell(turnHandler.activePlayer.transform.position)).name.Contains("win"))
         {
             SceneManager.LoadScene("End_Scene", LoadSceneMode.Single);
         }*/
        activePlayerPos = new Vector3(Mathf.Round(turnHandler.activePlayer.transform.position.x * 10) / 10, Mathf.Round(turnHandler.activePlayer.transform.position.y * 10) / 10, zAxis);
        if (turnHandler.activePlayer.tileRoom != null && !(activePlayerPos.x == RoundOffset(turnHandler.activePlayer.tileRoom.startX) ||
            activePlayerPos.x == RoundOffset(turnHandler.activePlayer.tileRoom.startX + 8) ||
            activePlayerPos.y == RoundOffset(turnHandler.activePlayer.tileRoom.startY) ||
            activePlayerPos.y == RoundOffset(turnHandler.activePlayer.tileRoom.startY + 8)))
        {
            addingRoom = false;
        }
        else
        {
            turnHandler.activePlayer.tileRoom = null;
        }
        if (!addingRoom && 
            world.GetTile(world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX - 5, turnHandler.activePlayer.prevRoom.startY + 1, zAxis))) == null && 
            activePlayerPos.x == RoundOffset(turnHandler.activePlayer.prevRoom.startX))
        {
            addingRoom = true;
            if (Random.Range(0, 2) > 0)
            {
                AddRoom(room_2, (turnHandler.activePlayer.prevRoom.startX - 8), turnHandler.activePlayer.prevRoom.startY);
            }
            else
            {
                AddRoom(room_1, (turnHandler.activePlayer.prevRoom.startX - 8), turnHandler.activePlayer.prevRoom.startY);
            }
        }
        else if(!addingRoom &&
            world.GetTile(world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX + 10, turnHandler.activePlayer.prevRoom.startY + 1, zAxis))) == null &&
            activePlayerPos.x == RoundOffset(turnHandler.activePlayer.prevRoom.startX + 8))
        {
            addingRoom = true;
            if (Random.Range(0, 2) > 0)
            {
                AddRoom(room_2, (turnHandler.activePlayer.prevRoom.startX + 8), turnHandler.activePlayer.prevRoom.startY);
            }
            else
            {
                AddRoom(room_1, (turnHandler.activePlayer.prevRoom.startX + 8), turnHandler.activePlayer.prevRoom.startY);
            }
        }
        else if (!addingRoom &&
            world.GetTile(world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX + 1, turnHandler.activePlayer.prevRoom.startY - 5, zAxis))) == null &&
            activePlayerPos.y == RoundOffset(turnHandler.activePlayer.prevRoom.startY))
        {
            addingRoom = true;
            if (Random.Range(0, 2) > 0)
            {
                AddRoom(room_2, (turnHandler.activePlayer.prevRoom.startX), turnHandler.activePlayer.prevRoom.startY - 8);
            }
            else
            {
                AddRoom(room_1, (turnHandler.activePlayer.prevRoom.startX), turnHandler.activePlayer.prevRoom.startY - 8);
            }
        }
        else if (!addingRoom &&
            world.GetTile(world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX + 1, turnHandler.activePlayer.prevRoom.startY + 10, zAxis))) == null &&
            activePlayerPos.y == RoundOffset(turnHandler.activePlayer.prevRoom.startY + 8))
        {
            addingRoom = true;
            if (Random.Range(0, 2) > 0)
            {
                AddRoom(room_2, (turnHandler.activePlayer.prevRoom.startX), turnHandler.activePlayer.prevRoom.startY + 8);
            }
            else
            {
                AddRoom(room_1, (turnHandler.activePlayer.prevRoom.startX), turnHandler.activePlayer.prevRoom.startY + 8);
            }
        }
    }

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
            highlighter.SetTile(left, floor_tile_asset);
            highlighter.SetTileFlags(left, TileFlags.None);
            highlighter.SetColor(left, color);
        }
        if (CheckTile(start, right))
        {
            possibleTiles[1] = right;
            highlighter.SetTile(right, floor_tile_asset);
            highlighter.SetTileFlags(right, TileFlags.None);
            highlighter.SetColor(right, color);
        }
        if (CheckTile(start, above))
        {
            possibleTiles[2] = above;
            highlighter.SetTile(above, floor_tile_asset);
            highlighter.SetTileFlags(above, TileFlags.None);
            highlighter.SetColor(above, color);
        }
        if (CheckTile(start, below))
        {
            possibleTiles[3] = below;
            highlighter.SetTile(below, floor_tile_asset);
            highlighter.SetTileFlags(below, TileFlags.None);
            highlighter.SetColor(below, color);
        }
    }

    public void UnhighlightOldNeighbors()
    {
        Color color = new Color(1, 1, 1, 0.0f);
        for(int i = 0; i < possibleTiles.Length; i++)
        {
            if (possibleTiles[i] != undefinedVec3Int)
            {
                highlighter.SetTile(possibleTiles[i], floor_tile_asset);
                highlighter.SetColor(possibleTiles[i], color);
                possibleTiles[i] = undefinedVec3Int;
            }
        }
    }

    public void OpenDoors(Player player)
    {
        Vector3 start = player.transform.position;
        int room = player.tileRoom.number;
        int roomIndex = -1;
        int x = -1, y = -1;
        int xCoor, yCoor;
        if (start.x < 0)
        {
            xCoor = (int)(start.x - 1);
        }
        else
        {
            xCoor = (int)(start.x);
        }
        if (start.y < 0)
        {
            yCoor = (int)(start.y - 1);
        }
        else
        {
            yCoor = (int)(start.y);
        }
        Vector3Int cell = new Vector3Int(xCoor, yCoor, zAxis);
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].number == room)
            {
                x = rooms[i].startX;
                y = rooms[i].startY;
                roomIndex = i;
                break;
            }
        }
        while (y < rooms[roomIndex].startY + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("door"))
            {
                world.SetTile(cell, floor_tile_asset);
            }
            y++;
        }
        y--;
        while (x < rooms[roomIndex].startX + 9)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("door"))
            {
                world.SetTile(cell, floor_tile_asset);
            }
            x++;
        }
        x--;
        while (y > rooms[roomIndex].startY - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("door"))
            {
                world.SetTile(cell, floor_tile_asset);
            }
            y--;
        }
        y++;
        while (x > rooms[roomIndex].startX - 1)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("door"))
            {
                world.SetTile(cell, floor_tile_asset);
            }
            x--;
        }

        /*
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
        }*/
    }

    /*public void DoorSearch(Vector3Int start, Vector3Int prev)
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
    }*/

    /*public void ChangeDoors(Vector3Int start, Vector3Int prev)
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
    }*/
}