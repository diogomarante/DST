using System;
using System.Collections.Generic;
using Utilities;
using MCTS.DST.Actions;
using MCTS.DST;

namespace MCTS.DST.WorldModels
{
    public class WorldModelFEAR_DST : WorldModelDST
    {
        public static int POSSESSED_ITEMS_NUM = 10;
        public static int WORLD_ITEMS_NUM = 18;

        public enum HandleableEquipment
        {
            none = -1,
            torch = 2,
            axe = 8,
            pickaxe = 9,
        }

        public int[] WorldObjects;
        public Pair<int, int>[] WorldObjectsPos;
        public int[] PossessedItems;

        public HandleableEquipment HandEquipped;            //Given the considered items, only handleable ones exist, therefore a single value will sufice

        public static Dictionary<string, int> WorldObjectsIndex = new Dictionary<string, int>()
        {
            {"twigs", 0},
            {"log", 1},
            {"torch", 2},
            {"cutgrass", 3},
            {"carrot", 4},
            {"berries", 5},
            {"rocks", 6},
            {"flint", 7},
            {"axe", 8},
            {"pickaxe", 9},
            {"tree", 10},
            {"boulder", 11},
            {"sapling", 12},
            {"berrybush", 13},
            {"grass", 14},
            {"carrot_planted", 15},
            {"campfire", 16},
            {"firepit", 17},
        };
        public static Dictionary<string, int> EntitiesIndex = new Dictionary<string, int>()
        {
            {"tree", 10},
            {"boulder", 11},
            {"sapling", 12},
            {"berrybush", 13},
            {"grass", 14},
            {"carrot_planted", 15},
            {"campfire", 16},
            {"firepit", 17},
        };
        public static Dictionary<string, int> FireIndex = new Dictionary<string, int>()
        {
            {"campfire", 16},
            {"firepit", 17},
        };
        public static Dictionary<string, int> FuelIndex = new Dictionary<string, int>()
        {
            {"twigs", 0},
            {"log", 1},
            {"cutgrass", 3},
        };
        public static Dictionary<string, int> PossessedItemsIndex = new Dictionary<string, int>()
        {
            {"twigs", 0},
            {"log", 1},
            {"torch", 2},
            {"cutgrass", 3},
            {"carrot", 4},
            {"berries", 5},
            {"rocks", 6},
            {"flint", 7},
            {"axe", 8},
            {"pickaxe", 9},
        };
        public static Dictionary<string, int> FoodItemsIndex = new Dictionary<string, int>()
        {
            {"carrot", 4},
            {"berries", 5},
        };


        public WorldModelFEAR_DST(Character character, Pair<int[], Pair<int, int>[]> worldObjects, int[] possessedItems, HandleableEquipment handEquipped, float cycle, int[] cycleInfo, List<ActionDST> availableActions, WorldModelDST parent)
        : base(character, cycle, cycleInfo, availableActions, parent)
        {
            this.WorldObjects = worldObjects.Item1;
            this.WorldObjectsPos = worldObjects.Item2;
            this.PossessedItems = possessedItems;
            this.HandEquipped = handEquipped;
        }

        public WorldModelFEAR_DST(PreWorldState preWorldState) : base(preWorldState)
        {
            //Getting Inventory from PreWorldState
            
            int size1 = preWorldState.Inventory.Count;            
            this.PossessedItems = new int[POSSESSED_ITEMS_NUM];

            for (int i = 0; i < size1; i++)
            {
                if (PossessedItemsIndex.ContainsKey(preWorldState.Inventory[i].Item1))
                    this.PossessedItems[PossessedItemsIndex[preWorldState.Inventory[i].Item1]] =
                    preWorldState.Inventory[i].Item3;
            }

            //Getting Equipped items from PreWorldState

            this.HandEquipped = preWorldState.Equipped.Count > 0 ? (HandleableEquipment)WorldObjectsIndex[preWorldState.Equipped[0].Item1] : HandleableEquipment.none;

            //Getting WorldObjects from PreWorldState's Entities

            int size3 = preWorldState.Entities.Count;
            this.WorldObjects = new int[WORLD_ITEMS_NUM];
            this.WorldObjectsPos = new Pair<int, int>[WORLD_ITEMS_NUM];

            for (int i = 0; i < size3; i++)
            {
                if (WorldObjectsIndex.ContainsKey(preWorldState.Entities[i].Prefab))
                {
                    this.WorldObjects[WorldObjectsIndex[preWorldState.Entities[i].Prefab]] = preWorldState.Entities[i].Quantity;
                    this.WorldObjectsPos[WorldObjectsIndex[preWorldState.Entities[i].Prefab]] = preWorldState.Entities[i].Position;
                }
            }

            //Getting Available Actions

            getActions();
            PrintWorldObjects(CheckWorldObjects());
            PrintInventory();
        }

        public WorldModelFEAR_DST()
        {
        }

