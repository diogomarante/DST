using System;
using System.Collections.Generic;
using Utilities;
using KnowledgeBase;
using MCTS.DST.WorldModels;
using WellFormedNames;
using System.Linq;

namespace MCTS.DST { 

    public class PreWorldState
    {        
        public Character Walter;
        public float Cycle;
        public int[] CycleInfo;

        public List<ObjectProperties> Entities;
        public List<Tuple<string, int, int>> Inventory;
        public List<Pair<string, int>> Equipped;
        public List<Tuple<string, int, int>> Fuel;
        public List<Tuple<string, int, int>> Fire;
        public KB KnowledgeBase;

       
        public PreWorldState(KB knowledgeBase)
        {
            this.KnowledgeBase = knowledgeBase;
            this.Entities = new List<ObjectProperties>();
            this.Inventory = new List<Tuple<string, int, int>>();
            this.Equipped = new List<Pair<string, int>>();
            this.Fuel = new List<Tuple<string, int, int>>();
            this.Fire = new List<Tuple<string, int, int>>();

            //Getting Character Stats

            var hp = knowledgeBase.AskProperty((Name)"Health(Walter)");           
            int HP = int.Parse(hp.Value.ToString());

            var hunger = knowledgeBase.AskProperty((Name)"Hunger(Walter)");
            int Hunger = int.Parse(hunger.Value.ToString());

            var sanity = knowledgeBase.AskProperty((Name)"Sanity(Walter)");
            int Sanity = int.Parse(sanity.Value.ToString());

            var posx = knowledgeBase.AskProperty((Name)"PosX(Walter)");
            var PosX = int.Parse(posx.Value.ToString());

            var posz = knowledgeBase.AskProperty((Name)"PosZ(Walter)");
            var PosZ = int.Parse(posz.Value.ToString());

            this.Walter = new Character(HP, Hunger, Sanity, PosX, PosZ);

            //Getting Day Properties

            var cycle = knowledgeBase.AskProperty((Name)"World(CurrentSegment)");
            this.Cycle = float.Parse(cycle.Value.ToString());

            var cycleinfo1 = knowledgeBase.AskProperty((Name)"World(PhaseLenght, day)");
            var Cycleinfo1 = int.Parse(cycleinfo1.Value.ToString());

            var cycleinfo2 = knowledgeBase.AskProperty((Name)"World(PhaseLenght, dusk)");
            var Cycleinfo2 = int.Parse(cycleinfo2.Value.ToString());

            var cycleinfo3 = knowledgeBase.AskProperty((Name)"World(PhaseLenght, night)");
            var Cycleinfo3 = int.Parse(cycleinfo3.Value.ToString());

            this.CycleInfo = new int[3];
            this.CycleInfo[0] = Cycleinfo1;
            this.CycleInfo[1] = Cycleinfo2;
            this.CycleInfo[2] = Cycleinfo3;

            //Getting Entities + Inventory + Equipped

            var subset = new List<SubstitutionSet> { new SubstitutionSet() };

            //Getting Equipped

            var equippeditems = knowledgeBase.AskPossibleProperties((Name)"IsEquipped([GUID])", Name.SELF_SYMBOL, subset);

           

            foreach (var item in equippeditems)
            {
                string strEntGuid = item.Item2.FirstOrDefault().FirstOrDefault().SubValue.Value.ToString();
                int entGuid = int.Parse(strEntGuid);
                string entPrefab = knowledgeBase.AskProperty((Name)("Entity(" + strEntGuid + ")")).Value.ToString();

                Pair<string, int> pair = new Pair<string, int>(entPrefab, entGuid);
                this.Equipped.Add(pair);
            }

            //Getting Inventory

            var inventory = knowledgeBase.AskPossibleProperties((Name)"InInventory([GUID])", Name.SELF_SYMBOL, subset);

            foreach (var item in inventory)
            {
                string strEntGuid = item.Item2.FirstOrDefault().FirstOrDefault().SubValue.Value.ToString();
                int entGuid = int.Parse(strEntGuid);
                string entPrefab = knowledgeBase.AskProperty((Name)("Entity(" + strEntGuid + ")")).Value.ToString();

                string strEntQuantity = "Quantity(" + strEntGuid + ")";
                var quantity = knowledgeBase.AskProperty((Name)strEntQuantity);
                int entQuantity = int.Parse(quantity.Value.ToString());

                Tuple<string, int, int> tuple = new Tuple<string, int, int>(entPrefab, entGuid, entQuantity);
                this.Inventory.Add(tuple);

                if (IsFuel(strEntGuid))
                {                    
                    this.Fuel.Add(tuple);
                }                
            }

            //Getting Entities

            var entities = knowledgeBase.AskPossibleProperties((Name)"Entity([GUID])", Name.SELF_SYMBOL, subset);

            foreach (var entity in entities)
            {
                Boolean b = false;
                string strEntGuid = entity.Item2.FirstOrDefault().FirstOrDefault().SubValue.Value.ToString();
                int entGuid = int.Parse(strEntGuid);
                string entPrefab = entity.Item1.Value.ToString();
                string realEntPrefab = RealEntityPrefab(entPrefab);

                if (IsFire(entPrefab))
                {
                    string strEntPosx = "PosX(" + strEntGuid + ")";
                    var POSx = knowledgeBase.AskProperty((Name)strEntPosx);
                    int entPosx = int.Parse(POSx.Value.ToString());

                    string strEntPosz = "PosZ(" + strEntGuid + ")";
                    var POSz = knowledgeBase.AskProperty((Name)strEntPosz);
                    int entPosz = int.Parse(POSz.Value.ToString());

                    Tuple<string, int, int> tuple = new Tuple<string, int, int>(entPrefab, entPosx, entPosz);
                    this.Fire.Add(tuple);
                }
                else if (realEntPrefab != "" && DistanceCalculator(strEntGuid) > 0)
                {
                    string strEntIsCollectable = "IsCollectable(" + strEntGuid + ")";
                    var isCollectable = knowledgeBase.AskProperty((Name)strEntIsCollectable);
                    Boolean entIsCollectable = Boolean.Parse(isCollectable.Value.ToString());

                    string strEntIsPickable = "IsPickable(" + strEntGuid + ")";
                    var isPickable = knowledgeBase.AskProperty((Name)strEntIsPickable);
                    Boolean entIsPickable = Boolean.Parse(isPickable.Value.ToString());

                    string strEntIsMineable = "IsMineable(" + strEntGuid + ")";
                    var isMineable = knowledgeBase.AskProperty((Name)strEntIsMineable);
                    Boolean entIsMineable = Boolean.Parse(isMineable.Value.ToString());

                    string strEntIsChoppable = "IsChoppable(" + strEntGuid + ")";
                    var isChoppable = knowledgeBase.AskProperty((Name)strEntIsChoppable);
                    Boolean entIsChoppable = Boolean.Parse(isChoppable.Value.ToString());

                    if (entIsPickable || entIsCollectable || entIsMineable || entIsChoppable)
                    {
                        string strEntQuantity = "Quantity(" + strEntGuid + ")";
                        var quantity = knowledgeBase.AskProperty((Name)strEntQuantity);
                        int entQuantity = int.Parse(quantity.Value.ToString());

                        string strEntPosx = "PosX(" + strEntGuid + ")";
                        var POSx = knowledgeBase.AskProperty((Name)strEntPosx);
                        int entPosx = int.Parse(POSx.Value.ToString());

                        string strEntPosz = "PosZ(" + strEntGuid + ")";
                        var POSz = knowledgeBase.AskProperty((Name)strEntPosz);
                        int entPosz = int.Parse(POSz.Value.ToString());

                        foreach (ObjectProperties objectproperty in this.Entities)
                        {
                            if (objectproperty.Prefab == realEntPrefab)
                            {
                                objectproperty.Add(entQuantity, entPrefab, entGuid, entPosx, entPosz, this.Walter);
                                b = true;
                                break;
                            }
                        }
                        if (!b)
                        {
                            ObjectProperties newObjectproperty = new ObjectProperties(realEntPrefab, entPrefab, entGuid, entQuantity, entPosx, entPosz, entIsCollectable, entIsPickable, entIsMineable, entIsChoppable);
                            this.Entities.Add(newObjectproperty);
                        }
                    }
                }
            }          
        }

