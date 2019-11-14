using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Turn_Handler : MonoBehaviour
{
    public GameObject players;
    public GameObject enemies;

    public Enemies enemyScript;

    /*public struct DespawnedEnemy
    {
        string name;
        int health;
    }*/

    public List<Player> playerList;
    public List<GameObject> enemyList;
    public List<DespawnedEnemy> despawnedEnemies;

    private int remainingPlayerTurns;
    public bool enemyTurn = false;
    public bool playerTurn = false;

    public GameObject worldObj;
    World world;

    public Player firstPlayer, activePlayer;
    public IEnemy activeEnemy;

    public bool confirm = false;

    public void Initialize()
    {
        world = worldObj.GetComponent<World>();
        playerList = new List<Player>();
        //enemyList = enemies.GetComponent<Enemies>().enemies;
        despawnedEnemies = new List<DespawnedEnemy>();
        foreach (Transform child in players.transform)
        {
            Player player = child.GetComponent<Player>();
            playerList.Add(player);
            player.start = world.world.WorldToCell(player.transform.position);
        }
        activePlayer = playerList[0];
        firstPlayer = activePlayer;
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (playerTurn)
        {
            if (!activePlayer.moving && !activePlayer.turnStarted)
            {
                playerList.Add(activePlayer);
                playerList.RemoveAt(0);
                activePlayer = playerList[0];
                activePlayer.HighlightStartPosition();
                activePlayer.StartTurn();
            }
        }
        else if(enemyTurn)
        {
            if (enemyList.Count > 0)
            {
                activeEnemy = FetchEnemyType(enemyList[0]);
            }
            else
            {
                enemyTurn = false;
                playerTurn = true;
            }
            if (enemyList.Count > 0 && !activeEnemy.awaitMovement && !activeEnemy.turnStarted)
            {
                enemyList.Add(enemyList[0]);
                enemyList.RemoveAt(0);
                activeEnemy = FetchEnemyType(enemyList[0]);
            }
        }
    }
    public IEnemy FetchEnemyType(GameObject enemy)
    {
        if (enemy.tag == "thrasher")
        {
            return enemy.GetComponent<ThrasherScript>().thrasher;
        }
        return null;
    }
}