using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Drop : ActionDST
    {
        public string Target;
        public float Duration;
        public int Quantity;
        public Pair<int, int> Position;

        public Drop(string target, int quantity, Pair<int, int> position) : base("Drop_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
            this.Quantity = quantity;
            this.Position = position;      
        }

        //Fazer Decompose

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;
           
            worldModel.RemoveFromPossessedItems(this.Target, 1);

            if (!worldModel.Possesses(this.Target))
            {
                worldModel.RemoveAction("Drop_" + this.Target);
            }
            worldModel.AddToWorld(this.Target, this.Quantity, this.Position.Item1, this.Position.Item2 );


        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int guid = preWorldState.GetInventoryGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair;

            pair = new Pair<string, string>("Action(DROP, -, -, -, -)", guid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
