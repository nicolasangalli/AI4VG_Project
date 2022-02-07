using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayerVisibility : MonoBehaviour
{

	public GameObject text;
	public GameObject player;

	float r1;
	float r2;

    // Start is called before the first frame update
    void Start()
    {
		r1 = player.GetComponent<MeshCollider>().bounds.size.x / 2;
		r2 = gameObject.GetComponent<MeshRenderer>().bounds.size.x / 2;
    }

    private void Update()
    {
		
		if (PlayerVisible())
		{
			text.GetComponent<Text>().text = "visible";
		}
		else
		{
			text.GetComponent<Text>().text = "not visible";
		}
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        

		Vector3 vector = transform.forward * 2;

		for (int i = -90 / 2; i <= 90 / 2; i = i + 2)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
			Ray ray = new Ray(transform.position, v);
			RaycastHit hit;
			bool hitted = Physics.Raycast(ray, out hit, v.magnitude);

			if (hitted == true && hit.collider.gameObject.tag == "Obstacle")
			{
				Debug.DrawRay(transform.position, v.normalized * (hit.point - transform.position).magnitude, Color.cyan, Time.deltaTime);
			}
			else
			{
				Debug.DrawRay(transform.position, v, Color.cyan, Time.deltaTime);
			}
		}
	}

	private bool PlayerVisible()
	{
		Vector3 vector = transform.forward * 2;

		for (int i = -90 / 2; i <= 90 / 2; i = i + 2)
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

		float distance = (player.transform.position - transform.position).magnitude - r1 - r2;
		if (distance < 0.05f)
        {
			return true;
        }

		return false;
	}

}
