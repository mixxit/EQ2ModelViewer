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
/// Author: Jonathan de Halleux
namespace Spart.Scanners
{
    using System;
	using System.IO;
	using Spart.Parsers;

	/// <summary>
	/// Scanner acting on a string.
	/// <seealso cref="IScanner"/>
	/// </summary>
    public class StringScanner : IScanner
    {
        private String m_InputString;
        private long m_Offset;
        private IFilter m_Filter;
        private Parser m_SkipParser;
        private bool m_IsSkipping;

		/// <summary>
		/// Creates a scanner on the string.
		/// </summary>
		/// <param name="inputString">Input string</param>
		/// <exception cref="ArgumentNullException">input string is null</exception>
		public StringScanner(String inputString) : this(inputString, Prims.Epsilon, 0)
        {
        }               


        /// <summary>
		/// Creates a scanner on the string at a specified offset
		/// </summary>
		/// <param name="inputString">Input string</param>
		/// <exception cref="ArgumentNullException">input string is null</exception>
		/// <exception cref="ArgumentException">offset if out of range</exception>
		public StringScanner(String inputString, long offset) : this(inputString, Prims.Epsilon, offset)
        {
        }               

        
        public StringScanner(String inputString, Parser skipParser) : this(inputString, skipParser, 0)
        {
        }
            
        public StringScanner(String inputString, Parser skipParser, long offset)
        {
            if (inputString == null)
                throw new ArgumentNullException("inputString is null");
            if (skipParser == null)
                throw new ArgumentNullException("skipParser is null");
            m_InputString = inputString;
            Offset = 0;
            Filter = null;
            IsSkipping = true;
            SkipParser = skipParser;
        }               
        
 
		/// <summary>
		/// the input string
		/// </summary>
        public String InputString  
        {
            get
            {
                return m_InputString;
            }
        }
 
		/// <summary>
		/// Current offset
		/// </summary>
        public long Offset  
        {
            get
            {
                return GetSkipOffset();
            }
            set
            {
                if (value < 0 || value > InputString.Length)
                    throw new ArgumentOutOfRangeException("offset out of bounds");
                m_Offset = value;
                m_Offset = GetSkipOffset();
            }
        }

		/// <summary>
		/// true if at the end of the string
		/// </summary>
        public bool AtEnd   
        {
            get
            { 
                return GetSkipOffset() == InputString.Length;
            }
        }

		/// <summary>
		/// Advance the cursor once
		/// </summary>
		/// <returns>true if not at end</returns>
		/// <exception cref="Exception">If called while AtEnd is true</exception>
        public bool Read()
        {
            if (AtEnd)
                throw new Exception("Scanner already at end");

            long newOffset = GetSkipOffset();
            m_Offset = newOffset + 1;

            return !AtEnd;
        }
          
		/// <summary>
		/// Current character
		/// </summary>
		/// <returns>character at cursor position</returns>
        public char Peek()
        {
            long newOffset = GetSkipOffset();

            if (Filter==null)
                return InputString[(int)newOffset];
            else
                return Filter.Filter(InputString[(int)newOffset]);
        }


        private long GetSkipOffset()
        {
            if (!IsSkipping)
            {
                return m_Offset;
            }
            else
            {
                IsSkipping = false;
                long offset = m_Offset;
                ParserMatch match = SkipParser.Parse(this);
                Seek(offset);
                IsSkipping = true;

                return match.Success ? match.Offset + match.Length : m_Offset;
            }
        }

		/// <summary>
		/// Extracts a substring 
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public String Substring(long offset, int length)
		{
			String s=InputString.Substring((int)offset,Math.Min(length, InputString.Length-(int)offset));
			
			if (Filter != null)
				s=Filter.Filter(s);

			return s;
		}

		/// <summary>
		/// Moves the cursor to the offset position
		/// </summary>
		/// <param name="offset"></param>
        public void Seek(long offset)
        {
			if (offset < 0 || offset > InputString.Length)
				throw new ArgumentOutOfRangeException("offset");

            Offset = offset;
        }
        
		/// <summary>
		/// Current filter
		/// </summary>
        public IFilter Filter 
        {
            get
            {
                return m_Filter;
            }
            set 
            { 
                m_Filter = value;
            }
        } 

		/// <summary>
		/// Failure match
		/// </summary>
        public ParserMatch NoMatch
        {
            get
            {
                return new ParserMatch(this,Offset,-1);
            }
        }

		/// <summary>
		/// Empty match
		/// </summary>
        public ParserMatch EmptyMatch
        {
            get
            {
                return new ParserMatch(this,GetSkipOffset(),0);
            }
        }

		/// <summary>
		/// Creates a successful match
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
        public ParserMatch CreateMatch(long offset, int length)
        {
            return new ParserMatch(this,offset,length);
        }


        public bool IsSkipping
        {
            get
            {
                return m_IsSkipping;
            }

            set
            {
                m_IsSkipping = value;
            }
        }

    
        public Parser SkipParser
        {
            get
            {
                return m_SkipParser;
            }

            set
            {
                m_SkipParser = value;
            }
        }
    }
}