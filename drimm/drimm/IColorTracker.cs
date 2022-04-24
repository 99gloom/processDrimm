using System;
using System.Collections.Generic;
using Util.Collection;

namespace SyntenyFast
{
    public interface IColorTracker
    {
        /// <summary>
        /// This method append all color edges to the inner structure of the ColorTrackerGraph, so that
        /// we will backtracking the color of the initial sequence, given the color from the final sequence.
        /// </summary>
        /// <param name="edges">A set of edges added to graph</param>
        void AppendColorEdges(HashSet<KeyValuePair<int, int>> edges);
        /// <summary>
        /// this method will backtrack the changes and gives the color to the nodes that are still in the final sequence
        /// </summary>
        /// <returns></returns>
        IDictionary<int, int> BackTracking(IList<IList<int>> list);
    }

    class ColorTracker : IColorTracker
    {
        private IDictionary<int, HashSet<int>> _graphStructure = new Dictionary<int, HashSet<int>>();
        private IDictionary<int, HashSet<int>> _reverseGraphStructure = new Dictionary<int, HashSet<int>>();
        public void AppendColorEdges(HashSet<KeyValuePair<int, int>> edges)
        {
            foreach (KeyValuePair<int, int> edge in edges)
            {
                if (_graphStructure.ContainsKey(edge.Key))
                {
                    if (!_graphStructure[edge.Key].Contains(edge.Value))
                        _graphStructure[edge.Key].Add(edge.Value);
                }
                else
                    _graphStructure.Add(edge.Key, new HashSet<int> {edge.Value});
                if (_reverseGraphStructure.ContainsKey(edge.Value))
                {
                    if (!_reverseGraphStructure[edge.Value].Contains(edge.Key))
                        _reverseGraphStructure[edge.Value].Add(edge.Key);
                }
                else
                    _reverseGraphStructure.Add(edge.Value, new HashSet<int> {edge.Key});
            }
        }

        public IDictionary<int, int> BackTracking(IList<IList<int>> list)
        {
            IDictionary<int, int> nodeColorByNodeID = new Dictionary<int, int>();
            HashSet<int> finalSequenceNodes = new HashSet<int>();
            //Build the mapping structure.
            int blockID = 0;
            foreach (IList<int> block in list)
            {
                foreach (int geneID in block)
                {
                    nodeColorByNodeID.Add(geneID, blockID);
                    finalSequenceNodes.Add(geneID);
                }
                blockID++;
            }
            //backtracking
            //find the current level nodes ( level 0: nodes in the final sequence )
            HashSet<int> currentLevelSet = new HashSet<int>();
            foreach (HashSet<int> set in _graphStructure.Values)
                foreach (int i in set)
                    if (!_graphStructure.Keys.Contains(i) && !currentLevelSet.Contains(i) && finalSequenceNodes.Contains(i))
                        currentLevelSet.Add(i);
            HashSet<int> nodeCheck = new HashSet<int>(_graphStructure.Keys);
            while (nodeCheck.Count!= 0)
            {
                HashSet<int> nextLevelSet = new HashSet<int>();
                foreach (int targetNode in currentLevelSet)
                {
                    //find a set of source at the upper level
                    if (!_reverseGraphStructure.ContainsKey(targetNode))
                    {
                        continue;
                    }

                    foreach (int i in _reverseGraphStructure[targetNode])
                    {
                        if (!nextLevelSet.Contains(i))
                            nextLevelSet.Add(i);
                        IDominantSet<int> dominantSet = new DominantSet<int>();
                        foreach (int targetNodeSuspect in _graphStructure[i])
                            if (nodeColorByNodeID.ContainsKey(targetNodeSuspect))
                                dominantSet.Add(nodeColorByNodeID[targetNodeSuspect]);
                        int dominantColor = dominantSet.GetDominant();
                        if (!nodeColorByNodeID.ContainsKey(i))
                            nodeColorByNodeID.Add(i, dominantColor);
                    }
                }

                foreach (int i in nextLevelSet)
                    nodeCheck.Remove(i);
                currentLevelSet = nextLevelSet;

            }
            return nodeColorByNodeID;
           
        }
    }
}