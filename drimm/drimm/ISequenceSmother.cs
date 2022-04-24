using System.Collections.Generic;

namespace SyntenyFast
{
    public interface ISequenceSmother
    {
        /// <summary>
        /// Smooth the color of a raw colored sequence
        /// </summary>
        /// <param name="sequence"> raw colored sequence</param>
        /// <param name="colorbyNodeID">color by NodeID of the raw colored sequence</param>
        /// <returns> a list of color ID corresponds to the sequence </returns>
        IList<int> Smooth(IList<int> sequence, IDictionary<int, int> colorbyNodeID);

        /// <summary>
        /// We remove the elements on the sequence that are repeated more than <paramref name="repeatThreshold"/>
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="repeatThreshold"></param>
        void RemoveDust(ref IList<int> sequence, int repeatThreshold);

        /// <summary>
        /// This is a pair function with RemoveDust function. The call is valid if RemoveDust was just called to the same sequence without any modification 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="colorbyNodePosition">list of color of the nodes of the sequence</param>
        IList<int>  ReStoreDust(ref IList<int> sequence, IList<int> colorbyNodePosition);


        /// <summary>
        /// Get the blocks ID from the modified sequence. The blockID will go with sign.
        /// </summary>
        /// <param name="modifiedSequence">Modified sequence</param>
        /// <param name="simplePath">simple Path as synteny blocks</param>
        /// <param name="minimumBlockLength">the minimum block Length</param>
        /// <returns></returns>
        IList<IList<int>> GetBlocksSign(IList<int> modifiedSequence, IList<IList<int>> simplePath, int minimumBlockLength);
    }
}