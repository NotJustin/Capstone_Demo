using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thrasher : IEnemy
{
    public Thrasher(GameObject obj) : base(obj)
    {
        health = 3;
        tier = 2;
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
        if (!awaitMovement)
        {
            awaitMovement = true;
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

public class ThrasherScript : MonoBehaviour
{
    public Thrasher thrasher;
    Turn_Handler turnHandler;
    bool cameraUpdated = false;
    void Awake()
    {
        thrasher = new Thrasher(this.transform.gameObject);
        turnHandler = thrasher.turnHandlerObj.GetComponent<Turn_Handler>();
    }
    void Update()
    {
        if (turnHandler.enemyTurn && turnHandler.activeEnemy == thrasher)
        {
            turnHandler.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

            Debug.Log("test");
            cameraUpdated = true;
        }
        else
        {
            cameraUpdated = false;
        }
        if (cameraUpdated && turnHandler.enemyTurn && turnHandler.activeEnemy == thrasher)
        {
            thrasher.UpdateRoom();
            thrasher.PrimaryAttack();
        }
    }
}