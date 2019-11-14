using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DespawnedEnemy
{
    public string name;
    public int health;
    public DespawnedEnemy(string _name, int _health)
    {
        name = _name;
        health = _health;
    }
}

public abstract class IEnemy
{
    public GameObject turnHandlerObj;
    GameObject worldObj;
    public int health;
    public GameObject obj;
    World world;
    List<Player> playerList;
    public Room room;
    public bool awaitMovement;
    public bool turnStarted;
    public bool attacked;
    public int moves;
    public int prevMoves;
    public float range;
    public List<Tile> path;
    public int tier;

    public bool moving;
    float startTime;
    Vector3 destination;
    float totalDistance;
    public float playerSpeed;
    int zAxis = 0;

    public IEnemy(GameObject _obj)
    {
        health = 0;
        obj = _obj;
        playerSpeed = 1.0f;
        attacked = false;
        awaitMovement = false;
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        worldObj = GameObject.FindGameObjectWithTag("World");
        world = worldObj.GetComponent<World>();
    }

    public abstract void PrimaryAttack();
    public abstract void SecondaryAttack();
    public abstract void SpecialAttack();

    public List<Tile> FindPathToNearestPlayer()
    {
        //world = world.world;
        //world = new Tile[2,2];

        /// setting all parents to null before creating paths because otherwise infinite loop problems occur.

        for (int x = 0; x < room.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < room.tiles.GetLength(1); y++)
            {
                room.tiles[x, y].parent = null;
            }
        }
        playerList = turnHandlerObj.GetComponent<Turn_Handler>().playerList;
        List<Tile> closestPath = null;
        List<Tile> currentPath = new List<Tile>();
        foreach (Player player in playerList)
        {
            if (player.room == null || room.number != player.room.number)
            {
                continue;
            }
            currentPath.Clear();
            Vector3Int start = world.world.WorldToCell(obj.transform.position);
            Vector3Int goal = world.world.WorldToCell(player.transform.position);
            start = new Vector3Int(start.x + room.startX - 1, start.y + room.startY - 1, start.z);
            goal = new Vector3Int(goal.x + room.startX - 1, goal.y + room.startY - 1, goal.z);
            if (room.tiles[start.x, start.y].room == room.tiles[goal.x, goal.y].room)
            {
                if (BuildPathAStar(room.tiles[start.x, start.y], room.tiles[goal.x, goal.y]))
                {
                    Tile temp = room.tiles[goal.x, goal.y];
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
                    closestPath = new List<Tile>(currentPath);
                }
                else if (closestPath.Count > currentPath.Count)
                {
                    closestPath.Clear();
                    closestPath = new List<Tile>(currentPath);
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
        destination = closestPath[0].position;
        destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
        if (obj.transform.position == destination)
        {
            closestPath[0].parent = null;
            closestPath.RemoveAt(0);
        }
        if (InRange(closestPath, range))
        {
            return null;
        }
        return closestPath;
    }

    public int GetHeuristic(Tile start, Tile goal)
    {
        return (int)Vector3.Distance(start.position, goal.position);
    }

    public bool UpdateF(Tile current, List<Tile> open, List<Tile> closed, Tile goal)
    {
        Tile previous = closed[closed.Count - 1];
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

    void PopulateAdjacentArray(List<Tile> adjacent, Tile tile, Room tRoom)
    {
        Tile neighbor;
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
    public bool BuildPathAStar(Tile start, Tile goal)
    {
        List<Tile> open = new List<Tile>();
        List<Tile> closed = new List<Tile>();
        open.Add(start);
        start.parent = null;
        start.g = 0;
        bool foundGoal = false;
        bool firstIteration = true;
        List<Tile> adjacent = new List<Tile>();
        while (open.Count > 0)
        {
            if (open[0] == goal)
            {
                foundGoal = true;
                break;
            }
            closed.Add(open[0]);
            open.RemoveAt(0);
            PopulateAdjacentArray(adjacent, closed[closed.Count - 1], room); //firstRoom

            foreach (Tile tile in adjacent)
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

    public void StartTurn()
    {
        turnStarted = true;
    }

    public void EndTurn()
    {
        moving = false;
        awaitMovement = false;
        if (path != null)
        {
            path.Clear();
        }
        turnHandlerObj.GetComponent<Turn_Handler>().enemyTurn = false;
        turnHandlerObj.GetComponent<Turn_Handler>().playerTurn = true;
    }

    public int MoveAlongPath(List<Tile> path, float range, int moves)
    {
        if (path == null)
        {
            EndTurn();
            return 0;
        }
        if (path.Count > 0 && !moving)
        {
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
                EndTurn();
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
                    destination = path[0].position;
                    totalDistance = Mathf.Abs(RoundOffset(obj.transform.position.x) - destination.x) + Mathf.Abs(RoundOffset(obj.transform.position.x) - destination.y);
                    return moves;
                }
                else
                {
                    EndTurn();
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

    public bool InRange(List<Tile> path, float range)
    {
        return path.Count <= range;
    }

    public void UpdateRoom()
    {
        bool pleaseExit = false;
        for (int j = 0; j < world.rooms.Count; j++)
        {
            if (pleaseExit)
            {
                break;
            }
            for (int x = 0; x < world.rooms[j].roomSize; x++)
            {
                if (pleaseExit)
                {
                    break;
                }
                for (int y = 0; y < world.rooms[j].roomSize; y++)
                {
                    if (new Vector3(RoundOffset(world.rooms[j].tiles[x, y].position.x), RoundOffset(world.rooms[j].tiles[x, y].position.y), zAxis) == obj.transform.position)
                    {
                        //Debug.Log("room is updated");
                        room = world.rooms[j];
                        pleaseExit = true;
                        break;
                    }
                }
            }
        }
    }

}

public class Enemies : MonoBehaviour
{
    public GameObject thrasher;
    public List<GameObject> tierOneEnemies, tierTwoEnemies, tierThreeEnemies, tierFourEnemies;

    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;

    void Awake()
    {
        tierTwoEnemies.Add(thrasher);
        //enemies = new List<GameObject>();
        //activeEnemy = null;
        //tierOneEnemies.Add("Maggot");
        //tierTwoEnemies.Add("Thrasher");
        /*foreach (Transform child in transform)
        {
            enemies.Add(new Thrasher(child.gameObject));
            //Debug.Log("child gameobject = " + child.gameObject);
            child.position = new Vector3(RoundOffset(child.position.x), RoundOffset(child.position.y), child.position.z);
        }*/
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