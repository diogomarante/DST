using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Mine : ActionDST
    {
        public string Target;
        public float Duration;

        public Mine(string target) : base("Mine_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
        }


        //Assume he will pick the flint and rocks
        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;
            worldModel.RemoveFromWorld(this.Target, 1);
            worldModel.AddToPossessedItems("rocks", 3);
            worldModel.AddToPossessedItems("flint", 2);
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int guid = preWorldState.GetInventoryGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair;

            pair = new Pair<string, string>("Action(MINE, -, -, -, -)", guid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
