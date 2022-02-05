using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSM : MonoBehaviour
{

    public float reactionTime = 0.2f;

    [HideInInspector]
    public MapGeneration map;
    [HideInInspector]
    public Edge[] path;
    [HideInInspector]
    public int pathCounter;


    private Node prevNode;
    private Node currentNode;
    private Node nextNode;
    private Node coverNode;
    private int landmarkOldPositionI;
    private int landmarkOldPositionJ;
    private GameObject landmark;
    private GameObject midTarget;
    private bool noPath;

    public bool nextPathNode;

    private FSM fsm;
    
    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.FindWithTag("Map").GetComponent<MapGeneration>();
        landmark = map.landmark;
        midTarget = new GameObject();
        midTarget.name = "Mid Target";

        path = null;
        pathCounter = 0;
        prevNode = null;
        currentNode = null;
        nextNode = null;
        coverNode = null;
        landmarkOldPositionI = -1;
        landmarkOldPositionJ = -1;
        noPath = false;

        nextPathNode = false;
        
        FSMState reach = new FSMState();
        reach.enterActions.Add(ResetPath);
        reach.enterActions.Add(SetTarget);
        reach.stayActions.Add(ReachTarget);
        FSMState cover = new FSMState();
        cover.enterActions.Add(ResetPath);
        cover.enterActions.Add(SaveOldTargetPosition);
        cover.stayActions.Add(FindCover);
        cover.exitActions.Add(UpdateGraph);
        FSMState stuck = new FSMState();
        stuck.enterActions.Add(ResetPath);
        stuck.enterActions.Add(StuckMsg);

        FSMTransition t1 = new FSMTransition(MapAlarm);
        FSMTransition t2 = new FSMTransition(MapNotAlarm);
        FSMTransition t3 = new FSMTransition(NoPath);

        reach.AddTransition(t1, cover);
        cover.AddTransition(t2, reach);
        reach.AddTransition(t3, stuck);

        fsm = new FSM(reach);

        StartCoroutine(Patrol());
    }

    private void ResetPath()
    {
        path = null;
        pathCounter = 0;
        nextPathNode = false;
        coverNode = null;
    }

    private void SetTarget()
    {
        if(landmarkOldPositionI != -1) //rimmetto come target il landmark precedente all'entrata nello stato cover (se possibile)
        {
            if(map.mapArray[landmarkOldPositionI, landmarkOldPositionJ] == 0)
            {
                landmark.transform.position = GetMapLocationFromArray(map.startPoint, landmarkOldPositionI, landmarkOldPositionJ);
//landmark.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                map.mapArray[landmarkOldPositionI, landmarkOldPositionJ] = 4;
                landmarkOldPositionI = -1;
                landmarkOldPositionJ = -1;
            } else
            {
                landmarkOldPositionI = -1;
                landmarkOldPositionJ = -1;
                SetTarget();
            }
        }
        else
        {
            int i = Random.Range(0, map.mapArray.GetLength(0) - 1);
            int j = Random.Range(0, map.mapArray.GetLength(1) - 1);
            while (map.mapArray[i, j] != 4)
            {
                if (map.mapArray[i, j] == 0)
                {
                    landmark.transform.position = GetMapLocationFromArray(map.startPoint, i, j);
//landmark.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                    map.mapArray[i, j] = 4;
                }
                else
                {
                    i = Random.Range(0, map.mapArray.GetLength(0) - 1);
                    j = Random.Range(0, map.mapArray.GetLength(1) - 1);
                }
            }

            Debug.Log("target new position: (" + i + "," + j + ")");
            map.DebugPrint();
        }
    }

    private void ReachTarget()
    {
        if (path == null)
        {   
            CalcPath();
            nextPathNode = true;
        }
        else
        {
            if(nextPathNode == true)
            {
                nextPathNode = false;
                if(pathCounter < path.Length)
                {
                    currentNode = path[pathCounter].from;
                    map.mapArray[currentNode.i, currentNode.j] = 1;

                    if (pathCounter > 0)
                    {
                        prevNode = path[pathCounter - 1].from;
                        map.mapArray[prevNode.i, prevNode.j] = 0;
                    }
                    nextNode = path[pathCounter].to;

if(prevNode != null)
{
Debug.Log("prev: " + prevNode.description + " - current: " + currentNode.description + " - next: " + nextNode.description);
} else
{
Debug.Log("prev: null - current: " + currentNode.description + " - next: " + nextNode.description);
}                 


                    midTarget.transform.position = GetMapLocationFromArray(map.startPoint, nextNode.i, nextNode.j);
                    gameObject.GetComponent<KMoveTo>().destination = midTarget.transform;
                    pathCounter++;
                } else //target reached
                {
                    prevNode = currentNode;
                    map.mapArray[prevNode.i, prevNode.j] = 0;

                    currentNode = nextNode;
                    map.mapArray[currentNode.i, currentNode.j] = 1;

                    nextNode = null;

Debug.Log("prev: " + prevNode.description + " - current: " + currentNode.description + " - next: null");

                    ResetPath();
                    SetTarget();
                }
            }
        }
    }

    private void SaveOldTargetPosition()
    {
        int iOld = -1;
        int jOld = -1;
        for (int i = 1; i < map.mapArray.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < map.mapArray.GetLength(1) - 1; j++)
            {
                if (map.mapArray[i, j] == 4)
                {
                    map.mapArray[i, j] = 0;
                    iOld = i;
                    jOld = j;
                    break;
                }
            }
            if(iOld != -1)
            {
                break;
            }
        }

        landmarkOldPositionI = iOld;
        landmarkOldPositionJ = jOld;
        Debug.Log("old target " + iOld + "," + jOld);
    }

    private void FindCover()
    {
        if(nextNode != null)
        {
            Debug.Log("find cover from: " + nextNode.description);
        }
        

        if (coverNode == null)
        {
            if (currentNode == null) //case: at start the agent is already in the sentinel zone
            {
                for (int i = 0; i < map.mapArray.GetLength(0); i++)
                {
                    for (int j = 0; j < map.mapArray.GetLength(1); j++)
                    {
                        if (map.mapArray[i, j] == 1)
                        {
                            currentNode = GetNodeByMapLocation(i, j);
                            break;
                        }
                    }
                    if (currentNode != null)
                    {
                        break;
                    }
                }
                prevNode = currentNode;
            }
            else
            {
                prevNode = currentNode;
                if (nextNode != null)
                {
                    currentNode = nextNode;
                }
                nextNode = null;
            }

            int step = 0;
            while (coverNode == null)
            {
                for (int x = currentNode.i - step; x <= currentNode.i + step; x++)
                {
                    for (int z = currentNode.j - step; z <= currentNode.j + step; z++)
                    {
                        if (x >= 0 && x < map.mapArray.GetLength(0) && z >= 0 && z < map.mapArray.GetLength(1))
                        {
                            if (map.mapArray[x, z] == 0)
                            {
                                Node candidate = GetNodeByMapLocation(x, z);
                                if (candidate.inSentinelView == false)
                                {
                                    coverNode = candidate;
                                    break;
                                }
                            }
                        }
                    }
                    if (coverNode != null)
                    {
                        break;
                    }
                }
                step++;
            }

            //coverNode = GetNodeByMapLocation(0, 0);

            for (int i = 0; i < map.mapArray.GetLength(0); i++)
            {
                for (int j = 0; j < map.mapArray.GetLength(1); j++)
                {
                    if (map.mapArray[i, j] == 4)
                    {
                        map.mapArray[i, j] = 0;
                    }
                }
            }

            map.mapArray[coverNode.i, coverNode.j] = 4;
            landmark.transform.position = GetMapLocationFromArray(map.startPoint, coverNode.i, coverNode.j);
//landmark.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        } else
        {
            ReachTarget();
        }
    }

    private void UpdateGraph()
    {
        Node[] nodes = map.graph.getNodes();
        foreach(Node n in nodes)
        {
            if(n.inSentinelView)
            {
                map.mapArray[n.i, n.j] = -1;
                map.graph.RemoveNode(n);
                //map.DebugPrint();
            }
        }
        Debug.Log("update graph");
        map.DebugPrint();

        currentNode = coverNode;
        map.mapArray[currentNode.i, currentNode.j] = 1;
    }

    private void StuckMsg()
    {
        Debug.Log("Impossibile raggiungere il bersaglio senza essere scoperti!");
    }

    private void CalcPath()
    {

        //int iStart = -1;
        //int jStart = -1;
        int iFinish = -1;
        int jFinish = -1;
        for (int i = 0; i < map.mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < map.mapArray.GetLength(1); j++)
            {
                if (map.mapArray[i, j] == 1)
                {
                    //iStart = i;
                    //jStart = j;
                }
                if (map.mapArray[i, j] == 4)
                {
                    iFinish = i;
                    jFinish = j;
                }
            }
        }

        //Node start = GetNodeByMapLocation(iStart, jStart);
        Node finish = GetNodeByMapLocation(iFinish, jFinish);
        AStarSolver.immediateStop = true;
        path = AStarSolver.Solve(map.graph, currentNode, finish, ManhattanEstimator);

        if(path.Length == 0) //target non reachable
        {
            noPath = true;
        }
        //Debug.Log("(" + path[0].from.i + "," + path[0].from.j + ") => (" + path[path.Length - 1].to.i + "," + path[path.Length - 1].to.j + ")");
    }

    private float ManhattanEstimator(Node from, Node to)
    {
        Vector3 posFrom = GetMapLocationFromArray(map.startPoint, from.i, from.j);
        Vector3 posTo = GetMapLocationFromArray(map.startPoint, to.i, to.j);

        float estimation = Mathf.Abs(posFrom.x - posTo.x) + Mathf.Abs(posFrom.z - posFrom.z);
        return estimation;
    }

    private bool MapAlarm()
    {
        if(map.gameObject.GetComponent<MapSM>().GetActiveSentinels().Count > 0)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private bool MapNotAlarm()
    {
        return !MapAlarm();
    }

    private bool NoPath()
    {
        if(noPath == false)
        {
            return false;
        } else
        {
            return true;
        }
    }

    //gets node from mapArray indexes
    public Node GetNodeByMapLocation(int i, int j)
    {
        Node[] nodes = map.graph.getNodes();
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

    private IEnumerator Patrol()
    {
        while (true)
        {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

}
