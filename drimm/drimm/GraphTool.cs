using System;
using System.Collections;
using System.Collections.Generic;
using Util.Collection;

namespace SyntenyFast
{
    public class GraphTool : IGraphTool
    {
        private int _maxInt = int.MaxValue;
        private const int _limitCycleProcessing = 30;

        /// <summary>
        /// Get a list of weak edges that is not in the maximum spanning tree
        /// </summary>
        /// <param name="multiplicityByEdges">A mapping between an edge and its multiplicity in the graph</param>
        /// <returns>list of weak edges, order from weakest edge to the strongest</returns>
        
        public IList<Pair<int>> GetWeakEdges(IDictionary<Pair<int>, int> multiplicityByEdges)
        {
            List<Pair<int>> edgeList = new List<Pair<int>>(multiplicityByEdges.Keys);
            //TODO this should be done with care
        //    edgeList.Sort( (a,b) =>  multiplicityByEdges[b].CompareTo(multiplicityByEdges[a]));
            edgeList = LocalSort(edgeList, multiplicityByEdges);
            IList<int> graphNodes = GenerateGraphNodesFromEdge(edgeList) ;
            Dictionary<Pair<int>, int> maximumSpanningTree = GetMaximumSpanningTree(multiplicityByEdges, edgeList, graphNodes);
            IList<Pair<int>> weakEdges = new List<Pair<int>>();
            foreach (Pair<int> edge in edgeList)
                if (!maximumSpanningTree.ContainsKey(edge))
                    weakEdges.Add(edge);
            return weakEdges; 
        }

        private static List<Pair<int>> LocalSort(List<Pair<int>> edgeList, IDictionary<Pair<int>, int> multiplicityByEdges)
        {
            List<Pair<int>> sortedList = new List<Pair<int>>();
            for (int i = 1; i < 3; i++)
            {
                for (int index = 0; index < edgeList.Count; index++)
                {
                    Pair<int> pair = edgeList[index];
                    if (multiplicityByEdges[pair] == i)
                    {
                        sortedList.Add(pair);
                        edgeList.RemoveAt(index);
                        index--;
                    }
                }
            }
            edgeList.Sort((a,b)=>multiplicityByEdges[a].CompareTo(multiplicityByEdges[b]));
            sortedList.AddRange(edgeList);
            sortedList.Reverse();
            return sortedList;
        }

        /// <summary>
        /// Make a list of nodes from a list of edge
        /// </summary>
        /// <param name="edgeList">list of edges in the graph</param>
        /// <returns>list of distinct nodes</returns>
        private static IList<int> GenerateGraphNodesFromEdge(IEnumerable<Pair<int>> edgeList)
        {
            HashSet<int> nodeSet = new HashSet<int>();
            foreach (Pair<int> edge in edgeList)
            {
                if (!nodeSet.Contains(edge.First))
                    nodeSet.Add(edge.First);
                if (!nodeSet.Contains(edge.Second))
                    nodeSet.Add(edge.Second);
            }
            return new List<int>(nodeSet);
        }


        /// <summary>
        /// Find and remove small cycles that contains the weak edge and has length smaller than cycleLengthThreshold
        /// </summary>
        /// <param name="weakEdge">Weak edge that the cycle must contains</param>
        /// <param name="cycleLengthThreshold">maximum cycle length to re-route</param>
        /// <param name="graph">Graph structure: node value maps a list of nodes that has that value</param>
        /// <returns>Suspected Weak Edge</returns>
        public HashSet<KeyValuePair<int,int>> ReSolveCycle(Pair<int> weakEdge, int cycleLengthThreshold, ref IDictionary<int, IList<Node<int>>> graph)
        {
            //TODO edge color return must be done
            HashSet<KeyValuePair<int, int>> colorEdgeSet = new HashSet<KeyValuePair<int, int>>();
            //each weakEdge can only be processed in a limit of times
            int count = 0; 
            //when there is a cycle contains this edge
            IList<int> cycle;
            bool isTandem;
            Node<int> firstNodeSequence;
            Node<int> secondNodeSequence; 
            while (  (cycle = GetSmallestCycle(weakEdge,cycleLengthThreshold,graph,out isTandem, out firstNodeSequence, out secondNodeSequence)).Count > 0 )
            {
                count++;
                if (count > _limitCycleProcessing)
                {
                    return colorEdgeSet;
                }
            //if it is a tandem cycle SolveTandem
                if (isTandem)
                {
                    return SolveTandem(cycle, firstNodeSequence, ref graph);
                 
                }
                else
                {
                    ChooseSequenceToReRoute(ref firstNodeSequence, ref secondNodeSequence, ref graph, cycle);
                    return SolveMissAlign(cycle, firstNodeSequence, secondNodeSequence, graph);
                }
                    
            }

            return colorEdgeSet;
        }
        /// <summary>
        ///  We check the weakest edge in the cycle( local weakedges are usually different from global weak edge), then if necessary, swap firstNodeSequence and secondNodeSequence
        /// If they have the same weak level, just get the one with fewer genes
        /// </summary>
        /// <param name="firstNodeSequence">The local weaker edge - This edge should be rerouted </param>
        /// <param name="secondNodeSequence">The local stronger edge</param>
        /// <param name="graph">Mapping between NodeID and List of Programmmed Nodes</param>
        /// <param name="cycle"></param>
        private static void ChooseSequenceToReRoute(ref Node<int> firstNodeSequence, ref Node<int> secondNodeSequence, ref IDictionary<int, IList<Node<int>>> graph, IList<int> cycle)
        {
            Node<int> firstEnterNode, firstExitNode, secondEnterNode, secondExitNode;
            List<Node<int>> firstSequence = FindEnterExitInfo(cycle, firstNodeSequence, out firstEnterNode,
                                                              out firstExitNode);
            List<Node<int>> secondSequence = FindEnterExitInfo(cycle, secondNodeSequence, out secondEnterNode,
                                                               out secondExitNode);
            HashSet<Pair<int>> firstSequenceEdges = new HashSet<Pair<int>>();
            HashSet<Pair<int>> secondSequenceEdges = new HashSet<Pair<int>>();
            for (int i = 0; i < firstSequence.Count - 1; i++)
            {
                Pair<int> edge = new Pair<int>(firstSequence[i].Value, firstSequence[i + 1].Value);
                firstSequenceEdges.Add(edge);
            }
            for (int i = 0; i < secondSequence.Count - 1; i++)
            {
                Pair<int> edge = new Pair<int>(secondSequence[i].Value, secondSequence[i + 1].Value);
                secondSequenceEdges.Add(edge);
            }
            HashSet<Pair<int>> firstUniqueSequenceEdges = new HashSet<Pair<int>>();
            HashSet<Pair<int>> secondUniqueSequenceEdges = new HashSet<Pair<int>>();
            foreach (Pair<int> edge in firstSequenceEdges)
                if (!secondSequenceEdges.Contains(edge))
                    firstUniqueSequenceEdges.Add(edge);
            foreach (Pair<int> edge in secondSequenceEdges)
                if (!firstSequenceEdges.Contains(edge))
                    secondUniqueSequenceEdges.Add(edge);
            //Find the smallest multiplicities 
            int smallestFirstSequence = GetSmallestMultiplicities(firstUniqueSequenceEdges, graph);

            int smallestSecondSequence = GetSmallestMultiplicities(secondUniqueSequenceEdges, graph);
            if ((smallestFirstSequence == smallestSecondSequence))
                if (firstSequenceEdges.Count <= secondSequenceEdges.Count)
                    return;
            if (smallestFirstSequence < smallestSecondSequence)
                return;

            //swap them
            Node<int> tmp = firstNodeSequence;
            firstNodeSequence = secondNodeSequence;
            secondNodeSequence = tmp;
            return;
        }

