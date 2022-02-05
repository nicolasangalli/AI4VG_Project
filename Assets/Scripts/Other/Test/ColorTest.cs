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
        Debug.Log(gameObject.GetComponent<MeshRenderer>().material.color);
        Debug.Log(a.color);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.GetComponent<MeshRenderer>().material.color == a.color)
        {
            gameObject.GetComponent<MeshRenderer>().material = b;
        } else
        {
            gameObject.GetComponent<MeshRenderer>().material = a;
        }
    }
}
