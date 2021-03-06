using System.Collections;
using UnityEngine;


public class SentinelSM : MonoBehaviour
{

	public float reactionTime = 0.1f;
	public GameObject node;
	[Range(90, 120)]
	public int fov = 90;
	[Range(1, 4)]
	public int distanceOfView = 2;

	private int stepOfView;
	private MapGeneration map;
	private MapSM mapSM;
	private FSM fsm;
	private bool showed;


	void Start()
    {
		stepOfView = 2;
		map = GameObject.FindGameObjectWithTag("Map").GetComponent<MapGeneration>();
		mapSM = GameObject.FindGameObjectWithTag("Map").GetComponent<MapSM>();
		showed = false;

		FSMState hide = new FSMState();
		hide.enterActions.Add(SentinelHide);
		hide.exitActions.Add(SentinelShow);

		FSMState alarm = new FSMState();
		alarm.enterActions.Add(MarkNode);
		alarm.enterActions.Add(AddSentinel);

		FSMState visible = new FSMState();
		visible.enterActions.Add(RemoveSentinel);

		FSMTransition t1 = new FSMTransition(AgentVisible);
		FSMTransition t2 = new FSMTransition(AgentNotVisible);

		hide.AddTransition(t1, alarm);
		alarm.AddTransition(t2, visible);
		visible.AddTransition(t1, alarm);

		fsm = new FSM(hide);

		StartCoroutine(Patrol());
	}

	private void SentinelHide()
    {
		GetComponent<Renderer>().enabled = false;
	}
	
	private void SentinelShow()
    {
		GetComponent<Renderer>().enabled = true;
		showed = true;
    }

	private void MarkNode() {
		Vector3 a = transform.forward * distanceOfView;

		Node[] nodes = map.graph.getNodes();
		foreach(Node n in nodes)
		{
			Vector3 nPosition = map.GetMapLocationFromArray(map.startPoint, n.i, n.j);
			Vector3 b = nPosition - transform.position;
			if(b.magnitude != 0)
            {
				float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));

				if(angle > fov / 2 + 10f)
				{
					n.inSentinelView = false;
				}
				else
				{
					if(angle <= fov / 2)
					{
						if(b.magnitude > distanceOfView + 0.4f)
						{
							n.inSentinelView = false;
						}
						else
						{
							n.inSentinelView = true;
						}
					}
					else
					{
						if(b.magnitude > distanceOfView)
						{
							n.inSentinelView = false;
						}
						else
						{
							n.inSentinelView = true;
						}
					}
				}
			}
			else
            {
				n.inSentinelView = true;
            }
			
			if(n.inSentinelView)
            {
				n.sceneObject = Instantiate(node, map.GetMapLocationFromArray(map.startPoint, n.i, n.j), Quaternion.identity);
			}
		}

		for(int i = -fov/2; i <= fov/2; i = i+stepOfView)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * a;
			Ray ray = new Ray(transform.position, v);
			RaycastHit hit;
			bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

			if(hitted == true && hit.collider.gameObject.tag == "Node")
			{
				Node n = map.graph.GetNodeByGameobject(hit.collider.gameObject);
				if(n != null)
                {
					DestroyImmediate(n.sceneObject);
				}
			}
		}

		GameObject[] nodesLeft = GameObject.FindGameObjectsWithTag("Node");
		foreach(GameObject nodeGO in nodesLeft)
        {
			Node node = map.graph.GetNodeByGameobject(nodeGO);
			if(map.mapArray[node.i, node.j] != 3)
			{
				node.inSentinelView = false;
			}
			DestroyImmediate(nodeGO);
		}
	}

	private void AddSentinel()
    {
		mapSM.AddActiveSentinel(gameObject);
    }

	private void RemoveSentinel()
    {
		mapSM.RemoveActiveSentinel(gameObject);
    }

	private bool AgentVisible()
    {
		Vector3 vector = transform.forward * distanceOfView;

		for(int i = -fov/2; i <= fov/2; i = i+stepOfView)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
			Ray ray = new Ray(transform.position, v);
			RaycastHit hit;
			bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

			if(hitted == true && hit.collider.gameObject.tag == "Agent")
			{
				return true;
			}
		}

		GameObject agent = GameObject.FindWithTag("Agent");
		float r1 = agent.GetComponent<MeshCollider>().bounds.size.x / 2;
		float r2 = gameObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
		float distance = (agent.transform.position - transform.position).magnitude - r1 - r2;
		if(distance < 0.05f)
		{
			return true;
		}

		return false;
	}

	private bool AgentNotVisible()
	{
		return !AgentVisible();
	}

	private IEnumerator Patrol()
	{
		while(true)
		{
			fsm.Update();
			yield return new WaitForSeconds(reactionTime);
		}
	}

	void FixedUpdate()
    {
		if(showed)
        {
			Vector3 vector = transform.forward * distanceOfView;

			for(int i = -fov/2; i <= fov/2; i = i+stepOfView)
			{
				Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
				Ray ray = new Ray(transform.position, v);
				RaycastHit hit;
				bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

				if(hitted == true && hit.collider.gameObject.tag == "Obstacle")
				{
					Debug.DrawRay(transform.position, v.normalized * (hit.point - transform.position).magnitude, Color.cyan, Time.deltaTime);
				}
				else
				{
					Debug.DrawRay(transform.position, v, Color.cyan, Time.deltaTime);
				}
			}
		}
		
	}

}
