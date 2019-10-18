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
        //turnHandlerObj = GameObject.FindGameObjectWithTag("MainCamera");
        //tileSelectorObj = GameObject.FindGameObjectWithTag("TileSelector");
        health = _health;
        obj = _obj;
    }
    public virtual void AttackOne()
    {

    }
    public virtual void AttackTwo()
    {

    }
    public virtual void AttackThree()
    {

    }

    public List<Vector3Int> FindPathToNearestPlayer(int maxDistance)
    {
        return null;
    }
}

public class Thrasher : IEnemy
{
    public Thrasher(int health, GameObject obj) : base(health, obj)
    {

    }
    public override void AttackOne()
    {
        Debug.Log("Attack one");
    }
    new public void AttackTwo()
    {

    }
    new public void AttackThree()
    {

    }
}
public class Enemies : MonoBehaviour
{
    public List<Thrasher> enemies;

    void Awake()
    {
        enemies = new List<Thrasher>();
        foreach (Transform child in transform)
        {
            enemies.Add(new Thrasher(3, child.gameObject));
        }
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}