        /// <summary>
        /// Get the smallest multiplicities of the edges in the Set
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        private static int GetSmallestMultiplicities(IEnumerable<Pair<int>> edges, IDictionary<int, IList<Node<int>>> graph)
        {
            int minValue = int.MaxValue;
            foreach (Pair<int> edge in edges)
                if (graph.ContainsKey(edge.First))
                {
                    int count = 0;
                    foreach (Node<int> node in graph[edge.First])
                    {
                        if (node.Next != null && node.Next.Value == edge.Second)
                            count++;
                        if (node.Previous != null && node.Previous.Value == edge.Second)
                            count++;
                    }
                    if (count < minValue)
                    {
                        minValue = count;
                    }
                    if (count == 1)
                        return 1;
                }
            return minValue; 
        }

        private static HashSet<KeyValuePair<int,int>> SolveMissAlign(IList<int> cycle, Node<int> firstNodeSequence, Node<int> secondNodeSequence, IDictionary<int, IList<Node<int>>> graph)
        {
            HashSet<int> deletedNodes = new HashSet<int>();
            //TODO the choice of rerouting sequence might be incorrect
            Node<int> firstEnterNodeReRoute;
            Node<int> firstExitNodeReRoute;
            IList<Node<int>> routeSequenceNodes = FindEnterExitInfo(cycle, firstNodeSequence, out firstEnterNodeReRoute, out firstExitNodeReRoute);
            /*
            if (CheckSelfCycle(secondNodeSequence,cycle))
            {
                SolveTandem(cycle,secondNodeSequence, graph); //it was firstNodeSequence before and creates error infinite loop.
                return new List<Pair<int>>();
            }
             */
            Node<int> secondEnterNode;
            Node<int> secondExitNode;
            List<Node<int>> secondSequenceNodes = FindEnterExitInfo(cycle, secondNodeSequence, out secondEnterNode, out secondExitNode);
            for (int i = 1; i < routeSequenceNodes.Count -1; i++)
            {
                Node<int> node = routeSequenceNodes[i];
                graph[node.Value].Remove(node);
                if (graph[node.Value].Count == 0)
                {
                    graph.Remove(node.Value);
                    if (!deletedNodes.Contains(node.Value))
                        deletedNodes.Add(node.Value);
                }   
            }
            IList<int> alternatePath = GetAlternatePath(routeSequenceNodes, secondSequenceNodes);
            if (alternatePath!= null && alternatePath.Count != 0)
            {

                Node<int> currentNodeReRoute = firstEnterNodeReRoute;
                GetCurrentNode(graph, alternatePath, ref currentNodeReRoute);
                currentNodeReRoute.Next = firstExitNodeReRoute;
                firstExitNodeReRoute.Previous = currentNodeReRoute; 
               
            }
            else
            {
                firstEnterNodeReRoute.Next = firstExitNodeReRoute;
                firstExitNodeReRoute.Previous = firstEnterNodeReRoute;
            }

            HashSet<KeyValuePair<int, int>> edgesColor = new HashSet<KeyValuePair<int, int>>();
            HashSet<int> baseNodes = new HashSet<int>();
            if (!baseNodes.Contains(firstEnterNodeReRoute.Value))
                baseNodes.Add(firstEnterNodeReRoute.Value);
            if (!baseNodes.Contains(firstExitNodeReRoute.Value))
                baseNodes.Add(firstExitNodeReRoute.Value);
            if (alternatePath != null && alternatePath.Count != 0)
                foreach (int i in alternatePath)
                    if (!baseNodes.Contains(i))
                        baseNodes.Add(i);

            foreach (int sourceNode in deletedNodes)
                foreach (int targetNode in baseNodes)
                {
                    KeyValuePair<int, int> colorEdge = new KeyValuePair<int, int>(sourceNode, targetNode);
                    if (!edgesColor.Contains(colorEdge))
                        edgesColor.Add(colorEdge);
                }
            return edgesColor;


            /*
     


            
            Node<int> currentNode = firstEnterNodeReRoute.Previous;
            if (ShouldReverse(firstEnterNodeReRoute,firstExitNodeReRoute,secondEnterNode,secondExitNode))
            {
                secondSequenceNodes.Reverse();
            }
            GetCurrentNode(graph, secondSequenceNodes, ref currentNode);

            currentNode.Next = firstExitNodeReRoute.Next;
            Node<int> nextOffCycle = firstExitNodeReRoute.Next;
            nextOffCycle.Previous = currentNode;

            IList<Pair<int>> suspectedEdge = new List<Pair<int>>();
            if (firstEnterNodeReRoute.Value != secondEnterNode.Value)
                suspectedEdge.Add(new Pair<int>(firstEnterNodeReRoute.Previous.Value, secondEnterNode.Value));
            if (firstExitNodeReRoute.Value != secondExitNode.Value)
                suspectedEdge.Add(new Pair<int>(firstExitNodeReRoute.Value, secondExitNode.Next.Value));
            //TODO suspectedEdge is not correct in the case indicated in the paper
            return suspectedEdge;
             */
        }

