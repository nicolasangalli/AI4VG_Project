using UnityEngine;


public class KMoveTo : MonoBehaviour
{

	[HideInInspector]
	public Transform destination;
	public float speed = 2f;
	public float stopAt = 0.01f;


	void FixedUpdate ()
	{
		if(destination)
		{
			Vector3 verticalAdj = new Vector3(destination.position.x, transform.position.y, destination.position.z);
			Vector3 toDestination = (verticalAdj - transform.position);
			if(toDestination.magnitude > stopAt)
			{
				transform.LookAt(destination.position);
				Vector3 rotationAdj = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
				transform.rotation = Quaternion.Euler(rotationAdj);

				transform.position += transform.forward * speed * Time.deltaTime;
			}
			else
            {
				gameObject.GetComponent<AgentSM>().nextPathNode = true;
			}
		}
	}

}
