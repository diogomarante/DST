using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.Actions;
using MCTS.DST;

namespace MCTS.DST.WorldModels
{
    public abstract class WorldModelDST 
    {
        public Character Walter;

        public float Cycle;
        public int[] CycleInfo;
        public List<ActionDST> AvailableActions;
        public int ActionTracker = 0;
        public int PosX;
        public int PosZ;
        protected WorldModelDST Parent;

        public WorldModelDST(Character character, float cycle, int[] cycleInfo, List<ActionDST> availableActions, WorldModelDST parent)
        {
            this.Walter = character;
            this.Cycle = cycle;
            this.CycleInfo = cycleInfo;
            this.AvailableActions = availableActions;
            this.Parent = parent;
        }

        public WorldModelDST(PreWorldState preWorldState)
        {
            this.Parent = null;

            this.Walter = preWorldState.Walter;
            this.Cycle = preWorldState.Cycle;
            this.CycleInfo = preWorldState.CycleInfo;
        }

        public void getActions()
        {
            //Getting Available Actions
            this.AvailableActions = new List<ActionDST>();


            //Getting Wander
            if (IsNight())
            {
                foreach (var recipe in Build.Recipes)
                {
                    bool EnoughIngredients = true;
                    foreach (var ingredient in recipe.Value)
                    {
                        if (!Possesses(ingredient.Key, ingredient.Value))
                        {
                            EnoughIngredients = false;
                        }
                    }
                    if (EnoughIngredients)
                    {
                        string recipeName = recipe.Key;

                        if (recipeName == "campfire" || recipeName == "firepit" || recipeName == "torch")
                        {
                            this.AvailableActions.Add(recipeName == "torch" ? new Build(recipeName) :
                                           new Build(this.Walter.GetPosX(), this.Walter.GetPosZ(), recipeName));

                        }
                    }
                }
            }
            else
            {
                //



                //<OPTIMIZATION - generalized cases from action's own lists>
                //Getting Eat based Actions
                foreach (var food in Eat.FoodIndex.Keys)
                {
                    int quantity = this.GetQuantity(food);
                    if (quantity > 0)
                    {
                        this.AvailableActions.Add(new Eat(food));
                        //this.AvailableActions.Add(new Drop(food, quantity, this.Walter.Position));
                    }
                }

                foreach (var recipe in Build.Recipes)
                {
                    bool EnoughIngredients = true;
                    foreach (var ingredient in recipe.Value)
                    {
                        if (!Possesses(ingredient.Key, ingredient.Value))
                        {
                            EnoughIngredients = false;
                        }
                    }
                    if (EnoughIngredients)
                    {
                        string recipeName = recipe.Key;

                        if (recipeName == "campfire" || recipeName == "firepit" || recipeName == "torch")
                        {
                            continue;

                        }
                        else
                        {
                            this.AvailableActions.Add(new Build(recipeName));
                        }
                    }
                }



                //Getting Pick based Actions
                foreach (var pickable in Pick.PickableConverter.Keys)
                {
                    if (WorldHas(pickable))
                    {
                        this.AvailableActions.Add(new Pick(pickable));
                    }
                }
                //</OPTIMIZATION>

                foreach (var weapon in Equip.Weapons)
                {
                    if (Possesses(weapon))
                    {
                        this.AvailableActions.Add(new Equip(weapon));
                    }
                }

                if (IsEquipped("axe") && WorldHas("tree"))
                {
                    this.AvailableActions.Add(new Chop("tree"));
                }

                //TODO dict in Pickup action with all pickable items

                foreach (var pickupable in Pickup.Pickupables)
                {
                    if (WorldHas(pickupable))
                    {
                        this.AvailableActions.Add(new Pickup(pickupable, 1));
                    }
                }

                if (IsEquipped("pickaxe") && WorldHas("boulder"))
                {
                    this.AvailableActions.Add(new Mine("boulder"));
                }



                //this.checkAvailableActions();
                this.AvailableActions.Add(new Wander());
            }
        }

        public void checkAvailableActions()
        {
            foreach (var action in this.AvailableActions)
            {
                Console.WriteLine(action);
            }
        }

