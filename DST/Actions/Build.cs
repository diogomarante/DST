using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.WorldModels;
using MCTS.DST;


namespace MCTS.DST.Actions
{

    public class Build : ActionDST
    {
        public string X;
        public string Y;
        public string Recipe;
        public float Duration;
        public static Dictionary<string, Dictionary<string, int>> Recipes = new Dictionary<string, Dictionary<string, int>>() //Recipes
        {
            {"torch", new Dictionary<string, int>() { 
                {"cutgrass", 2}, 
                {"twigs", 2},
            }},
            {"axe", new Dictionary<string, int>() { 
                {"flint", 1}, 
                {"twigs", 1},
            }},
            {"pickaxe", new Dictionary<string, int>() { 
                {"flint", 2}, 
                {"twigs", 2},
            }},
            {"campfire", new Dictionary<string, int>() { 
                {"cutgrass", 3}, 
                {"log", 2},
            }},
            {"firepit", new Dictionary<string, int>() { 
                {"rocks", 12}, 
                {"log", 2},
            }},
        };

        public Build(int x, int y, string Recipe) : base("Build_" + Recipe)
        {
            this.X = x.ToString();
            this.Y = y.ToString();
            this.Recipe = Recipe;
            this.Duration = 0.05f;
        }        
        
        public Build(string Recipe) : base("Build_" + Recipe)
        {
            this.X = "-";
            this.Y = "-";
            this.Recipe = Recipe;
            this.Duration = 0.05f;
        }

        public override void ApplyActionEffects(WorldModelDST worldModel)
        {
            worldModel.Cycle += this.Duration;

            if (Recipes.ContainsKey(this.Recipe))
            {
                foreach (var item in Recipes[this.Recipe])
                {
                    worldModel.RemoveFromPossessedItems(item.Key, item.Value);
                }

//                if (!worldModel.Possesses(this.Recipe))
//                {
//                    worldModel.RemoveAction("Build_" + this.Recipe);
//                }
            }
        }

        public override List<Pair<string, string>> Decompose(PreWorldState preWorldState)
        {
            List<Pair<string, string>> ListOfActions = new List<Pair<string, string>>(1);

            Pair<string, string> pair = new Pair<string, string>("Action(Build, -, " + this.X + ", " + this.Y + ", " + this.Recipe + ")", "-");

            ListOfActions.Add(pair);

            return ListOfActions;
        }

        public override Pair<string, int> NextActionInfo()
        {
            return new Pair<string, int>("", 0);
        }
    }
}
