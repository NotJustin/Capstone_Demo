using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maggot : IEnemy
{
    public Maggot(GameObject obj) : base(obj)
    {
        tier = 1;
    }
    public override void PrimaryAttack()
    {
        UpdateRoom();
        if (!attacked)
        {
            //Debug.Log("attacking");
            range = 1.0f;
            attacked = true;
        }
        if (!startedMoving)
        {
            awaitMovement = true;
            startedMoving = true;
            moves = 2;
            prevMoves = 2;
            path = FindPathToNearestPlayer();
            MoveAlongPath(path, range, moves);
        }
        else
        {
            moves = MoveAlongPath(path, range, moves);
        }
        if (moves == 0)
        {
            prevMoves = 2;
            moves = 2;
            path.Clear();
        }
    }
    public override void SecondaryAttack()
    {

    }
    public override void SpecialAttack()
    {

    }
}