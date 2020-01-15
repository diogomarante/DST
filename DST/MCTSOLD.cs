using System;
using MCTS.DST.Actions;
using MCTS.DST.WorldModels;
using System.Collections.Generic;

namespace MCTS.DST
{

    public class MCTSAlgorithm
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
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
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 4;
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
            MCTSNode selectedNode;
            float reward;

            Console.WriteLine("Running MCTS Search");
            //TO DO

            return new Wander();

        }

        protected MCTSNode Selection(MCTSNode nodeToDoSelection)
        {

            Action nextAction;
            MCTSNode currentNode = initialNode;
            //MCTSNode bestChild = null; //wrong

            while (!currentNode.State.IsTerminal())
            {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null)
                {
                    return Expand(currentNode, nextAction);
                }
                else
                    currentNode = BestUCTChild(currentNode); //or best UCT Child?

            }
            return currentNode;

            return new MCTSNode(new WorldModelDST());
        }

        protected MCTSNode Expand(MCTSNode parent, ActionDST action)
        {
            MCTSNode child = new MCTSNode(parent.State.GenerateChildWorldModel())
            {
                Action = action,
                Parent = parent,
                PlayerID = parent.State.GetNextPlayer(),
                Q = 0,
                N = 0
            };
            parent.ChildNodes.Add(child);
            child.Action.ApplyActionEffects(child.State);
            //LAB 7
            child.State.CalculateNextPlayer();
            //Debug.Log("Player id: " + child.PlayerID);            
            return child;

            return new MCTSNode(new WorldModelDST());
        }

        protected float Playout(WorldModelDST initialPlayoutState)
        {

            Action[] actions;
            int parentID = 0;
            CurrentDepth = 0;

            List<double> normalized;
            int i;

            WorldModel clone = initialPlayoutState.GenerateChildWorldModel();

            while (!clone.IsTerminal() && (!DepthLimited || (CurrentDepth < MaxPlayoutDepthReached)))
            {
                // Debug.Log("problem?");
                parentID = clone.GetNextPlayer(); //this parent=clone will be the parent of clone w/ action applied
                actions = clone.GetExecutableActions();
                System.Random r = new System.Random();
                double diceRoll = r.NextDouble(); //between 1 and 0
                double cumulative = 0.0;
                Action selectedAction = null;
                i = 0;

                normalized = SoftMax(actions, clone);
                foreach (var elem in normalized)
                {
                    cumulative += elem;
                    if (diceRoll < cumulative)
                    {
                        selectedAction = actions[i];
                        break;

                    }
                    i++;
                }

                selectedAction.ApplyActionEffects(clone);
                //LAB 7
                clone.CalculateNextPlayer();
                //Debug.Log("Action: " + actions[random].Name + " Player ID: " + clone.GetNextPlayer());
                //CurrentDepth += 1;
            }
            //Debug.Log("Score: " +  initialPlayoutState.GetScore());
            //Debug.Log(parentID);
            return new Reward() { Value = clone.GetScore(), PlayerID = parentID }; //Is this it? revisit pls


            return 0.0f;
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            while (node != null)
            {
                node.N++;
                var playerID = node.PlayerID;
                if (playerID == reward.PlayerID)
                    node.Q += reward.Value;

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

            MCTSNode bestChild = null;
            double uct, best = 0;

            foreach (var child in node.ChildNodes)
            {
                uct = (child.Q / child.N) + C * Math.Sqrt((Math.Log(node.N)) / (child.N));

                if (uct >= best)
                {
                    best = uct;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        protected ActionDST BestFinalAction(MCTSNode node)
        {
            float averageQ;
            float bestAverageQ = float.MinValue;
            MCTSNode bestNode = null;

            int i = 0;

            while (i < node.ChildNodes.Count)
            {
                averageQ = node.ChildNodes[i].Q / node.ChildNodes[i].N;
                if (averageQ > bestAverageQ)
                {
                    bestAverageQ = averageQ;
                    bestNode = node.ChildNodes[i];
                }
                i++;
            }
            return bestNode.Action;
        }


    }
}