        private static IList<int> GetAlternatePath(IList<Node<int>> shouldBeReRouteSequence, List<Node<int>> secondSequenceNodes)
        {
            IList<int> alternatePath = new List<int>();
            int endNode = shouldBeReRouteSequence[shouldBeReRouteSequence.Count - 1].Value;
            int beginNode = shouldBeReRouteSequence[0].Value;
            int nextNode = shouldBeReRouteSequence[1].Value;
            IList<int> secondSeq = new List<int>();
            foreach (Node<int> node in secondSequenceNodes)
                secondSeq.Add(node.Value);
            int index = secondSeq.IndexOf(beginNode);
            bool foundEnd = false;
            if ((index+1 < secondSeq.Count)&& (secondSeq[index+1]  != nextNode) )
            {
                for (int i = index+1; i < secondSeq.Count; i++)
                {
                    int value = secondSeq[i];
                    if (value == endNode){
                        foundEnd = true; 
                        break;
                        }
                    alternatePath.Add(value);
                }
                if (foundEnd)
                    return alternatePath;

                return null;
            }
            if ((index-1>= 0 ) && (secondSeq[index-1] != nextNode))
            {
                for (int i = index-1; i >=0; i--)
                {
                    int value = secondSeq[i];
                    if (value == endNode)
                    {
                        foundEnd = true;
                        break;
                    }
                    alternatePath.Add(value);
                }
                if (foundEnd)
                    return alternatePath;
                return null; 
            }
            


            return alternatePath;
        }

        /// <summary>
        /// check which direction we should follow through the cycle
        /// </summary>
        /// <param name="firstEnterNode"></param>
        /// <param name="firstExitNode"></param>
        /// <param name="secondEnterNode"></param>
        /// <param name="secondExitNode"></param>
        /// <returns></returns>
        private static bool ShouldReverse(Node<int> firstEnterNode, Node<int> firstExitNode, Node<int> secondEnterNode, Node<int> secondExitNode)
        {
            if (firstEnterNode.Value == secondExitNode.Value || firstExitNode.Value == secondEnterNode.Value)
            {
                return true;
            }
            return false; 
        }

        private static void GetCurrentNode(IDictionary<int, IList<Node<int>>> graph, IList<int> secondSequenceNodes, ref Node<int> currentNode)
        {

            foreach (int  node in secondSequenceNodes)
            {
                Node<int> newNode = new Node<int>(node);
                if (graph.ContainsKey(newNode.Value))
                    graph[newNode.Value].Add(newNode);
                else
                    graph.Add(newNode.Value, new List<Node<int>> { newNode });
                currentNode.Next = newNode;
                newNode.Previous = currentNode;
                currentNode = currentNode.Next;
            }
            /*
            foreach (Node<int> node in secondSequenceNodes)
            {
                Node<int> newNode = new Node<int>(node.Value);
                if (graph.ContainsKey(newNode.Value))
                    graph[newNode.Value].Add(newNode);
                else
                    graph.Add(newNode.Value, new List<Node<int>> {newNode});
                currentNode.Next = newNode;
                newNode.Previous = currentNode;
                currentNode = currentNode.Next; 
            }
             */
        }

        /// <summary>
        /// check if this pro-node can self create this cycle
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cycle"></param>
        /// <returns>true if it creates this cycle</returns>
        private static bool CheckSelfCycle(Node<int> node, IList<int> cycle)
        {
            HashSet<int> workCycle = new HashSet<int>(cycle);
            IList<int> sequence = new List<int>();
            Node<int> currentNode = node; 

            while (currentNode != null && workCycle.Contains(currentNode.Value))
            {
                sequence.Add(currentNode.Value);
                currentNode = currentNode.Next; 
                
            }
            currentNode = node.Previous;
            while (currentNode!= null&& workCycle.Contains(currentNode.Value))
            {
                sequence.Add(currentNode.Value);
                currentNode = currentNode.Previous; 
            }
            if (sequence.Count > workCycle.Count)
            {
                return true; 
            }
            return false; 
        }

