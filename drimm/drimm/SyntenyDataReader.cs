using System;
using System.Collections.Generic;
using System.IO;

namespace SyntenyFast
{
    public class SyntenyDataReader :IDataReader
    {
        private readonly char _separator;
        private readonly StreamReader _reader;

        public SyntenyDataReader(string fileName,char separator)
        {
            _separator = separator;
            _reader = new StreamReader(fileName);
        }
        /// <summary>
        /// Read the sequences of genes/units in multiple chromosomes, concatenate them and as a list of integer.
        /// </summary>
        /// <returns>One list of integer as a concatenated sequence</returns>
        public IList<int> ReadSequences(int paddingNumb)
        {
            const int minusInfinity = int.MinValue;
            int padingCounter = 0;
            string line;
            IList<int> allsequence = new List<int>();
            while ((line=_reader.ReadLine())!= null)
            {
                string lineStrimed = line.TrimEnd(_separator);
                string[] splitStrings = lineStrimed.Split(_separator);
                foreach (string s in splitStrings)
                {
                    int gene = int.Parse(s);
                    if(gene >= 0 )
                        allsequence.Add(gene);
                }
                for (int i = 0; i < paddingNumb+1; i++)
                {
                    allsequence.Add(minusInfinity + padingCounter);
                    padingCounter++;
                }
            }
            allsequence.Insert(0, -1);
            allsequence.Insert(allsequence.Count,-2);
            Console.Out.WriteLine("Padding numb"+ padingCounter); //TODO delete this out
            _reader.Close();
            return allsequence;
        }
    }
}