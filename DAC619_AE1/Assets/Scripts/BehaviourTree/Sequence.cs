using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> nodes = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        bool isAnyNodeRunning = false;
        for (int i = 0; i < nodes.Count; i++)
        {
            switch (nodes[i].Evaluate())
            {
                case NodeState.RUNNING:
                    isAnyNodeRunning = true;
                    break;
                case NodeState.SUCCESS:
                    break;
                case NodeState.FAILURE:
                    nodeState = NodeState.FAILURE;
                    return nodeState;
                    break;
                default:
                    break;
            }
        }
        nodeState = isAnyNodeRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return nodeState;
    }
}
