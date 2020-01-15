using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Wander : ActionDST
    {
        public float Duration;

        public Wander() : base("Wander")
        {
            this.Duration = 0.33f;
        }

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;
            worldModel.IncreaseHunger(1);
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair;

            System.Random RandomGenerator = new System.Random();

            pair = new Pair<string, string>("Action(WANDER, -, -, -, -)", "-");

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
