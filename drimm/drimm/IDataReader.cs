using System.Collections.Generic;

namespace SyntenyFast
{
    public interface IDataReader
    {
        /// <summary>
        /// Read the sequences of genes/units in multiple chromosomes, concatenate them and as a list of integer.
        /// </summary>
        /// <returns>One list of integer as a concatenated sequence</returns>
        IList<int> ReadSequences(int paddingNumb);
    }
}
