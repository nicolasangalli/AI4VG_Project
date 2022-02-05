using System.Collections;
using UnityEngine;


public class SentinelSM : MonoBehaviour
{
	public float reactionTime = 0.1f;
	[Range(90, 120)]
	public int fov = 120;
	public float distanceOfView = 2.5f;

	private int stepOfView;
	private GameObject map;
	private FSM fsm;
	private bool visible = false;

	//debug
	public GameObject node;

	// Start is called before the first frame update
	void Start()
    {
		stepOfView = 2;
		map = GameObject.FindGameObjectWithTag("Map");

		FSMState hide = new FSMState();
		hide.exitActions.Add(SentinelShow);
		FSMState alarm = new FSMState();
		alarm.enterActions.Add(MarkNode);
		alarm.enterActions.Add(AddSentinel);
		FSMState visible = new FSMState();
		visible.enterActions.Add(RemoveSentinel);

		FSMTransition t1 = new FSMTransition(PlayerVisible);
		FSMTransition t2 = new FSMTransition(PlayerNotVisible);

		hide.AddTransition(t1, alarm);
		alarm.AddTransition(t2, visible);
		visible.AddTransition(t1, alarm);

		fsm = new FSM(hide);

		StartCoroutine(Patrol());
		
	}
	
	private void SentinelShow()
    {
		GetComponent<Renderer>().enabled = true;
		visible = true;
    }

	public void MarkNode() {
		Vector3 a = transform.forward * distanceOfView;

		Node[] nodes = map.GetComponent<MapGeneration>().graph.getNodes();
		//map = GameObject.FindGameObjectWithTag("Map");
		//Node[] nodes = map.GetComponent<TestGraph>().graph.getNodes();

		foreach (Node n in nodes)
		{
			Vector3 nPosition = map.GetComponent<MapGeneration>().GetMapLocationFromArray(map.GetComponent<MapGeneration>().startPoint, n.i, n.j);
			//Vector3 nPosition = map.GetComponent<TestGraph>().GetMapLocationFromArray(map.GetComponent<TestGraph>().startPoint, n.i, n.j);

			Vector3 b = nPosition - transform.position;
			float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));

			if (angle > fov / 2 + 10f)
			{
				n.inSentinelView = false;
			}
			else
			{
				if (angle <= fov / 2)
				{
					if (b.magnitude > distanceOfView + 0.4f)
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
					if (b.magnitude > distanceOfView)
					{
						n.inSentinelView = false;
					}
					else
					{
						n.inSentinelView = true;
					}
				}
			}

			if(n.inSentinelView)
            {
				n.sceneObject = Instantiate(map.GetComponent<MapGeneration>().node, map.GetComponent<MapGeneration>().GetMapLocationFromArray(map.GetComponent<MapGeneration>().startPoint, n.i, n.j), Quaternion.identity);
			}

		}

		for (int i = -fov / 2; i <= fov / 2; i = i + stepOfView)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * a;
			Ray ray = new Ray(transform.position, v);
			RaycastHit hit;
			bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

			if (hitted == true && hit.collider.gameObject.tag == "Node")
			{
				Node n = map.GetComponent<MapGeneration>().graph.GetNodeByGameobject(hit.collider.gameObject);
				if(n != null)
                {
//map.GetComponent<MapGeneration>().mapArray[n.i, n.j] = -1;
					DestroyImmediate(n.sceneObject);
				}
			}
		}

		GameObject[] nodesLeft = GameObject.FindGameObjectsWithTag("Node");
		foreach(GameObject nodeGO in nodesLeft)
        {
			Node node = map.GetComponent<MapGeneration>().graph.GetNodeByGameobject(nodeGO);
			if(map.GetComponent<MapGeneration>().mapArray[node.i, node.j] != 3) {
				node.inSentinelView = false;
			}
			DestroyImmediate(nodeGO);
		}

		map.GetComponent<MapGeneration>().DebugPrint();
	}

	private void AddSentinel()
    {
		map.GetComponent<MapSM>().AddActiveSentinel(gameObject);
    }

	private void RemoveSentinel()
    {
		map.GetComponent<MapSM>().RemoveActiveSentinel(gameObject);
    }

	private bool PlayerVisible()
    {
		Vector3 vector = transform.forward * distanceOfView;

		for (int i = -fov / 2; i <= fov / 2; i = i + stepOfView)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
			Ray ray = new Ray(transform.position, v);
			RaycastHit hit;
			bool hitted = Physics.Raycast(ray, out hit, v.magnitude);
			if (hitted == true && hit.collider.gameObject.tag == "Agent")
			{
				return true;
			}
		}

		return false;
	}

	private bool PlayerNotVisible()
	{
		return !PlayerVisible();
	}

	private IEnumerator Patrol()
	{
		while (true)
		{
			fsm.Update();
			yield return new WaitForSeconds(reactionTime);
		}
	}

	private void FixedUpdate()
    {
		if(visible)
        {
			Vector3 vector = transform.forward * distanceOfView;

			for (int i = -fov / 2; i <= fov / 2; i = i + stepOfView)
			{
				Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
				Ray ray = new Ray(transform.position, v);
				RaycastHit hit;
				bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

				if (hitted == true && hit.collider.gameObject.tag == "Obstacle")
				{
					Debug.DrawRay(transform.position, v.normalized * (hit.point - transform.position).magnitude, Color.red, Time.deltaTime);
				}
				else
				{
					Debug.DrawRay(transform.position, v, Color.red, Time.deltaTime);
				}
			}
		}
		
	}

}
