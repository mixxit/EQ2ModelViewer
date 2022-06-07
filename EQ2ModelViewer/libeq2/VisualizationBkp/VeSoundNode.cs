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
using System.Diagnostics;

#endregion

namespace Everquest2.Visualization
{
    public class VeSoundNode : VeResourceNode
    {
        public class SoundData3D
        {
            public SoundData3D(Util.Eq2Reader reader)
            {
                version = reader.ReadByte();
                Debug.Assert(version <= 1, "VeSoundNode.SoundData3D version " + version + " not supported");

                if (version > 0)
                {
                    unk1[0] = reader.ReadSingle();
                    unk1[1] = reader.ReadSingle();
                    unk1[2] = reader.ReadSingle();
                    unk2[0] = reader.ReadSingle();
                    unk2[1] = reader.ReadSingle();
                    unk2[2] = reader.ReadSingle();
                    unk3    = reader.ReadSingle();
                }
                else
                {
                    for (uint i = 0; i < 16; ++i) unk0[i] = reader.ReadSingle();
                }

                unk4 = reader.ReadSingle();
                unk5 = reader.ReadSingle();
                unk6 = reader.ReadInt32();
                unk7 = reader.ReadSingle();

                unk8[0] = reader.ReadSingle();
                unk8[1] = reader.ReadSingle();
                unk8[2] = reader.ReadSingle();

                unk9  = reader.ReadSingle();
                unk10 = reader.ReadSingle();
                unk11 = reader.ReadSingle();
                unk12 = reader.ReadSingle();
                unk13 = reader.ReadSingle();
                unk14 = reader.ReadSingle();
                unk15 = reader.ReadSingle();
                unk16 = reader.ReadSingle();
            }


            private byte    version;
            private float[] unk0 = new float[16];
            private float[] unk1 = new float[3];
            private float[] unk2 = new float[3];
            private float   unk3;
            private float   unk4;
            private float   unk5;
            private int     unk6;
            private float   unk7;
            private float[] unk8 = new float[3];
            private float   unk9;
            private float   unk10;
            private float   unk11;
            private float   unk12;
            private float   unk13;
            private float   unk14;
            private float   unk15;
            private float   unk16;
        }


        public VeSoundNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeSoundNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeSoundNode)];

            Debug.Assert(classVersion >= 3 && classVersion <= 7, "VeSoundNode version " + classVersion + " not supported");

            if (classVersion >= 7) roomId = reader.ReadUInt32();

            uint soundFileCount;

            if (classVersion < 5)
            {
                soundFileCount = 1;
            }
            else
            {
                soundFileCount = reader.ReadUInt32();
            }

            soundFiles = new string[soundFileCount];
            for (uint i = 0; i < soundFileCount; ++i)
            {
                soundFiles[i] = reader.ReadString(2);
            }

            if (classVersion < 6) soundPriority = reader.ReadSingle();

            if (classVersion < 5)
            {
                soundData = reader.ReadBytes(116);
            }
            else
            {
                soundData3D = new SoundData3D(reader);
            }

            soundFadeTime = reader.ReadSingle();

            if (classVersion >= 4)
            {
                soundOnHour    = reader.ReadByte();
                soundOnMinute  = reader.ReadByte();
                soundOffHour   = reader.ReadByte();
                soundOffMinute = reader.ReadByte();
            }

            if (classVersion >= 5)
            {
                minLoopMilliseconds = reader.ReadUInt32();
                maxLoopMilliseconds = reader.ReadUInt32();
                variance            = reader.ReadSingle();
            }
        }


        public uint        roomId;
        public string[]    soundFiles;
        public float       soundPriority;
        public byte[]      soundData;
        public SoundData3D soundData3D;
        public float       soundFadeTime;
        public byte        soundOnHour;
        public byte        soundOnMinute;
        public byte        soundOffHour;
        public byte        soundOffMinute;
        public uint        minLoopMilliseconds;
        public uint        maxLoopMilliseconds;
        public float       variance;
    }
}

/* EOF */