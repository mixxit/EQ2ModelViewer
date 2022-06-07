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
    public class VdlVector3 : VdlElement
    {
        internal VdlVector3()
        {
        }


        internal override void AddElement(VdlElement child)
        {
            if (count == 3) throw new InvalidOperationException("Attempted to add a fourth component to a 3-vector.");

            if (!(child is VdlSimpleType) || (child as VdlSimpleType).Type != VdlSimpleType.ValueType.Vec3Component) 
            {
                throw new InvalidOperationException("Attempted to add a non-float value to a 3-vector.");
            }

            switch (count)
            {
            case 0: x = child as VdlSimpleType; x.Name = "x"; break;
            case 1: y = child as VdlSimpleType; y.Name = "y"; break;
            case 2: z = child as VdlSimpleType; z.Name = "z"; break;
            }

            ++count;
        }

        
        public override IEnumerator<VdlElement> GetEnumerator()
        {
            throw new NotSupportedException("Enumerators not supported on a VdlVector3.");
        }


        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(Name);
            writer.WriteAttributeString("VDLTYPE", "VEC3");

            x.WriteXml(writer);
            y.WriteXml(writer);
            z.WriteXml(writer);

            writer.WriteEndElement();
        }


        public VdlElement X { get { return x; } }
        public VdlElement Y { get { return y; } }
        public VdlElement Z { get { return z; } }


        private int count = 0;

        private VdlSimpleType x;
        private VdlSimpleType y;
        private VdlSimpleType z;
    }
}
