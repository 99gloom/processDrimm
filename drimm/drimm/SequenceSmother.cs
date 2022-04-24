using System;
using System.Collections.Generic;
using Util.Collection;

namespace SyntenyFast
{
    public class SequenceSmother : ISequenceSmother
    {
        private readonly int _smallNoiseThreshold;
        private readonly int _cycleLengthThreshold;
        private bool _removeDustCalled;
        private List<int> _originalList;

        public SequenceSmother(int smallNoiseThreshold, int cycleLengthThreshold)
        {
            _smallNoiseThreshold = smallNoiseThreshold;
            _cycleLengthThreshold = cycleLengthThreshold/2;
        }

        /// <summary>
        /// Smooth the color of a raw colored sequence
        /// </summary>
        /// <param name="sequence"> raw colored sequence</param>
        /// <param name="colorbyNodeID">color by NodeID of the raw colored sequence</param>
        /// <returns> a list of color ID corresponds to the sequence </returns>
        public IList<int> Smooth(IList<int> sequence, IDictionary<int, int> colorbyNodeID)
        {
            IList<int> colorOrder = new List<int>();
            for (int i = 0; i < sequence.Count; i++)
                colorOrder.Add(-1);

            IList<Pair<int>> blockPositions;
            IList<IList<int>> blocks = GetBlocks(colorbyNodeID, sequence,out blockPositions);
            IList<int> initialColor = new List<int>();
            foreach (int nodeID in sequence)
                initialColor.Add(colorbyNodeID[nodeID]);
            for (int i = 0; i < blocks.Count; i++)
            {
                IList<int> currentBlock = blocks[i];
                int currentEndPosition = blockPositions[i].Second;
                int currentColor = initialColor[currentEndPosition];
                int distance = 0;
                int nextCount = 1;
                while (distance < _cycleLengthThreshold)
                {
                    if (i + nextCount >= blocks.Count)
                        break;
                    IList<int> nextBlock = blocks[i + nextCount];
                    distance = distance + nextBlock.Count;
                    nextCount++;
                    int nextColor = initialColor[currentEndPosition + distance];
                    if (nextColor == currentColor)
                        for (int j = currentEndPosition + 1; j < currentEndPosition + distance + 1; j++)
                            initialColor[j] = currentColor;
                }
            }
            return initialColor; 
        }

        /// <summary>
        /// We remove the elements on the sequence that are repeated more than <paramref name="repeatThreshold"/>
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="repeatThreshold"></param>
        public void RemoveDust(ref IList<int> sequence, int repeatThreshold)
        {
            _originalList = new List<int>(sequence);
            _removeDustCalled = true; 
            IDictionary<int, int> counter = new Dictionary<int, int>();
            foreach (int i in sequence)
                if (counter.ContainsKey(i))
                    counter[i]++;
                else
                    counter.Add(i, 1);
            HashSet<int> dustSet = new HashSet<int>();
            foreach (KeyValuePair<int, int> pair in counter)
                if (pair.Value >= repeatThreshold)
                    dustSet.Add(pair.Key);
            for (int i = 0; i < sequence.Count; i++)
            {
                if (dustSet.Contains(sequence[i]))
                {
                    sequence.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// This is a pair function with RemoveDust function. The call is valid if RemoveDust was just called to the same sequence without any modification 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="colorbyNodePosition">list of color of the nodes of the sequence</param>
        public IList<int> ReStoreDust(ref IList<int> sequence, IList<int> colorbyNodePosition)
        {
            if (!_removeDustCalled)
                throw new InvalidOperationException();
            _removeDustCalled = false;
            for (int index = 0; index < _originalList.Count; index++)
            {
                int node = _originalList[index];
                if (sequence.Count<=index || sequence[index] != node)
                {
                    sequence.Insert(index, node);
                    int color = 0;
                    if (index < colorbyNodePosition.Count - 1)
                        color = colorbyNodePosition[index];
                    else
                        color = colorbyNodePosition[index];
                    colorbyNodePosition.Insert(index, color);
                }
            }
            return colorbyNodePosition;
        }

        public IList<IList<int>> GetBlocksSign(IList<int> modifiedSequence, IList<IList<int>> simplePath, int minimumBlockLength)
        {
            IList<IList<int>> modifiedSequencesChrs = new List<IList<int>>();
            IList<int> chr = new List<int>();
            for (int i = 1; i < modifiedSequence.Count-1; i++)
            {
                if (modifiedSequence[i] >= 0)
                    chr.Add(modifiedSequence[i]);
                else
                {
                    modifiedSequencesChrs.Add(chr);
                    chr = new List<int>();
                    while (i < modifiedSequence.Count && modifiedSequence[i] < 0)
                    {
                        i++;
                    }
                    i--;
                }
            }
            modifiedSequencesChrs.Add(chr);
            //hashing the simplePath
            IDictionary<int, int> synIDbyNodeID = new Dictionary<int, int>();
            IDictionary<int, int> synSizeBySynID = new Dictionary<int, int>();
            int synID = 0;
            foreach (IList<int> path in simplePath)
            {
                foreach (int i in path)
                    synIDbyNodeID.Add(i, synID);
                synSizeBySynID.Add(synID, path.Count);

                synID++;
            }
            IList<IList<int>> sequenceBlocks = new List<IList<int>>();
            foreach (IList<int> chromo in modifiedSequencesChrs)
            {
                IList<int> chromoInitialElement = new List<int>();//just contains the initial element in the block
                int previousSynID = -1;
                for (int i = 0; i < chromo.Count; i++)
                {
                    int currentSynID = synIDbyNodeID[chromo[i]];
                    if (currentSynID != previousSynID)
                    {
                        chromoInitialElement.Add(chromo[i]);
                    }
                    previousSynID = currentSynID;
                }
                //
                IList<int> chromoBlocks = new List<int>();
                foreach (int element in chromoInitialElement)
                {
                    int currentSynID = synIDbyNodeID[element];
                    if (synSizeBySynID[currentSynID] > minimumBlockLength)
                    {
                        int sign = 1;
                        if (simplePath[currentSynID][0] != element )
                        {
                            sign = -1;
                        }
                        chromoBlocks.Add(sign*currentSynID);
                    }
                }
                
                sequenceBlocks.Add(chromoBlocks);

            }
            return sequenceBlocks;

        }


        private static IList<IList<int>> GetBlocks(IDictionary<int, int> colorbyNodeID, IList<int> sequence, out IList<Pair<int>> blockPositions)
        {
            int colorID = colorbyNodeID[sequence[0]];
            blockPositions = new List<Pair<int>>();
            IList<IList<int>> blocksResult = new List<IList<int>>();
            IList<int> block = new List<int>();
            int last;
            int first = 0;
            for (int i = 0; i < sequence.Count; i++)
            {
                int currentColorID = colorbyNodeID[sequence[i]];
                if (currentColorID != colorID)
                {
                    colorID = currentColorID;
                    last = i - 1;
                    blocksResult.Add(block);
                    blockPositions.Add(new Pair<int>(first,last));
                    block = new List<int>();
                    first = i; 
                }
                block.Add(sequence[i]);
            }
            blockPositions.Add(new Pair<int>(first,sequence.Count-1));
            blocksResult.Add(block);
            return blocksResult; 
        }
    }
}