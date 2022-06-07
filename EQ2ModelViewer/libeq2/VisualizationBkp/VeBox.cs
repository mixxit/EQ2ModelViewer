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
    public class VeBox : VeVolume
    {
        public VeBox()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeBox(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeBox)];

            minPoint[0] = reader.ReadSingle();
            minPoint[1] = reader.ReadSingle();
            minPoint[2] = reader.ReadSingle();
            maxPoint[0] = reader.ReadSingle();
            maxPoint[1] = reader.ReadSingle();
            maxPoint[2] = reader.ReadSingle();
            if (classVersion == 0)
            {
                unk2[0] = reader.ReadSingle();
                unk2[1] = reader.ReadSingle();
                unk2[2] = reader.ReadSingle();
            }
        }


        public float[] minPoint = new float[3];
        public float[] maxPoint = new float[3];
        public float[] unk2     = new float[3];
    }
}
