using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentSM : MonoBehaviour
{

    public float reactionTime = 0.1f;
    public Material quietMaterial;
    public Material alarmMaterial;
    [HideInInspector]
    public bool nextPathNode;

    private MapGeneration map;
    private FSM fsm;

    private Node prevNode; //previous node in the path
    private Node currentNode; //current node in the graph / path
    private Node nextNode; //next node in the path
    private Node targetNode; //current target (standard or cover)
    private Node oldTargetNode; //old target before entering cover state

    private Edge[] path; //path to reach the target
    private int pathCounter; //path step
    private bool noPath; //no possible path between currentNode and targetNode
    private GameObject midPosition; //mid step position
    private bool resumeOldTarget; //if true the agent try to reach the old target
    private bool coverReached; //cover target reachead
    private FSMState cover;


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
        oldTargetNode = null;

        path = null;
        pathCounter = 0;
        noPath = false;
        nextPathNode = false;
        midPosition = new GameObject();
        midPosition.name = "Mid Target";
        resumeOldTarget = false;
        coverReached = false;

        FSMState reach = new FSMState();
        reach.enterActions.Add(ChangeColor);
        reach.enterActions.Add(ResetPath);
        reach.enterActions.Add(SetTarget);
        reach.stayActions.Add(ReachTarget);
        reach.exitActions.Add(SaveOldTarget);

        cover = new FSMState();
        cover.enterActions.Add(ChangeColor);
        cover.enterActions.Add(ResetPath);
        cover.enterActions.Add(FindCover);
        cover.stayActions.Add(ReachTarget);
        cover.exitActions.Add(UpdateGraph);
        cover.exitActions.Add(ResumeTarget);

        FSMState stuck = new FSMState();
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

    private void ChangeColor()
    {
        if(fsm != null)
        {
            if (gameObject.GetComponent<MeshRenderer>().material.color == alarmMaterial.color)
            {
                gameObject.GetComponent<MeshRenderer>().material = quietMaterial;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = alarmMaterial;
            }
        }
        
    }

    private void ResetPath()
    {
        nextNode = null;
        if(targetNode != null)
        {
            if(map.mapArray[targetNode.i, targetNode.j] == 4)
            {
                map.mapArray[targetNode.i, targetNode.j] = 0;
            }
            targetNode = null;
        }
        
        path = null;
        pathCounter = 0;
        noPath = false;
        nextPathNode = false;
    }

    private void SetTarget()
    {
        if(resumeOldTarget && oldTargetNode != null && map.mapArray[oldTargetNode.i, oldTargetNode.j] == 0)
        {
            targetNode = oldTargetNode;
            map.mapArray[targetNode.i, targetNode.j] = 4;
            map.landmark.transform.position = GetMapLocationFromArray(map.startPoint, targetNode.i, targetNode.j);
            map.landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            map.landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
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
        }
        resumeOldTarget = false;
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

                    if(fsm.current == cover) //reached the cover target
                    {
                        coverReached = true;
                    } else //reached standard target, goes to the next one
                    {
                        ResetPath();
                        SetTarget();
                    }
                }
            }
        }
    }

    private void SaveOldTarget()
    {
        if(targetNode != null)
        {
            oldTargetNode = targetNode;
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
                    if (x >= 0 && x < map.mapArray.GetLength(0) && z >= 0 && z < map.mapArray.GetLength(1))
                    {
                        if (map.mapArray[x, z] == 0)
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
        map.mapArray[targetNode.i, targetNode.j] = 4;
        map.landmark.transform.position = GetMapLocationFromArray(map.startPoint, targetNode.i, targetNode.j);
        map.landmark.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        map.landmark.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
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
map.DebugPrint();
    }

    private void ResumeTarget()
    {
        resumeOldTarget = true;
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
        if (map.gameObject.GetComponent<MapSM>().GetActiveSentinels().Count == 0 && coverReached == true)
        {
            coverReached = false;
            return true;
        }
        else
        {
            return false;
        }
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

    private Node GetNearestNode(Vector3 agentPos)
    {
        float minDistance = float.MaxValue;
        Node nearestNode = null;

        Node[] nodes = map.graph.getNodes();
        foreach(Node n in nodes)
        {
            Vector3 pos = GetMapLocationFromArray(map.startPoint, n.i, n.j);
            float distance = (pos - agentPos).magnitude;
            if(distance < minDistance && map.mapArray[n.i, n.j] == 0)
            {
                minDistance = distance;
                nearestNode = n;
            }
        }

        return nearestNode;
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
