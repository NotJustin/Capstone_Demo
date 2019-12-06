using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;
    public Sprite red_other_cursor;
    public Sprite yellow_other_cursor;
    public Sprite green_other_cursor;
    public GameObject guiObj;
    guiScript gui;
    public GameObject worldObj;
    World world;
    public GameObject turnHandlerObj;
    Turn_Handler turnHandler;
    private readonly int zAxis = 0;
    Vector3 mousePosition;
    private SpriteRenderer spriteRenderer;
    private int move = 0, attack = 1;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gui = guiObj.GetComponent<guiScript>();
        world = worldObj.GetComponent<World>();
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
    }
    void Update()
    {
        if (gui.mode == move)
        {
            spriteRenderer.sprite = yellow_cursor;
            CursorFollowMouse();
            if (!turnHandler.activePlayer.moving && Input.GetMouseButton(0))
            {
                /// This is just converting the Vector3 position of the cursor and player in the world to the cell position of
                /// the tile they are currently on when the player clicks their left mouse button.
                Vector3Int goal = world.world.WorldToCell(transform.position);
                /// If this statement is true, it adds the tile to the path. See CheckTile() for more info.
                if (turnHandler.activePlayer.moves > 0 && world.CheckTile(turnHandler.activePlayer.start, goal) && Array.Exists(world.possibleTiles, element => element == goal))
                {
                    turnHandler.activePlayer.AddTileToPath(goal);
                }
            }
            else if (!turnHandler.activePlayer.moving && Input.GetMouseButton(1))
            {
                /// Does the opposite of above. It checks if the cell the cursor is on is the last tile you selected. If so, it removes it from the list.
                /// In the future, I would want the "last selected tile" to be a different color so that people aren't forced to memorize what the last tile they selected is.
                /// Then, if they remove that tile, the one before that will change color as it is the "new" last tile selected.
                Vector3Int coordinate = world.world.WorldToCell(transform.position);
                turnHandler.activePlayer.RemoveTileFromPath(coordinate);
            }
        }
        else if (gui.mode == attack)
        {
            spriteRenderer.sprite = yellow_other_cursor;
            CursorFollowMouse();
            if (Input.GetMouseButtonDown(0))
            {  
                for (int i = 0; i < turnHandler.enemyList.Count; i++)
                {
                    if (gui.selectedCard != null && transform.position == turnHandler.enemyList[i].transform.position)
                    {
                        turnHandler.activePlayer.selectedEnemy = turnHandler.enemyList[i].gameObject;
                    }
                }
            }
        }
        else
        {
            HideCursor();
        }
    }
    void CursorFollowMouse()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = zAxis;
        transform.position = new Vector3((RoundOffset(mousePosition.x)), (RoundOffset(mousePosition.y)), zAxis);
    }
    void HideCursor()
    {
        spriteRenderer.sprite = null;
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