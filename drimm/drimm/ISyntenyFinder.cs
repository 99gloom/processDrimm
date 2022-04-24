namespace SyntenyFast
{
    public interface ISyntenyFinder
    {
        /// <summary>
        /// Run the synteny finder. It will do all task in synteny blocks identification and write results to files.
        /// </summary>
        /// <param name="cycleLenghtThreshold">maximum length of the cycle that can be re-route </param>
        /// <param name="smoothingThreshold"> maximum length of short simple blocks that should be splitted</param>
        /// <param name="propagationRadius"> maximum propagation radius</param>
        void Run(int cycleLenghtThreshold, int smoothingThreshold, int propagationRadius, int simplificationSteps, bool smooth, int dustThreshold,string outdir);
    }
}