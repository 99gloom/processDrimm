using System.Collections.Generic;

namespace SyntenyFast
{
    public interface IABruijnGraph
    {
        /// <summary>
        /// Thread a sequence of integer through an ABruijn Graph
        /// </summary>
        /// <param name="sequence">sequence wanted to thread</param>
        void ThreadSequence(IList<int> sequence);

        /// <summary>
        /// Simplify the graph by removing the cycles and smoothing techniques. and return the color graph structure 
        /// </summary>
        /// <param name="cycleLenghtThreshold">The maximum cycle length that can be re-route</param>
        /// <param name="smoothingThreshold">the maximum length of the simple path that can be splitted</param>
        /// <param name="shouldSmooth">If we should smooth the graph in this step</param>
        /// <param name="splitNodes">The list of split nodes</param>
        /// <returns> the color Edge Set</returns>
        HashSet<KeyValuePair<int, int>> Simplify(int cycleLenghtThreshold, int smoothingThreshold, bool shouldSmooth,out HashSet<int> splitNodes);

        /// <summary>
        /// Get the simple paths of the graph and their corresponding multiplicity
        /// <param name="correspondingMultiplicities"></param>
        /// <returns>return a list of simple paths</returns>
        IList<IList<int>> GetSimplePath(out IList<int> correspondingMultiplicities);
        
        /// <summary>
        /// Propagate the color of the skeleton color throught the ABruijn Graph  
        /// </summary>
        /// <param name="blockColors"> A list of Blocks. The color ID of each block is also its its position in the list </param>
        /// <param name="propagationRadius"> The maximum radius that the color can propagate </param>
        /// <returns>a mapping between nodeids and their colors</returns>
        IDictionary<int,int> PropagateSkeletonColor(IList<IList<int>> blockColors, int propagationRadius);

        /// <summary>
        /// Return the list nodes in the modified sequence
        /// </summary>
        /// <returns></returns>
        IList<int> GetModifiedSequence();
    }
}