        public bool IsNight()
        {
            int daytime = CycleInfo[0] + CycleInfo[1] + CycleInfo[2];
            return Cycle % daytime > (daytime - CycleInfo[2]);
        }

        public float Score(WorldModelDST newWorld)
        {
            Dictionary<string, float> RatioDict = new Dictionary<string, float>()
            {
                { "status", 0.2f},
                { "inventory", 0.1f},
                { "axes", 0.2f},
                { "light", 0.3f},
                { "fuel", 0.2f},
            };
           
            float total = 0;
            updateDict("status", statusLow());
            total += statusIncrease();
            total += inventoryIncreased();
            total += IsNight() ? this.LightValueNight() * RatioDict["light"] : this.LightValueDay() * RatioDict["light"];
            total += AxePickaxeValue() * hasAxes();
            total += HasFuel() ? RatioDict["fuel"] : 0;
            Console.WriteLine("Score: " + total);
            //Console.WriteLine("Night?: " + IsNight());
            return total;

            void updateDict(string key, float value)
            {
                RatioDict[key] += value;
                List<string> otherkeys = new List<string>();
                foreach (var otherkey in RatioDict.Keys)
                {
                    if (otherkey != key)
                    {
                        otherkeys.Add(otherkey);
                    }
                }
                foreach (var otherkey in otherkeys)
                {
                    RatioDict[otherkey] -= value / 4;
                }
            }

            float statusIncrease()
            {
                float sum = 0;
                sum +=  newWorld.Walter.Hunger - this.Walter.Hunger;
                sum += newWorld.Walter.Sanity - this.Walter.Sanity;
                return sum > 0 ? 1 * RatioDict["status"] /300 : 0;
            }

            float statusLow()
            {
                float sum = 0;
                if (newWorld.Walter.HP < 50)
                {
                    sum += 0.3f;
                }
                if (newWorld.Walter.Hunger > 50)
                {
                    sum += 0.1f;
                }

                return sum ;
            }

            float inventoryIncreased()
            {
                return (newWorld.InventoryQuantity() - this.InventoryQuantity()) * RatioDict["inventory"];
            }

            float hasAxes()
            {
                float sum = 0;
                foreach ( var weapon in Equip.Weapons)
                {
                    if (newWorld.Possesses(weapon) || newWorld.IsEquipped(weapon))
                    {
                        sum += 0.5f;
                    }
                }
                return sum * RatioDict["axes"] ;
            }
        }


    

        //Getting next action for mcts selection
        public ActionDST GetNextAction() //
        {
            if (this.ActionTracker < this.AvailableActions.Count ) {
                //Console.WriteLine("Tracker: " + ActionTracker);
                int ActionTrackerOld = this.ActionTracker;
                this.ActionTracker = (this.ActionTracker+1) % this.AvailableActions.Count;

                return this.AvailableActions[ActionTrackerOld];               
            }
            return null;
      }

        public List<ActionDST> GetExecutableActions()
        {
            return this.AvailableActions;
        }

        public WorldModelDST()
        {
        }

        public abstract void PrintWorldObjects(List<string> objects);
        public abstract List<string> CheckWorldObjects();


        public abstract WorldModelDST GenerateChildWorldModel();

        public abstract int InventoryQuantity();

        public abstract int FoodQuantity();

        public abstract int GetQuantity(string prefab);

        public abstract Boolean Possesses(string prefab);

        public abstract Boolean Possesses(string prefab, int quantity);

        public abstract Boolean IsEquipped(string prefab);

        public abstract Boolean WorldHas(string prefab);

        public abstract Boolean WorldHas(string prefab, int quantity);

        public abstract void RemoveFromPossessedItems(string prefab, int quantity);

        public abstract void AddToPossessedItems(string prefab, int quantity);

        public abstract void RemoveFromWorld(string prefab, int quantity);

        public abstract void AddToWorld(string prefab, int quantity, int posx, int posz);

