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
    public class VeCollisionMesh : VeBase
    {
        public struct Face
        {
            public short unk0;
            public short unk1;
            public byte unk2;
            public byte unk3;
        }

        public struct Leaf
        {
            public uint unk0;
            public short unk1;
        }


        public struct Branch
        {
            public float unk0;
            public short unk1;
            public short unk2;
            public byte unk3;
        }


        public VeCollisionMesh()
        {
        }

    
        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeCollisionMesh(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeCollisionMesh)];

            uint chunkId;
            uint chunkSize;

            chunkId = reader.ReadUInt32();
            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x20544948 && chunkSize > 0, "Invalid VeCollisionMesh HIT chunk");

            chunkId = reader.ReadUInt32();
            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x58444956 && chunkSize > 0, "Invalid VeCollisionMesh VIDX chunk");
            uint vertexIndexCount = chunkSize / 2;
            vertexIndices = new ushort[vertexIndexCount];
            for (uint i = 0; i < vertexIndexCount; ++i ) vertexIndices[i] = reader.ReadUInt16();

            chunkId = reader.ReadUInt32();
            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x58444945 && chunkSize > 0, "Invalid VeCollisionMesh EIDX chunk");
            uint edgeIndexCount = chunkSize / 2;
            edgeIndices = new ushort[edgeIndexCount];
            for (uint i = 0; i < edgeIndexCount; ++i) edgeIndices[i] = reader.ReadUInt16();

            chunkId = reader.ReadUInt32();

            if (chunkId == 0x58444946)
            {
                chunkSize = reader.ReadUInt32();

                uint faceIndexCount = chunkSize / 2;
                faceIndices = new ushort[faceIndexCount];
                for (uint i = 0; i < faceIndexCount; ++i) faceIndices[i] = reader.ReadUInt16();

                chunkId = reader.ReadUInt32();
            }

            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x54524556 && chunkSize > 0, "Invalid VeCollisionMesh VERT chunk");
            uint vertexCount = chunkSize / 12;
            vertices = new float[vertexCount,3];
            for (uint i = 0; i < vertexCount; ++i)
            {
                vertices[i, 0] = reader.ReadSingle();
                vertices[i, 1] = reader.ReadSingle();
                vertices[i, 2] = reader.ReadSingle();
            }

            chunkId = reader.ReadUInt32();
            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x45474445 && chunkSize > 0, "Invalid VeCollisionMesh EDGE chunk");
            uint edgeCount = chunkSize / 4;
            edges = new ushort[edgeCount,2];
            for (uint i = 0; i < edgeCount; ++i)
            {
                edges[i, 0] = reader.ReadUInt16();
                edges[i, 1] = reader.ReadUInt16();
            }

            chunkId = reader.ReadUInt32();
            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x45434146 && chunkSize > 0, "Invalid VeCollisionMesh FACE chunk");

            uint faceSize = (uint)(classVersion != 6 ? 6 : 5);
            uint faceCount = chunkSize / faceSize;
            faces = new Face[faceCount];
            for (uint i = 0; i < faceCount; ++i)
            {
                faces[i].unk0 = reader.ReadInt16();
                faces[i].unk1 = reader.ReadInt16();
                if (classVersion < 5 || classVersion > 6) faces[i].unk2 = reader.ReadByte();
                faces[i].unk3 = reader.ReadByte();
            }


            chunkId = reader.ReadUInt32();

            if (chunkId == 0x4641454C)
            {
                chunkSize = reader.ReadUInt32();
                Debug.Assert(chunkSize > 0, "Invalid VeCollisionMesh LEAF chunk");

                uint leafCount = chunkSize / 6;
                leaves = new Leaf[leafCount];
                for (uint i = 0; i < leafCount; ++i)
                {
                    leaves[i].unk0 = reader.ReadUInt32();
                    leaves[i].unk1 = reader.ReadInt16();
                }

                chunkId = reader.ReadUInt32();
            }

            if (chunkId == 0x4E415242)
            {
                chunkSize = reader.ReadUInt32();
                Debug.Assert(chunkSize > 0, "Invalid VeCollisionMesh BRAN chunk");

                uint branchCount = chunkSize / 9;
                branches = new Branch[branchCount];
                for (uint i = 0; i < branchCount; ++i)
                {
                    branches[i].unk0 = reader.ReadSingle();
                    branches[i].unk1 = reader.ReadInt16();
                    branches[i].unk2 = reader.ReadInt16();
                    branches[i].unk3 = reader.ReadByte();
                }

                chunkId = reader.ReadUInt32();
            }

            chunkSize = reader.ReadUInt32();
            Debug.Assert(chunkId == 0x59524442 && chunkSize > 0, "Invalid VeCollisionMesh BDRY chunk");
            uint bdryCount = chunkSize / 4;
            bdry = new float[bdryCount];
            for (uint i = 0; i < bdryCount; ++i) bdry[i] = reader.ReadSingle();
        }


        public ushort[]  vertexIndices;
        public ushort[]  edgeIndices;
        public ushort[]  faceIndices;
        public float[,]  vertices;
        public ushort[,] edges;
        public Face[]    faces;
        public Leaf[]    leaves;
        public Branch[]  branches;
        public float[]   bdry;
    }
}
