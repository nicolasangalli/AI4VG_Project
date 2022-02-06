using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGraph : MonoBehaviour
{

    public GameObject nodeGreen;
    public GameObject nodeRed;
    public GameObject nodeBlue;
    public GameObject obstacle;
    public GameObject agent;
    public GameObject sentinel;
    public GameObject landmark;
    [HideInInspector]
    public Vector3 startPoint;
    public int[,] mapArray;
    public Graph graph;

    //private bool marked;


    private Node currentNode;
    private Node targetNode;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        //marked = false;

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

        
        mapArray[7, 6] = 2;
        Vector3 posObstacle = GetMapLocationFromArray(startPoint, 7, 6);
        Instantiate(obstacle, posObstacle, Quaternion.identity);
        /*mapArray[7, 8] = 2;
        posObstacle = GetMapLocationFromArray(startPoint, 7, 8);
        Instantiate(obstacle, posObstacle, Quaternion.identity);*/
        mapArray[8, 8] = 2;
        posObstacle = GetMapLocationFromArray(startPoint, 8, 8);
        Instantiate(obstacle, posObstacle, Quaternion.identity);

        graph = new Graph();
        GenerateGraph();


        Node n = GetNodeByMapLocation(6, 7);
        n.inSentinelView = true;
        //Instantiate(nodeRed, GetMapLocationFromArray(startPoint, n.i, n.j), Quaternion.identity);
        n = GetNodeByMapLocation(7, 7);
        n.inSentinelView = true;
        //Instantiate(nodeRed, GetMapLocationFromArray(startPoint, n.i, n.j), Quaternion.identity);
        n = GetNodeByMapLocation(7, 8);
        n.inSentinelView = true;
        n = GetNodeByMapLocation(8, 6);
        n.inSentinelView = true;
        //Instantiate(nodeRed, GetMapLocationFromArray(startPoint, n.i, n.j), Quaternion.identity);
        n = GetNodeByMapLocation(8, 7);
        n.inSentinelView = true;
        //Instantiate(nodeRed, GetMapLocationFromArray(startPoint, n.i, n.j), Quaternion.identity);

        mapArray[7, 7] = 1;
        currentNode = GetNodeByMapLocation(7, 8);

        FindCover();
        //mapArray[targetNode.i, targetNode.j] = 4;


        mapArray[6, 7] = 3;
        Vector3 posSentinel = GetMapLocationFromArray(startPoint, 6, 7);
        sentinel = Instantiate(sentinel, posSentinel, Quaternion.Euler(0, 90, 0));

        //mapArray[12, 7] = 1;
        //Vector3 posPlayer = GetMapLocationFromArray(startPoint, 12, 7);
        //agent = Instantiate(agent, posPlayer, Quaternion.identity);

        PrintNode();
        DebugPrint();
    }

    void Update()
    {
        sentinel.GetComponent<MeshRenderer>().enabled = true;
        //sentinel.GetComponent<SentinelSM>().showed = true;

        /*
        if(marked == false)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                sentinel.GetComponent<SentinelSM>().MarkNode();
                /*for (int i = 0; i < mapArray.GetLength(0); i++)
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


    private void FindCover()
    {
        List<Node> candidateNodes = new List<Node>();
        int step = 0;
        while (candidateNodes.Count == 0)
        {
            for (int x = currentNode.i - step; x <= currentNode.i + step; x++)
            {
                for (int z = currentNode.j - step; z <= currentNode.j + step; z++)
                {
                    if (x >= 0 && x < mapArray.GetLength(0) && z >= 0 && z < mapArray.GetLength(1))
                    {
                        if (mapArray[x, z] == 0)
                        {
                            Node candidate = GetNodeByMapLocation(x, z);
                            if (candidate.inSentinelView == false)
                            {
                                candidateNodes.Add(candidate);
                            }
                        }
                    }
                }
            }
            step++;
        }

        int index = Random.Range(0, candidateNodes.Count - 1);
        targetNode = candidateNodes[index];
        foreach (Node c in candidateNodes)
        {
            if (c == targetNode)
            {
                Instantiate(nodeGreen, GetMapLocationFromArray(startPoint, c.i, c.j), Quaternion.identity);
            }
            else
            {
                Instantiate(nodeBlue, GetMapLocationFromArray(startPoint, c.i, c.j), Quaternion.identity);
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
        /*GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach(GameObject go in nodes)
        {
            DestroyImmediate(go);
        }*/

        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                Vector3 pos = GetMapLocationFromArray(startPoint, i, j);
                if(mapArray[i,j] != 2)
                {
                    Node n = GetNodeByMapLocation(i, j);
                    if(n.inSentinelView)
                    {
                        Instantiate(nodeRed, pos, Quaternion.identity);
                    } else
                    {
                        if(mapArray[i,j] == 4)
                        {
                            Instantiate(nodeBlue, pos, Quaternion.identity);
                        }
                        else
                        {
                            //Instantiate(nodeGreen, pos, Quaternion.identity);
                        }
                        
                    }
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