        private static HashSet<KeyValuePair<int,int>> SolveTandem(ICollection<int> cycle, Node<int> firstNodeSequence,ref IDictionary<int,  IList<Node<int>>> graph)
        {
            //TODO Solve tandem seems to be not correct. Check for Yeast data, edge.First == 3673
            Node<int> enterNode, exitNode;
            FindEnterExitInfo(cycle, firstNodeSequence, out enterNode, out exitNode);
            if (enterNode == null || exitNode == null)
                return new HashSet<KeyValuePair<int, int>>();


            IList<int> baseColor = new List<int>{enterNode.Value};
            IList<int> deletedNodes = new List<int>();
            Node<int> pointNode = enterNode.Next;
            while (pointNode.Value != enterNode.Value)
            {
                baseColor.Add(pointNode.Value);
                pointNode = pointNode.Next;
            }
            pointNode.Previous.Next = exitNode.Next;
            exitNode.Next.Previous = pointNode.Previous;
            while (pointNode!= exitNode)
            {
                graph[pointNode.Value].Remove(pointNode);
                if (graph[pointNode.Value].Count==0)
                {
                    graph.Remove(pointNode.Value);
                    deletedNodes.Add(pointNode.Value);
                }
                pointNode = pointNode.Next;
            }
            graph[exitNode.Value].Remove(exitNode);
            if (graph[exitNode.Value].Count==0)
            {
                graph.Remove(exitNode.Value);
                deletedNodes.Add(exitNode.Value);
            }
            HashSet<KeyValuePair<int, int>> colorEdges = new HashSet<KeyValuePair<int, int>>();
            foreach (int sourceNode in deletedNodes)
                foreach (int targetNode in baseColor)
                {
                    KeyValuePair<int, int> edge = new KeyValuePair<int, int>(sourceNode, targetNode);
                    if (!colorEdges.Contains(edge))
                        colorEdges.Add(edge);
                }
            return colorEdges;
            /*
                  IList<Node<int>> nodesInCycle = FindEnterExitInfo(cycle, firstNodeSequence, out enterNode, out exitNode);
              if (enterNode == null || exitNode == null)
                return new HashSet<KeyValuePair<int, int>>();
            if (enterNode.Value == exitNode.Value)
            {
                for (int i = 1; i < nodesInCycle.Count; i++)
                {
                    graph[nodesInCycle[i].Value].Remove(nodesInCycle[i]);
                    if (graph[nodesInCycle[i].Value].Count == 0)
                        graph.Remove(nodesInCycle[i].Value);
                }
                enterNode.Next = exitNode.Next;
                exitNode.Next.Previous = enterNode; 
            }
            else
            {
                for (int i = 1; i < nodesInCycle.Count -1 ; i++)
                {
                    graph[nodesInCycle[i].Value].Remove(nodesInCycle[i]);
                    if (graph[nodesInCycle[i].Value].Count == 0)
                        graph.Remove(nodesInCycle[i].Value);
                }
                IList<int> alternatePath = FindAlternatePath(cycle, enterNode, exitNode);
                Node<int> currentNode = enterNode; 
                for (int i = 1; i < alternatePath.Count-1; i++)
                {
                    Node<int> next = new Node<int>(alternatePath[i]);
                    if (graph.ContainsKey(next.Value))
                        graph[next.Value].Add(next);
                    else
                        graph.Add(next.Value, new List<Node<int>> {next});
                    currentNode.Next = next;
                    next.Previous = currentNode;
                    currentNode = currentNode.Next; 
                }
                currentNode.Next = exitNode;
                exitNode.Previous = currentNode; 
            }



            return new HashSet<KeyValuePair<int, int>>();
             */
        }
        /// <summary>
        /// Find the alternate path in the cycles that does not contains the edge
        /// </summary>
        /// <param name="cycle">list of nodes in the cycle</param>
        /// <param name="enterNode"></param>
        /// <param name="exitNode"></param>
        /// <returns>a list of nodeIDs in the alternate path</returns>
        private static IList<int> FindAlternatePath(ICollection<int> cycle, Node<int> enterNode, Node<int> exitNode)
        {
            Node<int> currentNode = enterNode;
            IList<int> path = new List<int> {currentNode.Value};
            while (currentNode.Value != exitNode.Value)
            {
                currentNode = currentNode.Next;
                path.Add(currentNode.Value);
            }
            return path; 

        }

        /// <summary>
        /// Find the enterNode and ExitNode of a sequence through a cycle and return a list of all pro-nodes in the cycles
        /// </summary>
        /// <param name="cycle">a list of all nodes in the cycle</param>
        /// <param name="memberNode">an arbitrary pro-node in the cycle</param>
        /// <param name="enterNode"></param>
        /// <param name="exitNode"></param>
        private static List<Node<int>> FindEnterExitInfo(ICollection<int> cycle, Node<int> memberNode, out Node<int> enterNode, out Node<int> exitNode)
        {

            enterNode = null;
            exitNode = null; 
            Node<int> currentNode = memberNode; 
            while (currentNode != null)
            {
                if (currentNode.Next == null || !cycle.Contains(currentNode.Next.Value) )
                {
                    exitNode = currentNode;
                    break; 
                }
                currentNode = currentNode.Next;
            }
            currentNode = memberNode;
            while (currentNode != null)
            {
                if (currentNode.Previous == null || !cycle.Contains(currentNode.Previous.Value))
                {
                    enterNode = currentNode;
                    break; 
                }
                currentNode = currentNode.Previous; 
            }
            currentNode = enterNode;
            List<Node<int>> nodeList = new List<Node<int>>(); 
            nodeList.Add(currentNode);
            while (currentNode != exitNode && currentNode != null )
            {
                currentNode = currentNode.Next;
                nodeList.Add(currentNode);
            }
            return nodeList;

        }

