using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class Tile
{
    public int type;
    public int f;
    public int g;
    public int h;
    public Room room;
    public Tile parent;
    public TileBase tileBase;
    public Vector3Int cell;
    public Vector3 position;

    public Tile()
    {

    }
}

public class Room
{
    public Tile[,] tiles;
    public int number;
    public int playerCount;
    public Tilemap map;
    public Tilemap world;
    public int roomSize = 7;
    public int startX, startY;
    int x, y;
    public TileBase empty_tile_asset;
    public List<GameObject> enemies;
    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;
    public GameObject enemiesObj;
    public Enemies enemyData;
    public World worldClass;
    public bool opened;
    public Room(World _worldClass, Tilemap _world, Tilemap _map, int _number, int _x, int _y)
    {
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        enemiesObj = GameObject.FindGameObjectWithTag("Enemies");
        enemyData = enemiesObj.GetComponent<Enemies>();
        world = _world;
        worldClass = _worldClass;
        map = _map;
        number = _number;
        tiles = new Tile[roomSize, roomSize];
        startX = _x;
        startY = _y;
        x = _x;
        y = _y;
        GenerateTileList(map, x, y);
        Show();
        enemies = new List<GameObject>();
        SpawnEnemies();
        playerCount = 0;
        opened = false;
    }

    void SpawnEnemies()
    {
        int enemyOne = 31, enemyTwo = 32, enemyThree = 33, enemyFour = 34;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y].type == enemyOne)
                {
                    GameObject enemy = GameObject.Instantiate(enemyData.tierOneEnemies[Random.Range(0, enemyData.tierOneEnemies.Count)]);
                    enemy.transform.position = tiles[x, y].position;
                    turnHandler.enemyList.Add(enemy);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemiesObj.transform;
                    turnHandler.FetchEnemyType(enemy).room = this;
                }
                else if (tiles[x, y].type == enemyTwo)
                {
                    GameObject enemy = GameObject.Instantiate(enemyData.tierTwoEnemies[Random.Range(0, enemyData.tierTwoEnemies.Count)]);
                    enemy.transform.position = new Vector3(enemyData.RoundOffset(tiles[x, y].position.x), enemyData.RoundOffset(tiles[x, y].position.y), tiles[x, y].position.z);
                    turnHandler.enemyList.Add(enemy);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemiesObj.transform;
                    turnHandler.FetchEnemyType(enemy).room = this;
                }
                else if (tiles[x, y].type == enemyThree)
                {
                    GameObject enemy = GameObject.Instantiate(enemyData.tierThreeEnemies[Random.Range(0, enemyData.tierThreeEnemies.Count)]);
                    enemy.transform.position = tiles[x, y].position;
                    turnHandler.enemyList.Add(enemy);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemiesObj.transform;
                    turnHandler.FetchEnemyType(enemy).room = this;
                }
                else if (tiles[x, y].type == enemyFour)
                {
                    GameObject enemy = GameObject.Instantiate(enemyData.tierFourEnemies[Random.Range(0, enemyData.tierFourEnemies.Count)]);
                    enemy.transform.position = tiles[x, y].position;
                    turnHandler.enemyList.Add(enemy);
                    enemies.Add(enemy);
                    enemy.transform.parent = enemiesObj.transform;
                    turnHandler.FetchEnemyType(enemy).room = this;
                }
            }
        }
    }
    Tile AddTile(Room room, int x, int y)
    {
        Tile tile = new Tile();
        tile.room = room;
        tile.cell = new Vector3Int(x + startX, y + startY, 0);
        tile.tileBase = map.GetTile(new Vector3Int(map.origin.x + x, map.origin.y + y, map.origin.z));
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
                tiles[x, y] = AddTile(this, x, y);
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

    public void Hide()
    {
        foreach (Tile tile in tiles)
        {
            Vector3Int newCell = new Vector3Int(tile.cell.x, tile.cell.y, 0);
            world.SetColor(newCell, new Color(1, 1, 1, 0.25f));
        }
        foreach (GameObject enemy in enemies)
        {
            IEnemy newEnemy = turnHandler.FetchEnemyType(enemy);
            DespawnedEnemy despawnedEnemy = new DespawnedEnemy(enemy.tag, newEnemy.health);
            turnHandler.despawnedEnemies.Add(despawnedEnemy);
            turnHandler.enemyList.Remove(enemy);
            GameObject.Destroy(enemy);
        }
        enemies.Clear();
    }
    public void Show()
    {
        foreach (Tile tile in tiles)
        {
            Vector3Int newCell = new Vector3Int(tile.cell.x, tile.cell.y, 0);
            world.SetTile(newCell, tile.tileBase);
            world.SetColor(newCell, new Color(1, 1, 1, 1));
            tile.position = new Vector3(tile.cell.x, tile.cell.y, 0);
        }
    }

    public void OpenDoors()
    {
        for (int x = startX - 1; x < startX + roomSize + 1; x++)
        {
            for (int y = startY - 1; y < startY + roomSize + 1; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tileBase = world.GetTile(position);
                if (tileBase != null && tileBase.name.Contains("door"))
                {
                    world.SetTile(position, worldClass.floor_tile_asset);
                }
            }
        }
        if (turnHandler.activePlayer.moves > 0 && !turnHandler.activePlayer.attacked)
        {
            worldClass.HighlightNeighbors(worldClass.world.WorldToCell(turnHandler.activePlayer.transform.position));
        }
    }
}

