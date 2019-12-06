using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public GameObject turnHandlerObj;
    public GameObject menuBottom;
    public GameObject menuRight;
    public GameObject menuLeft;
    public GameObject menuTop;
    public GameObject background;
    public GameObject mask;
    Turn_Handler turnHandler;
    void Awake()
    {
        turnHandler = turnHandlerObj.GetComponent<Turn_Handler>();
    }
    void Update()
    {
        menuBottom.transform.position = new Vector3(transform.position.x - 8, transform.position.y - 3, 0);
        menuRight.transform.position = new Vector3(transform.position.x - 3.5f, transform.position.y + 1.5f, 0);
        menuLeft.transform.position = new Vector3(transform.position.x - 12.5f, transform.position.y + 1.5f, 0);
        menuTop.transform.position = new Vector3(transform.position.x - 8, transform.position.y + 6, 0);
        mask.transform.position = new Vector3(transform.position.x - 8, transform.position.y + 1.5f, 0);
        background.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (turnHandler.playerTurn)
        {
            if (turnHandler.activePlayer.room != null)
            {
                transform.position = new Vector3(turnHandler.activePlayer.room.tiles[0, 0].position.x + 11.5f, turnHandler.activePlayer.room.tiles[0, 0].position.y + 2.0f, -10);
            }
            else
            {
                transform.position = new Vector3(turnHandler.activePlayer.transform.position.x + 10.5f, turnHandler.activePlayer.transform.position.y - 1.0f, -10);
            }
        }
        else if (turnHandler.enemyTurn && turnHandler.activeEnemy != null && turnHandler.activeEnemy.room != null)
        {
            transform.position = new Vector3(turnHandler.activeEnemy.room.tiles[0, 0].position.x + 11.5f, turnHandler.activeEnemy.room.tiles[0, 0].position.y + 2.0f, -10);
        }
    }
}
