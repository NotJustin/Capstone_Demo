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
    GameObject tileSelector;
    public Tile_Selector_Script tileSelectorScript;

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

    public List<Vector3Int> path;
    void Start()
    {
        tileSelector = GameObject.FindGameObjectWithTag("TileSelector");
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
        animator = GetComponent<Animator>();
        start = tileSelectorScript.tileMap.WorldToCell(transform.position);
        lastPosition = new Vector3(-1, -1, 0);
    }

    private void FixedUpdate()
    {
        if (!started && lastPosition != transform.position)
        {
            started = true;
            transform.position = new Vector3(RoundOffset(transform.position.x), RoundOffset(transform.position.y), zAxis);
            lastPosition = transform.position;
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

    public void AddTileToPath(Vector3Int goal)
    {
        start = goal;
        tileSelectorScript.UnhighlightOldNeighbors();
        /// Changes the tileflags to none so that we are able to change anything about the tile.
        /// Then, changiing the color to red, adds it to the path list, and adjusts the number of remaining moves appropriately.
        tileSelectorScript.tileMap.SetTileFlags(goal, TileFlags.None);
        Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        tileSelectorScript.tileMap.SetColor(goal, color);
        //spriteRenderer.sprite = green_cursor;
        path.Add(goal);
        --moves;
        ++pendingMoves;
        if (moves > 0)
        {
            tileSelectorScript.HighlightNeighbors(start);
        }
    }
    public void RemoveTileFromPath(Vector3Int coordinate)
    {
        if (path.Count > 0 && coordinate == path[path.Count - 1])
        {
            tileSelectorScript.UnhighlightOldNeighbors();
            tileSelectorScript.tileMap.SetColor(coordinate, Color.white);
            path.RemoveAt(path.Count - 1);
            moves++;
            pendingMoves--;
            if (path.Count > 0)
            {
                start = path[path.Count - 1];
                tileSelectorScript.HighlightNeighbors(path[path.Count - 1]);
            }
            else
            {
                Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(transform.position);
                start = playerCell;
                tileSelectorScript.HighlightNeighbors(playerCell);
            }
        }
    }

    public void MovePlayer()
    {
        if (!moving)
        {
            tileSelectorScript.UnhighlightOldNeighbors();
            for (int i = 0; i < path.Count; i++)
            {
                Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                tileSelectorScript.tileMap.SetColor(path[i], color);
            }
            moving = true;
            startTime = Time.time;
            destination = new Vector3(RoundOffset(path[0].x), RoundOffset(path[0].y), zAxis);
            totalDistance = Vector3.Distance(transform.position, destination);
            Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(transform.position);
            tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
            tileSelectorScript.tileMap.SetColor(playerCell, Color.white);
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
                tileSelectorScript.OpenDoors(path[0]);
                tileSelectorScript.tileMap.SetTileFlags(path[0], TileFlags.None);
                tileSelectorScript.tileMap.SetColor(path[0], Color.white);
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
                    Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(transform.position);
                    tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
                    tileSelectorScript.tileMap.SetColor(playerCell, Color.magenta);
                    tileSelectorScript.confirm = false;
                    if (moves > 0)
                    {
                        tileSelectorScript.HighlightNeighbors(playerCell);
                    }
                }
            }
        }
    }
    public void ClearPath()
    {
        tileSelectorScript.UnhighlightOldNeighbors();
        pendingMoves = 0;
        Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(transform.position);
        tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileSelectorScript.tileMap.SetColor(playerCell, Color.white);
        foreach (Vector3Int coordinate in path)
        {
            tileSelectorScript.tileMap.SetTileFlags(coordinate, TileFlags.None);
            tileSelectorScript.tileMap.SetColor(coordinate, Color.white);
            moves++;
        }
        path.Clear();
        start = tileSelectorScript.tileMap.WorldToCell(transform.position);
    }

    public void StartTurn()
    {
        moves = maxMoves;
    }
}