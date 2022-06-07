namespace Spart.Parsers.Directives
{
	using System;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Parsers.NonTerminal;

	public class LexemeDirective : UnaryTerminalParser
	{
		public LexemeDirective(Parser parser) : base(parser)
		{
        }

		public override ParserMatch ParseMain(IScanner scanner)
		{
            // Remove leading whitespace
            Prims.Epsilon.Parse(scanner);

            bool isSkipping = scanner.IsSkipping;
            scanner.IsSkipping = false;
            ParserMatch m = this.Parser.Parse(scanner);
            scanner.IsSkipping = isSkipping;

            return m;
		}

    
        public override Parser Clone()
        {
            return base.Clone();
        }
    }
}
