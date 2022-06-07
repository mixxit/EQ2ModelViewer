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

#endregion

namespace Everquest2.Visualization
{
    public class VeXformNode : VeNode
    {
        public VeXformNode()
        {
        }


        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeXformNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeXformNode)];

            if (classVersion == 0)
            {
                orientation[0] = reader.ReadSingle();
                orientation[1] = reader.ReadSingle();
                orientation[2] = reader.ReadSingle();
                //3x3 rotation matrix
                reader.ReadBytes(sizeof(float) * (3 * 3));
                scale = reader.ReadSingle();
                position[0] = reader.ReadSingle();
                position[1] = reader.ReadSingle();
                position[2] = reader.ReadSingle();
            }
            else
            {
                position[0]    = reader.ReadSingle();
                position[1]    = reader.ReadSingle();
                position[2]    = reader.ReadSingle();
                orientation[0] = reader.ReadSingle();
                orientation[1] = reader.ReadSingle();
                orientation[2] = reader.ReadSingle();
                scale          = reader.ReadSingle();
            }
        }


        public float[] position    = new float[3];
        public float[] orientation = new float[3];
        public float   scale;
    }
}
