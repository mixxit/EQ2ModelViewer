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

#endregion

namespace Everquest2.Visualization
{
    public class VePortalNode : VeSelectNode
    {
        public VePortalNode()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VePortalNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VePortalNode)];

            reader.ReadByte();
            unk0 = reader.ReadString();

            short count = reader.ReadInt16();

            unk1 = new float[count,3];
            for (short i = 0; i < count; ++i)
            {
                unk1[i,0] = reader.ReadSingle();
                unk1[i,1] = reader.ReadSingle();
                unk1[i,2] = reader.ReadSingle();
            }

            unk2 = reader.ReadByte();
        }

        public string   unk0;
        public float[,] unk1;
        public byte     unk2;
    }
}
