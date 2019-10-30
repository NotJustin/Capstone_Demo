using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Turn_Handler : MonoBehaviour
{
    public GameObject players;
    public GameObject enemies;

    public Enemies enemyScript;

    public List<Player> playerList;
    public List<IEnemy> enemyList;
    public List<IEnemy> despawnedEnemies;

    private int remainingPlayerTurns;
    public Player activePlayer;
    public bool changeTurn = false;

    public GameObject tileWorldObj;
    TileWorld tileWorld;

    public Player firstPlayer;

    public bool confirm = false;

    public void Initialize()
    {
        tileWorld = tileWorldObj.GetComponent<TileWorld>();
        playerList = new List<Player>();
        enemyList = enemies.GetComponent<Enemies>().enemies;
        despawnedEnemies = new List<IEnemy>();
        foreach (Transform child in players.transform)
        {
            Player player = child.GetComponent<Player>();
            playerList.Add(player);
            player.start = tileWorld.world.WorldToCell(player.transform.position);
        }
        activePlayer = playerList[0];
        firstPlayer = activePlayer;
    }
    void Start()
    {
        
    }

    //bool enemyTurn = false;

    void Update()
    {
        //Debug.Log(activePlayer);
        if (changeTurn)
        {
            if (enemyList.Count > 0)
            {
                enemyList.Add(enemyList[0]);
                enemyList.RemoveAt(0);
            }
            //Debug.Log("Changing turn");
            /// Set changeTurn to false because we used it to get into this conditional and don't want to do it again.
            changeTurn = false;
            //enemyTurn = true;
            /// Unhighlights any yellow tiles
            tileWorld.UnhighlightOldNeighbors();
            /// Unhighlights any yellow tiles
            tileWorld.UnhighlightOldNeighbors();
            /// Unhighlights all selected tiles
            activePlayer.ClearPath();
            /// Add the current activePlayer to the end of the list
            playerList.Add(activePlayer);
            /// Remove the current activePlayer from the front of the list
            playerList.RemoveAt(0);
            //enemyTurn = false;
            if (!(enemyList.Count > 0 && !enemyList[0].awaitMovement))
            {
                /// Set activePlayer to be the new front of the list
                activePlayer = playerList[0];
                /// Get the coordinate of the activePlayer's cell on the tileMap
                Vector3Int playerCell = tileWorld.world.WorldToCell(activePlayer.transform.position);
                /// Set the flags to none so that we can change the color to magenta
                tileWorld.highlighter.SetTileFlags(playerCell, TileFlags.None);
                tileWorld.highlighter.SetTile(playerCell, tileWorld.floor_tile_asset);
                /// Change the tile's color to magenta
                tileWorld.highlighter.SetColor(playerCell, Color.magenta);
                /// Increment/reset things related to this character since their turn is just beginning now.
                activePlayer.StartTurn();
                /// Highlight the neighboring cells as yellow tiles.
                tileWorld.HighlightNeighbors(playerCell);
            }
        }
        if (enemyList.Count > 0 && enemyList[0].awaitMovement)
        {
            transform.position = new Vector3(enemyList[0].obj.transform.position.x, enemyList[0].obj.transform.position.y, -10);
            enemyList[0].AttackOne();
        }
        else
        {
            //Debug.Log(enemyList.Count);
            //Debug.Log(activePlayer);
            transform.position = new Vector3(activePlayer.transform.position.x, activePlayer.transform.position.y, -10);
            if (confirm && activePlayer.pendingMoves > 0 && activePlayer.path.Count > 0)
            {
                activePlayer.MovePlayer();
            }
        }
    }
}