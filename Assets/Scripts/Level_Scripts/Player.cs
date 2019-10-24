using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    /// This script corrects the player's initial position to snap to the grid
    /// and stores the maxMoves/moves that the player has so that other scripts may view and change the numbers as fit.
    float zAxis = 0;
    public Animator animator;
    public Vector3 lastPosition;
    GameObject tileWorldObj;
    public TileWorld tileWorld;

    public int maxMoves = 2;
    public int moves = 2;
    public float playerSpeed = 0.25f;
    public int pendingMoves = 0;
    public bool moving = false;
    public bool selected;
    public bool started = false;

    public Vector3Int start;

    public Vector3 destination;
    public float startTime;
    public float totalDistance;

    public GameObject turnHandlerObj;
    public Turn_Handler turnHandler;

    public List<Vector3Int> path;
    void Start()
    {
        tileWorldObj = GameObject.FindGameObjectWithTag("TileWorld");
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
        tileWorld = tileWorldObj.GetComponent<TileWorld>();
        animator = GetComponent<Animator>();
        lastPosition = new Vector3(-1, -1, 0);
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
        tileWorld.world.SetTileFlags(goal, TileFlags.None);
        Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        tileWorld.world.SetColor(goal, color);
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
            tileWorld.world.SetColor(coordinate, Color.white);
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
            for (int i = 0; i < path.Count; i++)
            {
                Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                tileWorld.world.SetColor(path[i], color);
            }
            moving = true;
            startTime = Time.time;
            destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
            totalDistance = Vector3.Distance(transform.position, destination);
            Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
            tileWorld.world.SetTileFlags(playerCell, TileFlags.None);
            tileWorld.world.SetColor(playerCell, Color.white);
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
                tileWorld.OpenDoors(path[0]);
                tileWorld.world.SetTileFlags(path[0], TileFlags.None);
                tileWorld.world.SetColor(path[0], Color.white);
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
                    tileWorld.world.SetTileFlags(playerCell, TileFlags.None);
                    tileWorld.world.SetColor(playerCell, Color.magenta);
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
        tileWorld.UnhighlightOldNeighbors();
        pendingMoves = 0;
        Vector3Int playerCell = tileWorld.world.WorldToCell(transform.position);
        tileWorld.world.SetTileFlags(playerCell, TileFlags.None);
        tileWorld.world.SetColor(playerCell, Color.white);
        foreach (Vector3Int coordinate in path)
        {
            tileWorld.world.SetTileFlags(coordinate, TileFlags.None);
            tileWorld.world.SetColor(coordinate, Color.white);
            moves++;
        }
        path.Clear();
        start = tileWorld.world.WorldToCell(transform.position);
    }

    public void StartTurn()
    {
        moves = maxMoves;
    }
}