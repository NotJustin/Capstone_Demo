using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Turn_Handler : MonoBehaviour
{
    List<Player> playerList;

    public GameObject players;

    private int remainingPlayerTurns;
    private int enemyTurn;
    public Player activePlayer;
    public bool changeTurn = false;

    public GameObject tileSelector;
    Tile_Selector_Script tileSelectorScript;

    void Start()
    {
        tileSelectorScript = tileSelector.GetComponent<Tile_Selector_Script>();
        playerList = new List<Player>();
        Vector3Int playerCell;
        foreach (Transform child in players.transform)
        {
            Player player = child.GetComponent<Player>();
            playerList.Add(player);
            player.start = tileSelectorScript.tileMap.WorldToCell(player.transform.position);
        }
        activePlayer = playerList[0];
        //playerList.RemoveAt(0);
        playerCell = tileSelectorScript.tileMap.WorldToCell(activePlayer.transform.position);
        tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
        tileSelectorScript.tileMap.SetColor(playerCell, Color.magenta);

        /// Setting possible-to-select tiles as those surrounding the player, if the player has moves
        if (activePlayer.moves > 0)
        {
            tileSelectorScript.HighlightNeighbors(playerCell);
        }
    }

    void Update()
    {
        if (changeTurn)
        {
            tileSelectorScript.UnhighlightOldNeighbors();
            activePlayer.ClearPath();
            changeTurn = false;
            playerList.Add(activePlayer);
            playerList.RemoveAt(0);
            activePlayer = playerList[0];
            Vector3Int playerCell = tileSelectorScript.tileMap.WorldToCell(activePlayer.transform.position);
            tileSelectorScript.tileMap.SetTileFlags(playerCell, TileFlags.None);
            tileSelectorScript.tileMap.SetColor(playerCell, Color.magenta);
            if(activePlayer.moves > 0)
            {
                tileSelectorScript.HighlightNeighbors(playerCell);
            }
        }
    }
}