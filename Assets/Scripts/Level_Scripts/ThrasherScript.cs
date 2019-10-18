using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ThrasherScript : MonoBehaviour
{
    // Start is called before the first frame update

    public Thrasher thrasher;
    void Start()
    {
        thrasher = new Thrasher(3, transform.gameObject);
        //thrasher.AttackOne();
    }
}