public class World : MonoBehaviour
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
    public TileBase wall_left_outer_tile_asset;
    public TileBase wall_up_outer_tile_asset;
    public TileBase wall_right_outer_tile_asset;
    public TileBase wall_down_outer_tile_asset;
    public TileBase wall_double_vertical_tile_asset;
    public TileBase wall_double_horizontal_tile_asset;
    public TileBase wall_triple_left_tile_asset;
    public TileBase wall_triple_up_tile_asset;
    public TileBase wall_triple_right_tile_asset;
    public TileBase wall_triple_down_tile_asset;
    public TileBase wall_blank_tile_asset;
    public TileBase wall_four_corners_tile_asset;
    public TileBase wall_one_corners_one_tile_asset;
    public TileBase wall_one_corners_two_tile_asset;
    public TileBase wall_one_corners_three_tile_asset;
    public TileBase wall_one_corners_four_tile_asset;
    public TileBase wall_two_corners_left_tile_asset;
    public TileBase wall_two_corners_up_tile_asset;
    public TileBase wall_two_corners_right_tile_asset;
    public TileBase wall_two_corners_down_tile_asset;
    public TileBase wall_three_corners_two_tile_asset;
    public TileBase wall_three_corners_one_tile_asset;
    public TileBase wall_three_corners_four_tile_asset;
    public TileBase wall_three_corners_three_tile_asset;
    public TileBase wall_single_left_tile_asset;
    public TileBase wall_single_up_tile_asset;
    public TileBase wall_single_right_tile_asset;
    public TileBase wall_single_down_tile_asset;
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
    public Vector3Int[] possibleTiles = new Vector3Int[8];
    Vector3Int[] spawns;
    int spawnAmount = 0;

    int roomCount = 0;
    Room firstRoom;
    public List<Room> rooms;
    public List<GameObject> unusedRooms;

    public int maxRoomCount;
    public int totalDoorCount = 1;
    public int totalWallCount = 0;
    public bool WallExists(Vector3Int cell)
    {
        return world.GetTile(cell) != null;
    }

    public struct wall
    {
        public string name;
        public Vector3Int cell;
        public wall(string name, Vector3Int cell)
        {
            this.name = name;
            this.cell = cell;
        }
    }

    public void GenerateWalls(Room room)
    {
        List<wall> walls = new List<wall>();
        walls.Add(new wall("left", new Vector3Int(room.startX - 1, room.startY, zAxis)));
        walls.Add(new wall ("up", new Vector3Int(room.startX, room.startY + 7, zAxis)));
        walls.Add(new wall ("right", new Vector3Int(room.startX + 7, room.startY, zAxis)));
        walls.Add(new wall ("down", new Vector3Int(room.startX, room.startY - 1, zAxis)));
        int current = walls.Count;
        System.Random random = new System.Random();
        while (current > 1)
        {
            current--;
            int next = random.Next(current + 1);
            wall temp = walls[next];
            walls[next] = walls[current];
            walls[current] = temp;
        }
        //int doorCount = Random.Range(1, (maxRoomCount - rooms.Count + 1));
        int wallsLeft = walls.Count;
        for (int i = 0; i < walls.Count; i++)
        {
            /*if (maxRoomCount * 4 == totalWallCount)
            {
                
            }*/
            if (WallExists(walls[i].cell))
            {
                if (walls[i].name == "left")
                {
                    RegenerateLeftWall(room, room.startX - 1, room.startY - 1);
                }
                else if (walls[i].name == "up")
                {
                    RegenerateUpWall(room, room.startX - 1, room.startY + 7);
                }
                else if (walls[i].name == "right")
                {
                    RegenerateRightWall(room, room.startX + 7, room.startY + 7);
                }
                else if (walls[i].name == "down")
                {
                    RegenerateDownWall(room, room.startX + 7, room.startY - 1);
                }
            }
            else
            {
                if (walls[i].name == "left")
                {
                    GenerateLeftWall(room, room.startX - 1, room.startY - 1);
                    totalWallCount++;
                }
                else if (walls[i].name == "up")
                {
                    GenerateUpWall(room, room.startX - 1, room.startY + 7);
                    totalWallCount++;
                }
                else if (walls[i].name == "right")
                {
                    GenerateRightWall(room, room.startX + 7, room.startY + 7);
                    totalWallCount++;
                }
                else if (walls[i].name == "down")
                {
                    GenerateDownWall(room, room.startX + 7, room.startY - 1);
                    totalWallCount++;
                }
            }
        }
    }

    public bool HasGenerator(Room room)
    {
        /*for (int x = 0; x < room.roomSize; x++)
        {
            for (int y = 0; y < room.roomSize; y++)
            {
                if (room.tiles[x, y].tileBase.name.Contains("key"))
                {
                    return true;
                }
            }
        }*/
        return true;
    }

    public  void GenerateCorner(Vector3Int cell, Vector3Int one, Vector3Int two, Vector3Int three, Vector3Int four)
    {
        if (world.GetTile(two) == null && world.GetTile(one) == null && world.GetTile(four) == null && world.GetTile(three) == null)
        {
            Debug.Log("error, all four quadrants null");
        }
        else if (world.GetTile(two) == null && world.GetTile(three) == null && world.GetTile(four) == null)
        {
            world.SetTile(cell, wall_one_corners_one_tile_asset);
        }
        else if (world.GetTile(one) == null && world.GetTile(three) == null && world.GetTile(four) == null)
        {
            world.SetTile(cell, wall_one_corners_two_tile_asset);
        }
        else if (world.GetTile(one) == null && world.GetTile(two) == null && world.GetTile(four) == null)
        {
            world.SetTile(cell, wall_one_corners_three_tile_asset);
        }
        else if (world.GetTile(one) == null && world.GetTile(two) == null && world.GetTile(three) == null)
        {
            world.SetTile(cell, wall_one_corners_four_tile_asset);
        }
        else if (world.GetTile(one) == null && world.GetTile(four) == null)
        {
            world.SetTile(cell, wall_two_corners_left_tile_asset);
        }
        else if (world.GetTile(four) == null && world.GetTile(three) == null)
        {
            world.SetTile(cell, wall_two_corners_up_tile_asset);
        }
        else if (world.GetTile(three) == null && world.GetTile(two) == null)
        {
            world.SetTile(cell, wall_two_corners_right_tile_asset);
        }
        else if (world.GetTile(two) == null && world.GetTile(one) == null)
        {
            world.SetTile(cell, wall_two_corners_down_tile_asset);
        }
        else if (world.GetTile(one) == null)
        {
            world.SetTile(cell, wall_three_corners_three_tile_asset);
        }
        else if (world.GetTile(two) == null)
        {
            world.SetTile(cell, wall_three_corners_four_tile_asset);
        }
        else if (world.GetTile(three) == null)
        {
            world.SetTile(cell, wall_three_corners_one_tile_asset);
        }
        else if (world.GetTile(four) == null)
        {
            world.SetTile(cell, wall_three_corners_two_tile_asset);
        }
        else
        {
            world.SetTile(cell, wall_four_corners_tile_asset);
        }
    }

    public void GenerateLeftWall(Room room, int x, int y)
    {
        Vector3Int cell;
        float numerator = (float)unusedRooms.Count + 1 - totalDoorCount;
        float denominator = (float)(4 * rooms.Count - totalWallCount);
        float doorChance = 100 * numerator/denominator;
        float randomChance = Random.Range(0.0f, 100.0f);
        int chanceTile = Random.Range(room.startY, room.startY + 6);
        bool createdDoor = false;
        while (y < room.startY + 8)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (!createdDoor && doorChance >= randomChance && y == chanceTile)
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
                    totalDoorCount++;
                }
                else
                {
                    if (x == room.startX - 1 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_right_outer_tile_asset);
                    }
                    else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                    {
                        world.SetTile(cell, wall_down_outer_tile_asset);
                    }
                    else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_left_outer_tile_asset);
                    }
                    else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_up_outer_tile_asset);
                    }
                    else if (x == room.startX - 1 || x == room.startX + 7)
                    {
                        world.SetTile(cell, wall_double_vertical_tile_asset);
                    }
                    else if (y == room.startY - 1 || y == room.startY + 7)
                    {
                        world.SetTile(cell, wall_double_horizontal_tile_asset);
                    }
                    else
                    {
                    }
                }
            }
            y++;
        }
    }

    public void GenerateUpWall(Room room, int x, int y)
    {
        Vector3Int cell;
        float numerator = (float)unusedRooms.Count + 1 - totalDoorCount;
        float denominator = (float)(4 * rooms.Count - totalWallCount);
        float doorChance = 100 * numerator / denominator;
        float randomChance = Random.Range(0.0f, 100.0f);
        int chanceTile = Random.Range(room.startX, room.startX + 6);
        bool createdDoor = false;
        while (x < room.startX + 8)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (!createdDoor && doorChance >= randomChance && x == chanceTile)
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
                    totalDoorCount++;
                }
                else
                {
                    if (x == room.startX - 1 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_right_outer_tile_asset);
                    }
                    else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                    {
                        world.SetTile(cell, wall_down_outer_tile_asset);
                    }
                    else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_left_outer_tile_asset);
                    }
                    else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_up_outer_tile_asset);
                    }
                    else if (x == room.startX - 1 || x == room.startX + 7)
                    {
                        world.SetTile(cell, wall_double_vertical_tile_asset);
                    }
                    else if (y == room.startY - 1 || y == room.startY + 7)
                    {
                        world.SetTile(cell, wall_double_horizontal_tile_asset);
                    }
                }
            }
            x++;
        }
    }

    public void GenerateRightWall(Room room, int x, int y)
    {
        Vector3Int cell;
        float numerator = (float)unusedRooms.Count + 1 - totalDoorCount;
        float denominator = (float)(4 * rooms.Count - totalWallCount);
        float doorChance = 100 * numerator / denominator;
        float randomChance = Random.Range(0.0f, 100.0f);
        int chanceTile = Random.Range(room.startY, room.startY + 6);
        bool createdDoor = false;
        while (y > room.startY - 2)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (!createdDoor && doorChance >= randomChance && y == chanceTile)
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
                    totalDoorCount++;
                }
                else
                {
                    if (x == room.startX - 1 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_right_outer_tile_asset);
                    }
                    else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                    {
                        world.SetTile(cell, wall_down_outer_tile_asset);
                    }
                    else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_left_outer_tile_asset);
                    }
                    else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_up_outer_tile_asset);
                    }
                    else if (x == room.startX - 1 || x == room.startX + 7)
                    {
                        world.SetTile(cell, wall_double_vertical_tile_asset);
                    }
                    else if (y == room.startY - 1 || y == room.startY + 7)
                    {
                        world.SetTile(cell, wall_double_horizontal_tile_asset);
                    }
                }
            }
            y--;
        }
    }

    public void GenerateDownWall(Room room, int x, int y)
    {
        Vector3Int cell;
        float numerator = (float)unusedRooms.Count + 1 - totalDoorCount;
        float denominator = (float)(4 * rooms.Count - totalWallCount);
        float doorChance = 100 * numerator / denominator;
        float randomChance = Random.Range(0.0f, 100.0f);
        int chanceTile = Random.Range(room.startX, room.startX + 6);
        bool createdDoor = false;
        while (x > room.startX - 2)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell) == null)
            {
                if (!createdDoor && doorChance >= randomChance && x == chanceTile)
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
                    totalDoorCount++;
                }
                else
                {
                    if (x == room.startX - 1 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY + 7)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX + 7 && y == room.startY - 1)
                    {
                        Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                        Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                        Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                        Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                        GenerateCorner(cell, one, two, three, four);
                    }
                    else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_right_outer_tile_asset);
                    }
                    else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                    {
                        world.SetTile(cell, wall_down_outer_tile_asset);
                    }
                    else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_left_outer_tile_asset);
                    }
                    else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                    {
                        world.SetTile(cell, wall_up_outer_tile_asset);
                    }
                    else if (x == room.startX - 1 || x == room.startX + 7)
                    {
                        world.SetTile(cell, wall_double_vertical_tile_asset);
                    }
                    else if (y == room.startY - 1 || y == room.startY + 7)
                    {
                        world.SetTile(cell, wall_double_horizontal_tile_asset);
                    }
                }
            }
            x--;
        }
    }

    public void RegenerateLeftWall(Room room, int x, int y)
    {
        Vector3Int cell;
        while (y < room.startY + 8)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("wall"))
            {
                if (x == room.startX - 1 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_right_outer_tile_asset);
                }
                else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                {
                    world.SetTile(cell, wall_down_outer_tile_asset);
                }
                else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_left_outer_tile_asset);
                }
                else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                {
                    world.SetTile(cell, wall_up_outer_tile_asset);
                }
                else if (x == room.startX - 1 || x == room.startX + 7)
                {
                    world.SetTile(cell, wall_double_vertical_tile_asset);
                }
                else if (y == room.startY - 1 || y == room.startY + 7)
                {
                    world.SetTile(cell, wall_double_horizontal_tile_asset);
                }
            }
            y++;
        }
    }

    public void RegenerateUpWall(Room room, int x, int y)
    {
        Vector3Int cell;
        while (x < room.startX + 8)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("wall"))
            {
                if (x == room.startX - 1 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_right_outer_tile_asset);
                }
                else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                {
                    world.SetTile(cell, wall_down_outer_tile_asset);
                }
                else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_left_outer_tile_asset);
                }
                else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                {
                    world.SetTile(cell, wall_up_outer_tile_asset);
                }
                else if (x == room.startX - 1 || x == room.startX + 7)
                {
                    world.SetTile(cell, wall_double_vertical_tile_asset);
                }
                else if (y == room.startY - 1 || y == room.startY + 7)
                {
                    world.SetTile(cell, wall_double_horizontal_tile_asset);
                }
            }
            x++;
        }
    }

    public void RegenerateRightWall(Room room, int x, int y)
    {
        Vector3Int cell;
        while (y > room.startY - 2)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("wall"))
            {
                if (x == room.startX - 1 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_right_outer_tile_asset);
                }
                else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                {
                    world.SetTile(cell, wall_down_outer_tile_asset);
                }
                else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_left_outer_tile_asset);
                }
                else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                {
                    world.SetTile(cell, wall_up_outer_tile_asset);
                }
                else if (x == room.startX - 1 || x == room.startX + 7)
                {
                    world.SetTile(cell, wall_double_vertical_tile_asset);
                }
                else if (y == room.startY - 1 || y == room.startY + 7)
                {
                    world.SetTile(cell, wall_double_horizontal_tile_asset);
                }
            }
            y--;
        }
    }

    public void RegenerateDownWall(Room room, int x, int y)
    {
        Vector3Int cell;
        while (x > room.startX - 2)
        {
            cell = world.WorldToCell(new Vector3Int(x, y, zAxis));
            if (world.GetTile(cell).name.Contains("wall"))
            {
                if (x == room.startX - 1 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY + 7)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 10, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 10, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX + 7 && y == room.startY - 1)
                {
                    Vector3Int two = world.WorldToCell(new Vector3(room.startX + 2, room.startY + 2, 0));
                    Vector3Int one = world.WorldToCell(new Vector3(room.startX + 10, room.startY + 2, 0));
                    Vector3Int four = world.WorldToCell(new Vector3(room.startX + 10, room.startY - 2, 0));
                    Vector3Int three = world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0));
                    GenerateCorner(cell, one, two, three, four);
                }
                else if (x == room.startX - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX - 2, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_right_outer_tile_asset);
                }
                else if (y == room.startY + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY + 9, 0))) == null)
                {
                    world.SetTile(cell, wall_down_outer_tile_asset);
                }
                else if (x == room.startX + 7 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 9, room.startY + 2, 0))) == null)
                {
                    world.SetTile(cell, wall_left_outer_tile_asset);
                }
                else if (y == room.startY - 1 && world.GetTile(world.WorldToCell(new Vector3(room.startX + 2, room.startY - 2, 0))) == null)
                {
                    world.SetTile(cell, wall_up_outer_tile_asset);
                }
                else if (x == room.startX - 1 || x == room.startX + 7)
                {
                    world.SetTile(cell, wall_double_vertical_tile_asset);
                }
                else if (y == room.startY - 1 || y == room.startY + 7)
                {
                    world.SetTile(cell, wall_double_horizontal_tile_asset);
                }
            }
            x--;
        }
    }

    public Room AddRoom(GameObject room, int x, int y)
    {
        roomCount++;
        room.GetComponent<Tilemap>().CompressBounds();
        Room newRoom = new Room(this, world, room.GetComponent<Tilemap>(), roomCount, x, y);
        rooms.Add(newRoom);
        GenerateWalls(newRoom);
        unusedRooms.Remove(room);
        totalDoorCount--;
        return newRoom;
    }

    void Awake()
    {
        /// Getting components from other objects to refer to them in this script
        gui = camera.GetComponent<guiScript>();
        cursor = cursorObj.GetComponent<Cursor>();
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        rooms = new List<Room>();
        unusedRooms = new List<GameObject>();
        unusedRooms.Add(room_2);
        unusedRooms.Add(room_3);
        maxRoomCount = unusedRooms.Count + 2;
        firstRoom = AddRoom(room_1, 0, 0);
        world = tileMapWorldObj.GetComponent<Tilemap>();
        turnHandler.Initialize();
        world.CompressBounds();
    }

    void Start()
    {

        /// Initializing lists/arrays by creating new ones.
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
    float leftWall, upWall, rightWall, downWall;
    Vector3Int leftRoom, upRoom, rightRoom, downRoom;
    void Update()
    {
        //Debug.Log(world);
        if (world.GetTile(world.WorldToCell(turnHandler.activePlayer.transform.position)).name.Contains("win"))
         {
             SceneManager.LoadScene("Win_Scene", LoadSceneMode.Single);
         }
        activePlayerPos = new Vector3(Mathf.Round(turnHandler.activePlayer.transform.position.x * 10) / 10, Mathf.Round(turnHandler.activePlayer.transform.position.y * 10) / 10, zAxis);
        leftWall = RoundOffset(turnHandler.activePlayer.prevRoom.startX - 1);
        rightWall = RoundOffset(turnHandler.activePlayer.prevRoom.startX + 7);
        downWall = RoundOffset(turnHandler.activePlayer.prevRoom.startY - 1);
        upWall = RoundOffset(turnHandler.activePlayer.prevRoom.startY + 7);
        if (turnHandler.activePlayer.room != null && !(activePlayerPos.x == leftWall || activePlayerPos.x == rightWall || activePlayerPos.y == downWall || activePlayerPos.y == upWall))
        {
            addingRoom = false;
        }
        else
        {
            turnHandler.activePlayer.room = null;
        }
        if (!addingRoom)
        {
            leftRoom = world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX - 5, turnHandler.activePlayer.prevRoom.startY, zAxis));
            rightRoom = world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX + 10, turnHandler.activePlayer.prevRoom.startY, zAxis));
            downRoom = world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX, turnHandler.activePlayer.prevRoom.startY - 5, zAxis));
            upRoom = world.WorldToCell(new Vector3(turnHandler.activePlayer.prevRoom.startX, turnHandler.activePlayer.prevRoom.startY + 10, zAxis));
        }
        if (!addingRoom && world.GetTile(leftRoom) == null && activePlayerPos.x == RoundOffset(turnHandler.activePlayer.prevRoom.startX - 1))
        {
            addingRoom = true;
            ChooseRandomRoomToSpawnAt(turnHandler.activePlayer.prevRoom.startX - 8, turnHandler.activePlayer.prevRoom.startY);
        }
        else if(!addingRoom &&world.GetTile(rightRoom) == null &&activePlayerPos.x == RoundOffset(turnHandler.activePlayer.prevRoom.startX + 7))
        {
            addingRoom = true;
            ChooseRandomRoomToSpawnAt(turnHandler.activePlayer.prevRoom.startX + 8, turnHandler.activePlayer.prevRoom.startY);
        }
        else if (!addingRoom && world.GetTile(downRoom) == null && activePlayerPos.y == RoundOffset(turnHandler.activePlayer.prevRoom.startY - 1))
        {
            addingRoom = true;
            ChooseRandomRoomToSpawnAt(turnHandler.activePlayer.prevRoom.startX, turnHandler.activePlayer.prevRoom.startY - 8);
        }
        else if (!addingRoom && world.GetTile(upRoom) == null && activePlayerPos.y == RoundOffset(turnHandler.activePlayer.prevRoom.startY + 7))
        {
            addingRoom = true;
            ChooseRandomRoomToSpawnAt(turnHandler.activePlayer.prevRoom.startX, turnHandler.activePlayer.prevRoom.startY + 8);
        }
    }

    public void ChooseRandomRoomToSpawnAt(int x, int y)
    {
        Room newRoom;
        if (unusedRooms.Count == 0)
        {
            newRoom = AddRoom(room_4, x, y);
        }
        else
        {
            int index = Random.Range(0, unusedRooms.Count);
            newRoom = AddRoom(unusedRooms[index], x, y);
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
        else
        {
            foreach (Player player in turnHandler.playerList)
            {
                if (world.WorldToCell(player.transform.position) == goal)
                {
                    return false;
                }
            }
            if (turnHandler.activePlayer.room != null)
            {
                foreach (GameObject enemy in turnHandler.activePlayer.room.enemies)
                {
                    if (world.WorldToCell(enemy.transform.position) == goal)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void HighlightNeighbors(Vector3Int start)
    {
        Vector3Int left = new Vector3Int(start.x - 1, start.y, 0);
        Vector3Int leftUp = new Vector3Int(start.x - 1, start.y + 1, 0);
        Vector3Int leftDown = new Vector3Int(start.x - 1, start.y - 1, 0);
        Vector3Int right = new Vector3Int(start.x + 1, start.y, 0);
        Vector3Int rightUp = new Vector3Int(start.x + 1, start.y + 1, 0);
        Vector3Int rightDown = new Vector3Int(start.x + 1, start.y - 1, 0);
        Vector3Int up = new Vector3Int(start.x, start.y + 1, 0);
        Vector3Int down = new Vector3Int(start.x, start.y - 1, 0);
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
        if (CheckTile(start, up))
        {
            possibleTiles[2] = up;
            highlighter.SetTile(up, floor_tile_asset);
            highlighter.SetTileFlags(up, TileFlags.None);
            highlighter.SetColor(up, color);
        }
        if (CheckTile(start, down))
        {
            possibleTiles[3] = down;
            highlighter.SetTile(down, floor_tile_asset);
            highlighter.SetTileFlags(down, TileFlags.None);
            highlighter.SetColor(down, color);
        }
        if (CheckTile(start, leftUp))
        {
            possibleTiles[4] = leftUp;
            highlighter.SetTile(leftUp, floor_tile_asset);
            highlighter.SetTileFlags(leftUp, TileFlags.None);
            highlighter.SetColor(leftUp, color);
        }
        if (CheckTile(start, leftDown))
        {
            possibleTiles[5] = leftDown;
            highlighter.SetTile(leftDown, floor_tile_asset);
            highlighter.SetTileFlags(leftDown, TileFlags.None);
            highlighter.SetColor(leftDown, color);
        }
        if (CheckTile(start, rightUp))
        {
            possibleTiles[6] = rightUp;
            highlighter.SetTile(rightUp, floor_tile_asset);
            highlighter.SetTileFlags(rightUp, TileFlags.None);
            highlighter.SetColor(rightUp, color);
        }
        if (CheckTile(start, rightDown))
        {
            possibleTiles[7] = rightDown;
            highlighter.SetTile(rightDown, floor_tile_asset);
            highlighter.SetTileFlags(rightDown, TileFlags.None);
            highlighter.SetColor(rightDown, color);
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
}