        public void RemoveAction(string actionName)
        {          
            foreach(ActionDST action in this.AvailableActions)
            {
                if(action.Name == actionName)
                {
                    this.AvailableActions.Remove(action);
                    break;
                }
            }
        }

        public void AddAction(ActionDST action)
        {
            if (NowCanDo(action.Name))
            {
                this.AvailableActions.Add(action);
            }
        }

        public Boolean NowCanDo(string actionName)
        {
            foreach (var action in this.AvailableActions)
            {
                if (action.Name == actionName)
                {
                    return true;
                }
            }
            return false;
        }

        public abstract void AddToEquipped(string item);

        public abstract void RemoveFromEquipped(string item);

        public float LightValueDay()
        {
            if (this.IsEquipped("torch"))
            {
                return 0.0f;
            }
            else if (this.Possesses("torch"))
            {
                return 1.0f;
            }
            else if ((this.Possesses("cutgrass", 2) && this.Possesses("twigs", 2)) || (this.Possesses("log", 2) && this.Possesses("cutgrass", 3)) || (this.Possesses("log", 2) && this.Possesses("rocks", 12)))
            {
                return 0.6f;
            }
            else if (this.Possesses("cutgrass") || this.Possesses("twigs") || this.Possesses("log") || this.Possesses("rocks"))
            {
                return 0.3f;
            }
            else
            {
                return 0.0f;
            }
        }

        public float LightValueNight()
        {
            float maxdistance = float.MaxValue;
            float dist = float.MaxValue;
            foreach (var fire in GetFires())
            {
                dist = DistanceCalculator(fire.Item2, fire.Item3);
                if (dist < maxdistance)
                {
                    maxdistance = dist;
                }
            }

            if (this.IsEquipped("torch") || dist < 6)
            {
                return 1.0f;
            }
            else
            {
                return 0.0f;
            }
        }

        public abstract List<Tuple<string, int, int>> GetFires();

        public float AxePickaxeValue()
        {
            Boolean b1 = this.Possesses("axe");
            Boolean b2 = this.IsEquipped("axe");
            Boolean b3 = this.Possesses("pickaxe");
            Boolean b4 = this.IsEquipped("pickaxe");

            if ((b1 || b2) && (b3 || b4))
            {
                return 1.0f;
            }
            else if ((WorldHas("tree") && (b1 || b2)) || (WorldHas("boulder") && (b3 || b4)))
            {
                return 0.75f;
            }
            else if (b1 || b2 || b3 || b4)
            {
                return 0.4f;
            }
            else
            {
                return 0.0f;
            }
        }

        public float FoodValue()
        {
            int foodCount = this.FoodQuantity();
            float invFoodValue;

            if (foodCount >= 5)
            {
                invFoodValue = 1.0f;
            }
            else
            {
                float fc = Convert.ToSingle(foodCount);
                invFoodValue = fc / 5.0f;
            }

            float hungerValue;

            if (this.Walter.Hunger >= 100)
            {
                hungerValue = 1;
            }
            else
            {
                hungerValue = Convert.ToSingle(1.0 / (Math.Pow(Convert.ToDouble((Convert.ToSingle(this.Walter.Hunger) - 150.0)/50.0), 2)));
            }

            return hungerValue * 0.6f + invFoodValue * 0.4f;
        }

        public abstract Boolean HasFuel();

        protected float DistanceCalculator(int posxObject, int poszObject)
        {
            float Posx = Convert.ToSingle(posxObject);
            float Posz = Convert.ToSingle(poszObject);

            return Convert.ToSingle(Math.Pow(Convert.ToDouble(this.Walter.Position.Item1 - Posx), 2) + Math.Pow(Convert.ToDouble(this.Walter.Position.Item2 - Posz), 2));
        }

        public void IncreaseHunger(int n)
        {
            this.Walter.IncreaseHunger(n);
        }

        public void IncreaseHP(int n)
        {
            this.Walter.IncreaseHP(n);
        }

        public void DecreaseHunger(int n)
        {
            this.Walter.DecreaseHunger(n);
        }

        public void DecreaseHP(int n)
        {
            this.Walter.DecreaseHP(n);
        }
    }
}