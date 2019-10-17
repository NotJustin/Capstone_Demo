using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class IEnemy
{
    GameObject turnHandlerObj;
    GameObject tileSelectorObj;
    int health;
    GameObject obj;
    Tilemap tileMap;
    public IEnemy(int _health, GameObject _obj)
    {
        turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        tileSelectorObj = GameObject.FindGameObjectWithTag("TileSelector");
        health = _health;
        obj = _obj;
    }
    void AttackOne()
    {

    }
    void AttackTwo()
    {

    }
    void AttackThree()
    {

    }

    public List<Vector3Int> FindPathToNearestPlayer(int maxDistance)
    {
        List<Player> playerList = turnHandlerObj.GetComponent<Turn_Handler>().playerList;
        List<Vector3Int> path = new List<Vector3Int>();
        Player closestPlayer = null;
        float closestDistance = 999;
        foreach (Player player in playerList)
        {
            if (closestPlayer == null)
            {
                closestPlayer = player;
            }
            else
            {
                float d1 =
                    Mathf.Abs(closestPlayer.transform.position.x - obj.transform.position.x) +
                    Mathf.Abs(closestPlayer.transform.position.y - obj.transform.position.y);
                float d2 =
                    Mathf.Abs(player.transform.position.x - obj.transform.position.x) +
                    Mathf.Abs(player.transform.position.y - obj.transform.position.y);
                if(d1 < d2)
                {
                    closestDistance = d1;
                }
                else
                {
                    closestPlayer = player;
                    closestDistance = d2;
                }
            }
        }
        if (closestDistance > maxDistance)
        {

        }
        else
        {
            tileMap = tileSelectorObj.GetComponent<Tile_Selector_Script>().tileMap;
            Vector3Int start = tileMap.WorldToCell(obj.transform.position);
            Vector3Int goal = tileMap.WorldToCell(closestPlayer.transform.position);
            int i = 0;
            while (i < maxDistance)
            {

            }
        }
        return path;
    }
}
public class Thrasher : IEnemy
{
    public Thrasher(int health, GameObject obj) : base(health, obj)
    {

    }
    public void AttackOne()
    {

    }
    public void AttackTwo()
    {

    }
    public void AttackThree()
    {

    }
}
public class Enemies : MonoBehaviour
{
    public List<IEnemy> enemies;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}