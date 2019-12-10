using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scuttler : IEnemy
{
    public Scuttler(GameObject obj) : base(obj)
    {
        health = 1;
        tier = 2;
        attack = 1;
        armor = 0;
        text = "Move 2 Spaces toward players and deal 1 melee damage to player with highest plating/HP";
    }
    Player nearestPlayer = null;
    public override void PrimaryAttack()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 1.0f;
            attacked = true;
        }
        if (!awaitMovement)
        {
            awaitMovement = true;
            moves = 2;
            prevMoves = 2;
            path = FindPathToNearestPlayer();
            if (path != null)
            {
                nearestPlayer = GetPlayerAtDestination();
            }
            MoveAlongPath(path, range, moves);
            Attack(nearestPlayer);
        }
        else
        {
            moves = MoveAlongPath(path, range, moves);
        }
        if (moves == 0)
        {
            Attack(nearestPlayer);
            nearestPlayer = null;
            prevMoves = 2;
            moves = 2;
            path.Clear();
        }
    }

    public void Attack(Player player)
    {
        if (IsPlayerInLineOfSight(player) && InRange2(obj.transform.position, player.transform.position))
        {
            if (player.armor > 0)
            {
                player.armor--;
                return;
            }
            player.health--;
            if (player.health <= 0)
            {
                turnHandler.RemovePlayer(player);
            }
        }
    }
}

public class ScuttlerScript : MonoBehaviour
{
    public Scuttler scuttler;
    Turn_Handler turnHandler;
    void Awake()
    {
        scuttler = new Scuttler(this.transform.gameObject);
        turnHandler = scuttler.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    /*void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == scuttler)
        {
            Debug.Log("my turn");
            scuttler.turnStarted = true;
            scuttler.UpdateRoom();
            scuttler.PrimaryAttack();
        }
    }*/
}