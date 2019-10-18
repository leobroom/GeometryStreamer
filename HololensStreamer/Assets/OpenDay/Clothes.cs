using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clothes : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DoSomething", 2);//this will happen after 2 seconds
    }

    // Update is called once per frame
    void DoSomething()
    {
      //  Debug.Log("DoSomething");
      //var  loth = GetComponent<Cloth>();
     
      //  loth.enabled = false;
    }
}
