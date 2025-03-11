using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public int weight = 1;

    public List<Node> adjacents = new List<Node>();
    public Node previous = null;

    public bool CanVisit
    {
        get
        {
            return adjacents.Count > 0;
        }
    }
}
