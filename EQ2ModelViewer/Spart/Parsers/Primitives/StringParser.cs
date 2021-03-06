/// Spart License (zlib/png)
/// 
/// 
/// Copyright (c) 2003 Jonathan de Halleux
/// 
/// This software is provided 'as-is', without any express or implied warranty. 
/// In no event will the authors be held liable for any damages arising from 
/// the use of this software.
/// 
/// Permission is granted to anyone to use this software for any purpose, 
/// including commercial applications, and to alter it and redistribute it 
/// freely, subject to the following restrictions:
/// 
/// 1. The origin of this software must not be misrepresented; you must not 
/// claim that you wrote the original software. If you use this software in a 
/// product, an acknowledgment in the product documentation would be 
/// appreciated but is not required.
/// 
/// 2. Altered source versions must be plainly marked as such, and must not be 
/// misrepresented as being the original software.
/// 
/// 3. This notice may not be removed or altered from any source distribution.
/// 
/// Author: Jonathan de Halleuxnamespace Spart.Parsers.Primitives

namespace Spart.Parsers.Primitives
{
	using System;
	using Spart.Scanners;
	using Spart.Actions;
	using Spart.Parsers.NonTerminal;

	public class StringParser : TerminalParser
	{
		private String m_MatchedString;

		public StringParser(String str)
		{
			MatchedString = str;
		}

		public String MatchedString
		{
			get
			{
				return m_MatchedString;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("matched string is null");
				m_MatchedString = value;
			}
		}

		public override ParserMatch ParseMain(IScanner scanner)
		{
			long offset = scanner.Offset;
			foreach(Char c in MatchedString)
			{
				// if input consummed return null
				if (scanner.AtEnd || c != (Char)scanner.Peek()  )
				{
					scanner.Seek(offset);
					return scanner.NoMatch;
				}

				// read next characted
				scanner.Read();
			}
                
			// if we arrive at this point, we have a match                
			ParserMatch m = scanner.CreateMatch(offset, (int)(scanner.Offset - offset));

			// return match
			return m;
		}


        public override Parser Clone()
        {
            return base.Clone();
        }
	}
}
