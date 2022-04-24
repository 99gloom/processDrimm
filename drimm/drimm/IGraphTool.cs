using System.Collections.Generic;
using Util.Collection;

namespace SyntenyFast
{
    public interface IGraphTool
    {
        /// <summary>
        /// Get a list of weak edges that is not in the maximum spanning tree
        /// </summary>
        /// <param name="multiplicityByEdges">A mapping between an edge and its multiplicity in the graph</param>
        /// <returns>list of weak edges, order from weakest edge to the strongest</returns>
        IList<Pair<int>> GetWeakEdges(IDictionary<Pair<int>,int> multiplicityByEdges);

        /// <summary>
        /// Find and remove small cycles that contains the weak edge and has length smaller than cycleLengthThreshold
        /// </summary>
        /// <param name="weakEdge">Weak edge that the cycle must contains</param>
        /// <param name="cycleLengthThreshold">maximum cycle length to re-route</param>
        /// <param name="graph">Graph structure: node value maps a list of nodes that has that value</param>
        /// <returns>A hashSet containing the color mapping edge for the remove nodes</returns>
        HashSet<KeyValuePair<int, int>> ReSolveCycle(Pair<int> weakEdge, int cycleLengthThreshold, ref IDictionary<int, IList<Node<int>>> graph);

        /// <summary>
        /// Get the simple path in the graph
        /// </summary>
        /// <param name="graphLinkStructure">A mapping between a node and its neighbors</param>
        /// <param name="multiplicityByEdge">The mapping between an edge and its multiplicity</param>
        /// <param name="multiplicityByNodeID">A mapping between an nodeID and its node multiplicity</param>
        /// <returns>A list of simple paths</returns>
        IList<IList<int>> GetSimplePath(IDictionary<int, IList<int>> graphLinkStructure, IDictionary<Pair<int>, int> multiplicityByEdge, IDictionary<int, int> multiplicityByNodeID);

        /// <summary>
        /// Smoothing operation: split the noise out of the original long synteny blocks( If exists)
        /// </summary>
        /// <param name="shortNoisePath"> the short synteny block which is suspected as noise</param>
        /// <param name="graph">mapping between a value and a list of nodes that has that value</param>
        IList<int> Smooth(IList<int> shortNoisePath, ref IDictionary<int, IList<Node<int>>> graph);
        /// <summary>
        /// We process and remove the 3 length palindrom and also update the graph structure
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="graph"></param>
        void ProcessPalindrome(ref SimpleLinkList<int> sequence,ref IDictionary<int, IList<Node<int>>> graph);

        /// <summary>
        /// Remove all the simple tandem A-A
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="graph"></param>
        void ProcessTandem(ref SimpleLinkList<int> sequence, ref IDictionary<int, IList<Node<int>>> graph);
    }
}