        public Boolean IsTree(string tree)
        {
            return (tree == "evergreen" || tree == "mushtree_tall" || tree == "mushtree_medium" ||
                tree == "mushtree_small" || tree == "mushtree_tall_webbed" || tree == "evergreen_sparse" ||
                tree == "twiggy_short" || tree == "twiggy_normal" || tree == "twiggy_tall" || tree == "twiggy_old" || 
                tree == "deciduoustree" || tree == "twiggytree");
        }

        public Boolean IsBoulder(string boulder)
        {
            return (boulder == "rock1" || boulder == "rock2" || boulder == "rock_flintless" ||
                boulder == "rock_moon" || boulder == "rock_petrified_tree_short" ||
                boulder == "rock_petrified_tree_med" || boulder == "rock_petrified_tree_tall" ||
                boulder == "rock_petrified_tree_old");
        }

        public string RealEntityPrefab(string entity)
        {
            if (IsTree(entity))
            {
                return "tree";
            }
            else if (IsBoulder(entity))
            {
                return "boulder";
            }
            else if (entity == "sapling")
            {
                return "sapling";
            }
            else if (entity == "twigs")
            {
                return "twigs";
            }
            else if (entity == "berrybush")
            {
                return "berrybush";
            }
            else if (entity == "berrybush2")
            {
                return "berrybush";
            }
            else if (entity == "berrybush_juicy")
            {
                return "berrybush";
            }
            else if (entity == "log")
            {
                return "log";
            }
            else if (entity == "torch")
            {
                return "torch";
            }
            else if (entity == "grass")
            {
                return "grass";
            }
            else if (entity == "cutgrass")
            {
                return "cutgrass";
            }
            else if (entity == "carrot")
            {
                return "carrot";
            }
            else if (entity == "carrot_planted")
            {
                return "carrot_planted";
            }
            else if (entity == "berries")
            {
                return "berries";
            }
            else if (entity == "berries_juicy")
            {
                return "berries";
            }
            else if (entity == "rocks")
            {
                return "rocks";
            }
            else if (entity == "flint")
            {
                return "flint";
            }
            else if (entity == "axe")
            {
                return "axe";
            }
            else if (entity == "pickaxe")
            {
                return "pickaxe";
            }
            else if (entity == "campfire")
            {
                return "campfire";
            }
            else if (entity == "firepit")
            {
                return "firepit";
            }
            else
            {
                return "";
            }

        }

