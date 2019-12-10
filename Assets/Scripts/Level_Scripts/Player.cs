using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    /// This script corrects the player's initial position to snap to the grid
    /// and stores the maxMoves/moves that the player has so that other scripts may view and change the numbers as fit.
    int zAxis = 0;
    public Animator animator;
    public Vector3 lastPosition;
    GameObject worldObj;
    public World world;

    public bool finished;
    public bool attacked;

    public GameObject[] cards;

    public int maxMoves = 2;
    public int moves = 2;
    public int health;
    public int armor;
    public int energyMax;
    public int energy;
    public int generation;
    public int charge;
    public int attack;

    public Sprite sprite;

    public float playerSpeed = 0.25f;
    public int pendingMoves = 0;
    public bool moving = false;
    public bool started = false;
    public int range = 3;

    public Vector3Int start;

    public Vector3 destination;
    public float startTime;
    public float totalDistance;

    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;

    public Room room;
    public Room prevRoom;

    public List<Vector3Int> path;

    public GameObject selectedEnemy;

    public bool decreaseWeaponCost;
    public int decreaseWeaponCostBy;

    public bool turnStarted = false;
    void Awake()
    {
        cards = new GameObject[transform.GetChild(0).transform.childCount];
        for (int i = 0; i < transform.GetChild(0).transform.childCount; i++)
        {
            cards[i] = transform.GetChild(0).GetChild(i).gameObject;
        }
        worldObj = GameObject.FindGameObjectWithTag("TileWorld");
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        world = worldObj.GetComponent<World>();
        animator = GetComponent<Animator>();
        lastPosition = new Vector3(-1, -1, 0);
        finished = false;
        attacked = false;
    }

    //bool updating = false;

    void Update()
    {
        if (turnHandler.activePlayer == this && turnHandler.playerTurn)
        {
            if (turnHandler.confirm && pendingMoves > 0 && path.Count > 0)
            {
                MovePlayer();
            }
        }
        if (room != null && !room.opened && room.enemies.Count == 0)
        {
            room.OpenDoors();
            room.opened = true;
        }
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
                    if (new Vector3(RoundOffset(world.rooms[j].tiles[x, y].position.x), RoundOffset(world.rooms[j].tiles[x, y].position.y), zAxis) == transform.position)
                    {
                        //Debug.Log("room is updated");
                        room = world.rooms[j];
                        if (prevRoom != null && prevRoom.number != room.number)
                        {
                            prevRoom.playerCount--;
                            room.playerCount++;
                            /*if (prevRoom.playerCount == 0)
                            {
                                prevRoom.Hide();
                            }
                            if (room.playerCount == 1)
                            {
                                room.Show();
                            }*/
                        }
                        prevRoom = room;
                        pleaseExit = true;
                        if (moves > maxMoves && room.enemies.Count > 0)
                        {
                            moves = maxMoves - 1;
                        }
                        break;
                    }
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

    public void AdjustStartingPosition()
    {
        transform.position = new Vector3(RoundOffset(transform.position.x), RoundOffset(transform.position.y), zAxis);
        lastPosition = transform.position;
    }

    public void AddTileToPath(Vector3Int goal)
    {
        start = goal;
        world.UnhighlightOldNeighbors();
        /// Changes the tileflags to none so that we are able to change anything about the tile.
        /// Then, changiing the color to red, adds it to the path list, and adjusts the number of remaining moves appropriately.
        world.highlighter.SetTile(goal, world.floor_tile_asset);
        world.world.SetTileFlags(goal, TileFlags.None);
        Color color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
        world.highlighter.SetColor(goal, color);
        //spriteRenderer.sprite = green_cursor;
        path.Add(goal);
        --moves;
        ++pendingMoves;
        if (moves > 0)
        {
            world.HighlightNeighbors(start);
        }
    }
    public void RemoveTileFromPath(Vector3Int coordinate)
    {
        if (path.Count > 0 && coordinate == path[path.Count - 1])
        {
            world.UnhighlightOldNeighbors();
            world.highlighter.SetTile(coordinate, world.floor_tile_asset);
            world.highlighter.SetColor(coordinate, Color.white);
            path.RemoveAt(path.Count - 1);
            moves++;
            pendingMoves--;
            if (path.Count > 0)
            {
                start = path[path.Count - 1];
                world.HighlightNeighbors(path[path.Count - 1]);
            }
            else
            {
                Vector3Int playerCell = world.world.WorldToCell(transform.position);
                start = playerCell;
                world.HighlightNeighbors(playerCell);
            }
        }
    }

    public void MovePlayer()
    {
        if (!moving)
        {
            world.UnhighlightOldNeighbors();
            /*for (int i = 0; i < path.Count; i++)
            {
                Color color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
                //world.highlighter.SetTile(path[i], world.floor_tile_asset);
                //world.highlighter.SetColor(path[i], color);
            }*/
            moving = true;
            startTime = Time.time;
            destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
            totalDistance = Vector3.Distance(transform.position, destination);
            world.highlighter.SetTile(world.world.WorldToCell(transform.position), world.empty_tile_asset);
        }
        else
        {
            if (destination.x > transform.position.x)
            {
                animator.transform.localEulerAngles = new Vector3(0, 180, 0);
            }
            else
            {
                animator.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            animator.Play("walk");
            float distanceCovered, fractionOfJourney;
            if (transform.position != destination)
            {
                distanceCovered = (Time.time - startTime) * playerSpeed;
                fractionOfJourney = distanceCovered / totalDistance;
                transform.position = Vector3.Lerp(transform.position, destination, fractionOfJourney);
            }
            if (transform.position == destination)
            {
                UpdateRoom();
                int xCoor, yCoor;
                if(transform.position.x < 0)
                {
                    xCoor = (int)(transform.position.x - 1);
                }
                else
                {
                    xCoor = (int)(transform.position.x);
                }
                if (transform.position.y < 0)
                {
                    yCoor = (int)(transform.position.y - 1);
                }
                else
                {
                    yCoor = (int)(transform.position.y);
                }
                world.highlighter.SetTile(path[0], world.empty_tile_asset);
                path.RemoveAt(0);
                if (path.Count > 0)
                {
                    startTime = Time.time;
                    destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
                    totalDistance = Vector3.Distance(transform.position, destination);
                }
                else
                {
                    pendingMoves = 0;
                    animator.Play("idle");
                    moving = false;
                    Vector3Int playerCell = world.world.WorldToCell(transform.position);
                    world.highlighter.SetTile(playerCell, world.floor_tile_asset);
                    world.highlighter.SetTileFlags(playerCell, TileFlags.None);
                    world.highlighter.SetColor(playerCell, Color.magenta);
                    turnHandler.confirm = false;
                    if (moves > 0)
                    {
                        world.HighlightNeighbors(playerCell);
                    }
                }
            }
        }
    }
    public void ClearPath()
    {
        Color color = new Color(0, 0, 0, 0.0f);
        world.UnhighlightOldNeighbors();
        pendingMoves = 0;
        Vector3Int playerCell = world.world.WorldToCell(transform.position);
        world.highlighter.SetTile(playerCell, world.empty_tile_asset);
        world.highlighter.SetTileFlags(playerCell, TileFlags.None);
        world.highlighter.SetColor(playerCell, Color.white);
        foreach (Vector3Int coordinate in path)
        {
            world.highlighter.SetTile(coordinate, world.empty_tile_asset);
            world.highlighter.SetTileFlags(coordinate, TileFlags.None);
            //world.highlighter.SetColor(coordinate, color);
            moves++;
        }
        path.Clear();
        start = world.world.WorldToCell(transform.position);
    }

    /// Increment/reset things related to this character since their turn is just beginning now.
    public void StartTurn()
    {
        turnStarted = true;
        moves = maxMoves;
        energy += generation;
        if (energy > energyMax)
        {
            energy = energyMax;
        }
        if (room.enemies.Count == 0)
        {
            moves = 999;
        }
        attacked = false;
    }

    public void HighlightStartPosition()
    {
        Vector3Int playerCell = world.world.WorldToCell(transform.position);
        /// Set the flags to none so that we can change the color to magenta
        world.highlighter.SetTileFlags(playerCell, TileFlags.None);
        world.highlighter.SetTile(playerCell, world.floor_tile_asset);
        /// Change the tile's color to magenta
        world.highlighter.SetColor(playerCell, Color.magenta);
        world.HighlightNeighbors(playerCell);
    }

    public bool Attack(WeaponCard card, GameObject enemyObj)
    {
        range = card.range;
        if (IsEnemyInLineOfSight(enemyObj) && InRange(transform.position, enemyObj.transform.position))
        {
            IEnemy enemy;
            if (selectedEnemy == null)
            {
                enemy = turnHandler.FetchEnemyType(enemyObj);
            }
            else
            {
                enemy = turnHandler.FetchEnemyType(selectedEnemy);

            }
            int damage = attack - enemy.armor;
            if (card.usesCharge)
            {
                damage += charge;
                charge = 0;
            }
            enemy.health -= damage;
            if (enemy.health <= 0)
            {
                if (selectedEnemy == null)
                {
                    turnHandler.enemyList.Remove(enemyObj);
                    enemy.room.enemies.Remove(enemyObj);
                    Destroy(enemyObj);
                    turnHandler.activeEnemy = null;
                }
                else
                {
                    turnHandler.enemyList.Remove(selectedEnemy);
                    enemy.room.enemies.Remove(selectedEnemy);
                    Destroy(enemyObj);
                    turnHandler.activeEnemy = null;
                    selectedEnemy = null;
                }
            }
            attacked = true;
            return true;
        }
        return false;
    }

    public bool AOEAttack(WeaponCard card)
    {
        bool attacked = false;
        if (selectedEnemy != null)
        {
            selectedEnemy = null;
        }
        for (int i = 0; i < room.enemies.Count; i++)
        {
            if (room.enemies[i] != null && Attack(card, room.enemies[i]))
            {
                attacked = true;
            }
        }
        /*foreach (GameObject enemyObj in room.enemies)
        {
            if (Attack(enemyObj))
            {
                attacked = true;
            }
        }*/
        return attacked;
    }

    public bool IsEnemyInLineOfSight(GameObject enemy)
    {
        if (turnHandler.FetchEnemyType(enemy).room != room)
        {
            return false;
        }
        Vector2 l1p1 = new Vector2(RoundOffset(transform.position.x), RoundOffset(transform.position.y));
        Vector2 l1p2 = new Vector2(RoundOffset(enemy.transform.position.x), RoundOffset(enemy.transform.position.y));
        int xMin = (int) (transform.position.x < enemy.transform.position.x ? transform.position.x : enemy.transform.position.x);
        int yMin = (int) (transform.position.y < enemy.transform.position.y ? transform.position.y : enemy.transform.position.y);
        int xMax = (int) (transform.position.x > enemy.transform.position.x ? transform.position.x : enemy.transform.position.x);
        int yMax = (int) (transform.position.y > enemy.transform.position.y ? transform.position.y : enemy.transform.position.y);
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

    public bool InRange(Vector3 start, Vector3 goal)
    {
        return (int)(Mathf.Max(Mathf.Abs(start.x - goal.x), Mathf.Abs(start.y - goal.y))) <= range;
    }
}