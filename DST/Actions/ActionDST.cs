using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;

namespace MCTS.DST.Actions
{

    public class ActionDST
    {
        public string Name;
        
        public ActionDST(string name)
        {
            this.Name = name;
        }

        public virtual List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            List<Pair<string, string>> list = new List<Pair<string, string>>(1);

            //Initialized it with a random action, there are better ways of doing this
            Pair<string, string> pair = new Pair<string, string>("Action(Wander, -, -, -, -)", "-");
            list.Add(pair);

            return list;
        }

        public virtual void ApplyActionEffects(WorldModelDST worldState)
        {
        }

        public virtual Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
