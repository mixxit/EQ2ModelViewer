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
using Everquest2.Visualization.ParticleGenerator;

#endregion

namespace Everquest2.Visualization
{
    public class VeParticleGeneratorNode : VeGeometryNode
    {
        public VeParticleGeneratorNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeParticleGeneratorNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeParticleGeneratorNode)];

            byte nodeClassVersion = classVersion;

            //Begin VeParticleGeneratorNode Deserialization
            if(classVersion >= 24)
            {
                classVersion = (byte)reader.ReadUInt16();
                uint ver24BufferSize = reader.ReadUInt32();
            }

            //Begin VeParticleGenerator Deserialization
            uint opCount1 = reader.ReadUInt32();

            unkOp0 = new VeParticleGeneratorOp[opCount1];
            for (uint i = 0; i < opCount1; ++i)
            {
                unkOp0[i] = reader.ReadParticleGeneratorOp(classVersion);
            }

            uint opCount2 = reader.ReadUInt32();

            unkOp1 = new VeParticleGeneratorOp[opCount2];
            for (uint i = 0; i < opCount2; ++i)
            {
                unkOp1[i] = reader.ReadParticleGeneratorOp(classVersion);
            }

            if (classVersion >= 11)
            {
                uint opCount3 = reader.ReadUInt32();

                unkOp2 = new VeParticleGeneratorOp[opCount3];
                for (uint i = 0; i < opCount3; ++i)
                {
                    unkOp2[i] = reader.ReadParticleGeneratorOp(classVersion);
                }
            }

            if (classVersion >=  2) unk0 = reader.ReadByte();
            if (classVersion >= 12) unk1 = reader.ReadByte();
            if (classVersion >=  3) unk2 = reader.ReadByte();
            if (classVersion >=  4) unk3 = reader.ReadByte();
            if (classVersion >=  5) unk4 = reader.ReadByte();

            if (classVersion >= 6)
            {
                unk5 = reader.ReadByte();
                unk6 = reader.ReadSingle();
                unk7 = reader.ReadSingle();
            }

            if (classVersion >= 7)
            {
                onTimeHour    = reader.ReadByte();
                onTimeMinute  = reader.ReadByte();
                offTimeHour   = reader.ReadByte();
                offTimeMinute = reader.ReadByte();
                unk12         = reader.ReadString(2);
            }

            if (classVersion >= 8)
            {
                unk13 = reader.ReadByte();

                if (classVersion >= 14)
                {
                    unk14 = reader.ReadSingle();
                    unk15 = reader.ReadSingle();
                }

                if (classVersion >= 16) unk16 = reader.ReadByte();
                if (classVersion >= 17) unk17 = reader.ReadByte();

                unk18 = reader.ReadByte();
                unk19 = reader.ReadByte();
                unk20 = reader.ReadUInt32();
                unk21 = reader.ReadString(2);

                if (classVersion >= 17) unk22 = reader.ReadString(2);

                unk23 = reader.ReadByte();
                unk24 = reader.ReadUInt32();

                if (classVersion >=  9) unk25 = reader.ReadByte();
                if (classVersion >= 13) unk26 = reader.ReadByte();

                if (classVersion >= 19) 
                {
                    unk27 = reader.ReadByte();
                    unk28 = reader.ReadByte();
                }

                if (classVersion >= 20) 
                {
                    unk29 = reader.ReadByte();
                    unk30 = reader.ReadByte();
                }

                if (classVersion == 14) unk31 = reader.ReadByte();

                if (classVersion >= 16)
                {
                    unk32 = reader.ReadSingle();
                    unk33 = reader.ReadUInt32();

                    if (classVersion >= 23)
                    {
                        float unk1337 = reader.ReadSingle();
                        float unk1338 = reader.ReadSingle();
                    }

                    unk34 = reader.ReadSingle();
                    unk35 = reader.ReadByte();
                }

                if (classVersion >= 18) unk36 = reader.ReadByte();
            }

            if (classVersion == 21)
            {
                byte unk1339 = reader.ReadByte();
            }
            //End VeParticleGenerator Deserialization

            if (nodeClassVersion >= 10) unk37 = reader.ReadSingle();
            //End VeParticleGeneratorNode Deserialization
        }


        public VeParticleGeneratorOp[] unkOp0;
        public VeParticleGeneratorOp[] unkOp1;
        public VeParticleGeneratorOp[] unkOp2;

        public byte   unk0;
        public byte   unk1;
        public byte   unk2;
        public byte   unk3;
        public byte   unk4;
        public byte   unk5;
        public float  unk6;
        public float  unk7;
        public byte   onTimeHour;
        public byte   onTimeMinute;
        public byte   offTimeHour;
        public byte   offTimeMinute;
        public string unk12;
        public byte   unk13;
        public float  unk14;
        public float  unk15;
        public byte   unk16;
        public byte   unk17;
        public byte   unk18;
        public byte   unk19;
        public uint   unk20;
        public string unk21;
        public string unk22;
        public byte   unk23;
        public uint   unk24;
        public byte   unk25;
        public byte   unk26;
        public byte   unk27;
        public byte   unk28;
        public byte   unk29;
        public byte   unk30;
        public byte   unk31;
        public float  unk32;
        public uint   unk33;
        public float  unk34;
        public byte   unk35;
        public byte   unk36;
        public float  unk37;
    }
}
