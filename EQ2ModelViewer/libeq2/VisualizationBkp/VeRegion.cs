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

namespace Everquest2.Visualization {
    public class VeRegion : VeBase {
        public VeRegion() {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeRegion(Util.Eq2Reader reader, Util.StreamingContext context)
            : base(reader, context) {
            byte classVersion = context.ClassVersions[typeof(VeRegion)];

            if (classVersion == 0) unk0 = reader.ReadSingle();

            ushort count = reader.ReadUInt16();

            unk1 = new float[count, 3];
            unk2 = new float[count];
            unk3 = new short[count, 2];
            for (ushort i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
                unk1[i, 2] = reader.ReadSingle();
                unk2[i] = reader.ReadSingle();
                unk3[i, 0] = reader.ReadInt16();
                unk3[i, 1] = reader.ReadInt16();
            }

            if (classVersion >= 2) {
                uint unkcount = reader.ReadUInt32();
                float[,] unk6 = new float[unkcount, 4];

                for (int i = 0; i < unkcount; i++) {
                    unk6[i, 0] = reader.ReadSingle();
                    unk6[i, 1] = reader.ReadSingle();
                    unk6[i, 2] = reader.ReadSingle();
                    unk6[i, 3] = reader.ReadSingle();
                }
            }

            unk4[0] = reader.ReadSingle();
            unk4[1] = reader.ReadSingle();
            unk4[2] = reader.ReadSingle();
            unk5 = reader.ReadSingle();
        }


        private float unk0;
        private float[,] unk1;
        private float[] unk2;
        private short[,] unk3;
        private float[] unk4 = new float[3];
        private float unk5;
    }
}