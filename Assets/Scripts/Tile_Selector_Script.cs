using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{
    public GameObject cursor;

    public GameObject tileMapObj;
    Tilemap tileMap;
    public int[,] world;

    private SpriteRenderer spriteRenderer;
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;

    public GameObject player;

    Player playerData;

    Vector3 mousePosition;
    float zAxis = 10;

    public float startTime;
    public bool nextTile;
    public bool started;
    public int index = 0;
    public float totalDistance;

    public bool confirm;

    public int pendingMoves;

    List<Vector3Int> path;

    void Start()
    {
        tileMap = tileMapObj.GetComponent<Tilemap>();
        world = tileMapObj.GetComponent<TileMap>().world;

        spriteRenderer = cursor.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = yellow_cursor;

        playerData = player.GetComponent<Player>();
        path = new List<Vector3Int>();
        startTime = Time.time;
        nextTile = true;
        started = false;
        confirm = false;
        pendingMoves = 0;
    }

    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = zAxis;
        cursor.transform.position = new Vector3((RoundOffset(mousePosition.x)), (RoundOffset(mousePosition.y)), zAxis);

        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int goal = tileMap.WorldToCell(cursor.transform.position);
            Vector3Int start;
            if (path.Count > 0)
            {
                start = path[path.Count - 1];
            }
            else
            {
                start = tileMap.WorldToCell(player.transform.position);
            }

            if (playerData.moves > 0 && CheckTile(start, goal))
            {
                tileMap.SetTileFlags(goal, TileFlags.None);
                tileMap.SetColor(goal, Color.red);
                spriteRenderer.sprite = green_cursor;
                path.Add(goal);
                --playerData.moves;
                ++pendingMoves;
            }
            else
            {
                spriteRenderer.sprite = red_cursor;
            }
        }
        else
        {
            spriteRenderer.sprite = yellow_cursor;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3Int coordinate = tileMap.WorldToCell(cursor.transform.position);
            if (path.Count > 0 && coordinate == path[path.Count - 1])
            {
                tileMap.SetColor(coordinate, Color.white);
                path.RemoveAt(path.Count - 1);
                playerData.moves++;
                pendingMoves--;
            }
        }

        if (confirm && !started)
        {
            pendingMoves = 0;
            for (int i = 0; i < path.Count; i++)
            {
                tileMap.SetColor(path[i], Color.green);
            }
            started = true;
        }

        if (confirm && index < path.Count)
        {
            Vector3 destination = new Vector3(RoundOffset(path[index].x), RoundOffset(path[index].y), zAxis);
            if (nextTile)
            {
                startTime = Time.time;
                nextTile = false;
                totalDistance = Vector3.Distance(player.transform.position, destination);
            }
            float distanceCovered = Time.time - startTime;
            float fractionOfJourney = distanceCovered / totalDistance;
            player.transform.position = Vector3.Lerp(player.transform.position, destination, fractionOfJourney);
            if (player.transform.position == destination && index < path.Count)
            {
                nextTile = true;
                tileMap.SetColor(path[index], Color.white);
                ++index;
            }
        }
        if (path.Count > 0 && index == path.Count)
        {
            Debug.Log("Index: " + index + " | Count: " + path.Count);
            /*if (index == path.Count)
            {
                playerData.moves = 0;
            }*/
            do
            {
                --index;
                path.RemoveAt(index);
            }
            while (path.Count > 0);
            confirm = false;
            started = false;
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

    public bool CheckTile(Vector3Int start, Vector3Int goal)
    {
        TileBase tile = tileMap.GetTile(goal);
        if(goal != tileMap.WorldToCell(player.transform.position) && !path.Contains(goal) && IsNeighbor(start, goal))
        {
            if (tile != null && tile.name.Contains("floor"))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsNeighbor(Vector3Int start, Vector3Int goal)
    {
        if (goal.x == start.x - 1 && goal.y == start.y ||
            goal.x == start.x + 1 && goal.y == start.y ||
            goal.y == start.y - 1 && goal.x == start.x ||
            goal.y == start.y + 1 && goal.x == start.x)
        {
            return true;
        }
        return false;
    }

    /*public bool InRange(Vector3Int start, Vector3Int goal)
    {
        Vector3Int leftTile = new Vector3Int(start.x - 1, start.y, start.z);
        Vector3Int rightTile = new Vector3Int(start.x + 1, start.y, start.z);
        Vector3Int upTile = new Vector3Int(start.x, start.y + 1, start.z);
        Vector3Int downTile = new Vector3Int(start.x, start.y - 1, start.z);

        if (CheckTile(tileMap.GetTile(leftTile)) && !viewedTiles.Contains(leftTile) && (compareTiles(leftTile, goal)))
        {

        }

    public bool InRange()
    {
        if (cursor.transform.position == player.transform.position)
        {
            return false;
        }

        Vector3Int playerCell = tileMap.WorldToCell(player.transform.position);
        Vector3Int goalCell = tileMap.WorldToCell(cursor.transform.position);

        Queue<Vector3Int> previousTileList = new Queue<Vector3Int>();
        HashSet<Vector3Int> viewedTiles = new HashSet<Vector3Int>();
        return search(previousTileList, viewedTiles, playerCell, goalCell, playerData.moves);
    }

    public bool search(Queue<Vector3Int> previousTileList, HashSet<Vector3Int> viewedTiles, Vector3Int start, Vector3Int goal, int moves)
    {
        moves--;
        Debug.Log("Current: " + start);
        previousTileList.Enqueue(start);
        viewedTiles.Add(start);
        Vector3Int leftTile = new Vector3Int(start.x - 1, start.y, start.z);
        Vector3Int rightTile = new Vector3Int(start.x + 1, start.y, start.z);
        Vector3Int upTile = new Vector3Int(start.x, start.y + 1, start.z);
        Vector3Int downTile = new Vector3Int(start.x, start.y - 1, start.z);
        if (CheckTile(tileMap.GetTile(leftTile)) && !viewedTiles.Contains(leftTile) && (compareTiles(leftTile, goal)))
        {
            previousTileList.Enqueue(leftTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(rightTile)) && !viewedTiles.Contains(rightTile) && (compareTiles(rightTile, goal)))
        {
            previousTileList.Enqueue(rightTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(upTile)) && !viewedTiles.Contains(upTile) && (compareTiles(upTile, goal)))
        {
            previousTileList.Enqueue(upTile);
            return true;
        }
        if (CheckTile(tileMap.GetTile(downTile)) && !viewedTiles.Contains(downTile) && (compareTiles(downTile, goal)))
        {
            previousTileList.Enqueue(downTile);
            return true;
        }

        if (((CheckTile(tileMap.GetTile(leftTile)) && moves > 0) && !viewedTiles.Contains(leftTile) && search(previousTileList, viewedTiles, leftTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(rightTile)) && moves > 0) && !viewedTiles.Contains(rightTile) && search(previousTileList, viewedTiles, rightTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(upTile)) && moves > 0) && !viewedTiles.Contains(upTile) && search(previousTileList, viewedTiles, upTile, goal, moves)) ||
            ((CheckTile(tileMap.GetTile(downTile)) && moves > 0) && !viewedTiles.Contains(downTile) && search(previousTileList, viewedTiles, downTile, goal, moves)))
        {
            return true;
        }
        //Debug.Log("start after: " + start);
        //Debug.Log("DEQUEUING: " + previousTileList.Dequeue());
        return false;
    }

    public bool compareTiles(Vector3Int start, Vector3Int goal)
    {
        if (start.x >= 0 && start.x < world.GetLength(0) && start.y >= 0 && start.y < world.GetLength(1) && start.x == goal.x && start.y == goal.y)
        {
            return true;
        }
        return false;
    }*/
}