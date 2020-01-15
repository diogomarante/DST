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
        public static Dictionary<string, int[]> FoodIndex = new Dictionary<string, int[]>() //List of values for each food (format: Hunger, HP)
        {
            {"berries", new int[] {9}},
            {"berries_cooked", new int[] {13, 1}},
            {"carrot", new int[] {13, 1}},
            {"carrot_cooked", new int[] {13, 3}},
        };

        public Eat(string target) : base("Eat_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
        }

        //Fazer Decompose

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            //<OPTIMIZATION - Dict instead of chained if's>
            if (FoodIndex.ContainsKey(this.Target))
            {
                worldModel.RemoveFromPossessedItems(this.Target, 1);

                int[] foodValues = FoodIndex[this.Target];
                int HungerVal = foodValues[0];

                if (HungerVal > 0) worldModel.DecreaseHunger(foodValues[0]);
                else if (HungerVal < 0) worldModel.IncreaseHunger(Math.Abs(foodValues[0]));

                if (foodValues.Length > 1)
                {
                    int HPVal = foodValues[1];
                    if (HPVal > 0) worldModel.IncreaseHP(foodValues[1]);
                    else if (HPVal < 0) worldModel.DecreaseHP(Math.Abs(foodValues[1]));

                }

                if (!worldModel.Possesses(this.Target))
                {
                    worldModel.RemoveAction("Eat_" + this.Target);
                }
            }

            //</OPTIMIZATION>

//          <OLD_CODE>   
//            if (this.Target == "berries")
//            {
//                worldModel.RemoveFromPossessedItems("berries", 1);
//                worldModel.DecreaseHunger(9);
//
//                if (!worldModel.Possesses("berries"))
//                {
//                    worldModel.RemoveAction("Eat_berries");
//                }
//            }
//            else if (this.Target == "carrot")
//            {
//                worldModel.RemoveFromPossessedItems("carrot", 1);
//                worldModel.DecreaseHunger(13);
//                worldModel.IncreaseHP(1);
//
//                if (!worldModel.Possesses("carrot"))
//                {
//                    worldModel.RemoveAction("Eat_carrot");
//                }
//            }    
//          </OLD_CODE>
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
