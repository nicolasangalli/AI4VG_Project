using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Graph {

	// holds all edgeds going out from a node
	private Dictionary<Node, List<Edge>> data;

	public Graph() {
		data = new Dictionary<Node, List<Edge>>();
	}

	public void AddEdge(Edge e) {
		AddNode (e.from);
		AddNode (e.to);
		if (!data[e.from].Contains(e))
			data [e.from].Add (e);
	}

	// used only by AddEdge 
	public void AddNode(Node n) {
		if (!data.ContainsKey (n))
			data.Add (n, new List<Edge> ());
	}

	// returns the list of edged exiting from a node
	public Edge[] getConnections(Node n) {
		if (!data.ContainsKey (n)) return new Edge[0];
		return data [n].ToArray ();
	}

	public Node[] getNodes() {
		return data.Keys.ToArray ();
	}

	public void RemoveNode(Node n)
    {
		if(data.ContainsKey(n))
        {
			foreach(KeyValuePair<Node, List<Edge>> entry in data)
            {
				List<int> indexesToRemove = new List<int>();
				List<Edge> edges = entry.Value;
				foreach(Edge e in edges)
                {
					if(e.to == n)
                    {
						int index = edges.IndexOf(e);
						indexesToRemove.Add(index);
                    }
                }
				foreach(int i in indexesToRemove)
                {
					edges.RemoveAt(i);
				}
			}
			data.Remove(n);
		}
    }

	public Node GetNodeByGameobject(GameObject o)
    {
		foreach(KeyValuePair<Node, List<Edge>> entry in data)
        {
			if(entry.Key.sceneObject == o)
            {
				return entry.Key;
            }
        }
		return null;
	}

}
