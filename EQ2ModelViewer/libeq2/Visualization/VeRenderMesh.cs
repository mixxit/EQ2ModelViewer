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
using System.IO;
using Everquest2.Util;

#endregion

namespace Everquest2.Visualization
{
    public class VeRenderMesh : VeBase
    {
        public enum PrimitiveType : uint
        {
            Triangles = 2
        }

        public struct SubMesh
        {
            public PrimitiveType PrimType;
            public int           StartingVertex;
            public int           VertexCount;
            public int           StartingIndex;
            public int           PrimitiveCount;
        }

        public struct TexCoord
        {
            public float U;
            public float V;
        }

        public struct Vertex
        {
            public float X;
            public float Y;
            public float Z;
        }

        public VeRenderMesh()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeRenderMesh(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeRenderMesh)];

            Debug.Assert(classVersion >= 11 && classVersion <= 15, "VeRenderMesh version " + classVersion + " not supported");

            flags = reader.ReadUInt32();

            uint vertexCount = reader.ReadUInt32();
            vertices = new Vertex[vertexCount];
            for (uint i = 0; i < vertexCount; ++i)
            {
                vertices[i].X = reader.ReadSingle();
                vertices[i].Y = reader.ReadSingle();
                vertices[i].Z = reader.ReadSingle();
            }

            uint normalCount = reader.ReadUInt32();
            normals = new Vertex[normalCount];
            for (uint i = 0; i < normalCount; ++i)
            {
                normals[i].X = reader.ReadSingle();
                normals[i].Y = reader.ReadSingle();
                normals[i].Z = reader.ReadSingle();
            }

            uint texCoordSetCount = (uint)(classVersion >= 15 ? 5 : 8);
            texCoords = new TexCoord[texCoordSetCount][];
            for (uint i = 0; i < texCoordSetCount; ++i)
            {
                uint texCoordCount = reader.ReadUInt32();

                texCoords[i] = new TexCoord[texCoordCount];
                for (uint j = 0; j < texCoordCount; ++j)
                {
                    texCoords[i][j].U = reader.ReadSingle();
                    texCoords[i][j].V = reader.ReadSingle();
                }
            }

            uint indexCount = reader.ReadUInt32();
            indices = new short[indexCount];
            for (uint i = 0; i < indexCount; ++i) indices[i] = reader.ReadInt16();

            uint subMeshCount = reader.ReadUInt32();
            subMeshes = new SubMesh[subMeshCount];
            for (uint i = 0; i < subMeshCount; ++i)
            {
                subMeshes[i].PrimType       = (PrimitiveType)reader.ReadUInt32();
                subMeshes[i].StartingVertex = reader.ReadInt32();
                subMeshes[i].VertexCount    = reader.ReadInt32();
                subMeshes[i].StartingIndex  = reader.ReadInt32();
                subMeshes[i].PrimitiveCount = reader.ReadInt32();
            }

            uint faceNormalCount = reader.ReadUInt32();
            faceNormals = new float[faceNormalCount * 3];
            for (uint i = 0; i < faceNormalCount * 3; ++i) faceNormals[i] = reader.ReadSingle();

            uint edgeCount = reader.ReadUInt32();
            edges = new float[edgeCount * 3];
            for (uint i = 0; i < edgeCount * 3; ++i) edges[i] = reader.ReadSingle();

            uint tangentBasisCount = reader.ReadUInt32();
            tangentBases = new float[tangentBasisCount,9];
            for (uint i = 0; i < tangentBasisCount; ++i)
            {
                for (uint j = 0; j < tangentBasisCount; ++j)
                {
                    tangentBases[i,j] = reader.ReadSingle();
                }
            }

            uint boneNameCount = reader.ReadUInt32();
            boneNames = new string[boneNameCount];
            for (uint i = 0; i < boneNameCount; ++i) boneNames[i] = reader.ReadString(2);

            uint boneIndexCount = reader.ReadUInt32();
            boneIndices = reader.ReadBytes((int)boneIndexCount);

            uint boneWeightCount = reader.ReadUInt32();
            boneWeights = new float[boneWeightCount];
            for (uint i = 0; i < boneWeightCount; ++i) boneWeights[i] = reader.ReadSingle();

            uint autoDropVertexIndexCount = reader.ReadUInt32();
            autoDropVertexIndices = new short[autoDropVertexIndexCount];
            for (uint i = 0; i < autoDropVertexIndexCount; ++i) autoDropVertexIndices[i] = reader.ReadInt16();

            defaultShaderPaletteName = reader.ReadString(2);

            uint subMeshNameCount = reader.ReadUInt32();
            subMeshNames = new string[subMeshNameCount];
            for (uint i = 0; i < subMeshNameCount; ++i) subMeshNames[i] = reader.ReadString(2);

            centroid[0] = reader.ReadSingle();
            centroid[1] = reader.ReadSingle();
            centroid[2] = reader.ReadSingle();

            radius = reader.ReadSingle();

            aabbMin[0] = reader.ReadSingle();
            aabbMin[1] = reader.ReadSingle();
            aabbMin[2] = reader.ReadSingle();
            aabbMax[0] = reader.ReadSingle();
            aabbMax[1] = reader.ReadSingle();
            aabbMax[2] = reader.ReadSingle();

            if (classVersion >= 12)
            {
                uint degenerateVertexCount = reader.ReadUInt32();
                degenerateVertexIndices = new short[degenerateVertexCount];
                for (uint i = 0; i < degenerateVertexCount; ++i) degenerateVertexIndices[i] = reader.ReadInt16();

                uint degenerateEdgeCount = reader.ReadUInt32();
                degenerateEdgeIndices = new short[degenerateEdgeCount];
                for (uint i = 0; i < degenerateEdgeCount; ++i) degenerateEdgeIndices[i] = reader.ReadInt16();
            }

            if (classVersion >= 13)
            {
                uint noPolygonShadowTriangleIndexCount = reader.ReadUInt32();
                noPolygonShadowTriangleIndices = new short[noPolygonShadowTriangleIndexCount];
                for (uint i = 0; i < noPolygonShadowTriangleIndexCount; ++i) noPolygonShadowTriangleIndices[i] = reader.ReadInt16();
            }
        }

        public uint      flags;
        public Vertex[]   vertices;
        public Vertex[]   normals;
        public TexCoord[][] texCoords;
        public short[]   indices;
        public SubMesh[] subMeshes;
        public float[]   faceNormals;
        public float[]   edges;
        public float[,]  tangentBases;
        public string[]  boneNames;
        public byte[]    boneIndices;
        public float[]   boneWeights;
        public short[]   autoDropVertexIndices;
        public string    defaultShaderPaletteName;
        public string[]  subMeshNames;
        public float[]   centroid = new float[3];
        public float     radius;
        public float[]   aabbMin  = new float[3];
        public float[]   aabbMax  = new float[3];
        public short[]   degenerateVertexIndices;
        public short[]   degenerateEdgeIndices;
        public short[]   noPolygonShadowTriangleIndices;
    }
}
