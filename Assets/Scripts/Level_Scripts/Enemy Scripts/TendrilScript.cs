using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tendril : IEnemy
{
    public Tendril(GameObject obj) : base(obj)
    {
        health = 3;
        tier = 2;
        attack = 1;
        armor = 0;
        text = "Move 3 Spaces toward players and deal 2 melee damage to player with highest plating/HP";
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
            moves = 3;
            prevMoves = 3;
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
            prevMoves = 3;
            moves = 3;
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

public class TendrilScript : MonoBehaviour
{
    public Tendril tendril;
    Turn_Handler turnHandler;
    void Awake()
    {
        tendril = new Tendril(this.transform.gameObject);
        turnHandler = tendril.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == tendril)
        {
            tendril.UpdateRoom();
            tendril.PrimaryAttack();
        }
    }
}