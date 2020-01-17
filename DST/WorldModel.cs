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

            //Getting Wander
            this.AvailableActions = new List<ActionDST>();
            //


            //<OPTIMIZATION - generalized cases from action's own lists>
            //Getting Eat based Actions
            foreach (var food in Eat.FoodIndex.Keys)
            {
                int quantity = this.GetQuantity(food);
                if (quantity > 0)
                {
                    this.AvailableActions.Add(new Eat(food));
                    this.AvailableActions.Add(new Drop(food, quantity, this.Walter.Position));
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

            foreach ( var pickupable in Pickup.Pickupables)
            {
                if (WorldHas(pickupable))
                {
                    this.AvailableActions.Add(new Pickup(pickupable, 1));
                }
            }
               
            if (IsEquipped("pickaxe") && WorldHas("boulder"))
            {
                this.AvailableActions.Add(new Chop("tree"));
            }

            //this.checkAvailableActions();
        }

        public void checkAvailableActions()
        {
            foreach (var action in this.AvailableActions)
            {
                Console.WriteLine(action);
            }
        }

        public float Score(WorldModelDST newWorld)
        {
           
            float total = 0;
            total += statusIncrease(0.2f);
            int i = 0;
            Console.WriteLine("Score: " + i++ + " " + total);
            total += statusLow(1);
            Console.WriteLine("Score: " + i++ + " " + total);

            total += inventoryIncreased(0.5f);
            Console.WriteLine("Score: " + i++ + " " + total);

            total += hasAxes(1); 
            Console.WriteLine("Score: " + i++ + " " + total);

            total += this.LightValueDay() + this.LightValueNight();
            Console.WriteLine("Score: " + i++ + " " + total);

            total += AxePickaxeValue();
            Console.WriteLine("Score: " + i++ + " " + total);

            //Console.WriteLine("Score: " + total);
            return total;

            float statusIncrease(float ratio)
            {
                float sum = 0;
                //sum += newWorld.Walter.HP - this.Walter.HP;
                //Console.WriteLine("HP: " + sum);
                Console.WriteLine("Hunger old: " + this.Walter.Hunger);

                Console.WriteLine("Hunger new: " + newWorld.Walter.Hunger);
                sum +=  newWorld.Walter.Hunger - this.Walter.Hunger;
                Console.WriteLine("Hunger: " + sum);

                sum += newWorld.Walter.Sanity - this.Walter.Sanity;
                Console.WriteLine("Sanity: " + sum);

                return sum * ratio;
            }

            float statusLow(float ratio)
            {
                float sum = 0;
                if (newWorld.Walter.HP < 50)
                {
                    sum -= 1;
                }
                if (newWorld.Walter.Hunger > 50)
                {
                    sum -= 1;
                }
                if (newWorld.Walter.Sanity < 50)
                {
                    sum -= 1;
                }
                return sum * ratio;
            }

            float inventoryIncreased(float ratio)
            {
                return (newWorld.InventoryQuantity() - this.InventoryQuantity()) * ratio;
            }

            float hasAxes(float ratio)
            {
                float sum = 0;
                foreach ( var weapon in Equip.Weapons)
                {
                    if (newWorld.Possesses(weapon) || newWorld.IsEquipped(weapon))
                    {
                        sum += 1;
                    }
                }
                return sum * ratio;
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