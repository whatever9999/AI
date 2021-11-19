using System.Collections.Generic;

public class Sequence : Node
{
    protected List<Node> nodes = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    //public override NodeState Evaluate()
    //{
    //    bool isAnyNodeRunning = false;
    //    for (int i = 0; i < nodes.Count; i++)
    //    {
    //        switch (nodes[i].Evaluate())
    //        {
    //            case NodeState.RUNNING:
    //                isAnyNodeRunning = true;
    //                break;
    //            case NodeState.SUCCESS:
    //                break;
    //            case NodeState.FAILURE:
    //                nodeState = NodeState.FAILURE;
    //                return nodeState;
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    nodeState = isAnyNodeRunning ? NodeState.RUNNING : NodeState.SUCCESS;
    //    return nodeState;
    //}

    public override NodeState Evaluate()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodeState = nodes[i].Evaluate();
            if (nodeState == NodeState.SUCCESS)
            {

            }
            if (nodeState == NodeState.RUNNING)
            {
                break;
            }
            if (nodeState == NodeState.FAILURE)
            {
                break;
            }
        }
        return nodeState;
    }
}
