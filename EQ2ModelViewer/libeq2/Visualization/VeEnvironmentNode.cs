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
using Everquest2.Util;

#endregion

namespace Everquest2.Visualization
{
    public class VeEnvironmentNode : VeNode
    {
        public VeEnvironmentNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeEnvironmentNode(Util.Eq2Reader reader, Util.StreamingContext context)
            : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeEnvironmentNode)];

            bool hasRegionDefinition = reader.ReadBoolean();

            if (hasRegionDefinition)
            {
                regionDefinitionFile = reader.ReadString(2);
            }

            byte environmentDefinitionCount = reader.ReadByte();

            if (environmentDefinitionCount > 0)
            {
                environmentDefinitions = new string[environmentDefinitionCount];
                for (byte i = 0; i < environmentDefinitionCount; ++i)
                {
                    environmentDefinitions[i] = reader.ReadString(2);
                }
            }

            if (classVersion >= 2)
            {
                //4 bytes, not sure if its a float
                unk1 = reader.ReadInt32();
            }

            if (classVersion >= 3)
            {
                unk2 = reader.ReadByte();
            }
        }


        public string regionDefinitionFile;
        public string[] environmentDefinitions;
        public int unk1 = 0;
        public int unk2 = 0;
    }
}
