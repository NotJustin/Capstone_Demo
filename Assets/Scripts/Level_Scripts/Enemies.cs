using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class IEnemy
{
    public GameObject turnHandlerObj;
    GameObject tileWorldObj;
    public int health;
    public GameObject obj;
    TileWorld tileWorld;
    List<Player> playerList;
    public TileRoom tileRoom;
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
        tileWorldObj = GameObject.FindGameObjectWithTag("TileWorld");
        tileWorld = tileWorldObj.GetComponent<TileWorld>();
    }
    public abstract void AttackOne();
    public abstract void AttackTwo();
    public abstract void AttackThree();

    public List<TileClass> FindPathToNearestPlayer()
    {
        //world = tileWorld.world;
        //world = new TileClass[2,2];

        /// setting all parents to null before creating paths because otherwise infinite loop problems occur.
        for (int x = 0; x < tileRoom.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tileRoom.tiles.GetLength(1); y++)
            {
                tileRoom.tiles[x, y].parent = null;
            }
        }
        playerList = turnHandlerObj.GetComponent<Turn_Handler>().playerList;
        List<TileClass> closestPath = null;
        List<TileClass> currentPath = new List<TileClass>();
        foreach (Player player in playerList)
        {
            currentPath.Clear();
            Vector3Int start = tileWorld.world.WorldToCell(obj.transform.position);
            Vector3Int goal = tileWorld.world.WorldToCell(player.transform.position);
            start = new Vector3Int(start.x + tileRoom.startX - 1, start.y + tileRoom.startY - 1, start.z);
            goal = new Vector3Int(goal.x + tileRoom.startX - 1, goal.y + tileRoom.startY - 1, goal.z);
            Debug.Log(start);
            Debug.Log(goal);
            if (tileRoom.tiles[start.x, start.y].room == tileRoom.tiles[goal.x, goal.y].room)
            {
                if (BuildPathAStar(tileRoom.tiles[start.x, start.y], tileRoom.tiles[goal.x, goal.y]))
                {
                    TileClass temp = tileRoom.tiles[goal.x, goal.y];
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
            if (closestPath[i].position == new Vector3(obj.transform.position.x + 0.5f, obj.transform.position.y + 0.5f, zAxis))
            {
                Debug.Log("removing shit");
                if (closestPath[i].parent != null)
                {
                    closestPath[i].parent = null;
                }
                closestPath.RemoveAt(i);
            }
        }
        Debug.Log(closestPath.Count);
        return closestPath;
    }

    public int GetHeuristic(TileClass start, TileClass goal)
    {
        return (int)Vector3.Distance(start.position, goal.position);
    }

    public bool UpdateF(TileClass current, List<TileClass> open, List<TileClass> closed, TileClass goal)
    {
        TileClass previous = closed[closed.Count - 1];
        if (!closed.Contains(current))
        {
            if (!open.Contains(current))
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

    void PopulateAdjacentArray(List<TileClass> adjacent, TileClass tile, TileRoom tRoom)
    {
        TileClass neighbor;
        if (tile.cell.x > 1 && tile.cell.y > 0)
        {
            neighbor = tRoom.tiles[tile.cell.x - 2, tile.cell.y - 1];
            if (!neighbor.tileBase.name.Contains("wall"))
            {
                adjacent.Add(neighbor);
            }
        }
        if (tile.cell.x < tRoom.startX + 7 && tile.cell.y > 0)
        {
            neighbor = tRoom.tiles[tile.cell.x, tile.cell.y - 1];
            if (!neighbor.tileBase.name.Contains("wall"))
            {
                adjacent.Add(neighbor);
            }
        }
        if (tile.cell.x > 0 && tile.cell.y < tRoom.startY + 7)
        {
            neighbor = tRoom.tiles[tile.cell.x - 1, tile.cell.y];
            if (!neighbor.tileBase.name.Contains("wall"))
            {
                adjacent.Add(neighbor);
            }
        }
        if (tile.cell.x > 0 && tile.cell.y > 1)
        {
            neighbor = tRoom.tiles[tile.cell.x - 1, tile.cell.y - 2];
            if (!neighbor.tileBase.name.Contains("wall"))
            {
                adjacent.Add(neighbor);
            }
        }
    }
    public bool BuildPathAStar(TileClass start, TileClass goal)
    {
        List<TileClass> open = new List<TileClass>();
        List<TileClass> closed = new List<TileClass>();
        open.Add(start);
        start.parent = null;
        start.g = 0;
        bool foundGoal = false;
        bool firstIteration = true;
        List<TileClass> adjacent = new List<TileClass>();
        while (open.Count > 0)
        {
            if (open[0] == goal)
            {
                foundGoal = true;
                break;
            }
            closed.Add(open[0]);
            open.RemoveAt(0);
            PopulateAdjacentArray(adjacent, closed[closed.Count - 1], tileRoom); //firstRoom

            foreach (TileClass tile in adjacent)
            {
                if (firstIteration)
                {
                    open.Add(tile);
                    firstIteration = false;
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

    public bool moving;
    float startTime;
    Vector3 destination;
    float totalDistance;
    public float playerSpeed;
    int zAxis = 0;
    public int MoveAlongPath(List<TileClass> path, float range, int moves)
    {
        Debug.Log("moving along path");
        if (path == null)
        {
            Debug.Log("null path");
            moving = false;
            awaitMovement = false;
            startedMoving = false;
            turnHandlerObj.GetComponent<Turn_Handler>().changeTurn = true;
            return 0;
        }
        if (path.Count > 0 && !moving)
        {
            Debug.Log("getting ready to move to next tile");
            destination = path[0].position;
            destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
            moving = true;
            startTime = Time.time;
            if (obj.transform.position == destination)
            {
                Debug.Log("fixing problem");
                path[0].parent = null;
                path.RemoveAt(0);
                destination = path[0].position;
                destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
            }
            totalDistance = Mathf.Abs(obj.transform.position.x - destination.x) + Mathf.Abs(obj.transform.position.y - destination.y);
            if (path.Count > 0 && InRange(path, range))
            {
                Debug.Log("already in range");
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
            Debug.Log("actually moving");
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
                    destination = path[0].position;
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

    public void UpdateRoom()
    {
        bool pleaseExit = false;
        for (int j = 0; j < tileWorld.rooms.Count; j++)
        {
            if (pleaseExit)
            {
                break;
            }
            for (int x = 0; x < tileWorld.rooms[j].roomSize; x++)
            {
                if (pleaseExit)
                {
                    break;
                }
                for (int y = 0; y < tileWorld.rooms[j].roomSize; y++)
                {
                    if (new Vector3(RoundOffset(tileWorld.rooms[j].tiles[x, y].position.x), RoundOffset(tileWorld.rooms[j].tiles[x, y].position.y), zAxis) == obj.transform.position)
                    {
                        //Debug.Log("room is updated");
                        tileRoom = tileWorld.rooms[j];
                        pleaseExit = true;
                        break;
                    }
                }
            }
        }
    }

}

public class Thrasher : IEnemy
{
    public Thrasher(int health, GameObject obj) : base(health, obj)
    {

    }
    public override void AttackOne()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 1.0f;
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
    public override void AttackTwo()
    {
        
    }
    public override void AttackThree()
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