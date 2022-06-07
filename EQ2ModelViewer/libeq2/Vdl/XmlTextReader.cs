#region License information
// ----------------------------------------------------------------------------
//
//       libeq2 - A library for analyzing the Everquest II File Format
//                         Blaz (blaz@blazlabs.com)
//
//       This program is free software; you can redistribute it and/or
//        modify it under the terms of the GNU General Public License
//      as published by the Free Software Foundation; either version 2
//          of the License, or (at your option) any later version.
//
//      This program is distributed in the hope that it will be useful,
//      but WITHOUT ANY WARRANTY; without even the implied warranty of
//       MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//                GNU General Public License for more details.
//
//      You should have received a copy of the GNU General Public License
//         along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA
//
//   ( The full text of the license can be found in the License.txt file )
//
// ----------------------------------------------------------------------------
#endregion

#region Using directives

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using Everquest2.Vdl.Parser;

#endregion

namespace Everquest2.Vdl
{
    public class XmlTextReader : TextReader
    {
        #region Constructors
        public XmlTextReader(Stream stream, Everquest2.IO.FileSystem fileSystem)
        {
            TextReader reader = new StreamReader(stream, System.Text.Encoding.ASCII);
            Initialize(reader, fileSystem);
        }


        public XmlTextReader(String text, Everquest2.IO.FileSystem fileSystem)
        {
            TextReader reader = new StringReader(text);
            Initialize(reader, fileSystem);
        }
        #endregion


        private void Initialize(TextReader reader, Everquest2.IO.FileSystem fileSystem)
        {
            string originalVdlText   = reader.ReadToEnd();
            string translatedXmlText = Translate(originalVdlText, fileSystem);

            translatedXmlReader = new StringReader(translatedXmlText);
        }


        private string Translate(string text, Everquest2.IO.FileSystem fileSystem)
        {
            StringBuilder result = new StringBuilder();
            
            // Create the XML writer
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            XmlWriter writer = XmlWriter.Create(result, settings);

            // Parse the VDL file
            VdlParser  parser      = new VdlParser(text, fileSystem);
            VdlElement rootElement = parser.Parse();

            writer.WriteStartElement("VdlFile", "Vdl");
            foreach (VdlElement element in rootElement) element.WriteXml(writer);
            writer.WriteEndElement();

            writer.Close();

            return result.ToString();
        }


        #region Dispose semantics
        ~XmlTextReader()
        {
            Dispose(false);
        }


        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    translatedXmlReader.Close();
                }

                disposed = true;

                base.Dispose(disposing);
            }
        }

        private bool disposed = false;
        #endregion


        public override void Close()
        {
            Dispose();
        }


        public override string ToString()
        {
            return translatedXmlReader.ToString();
        }


        public override string ReadToEnd()
        {
            return translatedXmlReader.ReadToEnd();
        }


        public override string ReadLine()
        {
            return translatedXmlReader.ReadLine();
        }


        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return translatedXmlReader.ReadBlock(buffer, index, count);
        }


        public override int Read(char[] buffer, int index, int count)
        {
            return translatedXmlReader.Read(buffer, index, count);
        }


        public override int Read()
        {
            return translatedXmlReader.Read();
        }


        public override int Peek()
        {
            return translatedXmlReader.Peek();
        }


        private StringReader translatedXmlReader;
    }
}

/* EOF */