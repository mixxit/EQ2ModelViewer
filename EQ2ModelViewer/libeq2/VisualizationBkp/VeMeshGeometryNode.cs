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
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Everquest2.Util;

#endregion

namespace Everquest2.Visualization
{
    public class VeMeshGeometryNode : VeGeometryNode
    {
        public VeMeshGeometryNode()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeMeshGeometryNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {            
            byte classVersion = context.ClassVersions[typeof(VeMeshGeometryNode)];

            if (classVersion == 0 || classVersion > 7)
            {
                throw new Util.DeserializationException("VeMeshGeometryNode version " + classVersion + " not supported");
            }

            lodCount = reader.ReadUInt32();
            Debug.Assert(lodCount <= 10/*Configuration.GetValue<uint>("cl_max_lods")*/);
            
            extraLodCount = 0;

            renderMeshNames = new string[lodCount][];
            for (uint i = 0; i < lodCount; ++i)
            {
                uint renderMeshNameCount;

                if (classVersion >= 5)
                {
                    renderMeshNameCount = reader.ReadUInt32();
                }
                else
                {
                    renderMeshNameCount = 1;
                }

                renderMeshNames[i] = new string[renderMeshNameCount];
                for (uint j = 0; j < renderMeshNameCount; ++j)
                {
                    renderMeshNames[i][j] = reader.ReadString(2);
                }
            }

            bool shaderPaletteIsInline = false;
            if (classVersion >= 6) shaderPaletteIsInline = reader.ReadBoolean();

            shaderPaletteNames = new string[lodCount];
            shaderPalettes     = new VeShaderPalette[lodCount];
            for (uint i = 0; i < lodCount; ++i)
            {
                shaderPaletteNames[i] = reader.ReadString(2);

                if (shaderPaletteIsInline)
                {
                    shaderPalettes[i] = (VeShaderPalette)reader.ReadObject();
                }
            }

            collisionMeshName = reader.ReadString(2);

            if (classVersion == 2) legacyNavigationMeshName = reader.ReadString(2);

            if (classVersion < 4)
            {
                shadowMeshNames = new string[lodCount];
                for (uint i = 0; i < lodCount; ++i)
                {
                    shadowMeshNames[i] = reader.ReadString(2);
                }

                occluderMeshName = reader.ReadString(2);
            }

            lodDistances = new float[lodCount];
            for (uint i = 0; i < lodCount; ++i)
            {
                lodDistances[i] = reader.ReadSingle();
            }

            if (classVersion >= 7)
            {
                byte unk1 = reader.ReadByte();
            }
        }


        public uint              lodCount;
        public uint              extraLodCount;
        public string[][]        renderMeshNames;
        public string[]          shaderPaletteNames;
        public VeShaderPalette[] shaderPalettes;
        public string            collisionMeshName;
        public string            legacyNavigationMeshName;
        public string[]          shadowMeshNames;
        public string            occluderMeshName;
        public float[]           lodDistances;
    }
}
