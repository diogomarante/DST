using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.Actions;
using MCTS.DST;

namespace MCTS.DST.WorldModels
{
    public class WorldModelDST
    {
        public Character Walter;
        public List<Pair<Pair<string, int>, Pair<int, int>>> WorldObjects;
        public List<Tuple<string, int, int>> Fire;
        public List<Pair<string, int>> PossessedItems;
        public List<string> EquippedItems;
        public List<Pair<string, int>> Fuel;

        public float Cycle;
        public int[] CycleInfo;
        public List<ActionDST> AvailableActions;
        public int ActionTracker = 0;
        public int PosX;
        public int PosZ;


        public List<string> foods = new List<string>();

        public List<string> weapons = new List<string>();


        protected WorldModelDST Parent;

        public WorldModelDST(Character character, List<Pair<Pair<string, int>, Pair<int, int>>> worldObjects, List<Pair<string, int>> possessedItems, List<string> equippedItems, float cycle, int[] cycleInfo, List<ActionDST> availableActions, WorldModelDST parent, List<Pair<string, int>> fuel, List<Tuple<string, int, int>> fire)
        {
            this.Walter = character;
            this.WorldObjects = worldObjects;
            this.PossessedItems = possessedItems;
            this.EquippedItems = equippedItems;
            this.Cycle = cycle;
            this.CycleInfo = cycleInfo;
            this.AvailableActions = availableActions;
            this.Parent = parent;
            this.Fuel = fuel;
            this.Fire = fire;
        }

        public WorldModelDST(PreWorldState preWorldState)
        {
            this.Parent = null;

            this.Walter = preWorldState.Walter;
            this.Cycle = preWorldState.Cycle;
            this.CycleInfo = preWorldState.CycleInfo;

            //Getting Inventory from PreWorldState

            int size1 = preWorldState.Inventory.Count;
            this.PossessedItems = new List<Pair<string, int>>(size1);

            for (int i = 0; i < size1; i++)
            {
                Pair<string, int> tuple1 = new Pair<string, int>(preWorldState.Inventory[i].Item1, preWorldState.Inventory[i].Item3);
                this.PossessedItems.Add(tuple1);
            }

            //Getting Fuel items from PreWorldState

            this.Fuel = new List<Pair<string, int>>(preWorldState.Fuel.Count);

            foreach (var fuelItem in preWorldState.Fuel)
            {
                Pair<string, int> tuple1 = new Pair<string, int>(fuelItem.Item1, fuelItem.Item3);
                this.Fuel.Add(tuple1);
            }

            //Getting Fire Info

            this.Fire = preWorldState.Fire;

            //Getting Equipped items from PreWorldState

            int size2 = preWorldState.Equipped.Count;
            this.EquippedItems = new List<string>(size2);

            for (int i = 0; i < size2; i++)
            {
                this.EquippedItems.Add(preWorldState.Equipped[i].Item1);
            }

            //Getting WorldObjects from PreWorldState's Entities

            int size3 = preWorldState.Entities.Count;
            this.WorldObjects = new List<Pair<Pair<string, int>, Pair<int, int>>>(size3);

            for (int i = 0; i < size3; i++)
            {
                Pair<string, int> npair = new Pair<string, int>(preWorldState.Entities[i].Prefab, preWorldState.Entities[i].Quantity);
                Pair<Pair<string, int>, Pair<int, int>> tolist = new Pair<Pair<string, int>, Pair<int, int>>(npair, preWorldState.Entities[i].Position);
                this.WorldObjects.Add(tolist);
            }


            //Getting Available Actions

            //Getting Wander
            this.checkWorldObjects();
            this.AvailableActions = new List<ActionDST>();
            this.AvailableActions.Add(new Wander());

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

            foreach (var weapon in Equip.Weapons)
            {
                if (Possesses(weapon))
                {
                    this.AvailableActions.Add(new Equip(weapon));
                }
            }

            foreach (var weapon in this.EquippedItems)
            {
                if (weapon == "axe")
                {
                    foreach (var obj in this.WorldObjects)
                    {
                        if (obj.Item1.Item1 == "tree")
                        {
                            this.AvailableActions.Add(new Chop("tree"));

                        }
                    }

                }
            }

            foreach (var weapon in this.EquippedItems)
            {
                if (weapon == "pickaxe")
                {
                    foreach (var obj in this.WorldObjects)
                    {
                        if (obj.Item1.Item1 == "boulder")
                        {
                            this.AvailableActions.Add(new Chop("boulder"));
                        }
                    }

                }
            }
            //this.checkAvailableActions();
        }

