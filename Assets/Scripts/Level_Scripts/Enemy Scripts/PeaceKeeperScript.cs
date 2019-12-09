using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeaceKeeper : IEnemy
{
    public PeaceKeeper(GameObject obj) : base(obj)
    {
        health = 8;
        tier = 2;
        attack = 2;
        armor = 2;
        text = "Move 2 Spaces toward players and deal 1 melee damage to player with highest plating/HP";
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

public class PeaceKeeperScript : MonoBehaviour
{
    public PeaceKeeper peaceKeeper;
    Turn_Handler turnHandler;
    void Awake()
    {
        peaceKeeper = new PeaceKeeper(this.transform.gameObject);
        turnHandler = peaceKeeper.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == peaceKeeper)
        {
            peaceKeeper.UpdateRoom();
            peaceKeeper.PrimaryAttack();
        }
    }
}