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
using System.Diagnostics;

#endregion

namespace Everquest2.Visualization
{
    public class VeNode : VeBase
    {
        public VeNode()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeNode)];
            
            if (classVersion >= 1) nodeFlags = reader.ReadUInt32();
            if (classVersion >= 3) WidgetID = reader.ReadUInt32(); // the widget id

            byte[] bytesOfNodeFlags = BitConverter.GetBytes(nodeFlags);
            long pos = reader.BaseStream.Position;
            if (classVersion >= 2)
            {
                if (nodeFlags == 168624137)
                    reader.ReadByte();
                uint count = reader.ReadUInt32();
                AssociatedNodes = new uint[count];
                for (uint i = 0; i < count; ++i)
                {
                    AssociatedNodes[i] = reader.ReadUInt32();
                }
            }
        }


        public virtual void AddChild(VeNode child)
        {
            children.Add(child);
        }


        public virtual IEnumerator<VeNode> EnumerateChildren()
        {
            return children.GetEnumerator();
        }


        private IList<VeNode> children = new List<VeNode>();

        public uint    nodeFlags;
        public uint    WidgetID;
        public uint[]  AssociatedNodes;
    }
}
