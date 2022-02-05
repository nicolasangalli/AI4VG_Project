using System.Collections;
using UnityEngine;


public class AgentSM : MonoBehaviour
{

    public float reactionTime = 0.1f;
    [HideInInspector]
    public bool nextPathNode;

    private MapGeneration map;
    private FSM fsm;

    private Node prevNode;
    private Node currentNode;
    private Node nextNode;
    private Node targetNode;

    private Edge[] path;
    private int pathCounter;
    private bool noPath;
    private int landmarkOldPositionI;
    private int landmarkOldPositionJ;
    private GameObject midPosition;


    void Start()
    {
        map = GameObject.FindWithTag("Map").GetComponent<MapGeneration>();

        prevNode = null;
        currentNode = null;
        for (int i = 0; i < map.mapArray.GetLength(0); i++)
        {
            for (int j = 0; j < map.mapArray.GetLength(1); j++)
            {
                if(map.mapArray[i,j] == 1)
                {
                    currentNode = GetNodeByMapLocation(i, j);
                    break;
                }
            }
            if(currentNode != null)
            {
                break;
            }
        }
        nextNode = null;
        targetNode = null;

        path = null;
        pathCounter = 0;
        noPath = false;
        nextPathNode = false;
        landmarkOldPositionI = -1;
        landmarkOldPositionJ = -1;
        midPosition = new GameObject();
        midPosition.name = "Mid Target";

        FSMState reach = new FSMState();
        reach.enterActions.Add(ResetPath);
        reach.enterActions.Add(SetTarget);
        reach.stayActions.Add(ReachTarget);
        FSMState cover = new FSMState();
        cover.enterActions.Add(SaveOldTargetPosition);
        cover.enterActions.Add(ResetPath);
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
        nextNode = null;
        if(targetNode != null)
        {
            map.mapArray[targetNode.i, targetNode.j] = 0;
            targetNode = null;
        }
        
        path = null;
        pathCounter = 0;
        noPath = false;
        nextPathNode = false;
    }

    private void SetTarget()
    {
        /*
        if(landmarkOldPositionI != -1)
        {
Debug.Log("old position: " + landmarkOldPositionI + "," + landmarkOldPositionJ);
Debug.Log("mapArray: " + map.mapArray[landmarkOldPositionI, landmarkOldPositionJ]);
targetNode = GetNodeByMapLocation(landmarkOldPositionI, landmarkOldPositionI);
Debug.Log(targetNode.description);
            if(map.mapArray[landmarkOldPositionI, landmarkOldPositionJ] == 0 && targetNode != null)
            {
                map.mapArray[targetNode.i, targetNode.j] = 4;
                map.landmark.transform.position = GetMapLocationFromArray(map.startPoint, targetNode.i, targetNode.j);
                map.landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                map.landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                landmarkOldPositionI = -1;
                landmarkOldPositionJ = -1;
            }
            else
            {
                landmarkOldPositionI = -1;
                landmarkOldPositionJ = -1;
                SetTarget();
            }
        }
        else
        {
        */
            int i = Random.Range(0, map.mapArray.GetLength(0) - 1);
            int j = Random.Range(0, map.mapArray.GetLength(1) - 1);
            while (map.mapArray[i, j] != 4)
            {
                if (map.mapArray[i, j] == 0)
                {
                    targetNode = GetNodeByMapLocation(i, j);
                    map.mapArray[targetNode.i, targetNode.j] = 4;
                    map.landmark.transform.position = GetMapLocationFromArray(map.startPoint, i, j);
                    map.landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                    map.landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    i = Random.Range(0, map.mapArray.GetLength(0) - 1);
                    j = Random.Range(0, map.mapArray.GetLength(1) - 1);
                }
            }
        //}
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

                    midPosition.transform.position = GetMapLocationFromArray(map.startPoint, nextNode.i, nextNode.j);
                    gameObject.GetComponent<KMoveTo>().destination = midPosition.transform;

                    pathCounter++;
                }
                else //target reached
                {
                    prevNode = currentNode;
                    map.mapArray[prevNode.i, prevNode.j] = 0;

                    currentNode = nextNode;
                    map.mapArray[currentNode.i, currentNode.j] = 1;

                    ResetPath();
                    SetTarget();
                }
            }
        }
    }

    private void SaveOldTargetPosition()
    {
        if(targetNode != null)
        {
            landmarkOldPositionI = targetNode.i;
            landmarkOldPositionJ = targetNode.j;
        }
    }

    private void FindCover()
    {
        if (targetNode == null)
        {
            int step = 0;
            while (targetNode == null)
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
                                    targetNode = candidate;
                                    break;
                                }
                            }
                        }
                    }
                    if (targetNode != null)
                    {
                        break;
                    }
                }
                step++;
            }

            map.mapArray[targetNode.i, targetNode.j] = 4;
            map.landmark.transform.position = GetMapLocationFromArray(map.startPoint, targetNode.i, targetNode.j);
            map.landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            map.landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        }
        else
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
            }
        }
    }

    private void StuckMsg()
    {
        Debug.Log("Impossibile raggiungere il bersaglio senza essere scoperti!");
    }

    private void CalcPath()
    {
        AStarSolver.immediateStop = true;
        path = AStarSolver.Solve(map.graph, currentNode, targetNode, ManhattanEstimator);

        if(path.Length == 0) //target non reachable
        {
            noPath = true;
        }
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
        }
        else
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
        }
        else
        {
            return true;
        }
    }

    private Node GetNodeByMapLocation(int i, int j)
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

    private Vector3 GetMapLocationFromArray(Vector3 startPoint, int i, int j)
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
