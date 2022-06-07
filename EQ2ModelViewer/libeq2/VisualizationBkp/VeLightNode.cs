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
    public class VeLightNode : VeNode
    {
        public enum FalloffType : uint
        {
            Linear      = 0,
            Constant    = 1,
            ExpQuarter  = 2,
            ExpHalf     = 3,
            Exp2        = 4,
            Bilinear    = 5
        }


        public enum LightType : uint
        {
            Direction = 0,
            Point     = 1,
            ConeSpot  = 2,
            BoxSpot   = 3
        }


        public VeLightNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeLightNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeLightNode)];

            #region Preconditions
            Debug.Assert(classVersion <= 13, "Unsupported VeLightNode class version " + classVersion + " (maximum 13)");
            #endregion

            direction[0] = reader.ReadSingle();
            direction[1] = reader.ReadSingle();
            direction[2] = reader.ReadSingle();
            position[0]  = reader.ReadSingle();
            position[1]  = reader.ReadSingle();
            position[2]  = reader.ReadSingle();

            if (classVersion > 6)
            {
                for (uint i = 0; i < 16; ++i) transformMatrix[i] = reader.ReadSingle();

                spotX = reader.ReadSingle();
                spotY = reader.ReadSingle();
            }

            if (classVersion > 7)
            {
                // Image: don't see this in qey_harbor_qey_terrain_harbor_geo11_2.voc
                //    spotTextureFileName = reader.ReadString(2);
            }

            rgba[0] = reader.ReadSingle();
            rgba[1] = reader.ReadSingle();
            rgba[2] = reader.ReadSingle();

            radius      = reader.ReadSingle();
            lightType   = (LightType)reader.ReadUInt32();
            falloffType = (FalloffType)reader.ReadUInt32();

            unk10 = reader.ReadSingle();

            update = reader.ReadBoolean();
            if (classVersion >  0) unk12           = reader.ReadUInt32();
            if (classVersion >  3) sunShadow       = reader.ReadBoolean();
            if (classVersion >  8) negative        = reader.ReadBoolean();
            if (classVersion >  9) fogRadius       = reader.ReadSingle();
            if (classVersion > 10) fog             = reader.ReadBoolean();
            if (classVersion > 12) orientSunShadow = reader.ReadByte();
            if ((orientSunShadow & 2) == 2) reader.ReadUInt32();

            if (classVersion > 2)
            {
                minColor[0] = reader.ReadSingle();
                minColor[1] = reader.ReadSingle();
                minColor[2] = reader.ReadSingle();
                maxColor[0] = reader.ReadSingle();
                maxColor[1] = reader.ReadSingle();
                maxColor[2] = reader.ReadSingle();
                minRadius   = reader.ReadSingle();
                maxRadius   = reader.ReadSingle();
                colorRate   = reader.ReadSingle();
            }

            if (classVersion > 4)
            {
                minLightPriority  = reader.ReadUInt32();
                maxLightPriority  = reader.ReadUInt32();
                minShadowPriority = reader.ReadUInt32();
                maxShadowPriority = reader.ReadUInt32();
            }

            if (classVersion > 5)
            {
                offTimeHour   = reader.ReadByte();
                offTimeMinute = reader.ReadByte();
                onTimeHour    = reader.ReadByte();
                onTimeMinute  = reader.ReadByte();
            }
        }


        public float[]     direction = new float[3];
        public float[]     position = new float[3];
        public float[]     transformMatrix = new float[16];
        public float       spotX;
        public float       spotY;
        public string      spotTextureFileName;
        public float[]     rgba = new float[3];
        public float       radius;
        public LightType   lightType;
        public FalloffType falloffType;
        public float       unk10;
        public bool        update;
        public uint        unk12;
        public bool        sunShadow;
        public bool        negative;
        public float       fogRadius;
        public bool        fog;
        public byte        orientSunShadow;
        public float[]     minColor = new float[3];
        public float[]     maxColor = new float[3];
        public float       minRadius;
        public float       maxRadius;
        public float       colorRate;
        public uint        minLightPriority;
        public uint        maxLightPriority;
        public uint        minShadowPriority;
        public uint        maxShadowPriority;
        public byte        onTimeHour;
        public byte        onTimeMinute;
        public byte        offTimeHour;
        public byte        offTimeMinute;
    }
}
