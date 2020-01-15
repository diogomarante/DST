using System;
using System.Collections.Generic;
using MCTS.DST.Actions;
using MCTS.DST.WorldModels;

namespace MCTS.DST
{
    public class MCTSNode
    {
        public WorldModelDST State;
        public MCTSNode Parent;
        public ActionDST Action;
        public List<MCTSNode> ChildNodes;
        public int N;
        public float Q;

        public MCTSNode(WorldModelDST state)
        {
            State = state;
            ChildNodes = new List<MCTSNode>();
            N = 0;
            Q = 0.0f;
        }

    }
}