        public void checkWorldObjects()
        {
            foreach (var obj in this.WorldObjects)
            {
                Console.WriteLine(obj);
            }
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
            total += statusIncrease(1);
            total += statusLow(1);
            total += inventoryIncreased(0.5f);
            total += hasAxes(1);
            total += this.LightValueDay() + this.LightValueNight();
            Console.WriteLine("Score: " + total);
            return total;

            float statusIncrease(float ratio)
            {
                float sum = 0;
                sum += newWorld.Walter.HP - this.Walter.HP;
                sum += newWorld.Walter.Hunger - this.Walter.Hunger;
                sum += newWorld.Walter.Sanity - this.Walter.Sanity;
                return sum * ratio;
            }
            float statusLow(float ratio)
            {
                float sum = 0;
                if (newWorld.Walter.HP < 50)
                {
                    sum -= 1;
                }
                if (newWorld.Walter.Hunger < 50)
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
                return (newWorld.PossessedItems.Count - this.PossessedItems.Count) * ratio;
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
                return this.AvailableActions[this.ActionTracker++];               
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

        public WorldModelDST GenerateChildWorldModel()
        {
            Character walter = new Character(this.Walter.HP, this.Walter.Hunger, this.Walter.Sanity, this.Walter.Position.Item1, this.Walter.Position.Item2);
            List<Pair<Pair<string, int>, Pair<int, int>>> worldObjects = new List<Pair<Pair<string, int>, Pair<int, int>>>(this.WorldObjects);
            foreach (var item in this.WorldObjects)
            {
                worldObjects.Add(item);
            }

            List<Tuple<string, int, int>> fire = new List<Tuple<string, int, int>>(this.Fire.Count);
            foreach (var item in this.Fire)
            {
                fire.Add(item);
            }

            List<Pair<string, int>> possessedItems = new List<Pair<string, int>>(this.PossessedItems.Count);
            foreach (var item in this.PossessedItems)
            {
                possessedItems.Add(item);
            }

            List<string> equippedItems = new List<string>(this.EquippedItems.Count);
            foreach (var item in this.EquippedItems)
            {
                equippedItems.Add(item);
            }

            List<Pair<string, int>> fuel = new List<Pair<string, int>>(this.Fuel.Count);
            foreach (var item in this.Fuel)
            {
                fuel.Add(item);
            }

            float cycle = this.Cycle;

            int[] cycleInfo = new int[3];
            cycleInfo[0] = this.CycleInfo[0];
            cycleInfo[1] = this.CycleInfo[1];
            cycleInfo[2] = this.CycleInfo[2];

            List<ActionDST> availableActions = new List<ActionDST>(this.AvailableActions.Count);
            foreach (var item in this.AvailableActions)
            {
                availableActions.Add(item);
            }

            return new WorldModelDST(walter, worldObjects, possessedItems, equippedItems, cycle, cycleInfo, availableActions, this, fuel, fire);
        }

        public int FoodQuantity()
        {
            int r = 0;
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == "berries" || tuple.Item1 == "carrot")
                {
                    r += tuple.Item2;
                }
            }
            return r;
        }

