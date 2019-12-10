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
    public Turn_Handler turnHandler;
    GameObject worldObj;
    public int health;
    public int armor;
    public int attack;
    public GameObject obj;
    World world;
    public Room room;
    public bool awaitMovement;
    public bool turnStarted;
    public bool attacked;
    public int moves;
    public int prevMoves;
    public float range;
    public int damage;
    public List<Tile> path;
    public int tier;
    public string text;

    public bool moving;
    float startTime;
    Vector3 destination;
    float totalDistance;
    public float playerSpeed;
    int zAxis = 0;

    public Animator animator;
    public IEnemy(GameObject _obj)
    {
        health = 0;
        obj = _obj;
        playerSpeed = 1.0f;
        attacked = false;
        awaitMovement = false;
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        worldObj = GameObject.FindGameObjectWithTag("TileWorld");
        world = worldObj.GetComponent<World>();
    }

    public abstract void PrimaryAttack();

    public List<Tile> FindPathToNearestPlayer()
    {

        /// setting all parents to null before creating paths because otherwise infinite loop problems occur.

        for (int x = 0; x < room.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < room.tiles.GetLength(1); y++)
            {
                room.tiles[x, y].parent = null;
            }
        }
        Turn_Handler turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        List<Tile> closestPath = null;
        List<Tile> currentPath = new List<Tile>();
        foreach (Player player in turnHandler.playerList)
        {
            if (player.room == null || room.number != player.room.number)
            {
                continue;
            }
            currentPath.Clear();
            Vector3Int start = world.world.WorldToCell(obj.transform.position);
            Vector3Int goal = world.world.WorldToCell(player.transform.position);
            start = new Vector3Int(start.x - room.startX, start.y - room.startY, start.z);
            goal = new Vector3Int(goal.x - room.startX, goal.y - room.startY, goal.z);
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
        /*for (int i = 0; i < closestPath.Count; i++)
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
        }*/
        destination = closestPath[0].position;
        destination = new Vector3(RoundOffset(destination.x), RoundOffset(destination.y), destination.z);
        if (obj.transform.position == destination)
        {
            closestPath[0].parent = null;
            closestPath.RemoveAt(0);
        }
        /*if (InRange(closestPath, range))
        {
            return null;
        }*/
        return closestPath;
    }

    public int GetHeuristic(Tile start, Tile goal)
    {
        return (int)Mathf.Max(Mathf.Abs(start.position.x - goal.position.x), Mathf.Abs(start.position.y - goal.position.y));
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

    public bool DoesTileHaveEnemy(Vector3Int neighbor)
    {
        foreach (GameObject enemy in room.enemies)
        {
            if (world.world.WorldToCell(enemy.transform.position) == neighbor)
            {
                return true;
            }
        }
        return false;
    }
    void PopulateAdjacentArray(List<Tile> adjacent, Tile tile)
    {
        TileBase neighbor;
        Vector3Int neighborCell = new Vector3Int(tile.cell.x - 1, tile.cell.y, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x > tile.room.startX && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x - 1 - tile.room.startX, tile.cell.y - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x + 1, tile.cell.y, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x < tile.room.startX + 6 && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x + 1 - tile.room.startX, tile.cell.y - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x, tile.cell.y + 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.y < tile.room.startY + 6 && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x - tile.room.startX, tile.cell.y + 1 - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x, tile.cell.y - 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.y > tile.room.startY && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x - tile.room.startX, tile.cell.y - 1 - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x - 1, tile.cell.y - 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x > tile.room.startX && tile.cell.y > tile.room.startY && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x - 1 - tile.room.startX, tile.cell.y - 1 - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x + 1, tile.cell.y - 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x < tile.room.startX + 6 && tile.cell.y > tile.room.startY && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x + 1 - tile.room.startX, tile.cell.y - 1 - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x - 1, tile.cell.y + 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x > tile.room.startX && tile.cell.y < tile.room.startY + 6 && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x - 1 - tile.room.startX, tile.cell.y + 1 - tile.room.startY]);
        }
        neighborCell = new Vector3Int(tile.cell.x + 1, tile.cell.y + 1, 0);
        neighbor = world.world.GetTile(neighborCell);
        if (tile.cell.x < tile.room.startX + 6 && tile.cell.y < tile.room.startY + 6 && neighbor != null && !neighbor.name.Contains("wall") && !DoesTileHaveEnemy(neighborCell))
        {
            adjacent.Add(tile.room.tiles[tile.cell.x + 1 - tile.room.startX, tile.cell.y + 1 - tile.room.startY]);
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
            PopulateAdjacentArray(adjacent, closed[closed.Count - 1]); //firstRoom

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

    public Player GetPlayerAtDestination()
    {
        for (int i = 0; i < turnHandler.playerList.Count; i++)
        {
            if (turnHandler.playerList[i] != null)
            {
                Vector3Int playerCell = new Vector3Int((int)RoundOffset(turnHandler.playerList[i].transform.position.x), (int)RoundOffset(turnHandler.playerList[i].transform.position.y), 0);
                if (playerCell == path[path.Count - 1].cell)
                {
                    return turnHandler.playerList[i];
                }
            }
        }
        return null;
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
                        room = world.rooms[j];
                        pleaseExit = true;
                        break;
                    }
                }
            }
        }
    }

    public bool IsPlayerInLineOfSight(Player player)
    {
        if (player == null || player.room != room)
        {
            return false;
        }
        Vector2 l1p1 = new Vector2(RoundOffset(obj.transform.position.x), RoundOffset(obj.transform.position.y));
        Vector2 l1p2 = new Vector2(RoundOffset(player.transform.position.x), RoundOffset(player.transform.position.y));
        int xMin = (int)(obj.transform.position.x < player.transform.position.x ? obj.transform.position.x : player.transform.position.x);
        int yMin = (int)(obj.transform.position.y < player.transform.position.y ? obj.transform.position.y : player.transform.position.y);
        int xMax = (int)(obj.transform.position.x > player.transform.position.x ? obj.transform.position.x : player.transform.position.x);
        int yMax = (int)(obj.transform.position.y > player.transform.position.y ? obj.transform.position.y : player.transform.position.y);
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (room.tiles[x - room.startX, y - room.startY].type == -1)
                {
                    if (DoLinesIntersect(l1p1, l1p2, new Vector2(x - 0.5f, y - 0.5f), new Vector2(x - 0.5f, y + 0.5f)))
                    {
                        return false;
                    }
                    else if (DoLinesIntersect(l1p1, l1p2, new Vector2(x - 0.5f, y - 0.5f), new Vector2(x + 0.5f, y - 0.5f)))
                    {
                        return false;
                    }
                    else if (DoLinesIntersect(l1p1, l1p2, new Vector2(x + 0.5f, y + 0.5f), new Vector2(x + 0.5f, y - 0.5f)))
                    {
                        return false;
                    }
                    else if (DoLinesIntersect(l1p1, l1p2, new Vector2(x + 0.5f, y + 0.5f), new Vector2(x - 0.5f, y + 0.5f)))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool DoLinesIntersect(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
    {
        Vector2 a = l1p2 - l1p1, b = l2p1 - l2p2, c = l1p1 - l2p1;
        float alphaNumerator = b.y * c.x - b.x * c.y, betaNumerator = a.x * c.y - a.y * c.x, denominator = a.y * b.x - a.x * b.y;
        if (denominator == 0) return false;
        else if (denominator > 0 && (alphaNumerator < 0 || alphaNumerator > denominator || betaNumerator < 0 || betaNumerator > denominator)) return false;
        else if (alphaNumerator > 0 || alphaNumerator < denominator || betaNumerator > 0 || betaNumerator < denominator) return false;
        return true;
    }

    public bool InRange2(Vector3 start, Vector3 goal)
    {
        return (int)(Mathf.Max(Mathf.Abs(start.x - goal.x), Mathf.Abs(start.y - goal.y))) <= range;
    }
}

public class Enemies : MonoBehaviour
{
    public GameObject scuttler;
    public GameObject slinger;
    public GameObject carapace;
    public GameObject peacekeeper;
    public GameObject mauler;
    public GameObject tendrils;
    public GameObject ravager;

    public List<GameObject> tierOneEnemies, tierTwoEnemies, tierThreeEnemies, tierFourEnemies;

    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;

    void Awake()
    {
        tierOneEnemies.Add(scuttler);
        tierTwoEnemies.Add(scuttler);
        tierThreeEnemies.Add(scuttler);
        tierFourEnemies.Add(scuttler);

        tierOneEnemies.Add(slinger);
        tierTwoEnemies.Add(slinger);
        tierThreeEnemies.Add(slinger);
        tierFourEnemies.Add(slinger);

        tierThreeEnemies.Add(carapace);
        tierFourEnemies.Add(carapace);

        tierFourEnemies.Add(peacekeeper);

        tierThreeEnemies.Add(mauler);
        tierFourEnemies.Add(mauler);

        tierThreeEnemies.Add(tendrils);


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