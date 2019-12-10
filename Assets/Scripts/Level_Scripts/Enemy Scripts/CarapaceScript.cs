using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carapace : IEnemy
{
    public Carapace(GameObject obj) : base(obj)
    {
        health = 12;
        tier = 2;
        attack = 4;
        armor = 2;
        text = "Move 2. Attack nearest player. Range is 3 and 4 damage";
    }
    Player nearestPlayer = null;
    public override void PrimaryAttack()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 3.0f;
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

public class CarapaceScript : MonoBehaviour
{
    public Carapace carapace;
    Turn_Handler turnHandler;
    void Awake()
    {
        carapace = new Carapace(this.transform.gameObject);
        turnHandler = carapace.turnHandlerObj.GetComponent<Turn_Handler>();
    }

    /*void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == carapace)
        {
            carapace.UpdateRoom();
            carapace.PrimaryAttack();
        }
    }*/
}