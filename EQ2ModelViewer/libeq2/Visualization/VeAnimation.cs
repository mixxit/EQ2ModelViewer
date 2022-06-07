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
using System.Diagnostics;
using Everquest2.Util;

#endregion

namespace Everquest2.Visualization
{
    public class VeAnimation : VeBase
    {
        public struct TrackInfo<T>
        {
            public T Translation;
            public T Rotation;
            public T Scaling;
        }


        public VeAnimation()
        {
        }

    
        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeAnimation(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeAnimation)];
            Debug.Assert(classVersion >= 2 && classVersion <= 4, "VeAnimation class version " + classVersion + " not supported");

            uint trackCount = reader.ReadUInt32();

            boneNames    = new string[trackCount];
            curveDegrees = new TrackInfo<uint>[trackCount];
            curveKnots   = new TrackInfo<float[]>[trackCount];
            curveKeys    = new TrackInfo<float[][]>[trackCount];
            for (uint i = 0; i < trackCount; ++i)
            {
                boneNames[i] = reader.ReadString(2);

                ReadTrackInfo(reader, ref curveDegrees[i].Translation, ref curveKnots[i].Translation, ref curveKeys[i].Translation);
                ReadTrackInfo(reader, ref curveDegrees[i].Rotation,    ref curveKnots[i].Rotation,    ref curveKeys[i].Rotation);
                ReadTrackInfo(reader, ref curveDegrees[i].Scaling,     ref curveKnots[i].Scaling,     ref curveKeys[i].Scaling);
            }

            length = reader.ReadSingle();

            if (classVersion > 2) skeletonName = reader.ReadString(2);
        }


        private void ReadTrackInfo(Util.Eq2Reader reader, ref uint degree, ref float[] knots, ref float[][] keys)
        {
            degree = reader.ReadUInt32();

            uint curveKnotCount = reader.ReadUInt32();
            knots = new float[curveKnotCount];
            for (uint i = 0; i < curveKnotCount; ++i) knots[i] = reader.ReadSingle();

            uint curveKeyCount  = reader.ReadUInt32();
            uint componentCount = curveKeyCount / curveKnotCount;
            keys = new float[curveKnotCount][];
            for (uint i = 0; i < curveKnotCount; ++i)
            {
                keys[i] = new float[componentCount];
                for (uint j = 0; j < componentCount; ++j) keys[i][j] = reader.ReadSingle();
            }
        }


        public string[]               boneNames;
        public TrackInfo<uint>[]      curveDegrees;
        public TrackInfo<float[]>[]   curveKnots;
        public TrackInfo<float[][]>[] curveKeys;
        public float                  length;
        public string                 skeletonName;
    }
}