        /// <summary>
        /// Find a cycle that contains this weakedge, tandemCycle should have higher priority.
        /// </summary>
        /// <param name="weakEdge"></param>
        /// <param name="cycleLengthThreshold"></param>
        /// <param name="graph">a mapping of nodeid and its Pro-Nodes</param>
        /// <param name="isTandem"></param>
        /// <param name="firstNodeSequence">If Tandem, This node is found here</param>
        /// <param name="secondNodeSequence">The second one can be null in tandem case</param>
        /// <returns>a list of nodes in the corresponding cycle</returns>
        private static IList<int> GetSmallestCycle(Pair<int> weakEdge, int cycleLengthThreshold, IDictionary<int, IList<Node<int>>> graph, out bool isTandem,out Node<int> firstNodeSequence, out Node<int> secondNodeSequence)
        {

            isTandem = false;

            //find all Pro-Nodes that comprised this pair.
            IList<Node<int>> weakNodes = GetNodes(weakEdge, graph);
            //for each node, check for tandem first, then check for missAlign. If found a cycle, return immediately
            foreach (Node<int> weakNode in weakNodes)
            {
                firstNodeSequence = weakNode;
                 //checkTandem
                List<int> forwardList;
                List<int> backwardList;
                if (weakNode.Next!= null && weakNode.Next.Value == weakEdge.Second)
                {
                    forwardList = GetNext(weakNode.Next,cycleLengthThreshold);
                    backwardList = GetPrevious(weakNode, cycleLengthThreshold);
                }
                else
                {
                    forwardList = GetNext(weakNode, cycleLengthThreshold);
                    backwardList = GetPrevious(weakNode.Previous, cycleLengthThreshold);
                }
                List<int> smallestCycle = GetCycle(forwardList, backwardList, cycleLengthThreshold);
                if (smallestCycle.Count >0)
                {
                    isTandem = true;
                    secondNodeSequence = null;
                    return smallestCycle; 
                }
                //it is not a tandemCycle, now check if it is a MissAlign Cycle.
                //find tail head 
                Node<int> tail, head;
                FindTailHead(out tail, out head, weakNode, weakEdge);
                forwardList = GetForwardList(head, cycleLengthThreshold);
                backwardList = new List<int>();
                HashSet<Node<int>> deadNodes = new HashSet<Node<int>>();
                //Any other pair of nodes that has the same edge tail->head will not be consider.
                foreach (Node<int> valueP in graph[tail.Value])
                {
                    if (valueP.Next != null && valueP.Next.Value == head.Value)
                        deadNodes.Add(valueP);
                    if (valueP.Previous != null && valueP.Previous.Value == head.Value)
                            deadNodes.Add(valueP);//was tailNode
                }
                List<List<int>> circleCollection = new List<List<int>>();
                List<Node<int>> nodeCollection = new List<Node<int>>();
                Node<int> currentNode = tail;

                for (int i = 0; i < cycleLengthThreshold - 1; i++)
                {
                    //deadNodes.AddRange(GetDeadNodes(lenghtThresHold, currentNode));
                    foreach (Node<int> node in GetDeadNodes(cycleLengthThreshold, currentNode))
                        if (!deadNodes.Contains(node))
                            deadNodes.Add(node);
                    backwardList.Add(currentNode.Value);

                    foreach (Node<int> node in graph[currentNode.Value])
                        if (!deadNodes.Contains(node))
                            for (int k = 0; k < forwardList.Count; k++)
                            {
                                int targetNodeValue = forwardList[k];
                                List<int> otherEdgeListNodes;
                                if (ContainsInSmallRadius(node, targetNodeValue, cycleLengthThreshold,
                                                          out otherEdgeListNodes))
                                {
                                    List<int> subForwardList = forwardList.GetRange(0, k);
                                    List<int> copyBackwardList = new List<int>(backwardList);

                                    copyBackwardList.AddRange(otherEdgeListNodes.GetRange(1,
                                                                                          otherEdgeListNodes.Count - 1));
                                    subForwardList.Reverse();
                                    copyBackwardList.AddRange(subForwardList);
                                    if (copyBackwardList.Count <= cycleLengthThreshold &&
                                        DoesNotContainDuplicatedElement(copyBackwardList))
                                    {
                                        circleCollection.Add(copyBackwardList);
                                        nodeCollection.Add(node);
                                    }
                                }
                            }

                    if (currentNode.Previous != null)
                    {
                        currentNode = currentNode.Previous;
                    }
                }
                smallestCycle = GetOptimalCycle(out secondNodeSequence,circleCollection,nodeCollection);

                return smallestCycle;

            }


            firstNodeSequence = null;
            secondNodeSequence = null;
            return new List<int>();

        }
        /// <summary>
        /// Get the cycle with mi
        /// </summary>
        /// <param name="secondNodeSequence"></param>
        /// <param name="circleCollection"></param>
        /// <param name="nodeCollection"></param>
        /// <returns></returns>
        private static List<int> GetOptimalCycle(out Node<int> secondNodeSequence, IList<List<int>> circleCollection, IList<Node<int>> nodeCollection)
        {
            if (circleCollection.Count ==0)
            {
                secondNodeSequence = null;
                return new List<int>();
            }
            int mincycleLenght = circleCollection[0].Count;
            int index = 0;
            for (int i = 1; i < circleCollection.Count; i++)
            {
                if (mincycleLenght > circleCollection[i].Count )
                {
                    mincycleLenght = circleCollection[i].Count;
                    index = i;
                }
            }
            secondNodeSequence = nodeCollection[index];
            return circleCollection[index];
        }


        private static bool DoesNotContainDuplicatedElement(IEnumerable<int> list)
        {
            Dictionary<int, bool> dic = new Dictionary<int, bool>();
            foreach (int i in list)
            {
                if (!dic.ContainsKey(i))
                    dic.Add(i, true);
                else
                    return false;
            }
            return true;
        }


        private static bool ContainsInSmallRadius(Node<int> node, int targetNodeValue, int radius,
                                          out List<int> otherEdgeListNodes)
        {
            otherEdgeListNodes = new List<int>();
            Node<int> currentNode = node;
            for (int i = 0; i < radius; i++)
            {
                otherEdgeListNodes.Add(currentNode.Value);
                if (currentNode.Value == targetNodeValue)
                    return true;
                if (currentNode.Next != null)
                    currentNode = currentNode.Next;
                else
                    break;
            }
            otherEdgeListNodes = new List<int>();
            currentNode = node;
            for (int i = 0; i < radius; i++)
            {
                otherEdgeListNodes.Add(currentNode.Value);
                if (currentNode.Value == targetNodeValue)
                    return true;
                if (currentNode.Previous != null)
                    currentNode = currentNode.Previous;
                else
                    break;
            }
            return false;
        }


        private static IEnumerable GetDeadNodes(int lenghtThresHold, Node<int> node)
        {
            HashSet<Node<int>> deadNodes = new HashSet<Node<int>>();
            Node<int> pointerNode = node;
            for (int i = 0; i < lenghtThresHold - 1; i++)
            {
                if (pointerNode != null && !deadNodes.Contains(pointerNode))
                {
                    deadNodes.Add(pointerNode);
                }
                if (pointerNode != null && pointerNode.Previous != null) pointerNode = pointerNode.Previous;
                else
                    break;
            }
            pointerNode = node;
            for (int i = 0; i < lenghtThresHold - 1; i++)
            {
                if (pointerNode != null && !deadNodes.Contains(pointerNode))
                    deadNodes.Add(pointerNode);
                if (pointerNode != null && pointerNode.Next != null)
                    pointerNode = pointerNode.Next;
                else
                    break;
            }


            return deadNodes;
        }

        /// <summary>
        /// Get a list of forward nodes without repeat
        /// </summary>
        /// <param name="head">start node</param>
        /// <param name="threshold">distance can go</param>
        /// <returns></returns>
        private static List<int> GetForwardList(Node<int> head, int threshold)
        {
            List<int> forwardList = new List<int>();
            Node<int> currentNode = head; 
            for (int i = 0; i < threshold - 1; i++)
            {
                if (currentNode != null)
                {
                    if (forwardList.Contains(currentNode.Value))
                        break;
                    forwardList.Add(currentNode.Value);

                }
                if (currentNode != null && currentNode.Next != null)
                    currentNode = currentNode.Next;
                else
                    break;
            }
            return forwardList;
        }

        private static void FindTailHead(out Node<int> tail, out Node<int> head, Node<int> node, Pair<int> directedEdge)
        {
            tail = null;
            head = null; 
            if (node.Next!= null && node.Next.Value == directedEdge.Second)
            {
                tail = node;
                head = node.Next; 
            }
            if (node.Previous != null && node.Previous.Value == directedEdge.Second)
            {
                tail = node.Previous;
                head = node; 
            }
            return; 
        }

