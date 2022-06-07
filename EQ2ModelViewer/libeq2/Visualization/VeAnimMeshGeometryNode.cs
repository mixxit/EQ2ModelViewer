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
using System.Diagnostics;
using System.IO;
using Everquest2.Util;

#endregion

namespace Everquest2.Visualization
{
    public class VeAnimMeshGeometryNode : VeMeshGeometryNode
    {
        public VeAnimMeshGeometryNode()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeAnimMeshGeometryNode(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {            
            byte classVersion = context.ClassVersions[typeof(VeAnimMeshGeometryNode)];

            Debug.Assert(classVersion >= 5 && classVersion <= 7, "VeAnimMeshGeometryNode version " + classVersion + " not supported");

            skeletonName = reader.ReadString(2);

            if (classVersion != 6) defaultAnimationName = reader.ReadString(2);
        }


        public string skeletonName;
        public string defaultAnimationName;
    }
}
