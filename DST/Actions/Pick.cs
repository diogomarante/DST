using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Pick : ActionDST
    {
        public string Target;
        public float Duration;
        public static Dictionary<string, string> PickableConverter = new Dictionary<string, string>() //Dictionary with each pickable's drop
        {
            {"sapling", "twigs"},
            {"berrybush", "berries"},
            {"carrot_planted", "carrot"},
            {"grass", "cutgrass"},
        };

        public Pick(string target) : base("Pick_" + target)
        {
            this.Target = target;
            this.Duration = 0.05f;
        }

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            if (PickableConverter.ContainsKey(this.Target))
            {
                worldModel.RemoveFromWorld(this.Target, 1);
                worldModel.AddToPossessedItems(PickableConverter[this.Target], 1);

                if (!worldModel.WorldHas(this.Target))
                {
                    worldModel.RemoveAction(string.Concat("Pick_", this.Target));
                }
            }
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int targetGuid = preWorldState.GetEntitiesGUID(this.Target);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair = new Pair<string, string>("Action(Pick, -, -, -, -)", targetGuid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
