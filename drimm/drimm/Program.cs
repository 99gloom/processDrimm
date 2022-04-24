using System;

namespace SyntenyFast
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            
            int cycleLengthThreshold = 4;
            Console.Out.WriteLine("Enter inputfile");
            string infile = Console.ReadLine();
            Console.Out.WriteLine("Enter outdir");
            string outdir = Console.ReadLine();
            Console.Out.WriteLine("Enter cycleLengthThreshold");
            string length = Console.ReadLine();
            if (length != null) cycleLengthThreshold = int.Parse(length);
            Console.Out.WriteLine("Enter dustThreshold");

            int dustLengthThreshold = 10;
            string dustThreshold = Console.ReadLine();
            if (dustThreshold != null)
                dustLengthThreshold = int.Parse(dustThreshold);

            IGraphTool graphTool = new GraphTool();
            ABruijnGraph aBruijnGraph = new ABruijnGraph(graphTool);
            IDataReader dataReader = new SyntenyDataReader(infile, ' ');
            IDataWriter dataWriter = new SyntenyDataWriter(outdir+"/synteny.txt", ' ', outdir+"/sequenceColor.txt", infile,
                                                           outdir+"/modifiedSequence.txt");
            IColorTracker colorTracker = new ColorTracker();
            ISequenceSmother smother = new SequenceSmother(2, cycleLengthThreshold);
            ISyntenyFinder syntenyFinder = new SyntenyFinder(dataReader, dataWriter, aBruijnGraph, smother, colorTracker);
            syntenyFinder.Run(cycleLengthThreshold, 2, 3, 15, true, dustLengthThreshold,outdir);
        }
    }
}
        