using UnityEngine;

public class KMoveTo : MonoBehaviour {

	public Transform destination;
	public float speed = 2f;
	public float stopAt = 0.01f;

	//debug
	public GameObject place;
	private bool placed;

	void Start()
    {
		placed = false;
		place = Instantiate(place, new Vector3(400, 400, 400), Quaternion.identity);
	}

    void FixedUpdate () {
		if (destination) {
if(placed == false)
{
Debug.Log(destination.position);
place.transform.position = destination.position;
place.transform.rotation = destination.rotation;
placed = true;
}


			Vector3 verticalAdj = new Vector3 (destination.position.x, transform.position.y, destination.position.z);
			Vector3 toDestination = (verticalAdj - transform.position);
			if (toDestination.magnitude > stopAt) {

				// option a : we look at the destination position but with "our" height
				//transform.LookAt (verticalAdj);

				// option b : we care only about rotation on vertical axis
				transform.LookAt (destination.position);
				Vector3 rotationAdj = new Vector3 (0f, transform.rotation.eulerAngles.y, 0f);
				transform.rotation = Quaternion.Euler (rotationAdj);

				transform.position += transform.forward * speed * Time.deltaTime;
			} else
            {
placed = false;
				gameObject.GetComponent<AgentSM>().nextPathNode = true;
            }
		}
	}
}
