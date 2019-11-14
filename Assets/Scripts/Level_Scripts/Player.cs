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

    public int maxMoves = 2;
    public int moves = 2;
    public float playerSpeed = 0.25f;
    public int pendingMoves = 0;
    public bool moving = false;
    public bool started = false;

    public Vector3Int start;

    public Vector3 destination;
    public float startTime;
    public float totalDistance;

    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;

    public Room room;
    public Room prevRoom;

    public List<Vector3Int> path;

    public bool turnStarted = false;
    void Awake()
    {
        worldObj = GameObject.FindGameObjectWithTag("TileWorld");
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        world = worldObj.GetComponent<World>();
        animator = GetComponent<Animator>();
        lastPosition = new Vector3(-1, -1, 0);
    }

    //bool updating = false;

    void Update()
    {
        if (turnHandler.activePlayer == this && turnHandler.playerTurn)
        {
            turnHandler.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            if (turnHandler.confirm && pendingMoves > 0 && path.Count > 0)
            {
                MovePlayer();
            }
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
                            if (prevRoom.playerCount == 0)
                            {
                                prevRoom.Hide();
                            }
                            if (room.playerCount == 1)
                            {
                                room.Show();
                            }
                        }
                        prevRoom = room;
                        pleaseExit = true;
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
            animator.Play("walking");
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
                TileBase tileBase = world.world.GetTile(new Vector3Int(xCoor, yCoor, zAxis));
                if (tileBase != null && tileBase.name.Contains("key"))
                {
                    world.OpenDoors(this);
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

}