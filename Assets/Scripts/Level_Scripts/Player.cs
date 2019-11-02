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
    GameObject tileWorldObj;
    public TileWorld tileWorld;

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

    public TileRoom tileRoom;
    public TileRoom prevRoom;

    public List<Vector3Int> path;

    public bool turnStarted = false;
    void Start()
    {
        tileWorldObj = GameObject.FindGameObjectWithTag("TileWorld");
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        tileWorld = tileWorldObj.GetComponent<TileWorld>();
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
                    if (new Vector3(RoundOffset(tileWorld.rooms[j].tiles[x, y].position.x), RoundOffset(tileWorld.rooms[j].tiles[x, y].position.y), zAxis) == transform.position)
                    {
                        //Debug.Log("room is updated");
                        tileRoom = tileWorld.rooms[j];
                        if (prevRoom != null && prevRoom.number != tileRoom.number)
                        {
                            prevRoom.playerCount--;
                            tileRoom.playerCount++;
                            if (prevRoom.playerCount == 0)
                            {
                                prevRoom.HideRoom();
                            }
                            if (tileRoom.playerCount == 1)
                            {
                                tileRoom.ShowRoom();
                            }
                        }
                        prevRoom = tileRoom;
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
        tileWorld.UnhighlightOldNeighbors();
        /// Changes the tileflags to none so that we are able to change anything about the tile.
        /// Then, changiing the color to red, adds it to the path list, and adjusts the number of remaining moves appropriately.
        tileWorld.highlighter.SetTile(goal, tileWorld.floor_tile_asset);
        tileWorld.world.SetTileFlags(goal, TileFlags.None);
        Color color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
        tileWorld.highlighter.SetColor(goal, color);
        //spriteRenderer.sprite = green_cursor;
        path.Add(goal);
        --moves;
        ++pendingMoves;
        if (moves > 0)
        {
            tileWorld.HighlightNeighbors(start);
        }
    }
    public void RemoveTileFromPath(Vector3Int coordinate)
    {
        if (path.Count > 0 && coordinate == path[path.Count - 1])
        {
            tileWorld.UnhighlightOldNeighbors();
            tileWorld.highlighter.SetTile(coordinate, tileWorld.floor_tile_asset);
            tileWorld.highlighter.SetColor(coordinate, Color.white);
            path.RemoveAt(path.Count - 1);
            moves++;
            pendingMoves--;
            if (path.Count > 0)
            {
                start = path[path.Count - 1];
                tileWorld.HighlightNeighbors(path[path.Count - 1]);
            }
            else
            {
                Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
                start = playerCell;
                tileWorld.HighlightNeighbors(playerCell);
            }
        }
    }

    public void MovePlayer()
    {
        if (!moving)
        {
            tileWorld.UnhighlightOldNeighbors();
            /*for (int i = 0; i < path.Count; i++)
            {
                Color color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
                //tileWorld.highlighter.SetTile(path[i], tileWorld.floor_tile_asset);
                //tileWorld.highlighter.SetColor(path[i], color);
            }*/
            moving = true;
            startTime = Time.time;
            destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
            totalDistance = Vector3.Distance(transform.position, destination);
            tileWorld.highlighter.SetTile(tileWorld.world.WorldToCell(transform.position), tileWorld.empty_tile_asset);
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
                TileBase tileBase = tileWorld.world.GetTile(new Vector3Int(xCoor, yCoor, zAxis));
                if (tileBase != null && tileBase.name.Contains("key"))
                {
                    tileWorld.OpenDoors(this);
                }
                tileWorld.highlighter.SetTile(path[0], tileWorld.empty_tile_asset);
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
                    Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
                    tileWorld.highlighter.SetTile(playerCell, tileWorld.floor_tile_asset);
                    tileWorld.highlighter.SetTileFlags(playerCell, TileFlags.None);
                    tileWorld.highlighter.SetColor(playerCell, Color.magenta);
                    turnHandler.confirm = false;
                    if (moves > 0)
                    {
                        tileWorld.HighlightNeighbors(playerCell);
                    }
                }
            }
        }
    }
    public void ClearPath()
    {
        Color color = new Color(0, 0, 0, 0.0f);
        tileWorld.UnhighlightOldNeighbors();
        pendingMoves = 0;
        Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
        tileWorld.highlighter.SetTile(playerCell, tileWorld.empty_tile_asset);
        tileWorld.highlighter.SetTileFlags(playerCell, TileFlags.None);
        tileWorld.highlighter.SetColor(playerCell, Color.white);
        foreach (Vector3Int coordinate in path)
        {
            tileWorld.highlighter.SetTile(coordinate, tileWorld.empty_tile_asset);
            tileWorld.highlighter.SetTileFlags(coordinate, TileFlags.None);
            //tileWorld.highlighter.SetColor(coordinate, color);
            moves++;
        }
        path.Clear();
        start = tileWorld.world.WorldToCell(transform.position);
    }

    /// Increment/reset things related to this character since their turn is just beginning now.
    public void StartTurn()
    {
        turnStarted = true;
        moves = maxMoves;
    }

    public void HighlightStartPosition()
    {
        Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
        /// Set the flags to none so that we can change the color to magenta
        tileWorld.highlighter.SetTileFlags(playerCell, TileFlags.None);
        tileWorld.highlighter.SetTile(playerCell, tileWorld.floor_tile_asset);
        /// Change the tile's color to magenta
        tileWorld.highlighter.SetColor(playerCell, Color.magenta);
        tileWorld.HighlightNeighbors(playerCell);
    }

}