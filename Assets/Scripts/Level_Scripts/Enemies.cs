using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class IEnemy
{
    public GameObject turnHandlerObj;
    GameObject tileSelectorObj;
    public int health;
    public GameObject obj;
    public Tilemap tileMap;
    Tile_Selector_Script tileSelectorScript;
    List<Player> playerList;
    TileClass[,] world;
    public bool awaitMovement;
    public bool startedMoving;
    public bool attacked;
    public int moves;
    public int prevMoves;
    public float range;
    public List<TileClass> path;
    public IEnemy(int _health, GameObject _obj)
    {
        health = _health;
        obj = _obj;
        playerSpeed = 1.0f;
        attacked = false;
        awaitMovement = false;
        startedMoving = false;
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        tileSelectorObj = GameObject.FindGameObjectWithTag("TileSelector");
        tileSelectorScript = tileSelectorObj.GetComponent<Tile_Selector_Script>();
    }
    public virtual void AttackOne()
    {
        Debug.Log("default attack one");
    }
    public virtual void AttackTwo()
    {

    }
    public virtual void AttackThree()
    {

    }

    public List<TileClass> FindPathToNearestPlayer()
    {
        //Debug.Log("Finding nearest path");
        tileMap = tileSelectorScript.tileMap;
        world = tileSelectorScript.world;

        /// setting all parents to null before creating paths because otherwise infinite loop problems occur.
        for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                world[x, y].parent = null;
            }
        }
        playerList = turnHandlerObj.GetComponent<Turn_Handler>().playerList;
        List<TileClass> closestPath = null;
        List<TileClass> currentPath = new List<TileClass>();
        foreach (Player player in playerList)
        {
            currentPath.Clear();
            Vector3Int start = tileMap.WorldToCell(obj.transform.position);
            Vector3Int goal = tileMap.WorldToCell(player.transform.position);
            if (world[start.x, start.y].room == world[goal.x, goal.y].room)
            {
                if (tileSelectorScript.BuildPathAStar(world[start.x, start.y], world[goal.x, goal.y]))
                {
                    TileClass temp = world[goal.x, goal.y];
                    while (temp.parent != null)
                    {
                        currentPath.Add(temp);
                        temp = temp.parent;
                    }
                    if (temp.parent == null)
                    {
                        currentPath.Add(temp);
                    }
                    currentPath.Reverse();
                }
                else
                {
                    continue;
                }
                if (closestPath == null)
                {
                    closestPath = new List<TileClass>(currentPath);
                }
                else if (closestPath.Count > currentPath.Count)
                {
                    closestPath.Clear();
                    closestPath = new List<TileClass>(currentPath);
                }
            }
        }
        if (closestPath == null)
        {
            currentPath.Clear();
            currentPath = null;
            return null;
        }
        currentPath.Clear();
        currentPath = null;
        for (int i = 0; i < closestPath.Count; i++)
        {
            if (closestPath[i].coordinate == tileMap.WorldToCell(new Vector3(obj.transform.position.x + 0.5f, obj.transform.position.y + 0.5f, zAxis)))
            {
                Debug.Log("removing shit");
                if (closestPath[i].parent != null)
                {
                    closestPath[i].parent = null;
                }
                closestPath.RemoveAt(i);
            }
        }
        return closestPath;
    }

    public bool moving;
    float startTime;
    Vector3 destination;
    float totalDistance;
    public float playerSpeed;
    float zAxis = 0;
    public int MoveAlongPath(List<TileClass> path, float range, int moves)
    {
        if (path == null)
        {
            moving = false;
            awaitMovement = false;
            startedMoving = false;
            turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
            return 0;
        }
        if (path.Count > 0 && !moving)
        {
            destination = tileMap.CellToWorld(path[0].coordinate);
            destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
            moving = true;
            startTime = Time.time;
            if (obj.transform.position == destination)
            {
                path[0].parent = null;
                path.RemoveAt(0);
                destination = tileMap.CellToWorld(path[0].coordinate);
                destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
            }
            totalDistance = Mathf.Abs(obj.transform.position.x - destination.x) + Mathf.Abs(obj.transform.position.y - destination.y);
            if (path.Count > 0 && InRange(path, range))
            {
                moving = false;
                awaitMovement = false;
                startedMoving = false;
                turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
                return 0;
            }
            return moves;
        }
        else
        {
            float distanceCovered, fractionOfJourney;
            if (obj.transform.position != destination)
            {
                distanceCovered = (Time.time - startTime) * playerSpeed;
                fractionOfJourney = distanceCovered / totalDistance;
                obj.transform.position = Vector3.Lerp(obj.transform.position, destination, fractionOfJourney);
                return moves;
            }
            else
            {
                moving = false;
                moves--;
                if (path.Count > 0)
                {
                    path.RemoveAt(0);
                }
                if (!(path.Count == 0 || moves == 0))
                {
                    startTime = Time.time;
                    destination = new Vector3(path[0].coordinate.x, path[0].coordinate.y, zAxis);
                    totalDistance = Mathf.Abs(RoundOffset(obj.transform.position.x) - destination.x) + Mathf.Abs(RoundOffset(obj.transform.position.x) - destination.y);
                    return moves;
                }
                else
                {
                    awaitMovement = false;
                    startedMoving = false;
                    turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
                    path.Clear();
                    return 0;
                }
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

    public bool InRange(List<TileClass> path, float range)
    {
        return path.Count <= range;
    }
}

public class Thrasher : IEnemy
{
    public Thrasher(int health, GameObject obj) : base(health, obj)
    {

    }
    public override void AttackOne()
    {
        if (!attacked)
        {
            Debug.Log("attacking");
            range = 3.0f;
            attacked = true;
        }
        if (!startedMoving)
        {
            awaitMovement = true;
            startedMoving = true;
            moves = 2;
            prevMoves = 2;
            path = FindPathToNearestPlayer();
            MoveAlongPath(path, range, moves);
        }
        else
        {
            moves = MoveAlongPath(path, range, moves);
        }
        if (moves == 0)
        {
            prevMoves = 2;
            moves = 2;
            path.Clear();
        }
    }
    new public void AttackTwo()
    {

    }
    new public void AttackThree()
    {

    }
}

public class Enemies : MonoBehaviour
{
    public List<IEnemy> enemies;

    public IEnemy activeEnemy;

    void Awake()
    {
        enemies = new List<IEnemy>();
        activeEnemy = null;
        foreach (Transform child in transform)
        {
            enemies.Add(new Thrasher(3, child.gameObject));
            //Debug.Log("child gameobject = " + child.gameObject);
            child.position = new Vector3(RoundOffset(child.position.x), RoundOffset(child.position.y), child.position.z);
        }
    }

    void Start()
    {

    }

    void Update()
    {

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
}