        public override void PrintWorldObjects(List<string> objects)
        {
            Console.WriteLine("");
            Console.WriteLine("Objects:");
            foreach (var obj in objects)
            {
                Console.WriteLine("\t" + obj);
            }
            Console.WriteLine("");
        }

        public override void PrintInventory()
        {
            Console.WriteLine("Inventory: ");

            foreach (var index in PossessedItemsIndex)
            {
                if (PossessedItems[index.Value] > 0)
                {
                    Console.WriteLine("\t" + index.Key + ": " + PossessedItems[index.Value]);

                }
            }
            Console.WriteLine("");

            Console.WriteLine("Status: ");
            Console.WriteLine("\tHP: " + this.Walter.HP);
            Console.WriteLine("\tHunger: " + this.Walter.Hunger);


        }


        public override List<string> CheckWorldObjects()
        {
            List<string> objects = new List<string>();
            foreach( var obj in WorldObjectsIndex)
            {
                if (this.WorldObjects[obj.Value] > 0)
                {
                    objects.Add(obj.Key);
                }
            }
            return objects;
        }

        public override WorldModelDST GenerateChildWorldModel()
        {
            Character walter = new Character(this.Walter.HP, this.Walter.Hunger, this.Walter.Sanity, this.Walter.Position.Item1, this.Walter.Position.Item2);

            var worldObjects = (int[])this.WorldObjects.Clone();
            var worldObjectsPos = (Pair<int, int>[])this.WorldObjectsPos.Clone();
            var combinedWorldObjects = new Pair<int[], Pair<int, int>[]>(worldObjects, worldObjectsPos);

            var possessedItems = (int[]) this.PossessedItems.Clone();

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

            return new WorldModelFEAR_DST(walter, combinedWorldObjects, possessedItems, HandEquipped, cycle, cycleInfo, availableActions, this);
        }

        public override int InventoryQuantity()
        {
            int r = 0;
            foreach (var index in PossessedItemsIndex.Values)
            {
                r += this.PossessedItems[index];
            }
            return r;
        }

        public override int FoodQuantity()
        {
            int r = 0;
            foreach (var index in FoodItemsIndex.Values)
            {
                r += this.PossessedItems[index];
            }
            return r;
        }

        public override int GetQuantity(string prefab)
        {
            return this.PossessedItems[PossessedItemsIndex[prefab]];
        }

        public override List<string> GetPossessedItems()
        {
            List<string> items = new List<string>();
            foreach (var index in PossessedItemsIndex)
            {
                if (PossessedItems[index.Value] > 0)
                {
                    items.Add(index.Key);
                }
            }
            return items;
        }

        public override Boolean Possesses(string prefab)
        {
            return Possesses(prefab, 1);
        }

        public override Boolean Possesses(string prefab, int quantity)
        {
            return this.PossessedItems[PossessedItemsIndex[prefab]] >= quantity;
        }

        public override Boolean IsEquipped(string prefab)
        {
            return this.HandEquipped.ToString().Equals(prefab);
        }

        public override Boolean WorldHas(string prefab)
        {
            return WorldHas(prefab, 1);
        }

        public override Boolean WorldHas(string prefab, int quantity)
        {
            return this.WorldObjects[WorldObjectsIndex[prefab]] >= quantity;
        }

        public override void RemoveFromPossessedItems(string prefab, int quantity)
        {
            this.PossessedItems[PossessedItemsIndex[prefab]] -= quantity;
        }

        public override void AddToPossessedItems(string prefab, int quantity)
        {
            this.PossessedItems[PossessedItemsIndex[prefab]] += quantity;
        }

        public override void RemoveFromWorld(string prefab, int quantity)
        {
            this.WorldObjects[WorldObjectsIndex[prefab]] -= quantity;
        }

        public override void AddToWorld(string prefab, int quantity, int posx, int posz)
        {
            this.WorldObjects[WorldObjectsIndex[prefab]] += quantity;
            this.WorldObjectsPos[WorldObjectsIndex[prefab]] = new Pair<int, int>(posx, posz);
        }

        public override void AddToEquipped(string item)
        {
            this.HandEquipped = (HandleableEquipment) PossessedItemsIndex[item];
        }

        public override void RemoveFromEquipped(string item)
        {
            this.HandEquipped = HandleableEquipment.none;
        }

        public override List<Tuple<string, int, int>> GetFires()
        {
            var firePositionsList = new List<Tuple<string, int, int>>(FireIndex.Count);
            foreach (var entry in FireIndex)
            {
                if (WorldObjects[entry.Value] > 0)
                {
                    var pos = WorldObjectsPos[entry.Value];
                    firePositionsList.Add(new Tuple<string, int, int>(entry.Key, pos.Item1, pos.Item2));
                }
            }

            return firePositionsList;
        }

        public override Boolean HasFuel()
        {
            foreach (var index in FuelIndex.Values)
            {
                if (this.PossessedItems[index] > 0) return true;
            }

            return false;
        }
    }
}