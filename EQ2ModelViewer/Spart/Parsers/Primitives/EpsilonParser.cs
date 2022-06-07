using System;

namespace Spart.Parsers.Primitives
{
	using Spart.Actions;
	using Spart.Scanners;
	using Spart.Parsers.NonTerminal;

	public class EpsilonParser : TerminalParser
	{
		public override ParserMatch ParseMain(IScanner scanner)
		{
			if (scanner == null)
				throw new ArgumentNullException("scanner");

            ParserMatch match = scanner.EmptyMatch;
            scanner.Seek(match.Offset);
			return match;
		}

    
        public override Parser Clone()
        {
            return base.Clone();
        }
    }
}