        public Boolean IsFire(string prefab)
        {
            return prefab == "campfire" || prefab == "firepit";
        }

        public Boolean IsFuel(string guid)
        {
            string strEntFuel = "IsFuel(" + guid + ")";
            var entFuel = KnowledgeBase.AskProperty((Name)strEntFuel);
            var fuelQ = entFuel.Value.ToString();
            return fuelQ == "True";
        }

        public int GetEntitiesGUID(string prefab)
        {
            foreach (ObjectProperties entity in this.Entities)
            {
                if (entity.Prefab == prefab)
                {
                    return entity.GUID;
                }               
            }
            return 0;
        }

        public int GetEquippableGUID(string prefab)
        {
            foreach (Pair<string,int> item in this.Equipped)
            {
                if (item.Item1 == prefab)
                {
                    return item.Item2;
                }
            }
            foreach (Tuple<string, int, int> item in this.Inventory)
            {
                if (item.Item1 == prefab)
                {
                    return item.Item2;
                }
            }
            return 0;
        }

        public int GetInventoryGUID(string prefab)
        {
            foreach (Tuple<string, int, int> item in this.Inventory)
            {
                if (item.Item1 == prefab)
                {
                    return item.Item2;
                }
            }
            return 0;
        }

        public int GetEquippedGUID(string prefab)
        {
            foreach (var item in this.Equipped)
            {
                if (item.Item1 == prefab)
                {
                    return item.Item2;
                }
            }
            return 0;
        }

        public Boolean EntityIsPickable(string entity)
        {
            foreach (ObjectProperties item in this.Entities)
            {
                if (item.Prefab == entity)
                {
                    return item.IsPickable;
                }
            }
            return false;
        }

        public Boolean EntityIsCollectable(string entity)
        {
            foreach (ObjectProperties item in this.Entities)
            {
                if (item.Prefab == entity)
                {
                    return item.IsCollectable;
                }
            }
            return false;
        }

        public Boolean IsEquipped(string item)
        {
            foreach (var equip in this.Equipped)
            {
                if (equip.Item1 == item)
                {
                    return true;
                }
            }
            return false;
        }

        public string CompleteNextActionInfo(string info)
        {
            if (info == "berrybush")
            {
                foreach (var entity in this.Entities)
                {
                    if (entity.Prefab == "berrybush")
                    {
                        string realprefab = entity.RealPrefab;
                        if (realprefab == "berrybush" || realprefab == "berrybush2")
                        {
                            return "";
                        }
                        else
                        {
                            return "berries_juicy";
                        }
                    }                   
                }
                return info;
            }
            else
            {
                return info;
            }
        }

        private float DistanceCalculator(string guid)
        {
            string searchPosx = "PosX(" + guid + ")";
            var posx = this.KnowledgeBase.AskProperty((Name)searchPosx).Value.ToString();
            string searchPosz = "PosZ(" + guid + ")";
            var posz = this.KnowledgeBase.AskProperty((Name)searchPosz).Value.ToString();

            float Posx = float.Parse(posx);
            float Posz = float.Parse(posz);

            return Convert.ToSingle(Math.Pow(Convert.ToDouble(this.Walter.Position.Item1 - Posx), 2) + Math.Pow(Convert.ToDouble(this.Walter.Position.Item2 - Posz), 2));
        }
    }
}
