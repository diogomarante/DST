using System;
using Utilities;
using MCTS.DST.WorldModels;

namespace MCTS.DST{

    public class ObjectProperties
    {
        public string Prefab;
        public string RealPrefab;
        public int GUID;
        public int Quantity;
        public Pair<int, int> Position;
        public Boolean IsCollectable;
        public Boolean IsPickable;
        public Boolean IsMineable;
        public Boolean IsChoppable;

        public ObjectProperties()
        {
            this.GUID = 0;
        }

        public ObjectProperties(string prefab, string realprefab, int guid, int quantity, int posx, int posz, Boolean isCollectable, Boolean isPickable, Boolean isMineable, Boolean isChoppable)
        {
            this.Prefab = prefab;
            this.RealPrefab = realprefab;
            this.GUID = guid;
            this.Quantity = quantity;
            this.Position = new Pair<int, int>(posx, posz);
            this.IsCollectable = isCollectable;
            this.IsPickable = isPickable;
            this.IsMineable = isMineable;
            this.IsChoppable = isChoppable;
        }

        public void Add(int n, string realprefab, int guid, int posx, int posz, Character character)
        {
            if (this.Quantity == 0)
            {
                this.RealPrefab = realprefab;
                this.GUID = guid;
                this.Position = new Pair<int, int>(posx,posz);
            }
            else if (IsCloser(posx, posz, character.Position.Item1, character.Position.Item2))
            {
                this.GUID = guid;
                this.Position.Item1 = posx;
                this.Position.Item2 = posz;
            }
            this.Quantity += n;           
        }

        protected Boolean IsCloser(int posxObject, int poszObject, int posxWalter, int poszWalter)
        {
            return (Math.Pow(Convert.ToDouble(posxWalter - posxObject),2) + Math.Pow(Convert.ToDouble(poszWalter - poszObject),2)) < (Math.Pow(Convert.ToDouble(posxWalter - this.Position.Item1),2) + Math.Pow(Convert.ToDouble(poszWalter - this.Position.Item2),2));
        }
    }   
}