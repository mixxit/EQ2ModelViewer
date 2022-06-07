namespace Spart.Parsers
{
	using System;
	using Spart.Parsers.Directives;

	/// <summary>
	/// Static helper class to create directives
	/// </summary>
	public class Dirs
	{
		public static LexemeDirective Lexeme(Parser parser)
		{
		    return new LexemeDirective(parser);
		}
    }
}
