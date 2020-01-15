using System;
using System.Collections.Generic;
using KnowledgeBase;
using System.Linq;
using Utilities;
using WellFormedNames;
using System.Text;

namespace MCTS.DST.Actions
{
    class NextAction
    {
        public string Prefab;
        public KB KnowledgeBase;
        public Pair<float, float> Position;

        public NextAction(string prefab, KB kb)
        {
            this.Prefab = prefab;
            this.KnowledgeBase = kb;

            var posx = this.KnowledgeBase.AskProperty((Name)"PosX(Walter)");
            var PosX = float.Parse(posx.Value.ToString());

            var posz = this.KnowledgeBase.AskProperty((Name)"PosZ(Walter)");
            var PosZ = float.Parse(posz.Value.ToString());

            this.Position = new Pair<float, float>(PosX, PosZ);
        }

        public Pair<string, string> ConstructNextAction()
        {
            string itemToGrab = "0";
            Console.WriteLine("Construct Next Action");
            var subset = new List<SubstitutionSet> { new SubstitutionSet() };
            var entities = this.KnowledgeBase.AskPossibleProperties((Name)"Entity([GUID])", Name.SELF_SYMBOL, subset);
            float mindistance = float.MaxValue;

            foreach (var entity in entities)
            {
                string entPrefab = entity.Item1.Value.ToString();
                string strEntGuid = entity.Item2.FirstOrDefault().FirstOrDefault().SubValue.Value.ToString();

                float dist = DistanceCalculator(strEntGuid);

                if (entPrefab == this.Prefab && dist < mindistance && dist > 0)
                {
                    itemToGrab = strEntGuid;
                    mindistance = dist;
                }
            }

            Pair<string, string> pair = new Pair<string, string>("Action(Wander, -, -, -, -)", itemToGrab);
            return pair;
           
        }

        private float DistanceCalculator(string guid)
        {
            string searchPosx = "PosX(" + guid + ")";
            var posx = this.KnowledgeBase.AskProperty((Name)searchPosx).Value.ToString();
            string searchPosz = "PosZ(" + guid + ")";
            var posz = this.KnowledgeBase.AskProperty((Name)searchPosz).Value.ToString();

            float Posx = float.Parse(posx);
            float Posz = float.Parse(posz);

            return Convert.ToSingle(Math.Pow(Convert.ToDouble(this.Position.Item1 - Posx),2) + Math.Pow(Convert.ToDouble(this.Position.Item2 - Posz),2));
        }

        private Pair<string, float> MaxPairFromList(List<Pair<string, float>> list)
        {
            float max = float.MinValue;
            Pair<string, float> pair = null;

            foreach (var p in list)
            {
                if (p.Item2 > max)
                {
                    max = p.Item2;
                    pair = p;
                }
            }
            return pair;
        }

    }
}
