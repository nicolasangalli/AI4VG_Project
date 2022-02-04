using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestView : MonoBehaviour
{
    [Range(90f, 120f)]
    public int fov = 120;
    public float distanceOfView = 2.5f;
	public GameObject node;

	private int stepOfView = 2;

	// Start is called before the first frame update
	void Start()
    {
		Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 a = transform.forward * distanceOfView;
		Vector3 b = node.transform.position - transform.position;

		float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));

		bool obstacleHitted = false;
		Ray ray = new Ray(transform.position, (node.transform.position - transform.position).normalized);
		RaycastHit hit;
		bool hitted = Physics.Raycast(ray, out hit);
		if (hitted == true && hit.collider.gameObject.tag == "Obstacle")
		{
			obstacleHitted = true;
		}
		if (angle > fov/2 || b.magnitude > distanceOfView || obstacleHitted == true)
        {

			Debug.Log("out");
        } else
        {
			Debug.Log("in");
        }

		Vector3 vector = transform.forward * distanceOfView;

		for (int i = -fov / 2; i <= fov / 2; i = i + stepOfView)
		{
			Vector3 v = Quaternion.AngleAxis(i, transform.up) * vector;
			Ray ray2 = new Ray(transform.position, v);
			RaycastHit hit2;
			bool hitted2 = Physics.Raycast(ray2, out hit2, v.magnitude);

			if (hitted2 == true && hit2.collider.gameObject.tag == "Obstacle")
			{
				Debug.DrawRay(transform.position, v.normalized * (hit2.collider.gameObject.transform.position - transform.position).magnitude, Color.red, Time.deltaTime);
			}
			else
			{
				Debug.DrawRay(transform.position, v, Color.red, Time.deltaTime);
			}
		}
	}
}
