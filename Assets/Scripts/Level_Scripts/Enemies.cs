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
    List<TileClass> path;
    public int moves;
    public float range;
    public IEnemy(int _health, GameObject _obj)
    {
        health = _health;
        obj = _obj;
        playerSpeed = 1.0f;
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
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        tileSelectorObj = GameObject.FindGameObjectWithTag("TileSelector");
        tileSelectorScript = tileSelectorObj.GetComponent<Tile_Selector_Script>();
        tileMap = tileSelectorScript.tileMap;
        world = tileSelectorScript.world;
        List<TileClass> closestPath = null;
        List<TileClass> currentPath = new List<TileClass>();
        playerList = turnHandlerObj.GetComponent<Turn_Handler>().playerList;
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
                    return null;
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
            return null;
        }
        path = new List<TileClass>(closestPath);
        return closestPath;
    }

    public bool moving;
    float startTime;
    Vector3 destination;
    float totalDistance;
    public float playerSpeed;
    float zAxis = 0;
    bool weirdFix = true;
    public void MoveAlongPath(float range)
    {
        if (path == null)
        {
            Debug.Log("no path exists");
            moving = false;
            awaitMovement = false;
            turnHandlerObj.GetComponent<Turn_Handler>().enemyTurn = false;
            turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
            return;
        }
        if (path.Count > 0 && !moving)
        {
            if (weirdFix)
            {
                weirdFix = false;
                destination = new Vector3(RoundOffset(path[0].coordinate.x), RoundOffset(path[0].coordinate.y), zAxis);
            }
            else
            {
                path.RemoveAt(0);
                destination = new Vector3(RoundOffset(path[0].coordinate.x), RoundOffset(path[0].coordinate.y), zAxis);
            }
            moving = true;
            startTime = Time.time;
            totalDistance = Mathf.Abs(obj.transform.position.x - destination.x) + Mathf.Abs(obj.transform.position.y - destination.y);
        }
        else if(path.Count > 0 && InRange(range, obj.transform.position, tileMap.CellToWorld(path[path.Count - 1].coordinate)))
        {
            moving = false;
            awaitMovement = false;
            turnHandlerObj.GetComponent<Turn_Handler>().enemyTurn = false;
            turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
        }
        else
        {
            float distanceCovered, fractionOfJourney;
            if (obj.transform.position != destination)
            {
                distanceCovered = (Time.time - startTime) * playerSpeed;
                fractionOfJourney = distanceCovered / totalDistance;
                obj.transform.position = Vector3.Lerp(obj.transform.position, destination, fractionOfJourney);
            }
            else
            {
                moves--;
                if (path.Count > 0)
                {
                    path.RemoveAt(0);
                }
                if (!(path.Count == 0 || moves == 0))
                {
                    startTime = Time.time;
                    destination = new Vector3(RoundOffset(path[0].coordinate.x), RoundOffset(path[0].coordinate.y), zAxis);
                    totalDistance = Vector3.Distance(obj.transform.position, destination);
                }
                else
                {
                    //weirdFix = true;
                    moving = false;
                    awaitMovement = false;
                    turnHandlerObj.GetComponent<Turn_Handler>().enemyTurn = false;
                    turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
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

    public bool InRange(float range, Vector3 start, Vector3 goal)
    {
        return Mathf.Abs((start.x - 0.5f) - goal.x) + Mathf.Abs((start.y - 0.5f) - goal.y) <= range;
    }
}

public class Thrasher : IEnemy
{
    public Thrasher(int health, GameObject obj) : base(health, obj)
    {

    }
    public override void AttackOne()
    {
        Debug.Log("Starting Attack One");
        awaitMovement = true;
        moves = 4;
        range = 1.0f;
        List<TileClass> path = FindPathToNearestPlayer();
        MoveAlongPath(range);
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
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}