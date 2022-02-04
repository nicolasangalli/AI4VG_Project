using System.Collections.Generic;
using UnityEngine;


public class MapGeneration : MonoBehaviour
{
    public GameObject obstacle; //prefab
    [Range(0, 100)]
    public int obstacleProb = 35;
    public GameObject agent; //istance
    public GameObject sentinel; //prefab
    [Range(1, 3)]
    public int nSentinel = 3;
    public GameObject landmark; //istance

    [HideInInspector]
    public Vector3 startPoint;
    public int[,] mapArray;
    public Graph graph;

    private bool mapGenerated;

    //debug var
    public GameObject node;
    private Edge[] path;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        mapGenerated = false;

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
        GenerateObstacles(startPoint);

        //debug
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                if (mapArray[i, j] == 1)
                {
                    mapArray[i, j] = 2;
                }
            }
        }

        graph = new Graph();
        GenerateGraph();

        mapGenerated = true;     
    }

    void Update()
    {
        if(mapGenerated)
        {
            GenerateCharacters();

            gameObject.GetComponent<MapSM>().enabled = true;
            sentinel.GetComponent<SentinelSM>().enabled = true;
            agent.GetComponent<KMoveTo>().enabled = true;
            agent.GetComponent<AgentSM>().enabled = true;
            
            mapGenerated = false;
        }
    }

    //generates random position for obstacles and instantiate them
    private void GenerateObstacles(Vector3 startPoint)
    {
        for(int i=1; i<mapArray.GetLength(0)-1; i++)
        {
            for(int j=1; j<mapArray.GetLength(1)-1; j++)
            {
                if(mapArray[i,j] == 0) //check if the point is not already visited
                {
                    int x = i;  //point visited at this step k
                    int z = j;  //point visited at this step k
                    int prevX = i;  //point visited in the prev step of k
                    int prevZ = j;  //point visited in the prev step of k
                    int direction = Random.Range(1, 4); //direction of multidimensional obstacle creation (N, E, S, W)

                    int obstacleLength = Random.Range(1, 3); //choose the obstacle length
                    for(int k=1; k<=obstacleLength; k++)
                    {
                        if(FreePoint(x, z, prevX, prevZ))
                        {
                            int n = Random.Range(0, 100);
                            if(n < obstacleProb)
                            {
                                mapArray[x, z] = 1;

                                prevX = x;
                                prevZ = z;

                                if(direction == 1)
                                {
                                    z += 1;
                                }
                                else if(direction == 2)
                                {
                                    x += 1;
                                }
                                else if(direction == 3)
                                {
                                    z -= 1;
                                }
                                else if(direction == 4)
                                {
                                    x -= 1;
                                }
                                
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        for (int i=0; i<mapArray.GetLength(0); i++)
        {
            List<int> list = new List<int>();
            for (int j=0; j<mapArray.GetLength(1); j++)
            {
                if (mapArray[i, j] == 1)
                {
                    list.Add(j);
                }
                else
                {
                    if (list.Count > 1)
                    {
                        foreach (int elem in list)
                        {
                            mapArray[i, elem] = 2;
                        }

                        //obstacle.transform.parent = gameObject.transform;
                        obstacle.transform.localScale = new Vector3(0.5f, 0.5f, 1f * list.Count);
                        Instantiate(obstacle, new Vector3(startPoint.x + i, 0.25f, startPoint.z + Avg(list)), Quaternion.identity);
                    }
                    list.Clear();
                }
            }
        }

        for (int j=0; j<mapArray.GetLength(1); j++)
        {
            List<int> list = new List<int>();
            for (int i=0; i<mapArray.GetLength(0); i++)
            {
                if (mapArray[i, j] == 1)
                {
                    list.Add(i);
                }
                else
                {
                    if (list.Count > 0)
                    {
                        foreach (int elem in list)
                        {
                            mapArray[elem, j] = 2;
                        }

                        obstacle.transform.localScale = new Vector3(1f * list.Count, 0.5f, 0.5f);
                        Instantiate(obstacle, new Vector3(startPoint.x + Avg(list), 0.25f, startPoint.z + j), Quaternion.identity);
                    }
                    list.Clear();
                }
            }
        }
    }

    //check if all neighbours of point (x,z) (except the point (prevX, prevZ)) are already visited
    private bool FreePoint(int x, int z, int prevX, int prevZ)
    {
        for(int i=x-1; i<=x+1; i++)
        {
            for(int j=z-1; j<=z+1; j++)
            {
                if(i>=0 && i<mapArray.GetLength(0) && j>=0 && j<mapArray.GetLength(1))
                {
                    if (mapArray[i,j] != 0 && i!=x && i!=prevX && j!=z && j!=prevZ)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private float Avg(List<int> list)
    {
        float avg = 0f;
        foreach(int n in list)
        {
            avg += n;
        }
        if(list.Count != 0)
        {
            return avg / list.Count;
        } else
        {
            return -1f;
        }
    }

    //generates graph for pathfinding
    private void GenerateGraph()
    {
        for (int i = 0; i < mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < mapArray.GetLength(1); j++)
            {
                if(mapArray[i,j] == 0)
                {
                    Node n = GetNodeByMapLocation(i, j);
                    if(n == null)
                    {
                        n = new Node("(" + i + "," + j + ")", i, j);
                        graph.AddNode(n);
                    }
                    for (int xn=i-1; xn<=i+1; xn++)
                    {
                        for (int zn=j-1; zn<=j+1; zn++)
                        {
                            if (xn>=0 && xn<mapArray.GetLength(0) && zn>=0 && zn<mapArray.GetLength(1))
                            {
                                if (mapArray[xn, zn]==0)
                                {
                                    Node neig = GetNodeByMapLocation(xn, zn);
                                   
                                    if(neig == null)
                                    {
                                        neig = new Node("(" + xn + "," + zn + ")", xn, zn);
                                        graph.AddNode(neig);
                                    }

                                    if (!n.description.Equals(neig.description))
                                    {
                                        if(i != neig.i && j != neig.j)
                                        {
                                            Edge e = new Edge(n, neig, Mathf.Sqrt(2));
                                            graph.AddEdge(e);
                                        } else
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

    //gets node from mapArray indexes
    public Node GetNodeByMapLocation(int i, int j)
    {
        Node[] nodes = graph.getNodes();
        foreach(Node n in nodes)
        {
            if(n.i == i && n.j == j)
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

    //set player, sentinels and landmark in the map
    private void GenerateCharacters()
    {
        while(GameObject.FindWithTag("Agent") == null)
        {
            int i = Random.Range(0, mapArray.GetLength(0) - 1);
            int j = Random.Range(0, mapArray.GetLength(1) - 1);
            if (mapArray[i, j] == 0)
            {
                agent = Instantiate(agent, GetMapLocationFromArray(startPoint, i, j), Quaternion.identity);
                mapArray[i, j] = 1;
            }
        }

        while(GameObject.FindGameObjectsWithTag("Sentinel").Length < nSentinel)
        {
            int i = Random.Range(0, mapArray.GetLength(0) - 1);
            int j = Random.Range(0, mapArray.GetLength(1) - 1);
            if (mapArray[i, j] == 0)
            {
                Vector3 sentinelPos = GetMapLocationFromArray(startPoint, i, j);
                GameObject s = Instantiate(sentinel, sentinelPos, Quaternion.identity);
                mapArray[i, j] = 3;

                Ray nRay = new Ray(sentinelPos, s.transform.forward);
                float nDistance = 0f;
                Ray eRay = new Ray(sentinelPos, s.transform.right);
                float eDistance = 0f;
                Ray sRay = new Ray(sentinelPos, -s.transform.forward);
                float sDistance = 0f;
                Ray wRay = new Ray(sentinelPos, -s.transform.right);
                float wDistance = 0f;

                RaycastHit nHit;
                bool nHitted = Physics.Raycast(nRay, out nHit, s.GetComponent<SentinelSM>().distanceOfView);
                if (nHitted == true && nHit.collider.gameObject.tag == "Obstacle")
                {
                    nDistance = (nHit.collider.gameObject.transform.position - s.transform.position).magnitude;
                } else
                {
                    nDistance = float.MaxValue;
                }
                RaycastHit eHit;
                bool eHitted = Physics.Raycast(eRay, out eHit, s.GetComponent<SentinelSM>().distanceOfView);
                if (eHitted == true && eHit.collider.gameObject.tag == "Obstacle")
                {
                    eDistance = (eHit.collider.gameObject.transform.position - s.transform.position).magnitude;
                }
                else
                {
                    eDistance = float.MaxValue;
                }
                RaycastHit sHit;
                bool sHitted = Physics.Raycast(sRay, out sHit, s.GetComponent<SentinelSM>().distanceOfView);
                if (sHitted == true && sHit.collider.gameObject.tag == "Obstacle")
                {
                    sDistance = (sHit.collider.gameObject.transform.position - s.transform.position).magnitude;
                }
                else
                {
                    sDistance = float.MaxValue;
                }
                RaycastHit wHit;
                bool wHitted = Physics.Raycast(wRay, out wHit, s.GetComponent<SentinelSM>().distanceOfView);
                if (wHitted == true && wHit.collider.gameObject.tag == "Obstacle")
                {
                    wDistance = (wHit.collider.gameObject.transform.position - s.transform.position).magnitude;
                }
                else
                {
                    wDistance = float.MaxValue;
                }

                float maxVal = -1f;
                if(nDistance>maxVal)
                {
                    maxVal = nDistance;
                }
                if (eDistance > maxVal)
                {
                    maxVal = eDistance;
                }
                if(sDistance > maxVal)
                {
                    maxVal = sDistance;
                }
                if(wDistance > maxVal)
                {
                    maxVal = wDistance;
                }

                if(maxVal == eDistance)
                {
                    s.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
                else if(maxVal == sDistance)
                {
                    s.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else if(maxVal == wDistance)
                {
                    s.transform.rotation = Quaternion.Euler(0, -90, 0);
                }

            }
        }

        landmark = Instantiate(landmark, new Vector3(100, 100, 100), Quaternion.identity);
    }


    public void DebugPrint()
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

        /*
        Node[] nodes = graph.getNodes();
        foreach (Node n in nodes)
        {
            Vector3 pos = GetMapLocationFromArray(startPoint, n.i, n.j);
            Instantiate(node, pos, Quaternion.identity);
            Edge[] edges = graph.getConnections(n);
            //Debug.Log("neig: " + edges.Length);
            foreach (Edge e in edges)
            {
                Vector3 posNeig = GetMapLocationFromArray(startPoint, e.to.i, e.to.j);
                Debug.DrawLine(pos, posNeig, Color.red, 1000f);
                //Debug.Log(e.from.description + " - " + e.to.description + " - " + e.weight);
            }
        }
        /*
        Debug.Log(path.Length);
        if(path.Length > 0)
        {
            print = "(" + path[0].from.i + "," + path[0].from.j + ")";
            Vector3 pos = GetMapLocationFromArray(startPoint, path[0].from.i, path[0].from.j);
            Instantiate(node, pos, Quaternion.identity);
            foreach (Edge e in path)
            {
                print += " => (" + e.to.i + "," + e.to.j + ")";
                pos = GetMapLocationFromArray(startPoint, e.to.i, e.to.j);
                Instantiate(node, pos, Quaternion.identity);
            }
            Debug.Log(print);
        }
         */
    }

}
