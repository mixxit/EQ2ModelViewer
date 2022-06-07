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

            myId_grid = reader.ReadUInt32();

            if (classVersion > 0)
            {
                byte boolVals = reader.ReadByte();
                bOutside = (boolVals & 1) == 1;
                unkBool = (boolVals & 2) == 2;
            }

            roomLoadRadius = reader.ReadSingle();

            if (classVersion > 1)
            {
                bOverrideAmbient = reader.ReadBoolean();
                dayLightColor[0] = reader.ReadSingle();
                dayLightColor[1] = reader.ReadSingle();
                dayLightColor[2] = reader.ReadSingle();
                nightLightColor[0] = reader.ReadSingle();
                nightLightColor[1] = reader.ReadSingle();
                nightLightColor[2] = reader.ReadSingle();
                onHour = reader.ReadByte();
                onMinute = reader.ReadByte();
                offHour = reader.ReadByte();
                offMinute = reader.ReadByte();
            }
        }


        public uint myId_grid;
        public float roomLoadRadius;
        public bool bOverrideAmbient;
        public float[] dayLightColor = new float[3];
        public float[] nightLightColor = new float[3];
        public byte onHour;
        public byte onMinute;
        public byte offHour;
        public byte offMinute;
        public bool unkBool;
        public bool bOutside;
    }
}

/* EOF */
