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
    public class VeSkeleton : VeBase
    {
        public struct BoneTransform
        {
            public float[] TranslationVector;
            public float[] RotationQuaternion;
            public float[] ScalingMatrix;
        }


        public VeSkeleton()
        {
        }

        /// <summary>
        /// Special constructor used when deserializing the instance of the class.
        /// </summary>
        /// <param name="reader">Reader used to read the instance data.</param>
        protected VeSkeleton(Util.Eq2Reader reader, Util.StreamingContext context) : base(reader, context)
        {
            byte classVersion = context.ClassVersions[typeof(VeSkeleton)];

            Debug.Assert(classVersion >= 4 && classVersion <= 6, "VeSkeleton class version " + classVersion + " not supported");

            uint boneNameCount = reader.ReadUInt32();
            boneNames = new string[boneNameCount];
            for (uint i = 0; i < boneNameCount; ++i) boneNames[i] = reader.ReadString(2);

            uint boneParentIndexCount = reader.ReadUInt32();
            boneParentIndices = reader.ReadBytes((int)boneParentIndexCount);

            uint boneRelativeTransformCount = reader.ReadUInt32();
            boneRelativeTransforms = new BoneTransform[boneRelativeTransformCount];

            for (uint i = 0; i < boneRelativeTransformCount; ++i)
            {
                boneRelativeTransforms[i].TranslationVector  = new float[4];
                boneRelativeTransforms[i].RotationQuaternion = new float[4];
                boneRelativeTransforms[i].ScalingMatrix      = new float[9];

                switch (classVersion)
                {
                case 4:
                    // 4-component translation vector, rotation quaternion, 3x3 scaling matrix
                    ReadVector(reader, boneRelativeTransforms[i].TranslationVector, 4);
                    ReadVector(reader, boneRelativeTransforms[i].RotationQuaternion, 4);
                    ReadVector(reader, boneRelativeTransforms[i].ScalingMatrix, 9);
                    break;
                case 5:
                    // 3-component translation vector, rotation quaternion, 3x3 scaling matrix
                    ReadVector(reader, boneRelativeTransforms[i].TranslationVector, 3);
                    boneRelativeTransforms[i].TranslationVector[3] = 0;
                    ReadVector(reader, boneRelativeTransforms[i].RotationQuaternion, 4);
                    ReadVector(reader, boneRelativeTransforms[i].ScalingMatrix, 9);
                    break;
                default:
                    // 3-component translation vector, rotation quaternion, uniform scaling factor
                    ReadVector(reader, boneRelativeTransforms[i].TranslationVector, 3);
                    boneRelativeTransforms[i].TranslationVector[3] = 0;
                    ReadVector(reader, boneRelativeTransforms[i].RotationQuaternion, 4);

                    float scale = reader.ReadSingle();
                    boneRelativeTransforms[i].ScalingMatrix = new float[9] 
                                                                  { 
                                                                      scale, 0, 0,
                                                                      0, scale, 0,
                                                                      0, 0, scale
                                                                  };
                    break;
                }
            }

            uint inverseSkeletonRelativeTransformCount = reader.ReadUInt32();
            inverseSkeletonRelativeTransforms = new float[inverseSkeletonRelativeTransformCount, 16];
            for (uint i = 0; i < inverseSkeletonRelativeTransformCount; ++i)
            {
                for (uint j = 0; j < 16; ++j) inverseSkeletonRelativeTransforms[i, j] = reader.ReadSingle();
            }
        }


        private void ReadVector(Util.Eq2Reader reader, float[] vector, int count)
        {
            for (int i = 0; i < count; ++i) vector[i] = reader.ReadSingle();
        }


        public string[]        boneNames;
        public byte[]          boneParentIndices;
        public BoneTransform[] boneRelativeTransforms;
        public float[,]        inverseSkeletonRelativeTransforms;
    }
}