        public int GetQuantity(string prefab)
        {
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == prefab)
                {
                    return tuple.Item2; // quantity;
                }
            }
            return 0;
        }
        public Boolean Possesses(string prefab)
        {
            Boolean r = false;
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == prefab)
                {
                    r = true;
                    break;
                }
            }
            return r;
        }

        public Boolean Possesses(string prefab, int quantity)
        {
            Boolean r = false;
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == prefab && tuple.Item2 >= quantity)
                {
                    r = true;
                    break;
                }
            }
            return r;
        }

        public Boolean IsEquipped(string prefab)
        {
            Boolean r = false;
            foreach (string str in this.EquippedItems)
            {
                if (str == prefab)
                {
                    r = true;
                    break;
                }
            }
            return r;
        }

        public Boolean WorldHas(string prefab)
        {
            Boolean r = false;
            foreach (var tuple in this.WorldObjects)
            {
                if (tuple.Item1.Item1 == prefab)
                {
                    r = true;
                    break;
                }
            }
            return r;
        }

        public void AddToFire(string prefab, int posx, int posz)
        {
            Tuple<string, int, int> tuple = new Tuple<string, int, int>(prefab, posx, posz);
            this.Fire.Add(tuple);
        }

        public void RemoveFromFuel(string prefab)
        {
            foreach (var tuple in this.Fuel)
            {
                if (tuple.Item1 == prefab)
                {
                    if (tuple.Item2 == 1)
                    {
                        Pair<string, int> itemtoremove = new Pair<string, int>(prefab, 1);
                        this.Fuel.Remove(itemtoremove);
                    }
                    else
                    {
                        tuple.Item2 -= 1;
                    }
                    break;
                }
            }
        }

        public void AddToFuel(string prefab, int quantity)
        {
            Boolean r = true;
            foreach (Pair<string, int> tuple in this.Fuel)
            {
                if (tuple.Item1 == prefab)
                {
                    tuple.Item2 += quantity;
                    r = false;
                    break;
                }
            }
            if (r)
            {
                Pair<string, int> newitem = new Pair<string, int>(prefab, quantity);
                this.PossessedItems.Add(newitem);
            }
        }

        public void RemoveFromPossessedItems(string prefab, int quantity)
        {
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == prefab)
                {
                    if (tuple.Item2 == quantity)
                    {
                        Pair<string, int> itemtoremove = new Pair<string, int>(prefab, quantity);
                        this.PossessedItems.Remove(itemtoremove);
                    }
                    else
                    {
                        tuple.Item2 -= quantity;
                    }
                    break;
                }
            }
        }

        
        public void AddToPossessedItems(string prefab, int quantity)
        {
            Boolean r = true;
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                if (tuple.Item1 == prefab)
                {
                    tuple.Item2 += quantity;
                    r = false;
                    break;
                }
            }
            if (r)
            {
                Pair<string, int> newitem = new Pair<string, int>(prefab, quantity);
                this.PossessedItems.Add(newitem);
            }
        }

        public void RemoveFromWorld(string prefab, int quantity)
        {
            foreach (var tuple in this.WorldObjects)
            {
                if (tuple.Item1.Item1 == prefab)
                {
                    if (tuple.Item1.Item2 == quantity)
                    {                        
                        this.WorldObjects.Remove(tuple);
                    }
                    else
                    {
                        tuple.Item1.Item2 -= quantity;
                    }
                    break;
                }
            }
        }

        public void AddToWorld(string prefab, int quantity, int posx, int posz)
        {
            Boolean r = true;
            foreach (var tuple in this.WorldObjects)
            {
                if (tuple.Item1.Item1 == prefab)
                {
                    tuple.Item1.Item2 += quantity;
                    r = false;
                    break;
                }
            }
            if (r)
            {
                Pair<int, int> position = new Pair<int, int>(posx, posz);
                Pair<string, int> newitem = new Pair<string, int>(prefab, quantity);
                Pair<Pair<string, int>, Pair<int, int>> newpair = new Pair<Pair<string, int>, Pair<int, int>>(newitem, position);
                this.WorldObjects.Add(newpair);
            }
        }

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

        public void AddToEquipped(string item)
        {
            this.EquippedItems.Add(item);
        }

        public void RemoveFromEquipped(string item)
        {
            this.EquippedItems.Remove(item);
        }

      

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
            foreach (var fire in this.Fire)
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

        //public float AxePickaxeValue()
        //{
        //    Boolean b1 = this.Possesses("axe");
        //    Boolean b2 = this.IsEquipped("axe");
        //    Boolean b3 = this.Possesses("pickaxe");
        //    Boolean b4 = this.IsEquipped("pickaxe");

        //    if ((b1 || b2) && (b3 || b4))
        //    {
        //        return 1.0f;
        //    }
        //    else if ((WorldHas("tree") && (b1 || b2)) || (WorldHas("boulder") && (b3 || b4)))
        //    {
        //        return 0.75f;
        //    }
        //    else if (b1 || b2 || b3 || b4)
        //    {
        //        return 0.4f;
        //    }
        //    else
        //    {
        //        return 0.0f;
        //    }
        //}

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

        public Boolean HasFuel()
        {
            return this.Fuel.Count > 0;
        }

        private float DistanceCalculator(int posxObject, int poszObject)
        {
            float Posx = Convert.ToSingle(posxObject);
            float Posz = Convert.ToSingle(poszObject);

            return Convert.ToSingle(Math.Pow(Convert.ToDouble(this.Walter.Position.Item1 - Posx), 2) + Math.Pow(Convert.ToDouble(this.Walter.Position.Item2 - Posz), 2));
        }

        public Pair<int, int> GetNextPosition(string prefab, string place)
        {
            if (place == "fire")
            {
                foreach (var item in this.Fire)
                {
                    if (item.Item1 == prefab)
                    {
                        Pair<int, int> pair = new Pair<int, int>(item.Item2, item.Item3);
                        return pair;
                    }                   
                }
            }
            else if (place == "world")
            {
                foreach (var item in this.WorldObjects)
                {
                    if (item.Item1.Item1 == prefab)
                    {
                        return item.Item2;
                    }
                }
            }

            return this.Walter.Position;
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