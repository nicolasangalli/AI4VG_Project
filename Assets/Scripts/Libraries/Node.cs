using UnityEngine;


public class Node {

	public string description;
	public int i;
	public int j;
	public bool inSentinelView;
	public GameObject sceneObject;

	public Node(string description, int i = -1, int j = -1, GameObject o = null) {
		this.description = description;
		this.i = i;
		this.j = j;
		this.inSentinelView = false;
		this.sceneObject = o;
	}

}
