using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Pickup : ActionDST
    {
        public string Target;
        public float Duration;
        public int Quantity;
        public static List<string> Pickables = new List<string>() //List of values for each pickable item 
        {
            "twigs",
            "flint"
        };

        public Pickup(string target, int quantity) : base("Pickup_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
            this.Quantity = quantity;
        }

        //Fazer Decompose

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            worldModel.AddToPossessedItems(this.Target, this.Quantity);

            worldModel.RemoveFromWorld(this.Target, this.Quantity);


        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int guid = preWorldState.GetEntitiesGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair;

            pair = new Pair<string, string>("Action(PICKUP, -, -, -, -)", guid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
