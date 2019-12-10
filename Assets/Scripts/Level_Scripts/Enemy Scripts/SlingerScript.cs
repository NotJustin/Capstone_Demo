using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slinger : IEnemy
{
    public Slinger(GameObject obj) : base(obj)
    {
        health = 1;
        tier = 2;
        attack = 2;
        armor = 0;
        text = "Either move 4 to keep nearest player in range, or Deal 2 damage at range 4. Min range 2";
    }
    Player nearestPlayer = null;
    public override void PrimaryAttack()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 4.0f;
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

public class SlingerScript : MonoBehaviour
{
    public Slinger slinger;
    Turn_Handler turnHandler;
    void Awake()
    {
        slinger = new Slinger(this.transform.gameObject);
        turnHandler = slinger.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    /*void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == slinger)
        {
            slinger.UpdateRoom();
            slinger.PrimaryAttack();
        }
    }*/
}