        /// <summary>
        /// Get the shortest cycle that contains the edge (forwardList[0]  backwardList[0])
        /// </summary>
        /// <param name="forwardList"></param>
        /// <param name="backwardList"></param>
        /// <param name="cycleLengthThreshold"></param>
        /// <returns>the smallest cycle</returns>
        private static List<int> GetCycle(List<int> forwardList, List<int> backwardList, int cycleLengthThreshold)
        {

            List<Pair<int>> cyclePositions = new List<Pair<int>>();
            for (int i = 0; i < backwardList.Count; i++)
            {
                for (int j = 0; j < forwardList.Count; j++)
                {
                    if (backwardList[i] == forwardList[j])
                    {
                        cyclePositions.Add(new Pair<int>(i, j));
                        break;
                    }
                }
            }
            cyclePositions.Sort((a,b) => (a.First+a.Second).CompareTo(b.First+b.Second));
            List<int> shortestCycles = new List<int>();
            if (cyclePositions.Count >0)
            {
                Pair<int> position = cyclePositions[0];
                shortestCycles.AddRange(backwardList.GetRange(0,position.First+1));
                shortestCycles.Reverse();
                shortestCycles.AddRange(forwardList.GetRange(0, position.Second + 1));

            }
            HashSet<int> hashCycle = new HashSet<int>(shortestCycles);
            if (hashCycle.Count <= cycleLengthThreshold)
            {
                return new List<int>(hashCycle);
            }
            return new List<int>();
        }


        private static List<int> GetNext(Node<int> node, int threshold)
        {
            List<int> forwardList = new List<int>();
            Node<int> currentNode = node;
            int step = 0; 
            while (currentNode!= null && step <= threshold )
            {
                forwardList.Add(currentNode.Value);
                currentNode = currentNode.Next;
                step++; 
            }
            return forwardList; 
        }
        private static List<int> GetPrevious(Node<int> node, int threshold)
        {
            List<int> backwardList = new List<int>();
            Node<int> currentNode = node;
            int step = 0;
            while (currentNode != null && step <= threshold)
            {
                backwardList.Add(currentNode.Value);
                currentNode = currentNode.Previous;
                step++;
            }
            return backwardList;
        }

        /// <summary>
        /// Get a list of nodes correspond to an weak edge.First. ( if mul =k , there should be k nodes)
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="graph">a mapping between a nodeID and its Pro-Node</param>
        /// <returns></returns>
        private static IList<Node<int>> GetNodes(Pair<int> edge, IDictionary<int, IList<Node<int>>> graph)
        {
            IList<Node<int>> selectedNodes = new List<Node<int>>();
            if (!graph.ContainsKey(edge.First) || !graph.ContainsKey(edge.Second))
                return selectedNodes;
            foreach (Node<int> node in graph[edge.First])
            {
                if (node.Previous != null && node.Previous.Value == edge.Second && !selectedNodes.Contains(node) )
                    selectedNodes.Add(node);
                if (node.Next != null && node.Next.Value == edge.Second && !selectedNodes.Contains(node))
                    selectedNodes.Add(node);
            }
            return selectedNodes; 
           
        }

        /// <summary>
        /// Get the simple path in the graph
        /// </summary>
        /// <param name="graphLinkStructure">A mapping between a node and its neighbors</param>
        /// <param name="multiplicityByEdge">The mapping between an edge and its multiplicity</param>
        /// <param name="multiplicityByNodeID"></param>
        /// <returns>A list of simple paths</returns>
        public IList<IList<int>> GetSimplePath(IDictionary<int, IList<int>> graphLinkStructure, IDictionary<Pair<int>, int> multiplicityByEdge, IDictionary<int, int> multiplicityByNodeID)
        {

            IList<IList<int>> simplePaths = new List<IList<int>>();
            IList<int> nodeCollection = new List<int>(multiplicityByNodeID.Keys);
            HashSet<int> nodeSet = new HashSet<int>(nodeCollection);
            while (nodeCollection.Count!= 0)
            {
                if (!nodeSet.Contains(nodeCollection[0]))
                {
                    nodeCollection.RemoveAt(0);
                    continue; 
                }
                int startNode = nodeCollection[0];
                nodeCollection.RemoveAt(0);
                nodeSet.Remove(startNode);
                List<int> oneDirection = GetNodesTowardDirection(startNode, graphLinkStructure, multiplicityByEdge,
                                                                  multiplicityByNodeID, new List<int>(), ref nodeSet);
                List<int> otherDirection = GetNodesTowardDirection(startNode, graphLinkStructure, multiplicityByEdge,
                                                                    multiplicityByNodeID, oneDirection, ref nodeSet);
                oneDirection.Reverse();
                otherDirection.RemoveAt(0);
                oneDirection.AddRange(otherDirection);
                simplePaths.Add(oneDirection);

            }
            return simplePaths;
        }

        /// <summary>
        /// Get a list of nodes start from a startNode and move toward a smooth (without horns ) branch in a direction that is not forbidden.
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="graphLinkStructure"> mapping between a node and its neighbors </param>
        /// <param name="multiplicityByEdge"> mapping between an edge and its multiplicity </param>
        /// <param name="multiplicityByNodeID">mapping between a node and its multiplicity </param>
        /// <param name="forbiddenPath">a list of nodes that create the forbidden direction</param>
        /// <returns>a list of nodes in the simple path in one direction</returns>
        private static List<int> GetNodesTowardDirection(int startNode, IDictionary<int, IList<int>> graphLinkStructure, IDictionary<Pair<int>, int> multiplicityByEdge, IDictionary<int, int> multiplicityByNodeID, IEnumerable<int> forbiddenPath, ref HashSet<int> availableNode)
        {
            HashSet<int> workForbiddenPath = new HashSet<int>(forbiddenPath);
            workForbiddenPath.Add(startNode);
            List<int> path = new List<int>{startNode};

            int multiplicity = multiplicityByNodeID[startNode];
            bool stillCanGo = true ;
            int currentNode = startNode; 
            while(stillCanGo )
            {

                stillCanGo = false; 
                foreach (int neighbor in graphLinkStructure[currentNode])
                {
                    if (multiplicityByNodeID[neighbor] == multiplicity && multiplicityByEdge[new Pair<int>(currentNode,neighbor)] == multiplicity && 
                        !workForbiddenPath.Contains(neighbor) && availableNode.Contains(neighbor))
                    {
                        availableNode.Remove(neighbor);
                        workForbiddenPath.Add(neighbor);
                        path.Add(neighbor);
                        currentNode = neighbor; 
                        stillCanGo = true;
                        break; 
                    }
                }
            }
            return path; 
        }

