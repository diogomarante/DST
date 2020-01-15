using System;
using Utilities;

namespace MCTS.DST.WorldModels
{

    public class Character
    {
        public int HP;
        public int Hunger;
        public int Sanity;
        public Pair<int, int> Position;

        public Character()
        {
        }

        public Character(int hp, int hunger, int sanity, int posx, int posz)
        {
            this.HP = hp;
            this.Hunger = hunger;
            this.Sanity = sanity;
            this.Position = new Pair<int, int>(posx, posz);
        }

        public int GetPosX()
        {
            return Position.Item1;
        }

        public int GetPosZ()
        {
            return Position.Item2;
        }

        public void DecreaseHunger(int n)
        {
            if (this.Hunger + n > 150)
            {
                this.Hunger = 150;
            }
            else
            {
                this.Hunger += n;
            }
        }

        public void IncreaseHunger(int n)
        {
            if (this.Hunger - n < 0)
            {
                this.Hunger = 0;
            }
            else
            {
                this.Hunger -= n;
            }
        }

        public void IncreaseHP(int n)
        {
            if (this.Hunger + n > 150)
            {
                this.Hunger = 150;
            }
            else
            {
                this.Hunger += n;
            }
        }

        public void DecreaseHP(int n)
        {
            if (this.Hunger - n < 0)
            {
                this.Hunger = 0;    
            }
            else
            {
                this.Hunger -= n;
            }
        }
    } 
}
