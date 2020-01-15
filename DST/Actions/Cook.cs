using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Cook : ActionDST
    {
        public string InvObject;
        public string Target;
        public float Duration;
        public static Dictionary<string, string> FoodConverter = new Dictionary<string, string>() //Dictionary with each food's cooked variant
        {
            {"berries", "berries_cooked"},
            {"carrot", "carrot_cooked"},
        };

        public Cook(string invobject, string target) : base("Cook_" + target)
        {
            this.InvObject = invobject;
            this.Target = target;
            this.Duration = 0.05f;
        }

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            if (FoodConverter.ContainsKey(this.InvObject))
            {
                worldModel.RemoveFromPossessedItems(this.InvObject, 1);
                worldModel.AddToPossessedItems(FoodConverter[this.InvObject], 1);

                if (!worldModel.Possesses(this.InvObject))
                {
                    worldModel.RemoveAction(string.Concat("Cook_", this.InvObject));
                }
            }
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int objectGuid = preWorldState.GetInventoryGUID(this.InvObject);
            int targetGuid = preWorldState.GetEntitiesGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair = new Pair<string, string>("Action(Cook, " + objectGuid + ", -, -, -)", targetGuid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
