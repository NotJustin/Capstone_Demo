using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mauler : IEnemy
{
    public Mauler(GameObject obj) : base(obj)
    {
        health = 5;
        tier = 2;
        attack = 2;
        armor = 1;
        text = "Move 4 Spaces toward players and deal 2 damage to player ";
    }
    Player nearestPlayer = null;
    public override void PrimaryAttack()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 10.0f;
            attacked = true;
        }
        if (!awaitMovement)
        {
            awaitMovement = true;
            moves = 4;
            prevMoves = 4;
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
            prevMoves = 4;
            moves = 4;
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

public class MaulerScript : MonoBehaviour
{
    public Mauler mauler;
    Turn_Handler turnHandler;
    void Awake()
    {
        mauler = new Mauler(this.transform.gameObject);
        turnHandler = mauler.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    /*void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == mauler)
        {
            mauler.UpdateRoom();
            mauler.PrimaryAttack();
        }
    }*/
}