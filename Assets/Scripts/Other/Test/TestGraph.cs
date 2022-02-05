using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGraph : MonoBehaviour
{

    public GameObject nodeGreen;
    public GameObject nodeRed;
    public GameObject nodeBlue;
    public GameObject agent;
    public GameObject sentinel;
    public GameObject landmark;
    [HideInInspector]
    public Vector3 startPoint;
    public int[,] mapArray;
    public Graph graph;

    private bool marked;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        marked = false;

        float xSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.x;
        float zSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.z;
        Vector3 mapDimension = new Vector3(xSize * transform.localScale.x, 0f, zSize * transform.localScale.z);

        mapArray = new int[(int)mapDimension.x, (int)mapDimension.z];
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                mapArray[i, j] = 0;
            }
        }

        startPoint = new Vector3(transform.position.x - mapDimension.x / 2 + 0.5f, 0f, transform.position.z - mapDimension.z / 2 + 0.5f);

        graph = new Graph();
        GenerateGraph();
        //PrintNode();

        mapArray[6, 7] = 3;
        Vector3 posSentinel = GetMapLocationFromArray(startPoint, 6, 7);
        sentinel = Instantiate(sentinel, posSentinel, Quaternion.Euler(0, 90, 0));

        mapArray[12, 7] = 1;
        Vector3 posPlayer = GetMapLocationFromArray(startPoint, 12, 7);
        agent = Instantiate(agent, posPlayer, Quaternion.identity);

        DebugPrint();

        Vector3 a = sentinel.transform.forward * 2;
        //Debug.Log(a);
        Vector3 nPosition = GetMapLocationFromArray(startPoint, 7, 7);
        Vector3 b = nPosition - sentinel.transform.position;
        //Debug.Log(b);
        float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
        //Debug.Log(angle);
    }

    void Update()
    {
        sentinel.GetComponent<MeshRenderer>().enabled = true;
        sentinel.GetComponent<SentinelSM>().showed = true;
        /*
        if(marked == false)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //sentinel.GetComponent<SentinelSM>().MarkNode();
                for (int i = 0; i < mapArray.GetLength(0); i++)
                {
                    for (int j = 0; j < mapArray.GetLength(1); j++)
                    {
                        Node n = GetNodeByMapLocation(i, j);
                        if(n.inSentinelView)
                        {
                            n.sceneObject = Instantiate(nodeRed, GetMapLocationFromArray(startPoint, i, j), Quaternion.identity);
                        }
                    }
                }

                Vector3 vector = sentinel.transform.forward * 2;
                for (int i = -90 / 2; i <= 90 / 2; i = i + 2)
                {
                    Vector3 v = Quaternion.AngleAxis(i, sentinel.transform.up) * vector;
                    Ray ray = new Ray(sentinel.transform.position, v);
                    RaycastHit hit;
                    bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

                    
                    if (hitted == true && hit.collider.gameObject.tag == "Node")
                    {
                        Node n = graph.GetNodeByGameobject(hit.collider.gameObject);
                        mapArray[n.i, n.j] = -1;
                        DestroyImmediate(n.sceneObject);
                    }
                    
                }

                GameObject go = GameObject.FindWithTag("Node");
                Node sNode = graph.GetNodeByGameobject(go);
                mapArray[sNode.i, sNode.j] = -1;
                DestroyImmediate(go);

                

                        PrintNode();
                        DebugPrint();
                marked = true;
            }
        }*/
        
    }

    private void GenerateGraph()
    {
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                if (mapArray[i, j] == 0)
                {
                    Node n = GetNodeByMapLocation(i, j);
                    if (n == null)
                    {
                        n = new Node("(" + i + "," + j + ")", i, j);
                        graph.AddNode(n);
                    }
                    for (int xn = i - 1; xn <= i + 1; xn++)
                    {
                        for (int zn = j - 1; zn <= j + 1; zn++)
                        {
                            if (xn >= 0 && xn < mapArray.GetLength(0) && zn >= 0 && zn < mapArray.GetLength(1))
                            {
                                if (mapArray[xn, zn] == 0)
                                {
                                    Node neig = GetNodeByMapLocation(xn, zn);

                                    if (neig == null)
                                    {
                                        neig = new Node("(" + xn + "," + zn + ")", xn, zn);
                                        graph.AddNode(neig);
                                    }

                                    if (!n.description.Equals(neig.description))
                                    {
                                        if (i != neig.i && j != neig.j)
                                        {
                                            Edge e = new Edge(n, neig, Mathf.Sqrt(2));
                                            graph.AddEdge(e);
                                        }
                                        else
                                        {
                                            Edge e = new Edge(n, neig);
                                            graph.AddEdge(e);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Node GetNodeByMapLocation(int i, int j)
    {
        Node[] nodes = graph.getNodes();
        foreach (Node n in nodes)
        {
            if (n.i == i && n.j == j)
            {
                return n;
            }
        }
        return null;
    }

    //gets position from mapArray indexes
    public Vector3 GetMapLocationFromArray(Vector3 startPoint, int i, int j)
    {
        return new Vector3(startPoint.x + i, 0.25f, startPoint.z + j);
    }

    private void PrintNode()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach(GameObject go in nodes)
        {
            DestroyImmediate(go);
        }

        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                Vector3 pos = GetMapLocationFromArray(startPoint, i, j);
                if(mapArray[i,j] >= 0)
                {
                    if(i == 7 && j == 7)
                    {
                        Instantiate(nodeBlue, pos, Quaternion.identity);
                    } else
                    {
                        Instantiate(nodeGreen, pos, Quaternion.identity);
                    }
                    
                } else
                {
                    //Instantiate(nodeRed, pos, Quaternion.identity);
                }
                
            }
        }
    }

    private void DebugPrint()
    { 
        string print = "";
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                print += mapArray[i, j] + " ";

            }
            print += "\n";
        }
        Debug.Log(print);

    }

}
