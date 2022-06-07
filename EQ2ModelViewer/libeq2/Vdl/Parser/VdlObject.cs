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
using System.Collections.Generic;

#endregion

namespace Everquest2.Vdl.Parser
{
    public class VdlObject : VdlElement
    {
        internal VdlObject(string type)
        {
            this.type = type;
        }


        internal override void AddElement(VdlElement child)
        {
            elements.Add(child);
        }

        
        public override IEnumerator<VdlElement> GetEnumerator()
        {
            return elements.GetEnumerator();
        }


        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            string name = Name != null ? Name : Type;
            string type = Name != null ? Type : "OBJECT";

            writer.WriteStartElement(name);
            writer.WriteAttributeString("VDLTYPE", type);
            foreach (VdlElement child in elements) child.WriteXml(writer);
            writer.WriteEndElement();
        }


        public string Type 
        { 
            get { return type; } 
        }

        
        private ICollection<VdlElement> elements = new LinkedList<VdlElement>();
        private string type;
    }
}
