using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Turn_Handler : MonoBehaviour
{
    public GameObject players;
    public GameObject enemies;

    public Enemies enemyScript;

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

    public bool AllPlayersFinished()
    {
        foreach (Player player in playerList)
        {
            if (!player.finished)
            {
                return false;
            }
        }
        return true;
    }

    public void ResetAllPlayerFinishedStates()
    {
        foreach (Player player in playerList)
        {
            player.finished = false;
        }
    }

    Player prevPlayer;

    public void SetActivePlayerToNonFinishedPlayer()
    {
        prevPlayer = activePlayer;
        foreach (Player player in playerList)
        {
            if (!player.finished)
            {
                activePlayer = player;
                if (!(prevPlayer == activePlayer))
                {
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (playerList.Count == 0)
        {
            SceneManager.LoadScene("End_Scene", LoadSceneMode.Single);
        }
        if (playerTurn)
        {
            if (!activePlayer.turnStarted && AllPlayersFinished())
            {
                ResetAllPlayerFinishedStates();
            }
            if (!activePlayer.moving && !activePlayer.turnStarted)
            {
                playerList.Remove(activePlayer);
                playerList.Add(activePlayer);
                SetActivePlayerToNonFinishedPlayer();
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
        Debug.Log(enemy.name);
        if (enemy.name.Contains("Scuttler"))
        {
            return enemy.GetComponent<ScuttlerScript>().scuttler;
        }
        else if (enemy.name.Contains("Slinger"))
        {
            return enemy.GetComponent<SlingerScript>().slinger;
        }
        else if (enemy.name.Contains("Carapace"))
        {
            return enemy.GetComponent<CarapaceScript>().carapace;
        }
        else if (enemy.name.Contains("PeaceKeeper"))
        {
            return enemy.GetComponent<PeaceKeeperScript>().peaceKeeper;
        }
        else if (enemy.name.Contains("Mauler"))
        {
            return enemy.GetComponent<MaulerScript>().mauler;
        }
        else if (enemy.name.Contains("Tendril"))
        {
            return enemy.GetComponent<TendrilScript>().tendril;
        }
        return null;
    }

    public void RemovePlayer(Player player)
    {
        if (activePlayer == player)
        {
            int index = playerList.IndexOf(player);
            if (index == 0)
            {
                index = playerList.Count - 1;
            }
            else
            {
                index = index - 1;
            }
            activePlayer = playerList[index];
        }
        playerList.Remove(player);
        player.transform.position = new Vector3(-99, -99, 0);
        if (playerList.Count == 1)
        {
            activePlayer = playerList[0];
        }
    }
}