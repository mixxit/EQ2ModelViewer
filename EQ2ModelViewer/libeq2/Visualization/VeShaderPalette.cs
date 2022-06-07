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
    public class VeShaderPalette : VeBase
    {
        public VeShaderPalette()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeShaderPalette(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeShaderPalette)];
            Debug.Assert(classVersion == 0, "VeShaderPalette version " + classVersion + " not supported (maximum 0)");

            uint shaderNameCount = reader.ReadUInt32();

            shaderNames = new string[shaderNameCount];
            for (uint i = 0; i < shaderNameCount; ++i)
            {
                shaderNames[i] = reader.ReadString(2);
                // Note: The string length on the stream includes the zero terminator, so we remove it afterwards.
                shaderNames[i] = shaderNames[i].Remove(shaderNames[i].Length - 1, 1);
            }
        }


        public string[] shaderNames;
    }
}
