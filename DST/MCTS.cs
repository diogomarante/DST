using System;
using System.Collections.Generic;
using MCTS.DST.Actions;
using MCTS.DST.WorldModels;

namespace MCTS.DST
{

    public class MCTSAlgorithm
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }

        private bool SHOULDRUN = true;

        public int MaxIterations { get; set; }
        public int MaxPlayouts { get; set; } //
        public int CurrentPlayout { get; set; } // 
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public MCTSNode BestFirstChild { get; set; }

        protected int CurrentIterations { get; set; }
        protected int CurrentDepth { get; set; }

        protected WorldModelDST CurrentState { get; set; }
        public MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }

        public MCTSAlgorithm(WorldModelDST currentState)
        {
            this.InProgress = false;
            this.CurrentState = currentState;
            this.MaxIterations = 100;
            this.MaxPlayouts = 5; //
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 2;
            this.MaxSelectionDepthReached = 2;
            this.CurrentIterations = 0;
            this.InitialNode = new MCTSNode(this.CurrentState)
            {
                Action = null,
                Parent = null,
            };
            this.InProgress = true;
            this.BestFirstChild = null;
        }

        public ActionDST Run()
        {
            if (!this.SHOULDRUN)
            {
                return new Wander();
            }
            MCTSNode selectedNode; 
            float reward;

            Console.WriteLine("Running MCTS Search");
            while ( this.CurrentIterations<this.MaxIterations ) {

                //Console.WriteLine("Iteration: " + this.CurrentIterations);

                selectedNode = this.Selection(this.InitialNode);
                reward = Playouts(selectedNode.State);
                Backpropagate(selectedNode, reward);
                this.CurrentIterations++;
            }
            this.InProgress = false;
            return BestFinalAction(this.InitialNode);
        }

        protected MCTSNode Selection(MCTSNode nodeToDoSelection)
        {
            //Console.WriteLine("Selection...");
            ActionDST nextAction;
            MCTSNode currentNode = nodeToDoSelection;

            CurrentDepth = 0;

            while(CurrentDepth < MaxSelectionDepthReached) {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null)
                {
                    //Console.WriteLine("Testes action: " + nextAction);

                    return Expand(currentNode, nextAction);
                }
                else
                {
                    currentNode = this.BestUCTChild(currentNode);
                    // Console.WriteLine("null action found");
                }

                CurrentDepth++;
            }

            return currentNode;
        }

        protected MCTSNode Expand(MCTSNode parent, ActionDST action)
        {
            //Console.WriteLine("Expand...");

            MCTSNode child = new MCTSNode(parent.State.GenerateChildWorldModel())
            {
                Action = action,
                Parent = parent,
                Q = 0,
                N = 0
            };
            parent.ChildNodes.Add(child);
            child.Action.ApplyActionEffects(child.State);
            return child;
        }

        protected float Playouts(WorldModelDST initialPlayoutState) {
            //Console.WriteLine("Playouts...");

            List<float> rewards = new List<float>(this.MaxPlayouts);
            float total = 0;
            for (int CurrentPlayout = 0;
                 CurrentPlayout < MaxPlayouts;
                 CurrentPlayout++) 
            {
                //Console.WriteLine("Playout n." + CurrentPlayout.ToString());

                float reward = Playout(initialPlayoutState);
                rewards.Add(reward);
                //Console.WriteLine("Reward: " + reward);
                total += reward;
                
            }
            float ratio = total/MaxPlayouts;
            /* OLD
            if (ratio > 0.70f) 
                return 1f;
            else if ( ratio == 0) 
                return 0f;
            else
                return ratio * 0.4f;
                */
            //Console.WriteLine("ratio: " + ratio);
            //Console.WriteLine("init score: " + initialPlayoutState.Score(initialPlayoutState));
            if (ratio > initialPlayoutState.Score(initialPlayoutState))
           
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        protected float Playout(WorldModelDST initialPlayoutState)
        {
            List<ActionDST> actions;

            WorldModelDST clone = initialPlayoutState.GenerateChildWorldModel();
            CurrentDepth = 0;
            //int parentID = 0; // ?
            int size = 0, random = 0;

            while (CurrentDepth < MaxPlayoutDepthReached)
            {
                actions = clone.GetExecutableActions();
                foreach(var a in actions)
                {
                   // Console.WriteLine(a);

                }

                size = actions.Count;
                //Console.WriteLine("size:" + size);

                random = this.RandomGenerator.Next(0, size-1);
                actions[random].ApplyActionEffects(clone);
             
                //Had clone.getNextPlayer here in proj algorithm
                CurrentDepth++;
            }
            //return score
            return initialPlayoutState.Score(clone);
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            while ( node != null )
            {
                node.N++;
                node.Q += reward;
                node = node.Parent;
            }
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            float UCTValue;
            float bestUCT = float.MinValue;
            MCTSNode bestNode = null;

            int i = 0;

            while (i < node.ChildNodes.Count)
            {
                UCTValue = (float)((node.ChildNodes[i].Q / node.ChildNodes[i].N) + 1.4f * Math.Sqrt(Math.Log(node.N) / node.ChildNodes[i].N));
                if (UCTValue > bestUCT)
                {
                    bestUCT = UCTValue;
                    bestNode = node.ChildNodes[i];
                }
                i++;
            }
            return bestNode;
        }

        protected ActionDST BestFinalAction(MCTSNode node)
        {
            float averageQ;
            float bestAverageQ = float.MinValue;
            MCTSNode bestNode = null;

            int i = 0;
            //Console.WriteLine("Evaulated actions: " + node.ChildNodes.Count);

            while (i < node.ChildNodes.Count)
            {
                //Console.WriteLine("Evaulated action: " + node.ChildNodes[i].Action);
                averageQ = node.ChildNodes[i].Q / node.ChildNodes[i].N;
                //Console.WriteLine("Q and N: " + node.ChildNodes[i].Q + " " + node.ChildNodes[i].N);

                if (averageQ > bestAverageQ)
                {
                    bestAverageQ = averageQ;
                    bestNode = node.ChildNodes[i];
                }
                i++;
                //Console.WriteLine("Value: " + averageQ);

            }
            return bestNode.Action;
        }


    }
}