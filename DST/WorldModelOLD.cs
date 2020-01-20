using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.Actions;
using MCTS.DST;

namespace MCTS.DST.WorldModels
{
    public class WorldModelOLD_DST : WorldModelDST
    {
        public List<Pair<Pair<string, int>, Pair<int, int>>> WorldObjects;
        public List<Tuple<string, int, int>> Fire;
        public List<Pair<string, int>> PossessedItems;
        public List<string> EquippedItems;
        public List<Pair<string, int>> Fuel;

        public WorldModelOLD_DST(Character character, List<Pair<Pair<string, int>, Pair<int, int>>> worldObjects, List<Pair<string, int>> possessedItems, List<string> equippedItems, float cycle, int[] cycleInfo, List<ActionDST> availableActions, WorldModelDST parent, List<Pair<string, int>> fuel, List<Tuple<string, int, int>> fire) 
            : base(character, cycle, cycleInfo, availableActions, parent)
        {
            this.WorldObjects = worldObjects;
            this.PossessedItems = possessedItems;
            this.EquippedItems = equippedItems;
            this.Fuel = fuel;
            this.Fire = fire;
        }

        public WorldModelOLD_DST(PreWorldState preWorldState) : base(preWorldState)
        {

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
            getActions();
        }

        public WorldModelOLD_DST()
        {
        }


        public override void PrintWorldObjects(List<string> objects)
        {

        }
        public override void PrintInventory()
        {

        }

        public override List<string> CheckWorldObjects()
        {
            return null;
        }

        public override WorldModelDST GenerateChildWorldModel()
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

            return new WorldModelOLD_DST(walter, worldObjects, possessedItems, equippedItems, cycle, cycleInfo, availableActions, this, fuel, fire);
        }

        public override int InventoryQuantity()
        {
            int r = 0;
            foreach (Pair<string, int> tuple in this.PossessedItems)
            {
                r += tuple.Item2;
            }
            return r;
        }

        public override int FoodQuantity()
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

        public override int GetQuantity(string prefab)
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

        public override List<string> GetPossessedItems()
        {
            return null;
        }
        public override Boolean Possesses(string prefab)
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

        public override Boolean Possesses(string prefab, int quantity)
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

        public override Boolean IsEquipped(string prefab)
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

        public override Boolean WorldHas(string prefab)
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

        public override Boolean WorldHas(string prefab, int quantity)
        {
            Boolean r = false;
            foreach (var tuple in this.WorldObjects)
            {
                var pair = tuple.Item1;
                if (pair.Item1 == prefab && pair.Item2 >= quantity)
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

        public override void RemoveFromPossessedItems(string prefab, int quantity)
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

        
        public override void AddToPossessedItems(string prefab, int quantity)
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

        public override void RemoveFromWorld(string prefab, int quantity)
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

        public override void AddToWorld(string prefab, int quantity, int posx, int posz)
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

        public override void AddToEquipped(string item)
        {
            this.EquippedItems.Add(item);
        }

        public override void RemoveFromEquipped(string item)
        {
            this.EquippedItems.Remove(item);
        }

        public override List<Tuple<string, int, int>> GetFires()
        {
            return this.Fire;
        }

        public override Boolean HasFuel()
        {
            return this.Fuel.Count > 0;
        }
    }
}