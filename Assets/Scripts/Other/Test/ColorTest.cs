using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour
{

    public Material a;
    public Material b;

    // Start is called before the first frame update
    void Start()
    {
        int c = 0;
        while(c<100)
        {
            float dir = Random.Range(0, 4);
            Debug.Log(dir);
            c++;
        }
        /*Debug.Log(gameObject.GetComponent<MeshRenderer>().material.color);
        Debug.Log(a.color);*/
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(gameObject.GetComponent<MeshRenderer>().material.color == a.color)
        {
            gameObject.GetComponent<MeshRenderer>().material = b;
        } else
        {
            gameObject.GetComponent<MeshRenderer>().material = a;
        }*/
    }
}
