using System.Collections.Generic;

namespace SyntenyFast
{
    public interface IDataWriter
    {
        /// <summary>
        /// Write the consensus synteny blocks and its multiplicity
        /// </summary>
        /// <param name="multiplicity"></param>
        /// <param name="consensusPath"></param>
        /// <returns></returns>
        void WriteSyntenyConsensus(IList<int> multiplicity, IList<IList<int>> consensusPath);

        /// <summary>
        /// Write the sequence with color.
        /// </summary>
        /// <param name="sequence">sequence</param>
        /// <param name="listInColor">sequence of color</param>
        void WriteSequenceWithColor(IList<int> sequence, IList<int> listInColor);
        /// <summary>
        /// Write the modified sequence to a file
        /// </summary>
        /// <param name="sequence"></param>
        void WriteModifiedSequence(IList<int> sequence);

        void WriteBlocksSign(IList<IList<int>> blockSign,string outdir);
    }
}