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
    public class VeRoomItemNode : VeItemNode
    {
        public VeRoomItemNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeRoomItemNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeRoomItemNode)];

            unk0 = reader.ReadUInt32();

            if (classVersion > 0) unk1 = reader.ReadByte();

            unk2 = reader.ReadSingle();

            if (classVersion > 1)
            {
                unk3    = reader.ReadByte();
                unk4[0] = reader.ReadSingle();
                unk4[1] = reader.ReadSingle();
                unk4[2] = reader.ReadSingle();
                unk5[0] = reader.ReadSingle();
                unk5[1] = reader.ReadSingle();
                unk5[2] = reader.ReadSingle();
                unk6    = reader.ReadByte();
                unk7    = reader.ReadByte();
                unk8    = reader.ReadByte();
                unk9    = reader.ReadByte();
            }
        }


        public uint    unk0;
        public byte    unk1;
        public float   unk2;
        public byte    unk3;
        public float[] unk4 = new float[3];
        public float[] unk5 = new float[3];
        public byte    unk6;
        public byte    unk7;
        public byte    unk8;
        public byte    unk9;
    }
}

/* EOF */