        /// <summary>
        /// Smoothing operation: split the noise out of the original long synteny blocks( If exists)
        /// </summary>
        /// <param name="shortNoisePath"> the short synteny block which is suspected as noise</param>
        /// <param name="graph">mapping between a value and a list of nodes that has that value</param>
        public IList<int> Smooth(IList<int> shortNoisePath, ref IDictionary<int, IList<Node<int>>> graph)
        {

            IList<int> noiseSplitNodes = new List<int>();
            IList<IList<Node<int>>> block = SplitBlock(shortNoisePath, graph);
            if (block != null && block.Count >= 1)
                foreach (int i in shortNoisePath)
                    noiseSplitNodes.Add(i);

            int blockCounter = 0;
            IDictionary<int, int> newIDByOldID = new Dictionary<int, int>();
            for (int i = 0; i < shortNoisePath.Count; i++)
            {
                newIDByOldID.Add(shortNoisePath[i], _maxInt - i);
            }
            _maxInt = _maxInt - shortNoisePath.Count - 1;
           
            for(int i = 0 ; i < block.Count ; i++)
            {
                IList<Node<int>> cluster = block[i];

                _maxInt = _maxInt - cluster.Count - 1; 
                foreach (Node<int> node in cluster)
                    {
                        //change current
                        ChangeNodeID(newIDByOldID[node.Value] - blockCounter*(shortNoisePath.Count + 1), graph, node);
                        //probagate previous
                        Node<int> currentNode = node; 
                        for(int j = 0 ; j < shortNoisePath.Count ; j++)
                        {
                            Node<int> previous = currentNode.Previous;
                            if (previous == null || !shortNoisePath.Contains(previous.Value))
                                break;
                            ChangeNodeID(newIDByOldID[previous.Value] - blockCounter*(shortNoisePath.Count+1),graph,previous );
                            currentNode = previous; 
                        }
                        //probagate next
                        currentNode = node; 
                        for (int j = 0; j < shortNoisePath.Count; j++)
                        {
                            Node<int> next = currentNode.Next;
                            if (next == null || !shortNoisePath.Contains(next.Value))
                                break;
                            ChangeNodeID(newIDByOldID[next.Value] - blockCounter*(shortNoisePath.Count + 1), graph, next);
                            currentNode = next; 
                        }
                    }
                    blockCounter++;
            }
            return noiseSplitNodes;

        }

        public void ProcessPalindrome(ref SimpleLinkList<int> sequence, ref IDictionary<int, IList<Node<int>>> graph)
        {
            IList<Node<int>> members = sequence.GetMembers();
            Node<int> currentNode = members[0];
            while (currentNode != null)
            {
                if (currentNode.Previous != null && currentNode.Next != null && currentNode.Previous.Value == currentNode.Next.Value)
                {
                    Node<int> tmpNode = currentNode.Next;
                    currentNode.Next = currentNode.Next.Next;
                    if (currentNode.Next != null)
                        currentNode.Next.Previous = currentNode;

                    int index = graph[tmpNode.Value].IndexOf(tmpNode);
                    graph[tmpNode.Value].RemoveAt(index);
                    if (graph[tmpNode.Value].Count == 0)
                        graph.Remove(tmpNode.Value);

                }

                currentNode = currentNode.Next;
            }
        }

        public void ProcessTandem(ref SimpleLinkList<int> sequence, ref IDictionary<int, IList<Node<int>>> graph)
        {
            IList<Node<int>> members = sequence.GetMembers();
            Node<int> currentNode = members[0];
            while (currentNode!=null)
            {
                bool found = false; 
                if (currentNode.Next!=null && currentNode.Value == currentNode.Next.Value)
                {
                    Node<int> tmpNode = currentNode.Next;
                    currentNode.Next = currentNode.Next.Next;
                    if (currentNode.Next != null)
                        currentNode.Next.Previous = currentNode;
                    int index = graph[tmpNode.Value].IndexOf(tmpNode);
                    graph[tmpNode.Value].RemoveAt(index);
                    if (graph[tmpNode.Value].Count == 0)
                        graph.Remove(tmpNode.Value);
                    found = true; 
                }
                if (!found)
                    currentNode = currentNode.Next;
            }
        }

