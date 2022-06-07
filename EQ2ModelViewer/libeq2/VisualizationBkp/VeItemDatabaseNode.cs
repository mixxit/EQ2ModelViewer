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
    public class VeItemDatabaseNode : VeNode
    {
        public VeItemDatabaseNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeItemDatabaseNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeItemDatabaseNode)];
            
            unk0[0] = reader.ReadSingle();
            unk0[1] = reader.ReadSingle();
            unk0[2] = reader.ReadSingle();
            unk1[0] = reader.ReadSingle();
            unk1[1] = reader.ReadSingle();
            unk1[2] = reader.ReadSingle();
            unk2    = reader.ReadSingle();
        }


        public float[] unk0 = new float[3];
        public float[] unk1 = new float[3];
        public float   unk2;
    }
}
