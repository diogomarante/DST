using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{
    class Equip : ActionDST
    {
        public string InvObject;
        public float Duration;
        public static List<string> Weapons = new List<string>() //List of values for each weapon 
        {
            "axe",
            "pickaxe"
           
        };

        public Equip(string invobject) : base("Equip_" + invobject)
        {
            this.InvObject = invobject;
            this.Duration = 0.05f;
        }

        //Fazer Decompose

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            worldModel.AddToEquipped(this.InvObject);
            worldModel.RemoveFromPossessedItems(this.InvObject, 1);
            worldModel.RemoveAction("Equip_" + this.InvObject);

        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            int guid = preWorldState.GetInventoryGUID(this.InvObject);

            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);
            Pair<string, string> pair;

            pair = new Pair<string, string>("Action(EQUIP,"+ this.InvObject +", -, -, -)", guid.ToString());

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}

