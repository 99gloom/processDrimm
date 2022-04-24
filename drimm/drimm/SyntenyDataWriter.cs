using System;
using System.Collections.Generic;
using System.IO;

namespace SyntenyFast
{
    public class SyntenyDataWriter:IDataWriter
    {
        private readonly char __separator;
        private readonly StreamWriter _syntenyWriter;
        private readonly StreamWriter _sequenceWriter;
        private readonly StreamReader _inputReader;
        private readonly StreamWriter _modifiedWriter;
        public SyntenyDataWriter(string syntenyFileName, char _separator, string sequenceFileName, string inputFile, string modifiedSequenceFileName)
        {
            __separator = _separator;
            _syntenyWriter = new StreamWriter(syntenyFileName);
            _sequenceWriter = new StreamWriter(sequenceFileName);
            _inputReader = new StreamReader(inputFile);
            _modifiedWriter = new StreamWriter(modifiedSequenceFileName);

        }
        /// <summary>
        /// Write the consensus synteny blocks and its multiplicity
        /// </summary>
        /// <param name="multiplicity"></param>
        /// <param name="consensusPath"></param>
        /// <returns></returns>
        public void WriteSyntenyConsensus(IList<int> multiplicity, IList<IList<int>> consensusPath)
        {
            if (multiplicity.Count != consensusPath.Count)
                throw new ArgumentException("data invalid");
            

            for (int i = 0; i < consensusPath.Count; i++)
            {
                _syntenyWriter.Write( i + ":"+  multiplicity[i] + __separator.ToString());
                foreach (int node in consensusPath[i])
                {
                    _syntenyWriter.Write(node + __separator.ToString());
                }
                _syntenyWriter.WriteLine();
            }
            _syntenyWriter.Flush();
            _syntenyWriter.Close();
            return; 

        }

        /// <summary>
        /// Write the sequence with color.
        /// </summary>
        /// <param name="sequence">sequence</param>
        /// <param name="listInColor">sequence of color</param>
        public void WriteSequenceWithColor(IList<int> sequence, IList<int> listInColor)
        {
            IList<int> sequencesLength = new List<int>();
            string line;
            while ((line = _inputReader.ReadLine())!= null)
            {
                line = line.Trim(__separator);
                string[] numbers = line.Split(__separator);
                sequencesLength.Add(numbers.Length); //TODO devide this number in to 2 ; Just temporary , since the format of the output is still old. 
            }
            int sequenceID = 0;
            int j = 0;
            //remove all padding numbers
           /* IList<int> paddingFreeSequence = new List<int>();
            for (int i = 0; i < sequence.Count; i++)
                if (sequence[i] >= -2) //begin and end of the sequence were added with -1 and -2 to get rid of the null care!!!
                    paddingFreeSequence.Add(sequence[i]);
           */
            //TODO this is the source of error. 
            for (int i = 0; i < sequence.Count ; i++)
            {
                if (sequenceID == sequencesLength.Count)
                    break; 
                if (j == sequencesLength[sequenceID])
                {
                    _sequenceWriter.WriteLine();
                    sequenceID++;
                    j = 0; 
                }
                if (sequence[i] >= 0)
                {
                    _sequenceWriter.Write(sequence[i] + __separator.ToString() + listInColor[i] + __separator);
                    j++;
                }
            }
            _sequenceWriter.Flush();
            _sequenceWriter.Close();

        }

        /// <summary>
        /// Write the modified sequence to a file
        /// </summary>
        /// <param name="sequence"></param>
        public void WriteModifiedSequence(IList<int> sequence)
        {
            bool isNewLine = false;
            sequence.RemoveAt(0);
            sequence.RemoveAt(sequence.Count-1);
            foreach (int i in sequence)
            {
                if (!isNewLine && i < 0)
                {
                    _modifiedWriter.WriteLine();
                    isNewLine = true; 
                }
                if (i < 0 && isNewLine )
                    continue;

                _modifiedWriter.Write(i+" ");
                isNewLine = false; 

            }
            _modifiedWriter.Flush();
            _modifiedWriter.Close();
        }

        public void WriteBlocksSign(IList<IList<int>> blockSign,string outdir)
        {
            StreamWriter sw = new StreamWriter(outdir+"/blocks.txt");
            foreach (IList<int> sequence in blockSign)
            {
                foreach (int i in sequence)
                {
                    sw.Write(i+" ");
                }
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
        }
    }
}