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
    public class VdlSimpleType : VdlElement
    {
        public enum ValueType
        {
            Float,
            Int,
            Bool,
            String,
            Vec3Component
        }

        
        internal VdlSimpleType(string value, ValueType type)
        {
            this.value = value;
            this.type  = type;
        }


        internal override void AddElement(VdlElement child)
        {
            throw new NotSupportedException("Attempted to add a child object to a value type.");
        }

        
        public override IEnumerator<VdlElement> GetEnumerator()
        {
            throw new NotSupportedException("Attempted to enumerate the children of a value type.");
        }


        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            string name     = Name != null ? Name : GetNameFromType(Type);
            string typeName = GetTypeName(Type);

            writer.WriteStartElement(name);
            writer.WriteAttributeString("VDLTYPE", typeName);
            writer.WriteString(Value);
            writer.WriteEndElement();
        }


        private static string GetNameFromType(ValueType type)
        {
            switch (type)
            {
            case ValueType.Float:         return "FloatValue";
            case ValueType.Int:           return "IntValue";
            case ValueType.Bool:          return "BoolValue";
            case ValueType.String:        return "StringValue";
            case ValueType.Vec3Component: return "Vec3ComponentValue"; // Never used
            default:                      return null;
            }
        }


        private static string GetTypeName(ValueType type)
        {
            switch (type)
            {
            case ValueType.Float:         return "FLOAT";
            case ValueType.Int:           return "INT";
            case ValueType.Bool:          return "BOOL";
            case ValueType.String:        return "STRING";
            case ValueType.Vec3Component: return "VECCOMPONENT";
            default:                      return null;
            }
        }


        public string Value
        {
            get { return value; }
        }


        public ValueType Type
        {
            get { return type; }
        }


        private string    value;
        private ValueType type;
    }
}
