using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Eat : ActionDST
    {
        public string Target;
        public float Duration;

        public Eat(string target) : base("Eat_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
        }

        //Fazer Decompose

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            if (this.Target == "berries")
            {
                worldModel.RemoveFromPossessedItems("berries", 1);
                worldModel.DecreaseHunger(9);

                if (!worldModel.Possesses("berries"))
                {
                    worldModel.RemoveAction("Eat_berries");
                }
            }
            else if (this.Target == "carrot")
            {
                worldModel.RemoveFromPossessedItems("carrot", 1);
                worldModel.DecreaseHunger(13);
                worldModel.IncreaseHP(1);

                if (!worldModel.Possesses("carrot"))
                {
                    worldModel.RemoveAction("Eat_carrot");
                }
            }           
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int guid = preWorldState.GetInventoryGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair< string, string>>(1);
            Pair<string, string> pair;

            pair = new Pair<string, string>("Action(EAT, -, -, -, -)", guid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;                   
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