        private static void ChangeNodeID(int newNodeID, IDictionary<int, IList<Node<int>>> graph, Node<int> node)
        {
            int index = graph[node.Value].IndexOf(node);
            graph[node.Value].RemoveAt(index);
            if (graph[node.Value].Count == 0)
                graph.Remove(node.Value);
            node.Value = newNodeID;
            if (graph.ContainsKey(newNodeID))
                graph[newNodeID].Add(node);
            else
                graph.Add(newNodeID, new List<Node<int>> {node});

        }
        /// <summary>
        /// splitting algorithm as follow:
        /// given a short synteny blocks, the problem is how to split these blocks in to multiple sub-blocks so that
        /// this shorter syntenic blocks disappear. The algorithm first find the blocks with 2-ends continuous and keep the block
        /// with largest multiplicity. Then, grouping other blocks where each path in one block are connected by one way with other 
        /// path in the blocks. This inference is recursively defined
        /// </summary>
        /// <param name="synList"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        private static IList<IList<Node<int>>> SplitBlock(IList<int> synList, IDictionary<int, IList<Node<int>>> graph)
        {
            //Check the last 
            IList<Node<int>> leftNodes = graph[synList[0]];
            HashSet<Node<int>> testSet = new HashSet<Node<int>>(leftNodes);
            IDictionary< Pair<int>, List<Node<int>>> listNodesByEndingPoints = new Dictionary< Pair<int>,List<Node<int>>>();
            List<IList<Node<int>>> cluster = new List<IList<Node<int>>>();


            foreach (Node<int> node in leftNodes)
            {
                int[] endings = FindEnding(node, synList);
                if (endings.Length != 2)
                    continue;
                Pair<int> pair = new Pair<int>(endings[0],endings[1]);
               
                if (listNodesByEndingPoints.ContainsKey(pair))
                    listNodesByEndingPoints[pair].Add(node);
                else
                    listNodesByEndingPoints.Add(pair, new List<Node<int>>{node});
            }


            HashSet<Pair<int>> independentEnds = new HashSet<Pair<int>>();

            foreach (Pair<int> pair in listNodesByEndingPoints.Keys)
            {
                bool anyCommon = false;
                foreach (Pair<int> key in listNodesByEndingPoints.Keys)
                {
                    if (pair != key && (pair.First == key.First || pair.First == key.Second || pair.Second == key.First || pair.Second == key.Second ))
                    {
                        anyCommon = true;
                        break; 
                    }
                    
                }
                if (!anyCommon)
                {
                    cluster.Add(new List<Node<int>>(listNodesByEndingPoints[pair]));
                    independentEnds.Add(pair);
                }
            }
            /*
            #region secondarySpliting
            IDictionary<Pair<int>, HashSet<Pair<int>>> adjacencyListByKey = new Dictionary<Pair<int>, HashSet<Pair<int>>>();

            HashSet<Pair<int>> workingSet = new HashSet<Pair<int>>();
            foreach (Pair<int> key in listNodesByEndingPoints.Keys)
            {
                if (!independentEnds.Contains(key))
                {
                    workingSet.Add(key);
                    adjacencyListByKey.Add(key, new HashSet<Pair<int>>());
                }
            }
            foreach (Pair<int> keyEnds in workingSet)
                foreach (Pair<int> pair in workingSet)
                    if ((pair != keyEnds) &&
                        ((pair.First == keyEnds.First || pair.First == keyEnds.Second || pair.Second == keyEnds.First ||
                          pair.Second == keyEnds.Second)))
                        adjacencyListByKey[keyEnds].Add(pair);

            HashSet<Pair<int>> remainSet = new HashSet<Pair<int>>(workingSet);
            List<IList<Node<int>>> secondCluster = new List<IList<Node<int>>>();
            while (remainSet.Count!= 0)
            {
                foreach (Pair<int> key in workingSet)
                {
                    if (remainSet.Contains(key))
                    {
                        HashSet<Pair<int>> listNodes = new HashSet<Pair<int>>();
                        listNodes.Add(key);
                        remainSet.Remove(key);
                        foreach (Pair<int> set in adjacencyListByKey[key])
                        {
                            listNodes.Add(set);
                            remainSet.Remove(set);
                            foreach ( Pair<int> secondOrderKey in adjacencyListByKey[set])
                            {
                                if (remainSet.Contains(secondOrderKey))
                                {
                                    listNodes.Add(secondOrderKey);
                                    remainSet.Remove(secondOrderKey);
                                }

                            }
                        }
                        IList<Node<int>> listSecondNodes = new List<Node<int>>();
                        foreach (Pair<int> set in listNodes)
                        {
                            foreach (Node<int> node in listNodesByEndingPoints[set])
                                listSecondNodes.Add(node);
                        }
                        secondCluster.Add(listSecondNodes);
                    }

                }
            }

            #endregion 
             */
            cluster.Sort((b, a) => a.Count.CompareTo(b.Count));
            
            int nodeCount = 0;
            foreach (IList<Node<int>> list in cluster)
            {
                nodeCount += list.Count; 
            }
             
            if (nodeCount == leftNodes.Count && cluster.Count>=1)
                cluster.RemoveAt(0);
            
             /* 
            foreach (IList<Node<int>> list in secondCluster)
                cluster.Add(list);
            */
            
            return cluster;
        }
        private static int[] FindEnding(Node<int> node, ICollection<int> list)
        {
            IList<int> endings = new List<int>();
            Node<int> currentNode = node;
            while (currentNode!= null && list.Contains(currentNode.Value))
            {
                currentNode = currentNode.Previous; 
            }
            if (currentNode != null)
                endings.Add(currentNode.Value);
            currentNode = node;
            while(currentNode!= null && list.Contains(currentNode.Value))
            {
                currentNode = currentNode.Next;
            }
            if(currentNode!= null)
                endings.Add(currentNode.Value);
            int[] result = new int[endings.Count];
            for (int i = 0; i < endings.Count; i++)
                result[i] = endings[i];
            return result;

        }


        /// <summary>
        /// Get a maximum spanning tree of the graph
        /// </summary>
        /// <param name="graph"> mapping between an edge and its multiplicity </param>
        /// <param name="edgeList">list of edges in descending multiplicity</param>
        /// <param name="graphNodes">all nodes of the graph</param>
        /// <returns></returns>
        private static Dictionary<Pair<int>, int> GetMaximumSpanningTree(IDictionary<Pair<int>, int> graph,
                                                                         IEnumerable<Pair<int>> edgeList,
                                                                         IList<int> graphNodes)
        {
            Dictionary<Pair<int>, int> mstMultiplicityByEdge = new Dictionary<Pair<int>, int>();
            IDictionary<int, int> partitionIDByNodeID = new Dictionary<int, int>();
            IDictionary<int, List<int>> partitionByPartitionID = new Dictionary<int, List<int>>();
            for (int i = 0; i < graphNodes.Count; i++)
            {
                partitionIDByNodeID[graphNodes[i]] = i;
                partitionByPartitionID[i] = new List<int> { graphNodes[i] };
            }
            foreach (Pair<int> pair in edgeList)
            {
                int partitionIDForB = partitionIDByNodeID[pair.Second];
                int partitionIDForA = partitionIDByNodeID[pair.First];
                if (partitionIDForA != partitionIDForB)
                {
                    mstMultiplicityByEdge.Add(pair, graph[pair]);
                    partitionByPartitionID[partitionIDForA].AddRange(partitionByPartitionID[partitionIDForB]);
                    foreach (int nodeID in partitionByPartitionID[partitionIDForB])
                        partitionIDByNodeID[nodeID] = partitionIDForA;

                    partitionByPartitionID.Remove(partitionIDForB);
                }
                if (partitionByPartitionID.Count == 1)
                    break;
            }

            return mstMultiplicityByEdge;
        }
    }
}