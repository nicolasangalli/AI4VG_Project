using System.Collections.Generic;
using UnityEngine;


public class MapGeneration : MonoBehaviour
{

    public GameObject obstacle; //prefab
    public GameObject agent; //istance
    public GameObject sentinel; //prefab
    public GameObject landmark; //istance
    [Range(0, 3)]
    public int nSentinel = 3;

    [HideInInspector]
    public Vector3 startPoint;
    public int[,] mapArray;
    public Graph graph;

    private int obstacleProb;
    private bool mapGenerated;

   
    void Start()
    {
        Application.targetFrameRate = 60;

        obstacleProb = 35;
        mapGenerated = false;

        float xSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.x;
        float zSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.size.z;
        Vector3 mapDimension = new Vector3(xSize * transform.localScale.x, 0f, zSize * transform.localScale.z);

        mapArray = new int[(int)mapDimension.x, (int)mapDimension.z];
        for(int i = 0; i < mapArray.GetLength(0); i++)
        {
            for(int j = 0; j < mapArray.GetLength(1); j++)
            {
                mapArray[i, j] = 0;
            }
        }

        startPoint = new Vector3(transform.position.x - mapDimension.x / 2 + 0.5f, 0f, transform.position.z - mapDimension.z / 2 + 0.5f);
        GenerateObstacles(startPoint);

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
        for(int i = 1; i < mapArray.GetLength(0)-1; i++)
        {
            for(int j = 1; j < mapArray.GetLength(1)-1; j++)
            {
                if(mapArray[i,j] == 0) //check if the point is not already visited
                {
                    int x = i;  //point visited at this step k
                    int z = j;  //point visited at this step k
                    int prevX = i;  //point visited in the prev step of k
                    int prevZ = j;  //point visited in the prev step of k
                    int direction = Random.Range(1, 5); //direction of multidimensional obstacle creation (N, E, S, W)

                    int obstacleLength = Random.Range(1, 3); //choose the obstacle length
                    for(int k = 1; k <= obstacleLength; k++)
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

        for(int i = 0; i < mapArray.GetLength(0); i++)
        {
            List<int> list = new List<int>();
            for(int j = 0; j < mapArray.GetLength(1); j++)
            {
                if(mapArray[i, j] == 1)
                {
                    list.Add(j);
                }
                else
                {
                    if(list.Count > 1)
                    {
                        foreach(int elem in list)
                        {
                            mapArray[i, elem] = 2;
                        }

                        obstacle.transform.localScale = new Vector3(0.5f, 0.5f, 1f * list.Count);
                        Instantiate(obstacle, new Vector3(startPoint.x + i, 0.25f, startPoint.z + Avg(list)), Quaternion.identity);
                    }
                    list.Clear();
                }
            }
        }

        for(int j = 0; j < mapArray.GetLength(1); j++)
        {
            List<int> list = new List<int>();
            for(int i = 0; i < mapArray.GetLength(0); i++)
            {
                if(mapArray[i, j] == 1)
                {
                    list.Add(i);
                }
                else
                {
                    if(list.Count > 0)
                    {
                        foreach(int elem in list)
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

        for(int i = 0; i < mapArray.GetLength(0); i++)
        {
            for(int j = 0; j < mapArray.GetLength(1); j++)
            {
                if(mapArray[i, j] == 1)
                {
                    mapArray[i, j] = 2;
                }
            }
        }
    }

    //check if all neighbours of point (x,z) (except the point (prevX, prevZ)) are already visited
    private bool FreePoint(int x, int z, int prevX, int prevZ)
    {
        for(int i = x-1; i <= x+1; i++)
        {
            for(int j = z-1; j <= z+1; j++)
            {
                if(i >= 0 && i < mapArray.GetLength(0) && j >= 0 && j < mapArray.GetLength(1))
                {
                    if(mapArray[i,j] != 0 && i != x && i != prevX && j != z && j != prevZ)
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
        }
        else
        {
            return -1f;
        }
    }

    //generates graph for pathfinding
    private void GenerateGraph()
    {
        for(int i = 0; i < mapArray.GetLength(0); i++)
        {
            for(int j = 0; j < mapArray.GetLength(1); j++)
            {
                if(mapArray[i,j] == 0)
                {
                    Node n = GetNodeByMapLocation(i, j);
                    if(n == null)
                    {
                        n = new Node("(" + i + "," + j + ")", i, j);
                        graph.AddNode(n);
                    }
                    for(int xn = i-1; xn <= i+1; xn++)
                    {
                        for(int zn = j-1; zn <= j+1; zn++)
                        {
                            if(xn >= 0 && xn < mapArray.GetLength(0) && zn >= 0 && zn < mapArray.GetLength(1))
                            {
                                if(mapArray[xn, zn] == 0)
                                {
                                    Node neig = GetNodeByMapLocation(xn, zn);
                                   
                                    if(neig == null)
                                    {
                                        neig = new Node("(" + xn + "," + zn + ")", xn, zn);
                                        graph.AddNode(neig);
                                    }

                                    if(!n.description.Equals(neig.description))
                                    {
                                        if(i != neig.i && j != neig.j)
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

    //set player, sentinels and landmark in the map
    private void GenerateCharacters()
    {
        while(GameObject.FindGameObjectsWithTag("Sentinel").Length < nSentinel)
        {
            int i = Random.Range(0, mapArray.GetLength(0));
            int j = Random.Range(0, mapArray.GetLength(1));

            if(mapArray[i, j] == 0)
            {
                Vector3 sentinelPos = GetMapLocationFromArray(startPoint, i, j);
                bool enoughDistance = EnoughDistance(sentinelPos);
                if(enoughDistance)
                {
                    GameObject s = Instantiate(sentinel, sentinelPos, Quaternion.identity);
                    float angle = 0f;
                    int dir = Random.Range(0, 4);
                    if(dir == 0)
                    {
                        angle = -90f;
                    }
                    else if(dir == 2)
                    {
                        angle = 90f;
                    }
                    else if(dir == 3)
                    {
                        angle = 180f;
                    }
                    s.transform.rotation = Quaternion.Euler(0, angle, 0);
                    mapArray[i, j] = 3;
                }
            }
        }

        while(GameObject.FindWithTag("Agent") == null)
        {
            int i = Random.Range(0, mapArray.GetLength(0));
            int j = Random.Range(0, mapArray.GetLength(1));

            if(mapArray[i, j] == 0)
            {
                Vector3 agentPos = GetMapLocationFromArray(startPoint, i, j);
                bool enoughDistance = EnoughDistance(agentPos);
                if(enoughDistance)
                {
                    agent = Instantiate(agent, GetMapLocationFromArray(startPoint, i, j), Quaternion.identity);
                    mapArray[i, j] = 1;
                }
            }
        }

        landmark = Instantiate(landmark, new Vector3(100, 100, 100), Quaternion.identity);
    }

    //check if there is enough distance from a pos and the other sentinels
    private bool EnoughDistance(Vector3 myPos)
    {
        GameObject[] sentinels = GameObject.FindGameObjectsWithTag("Sentinel");
        if(sentinels.Length > 0)
        {
            foreach(GameObject otherSentinel in sentinels)
            {
                float distance = (otherSentinel.transform.position - myPos).magnitude;
                if(distance <= otherSentinel.GetComponent<SentinelSM>().distanceOfView + 2f)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return true;
        }
    }

    //gets node from mapArray indexes
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

}
