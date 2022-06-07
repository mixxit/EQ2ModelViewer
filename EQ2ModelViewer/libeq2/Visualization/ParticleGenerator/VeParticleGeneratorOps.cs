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

#endregion

namespace Everquest2.Visualization.ParticleGenerator {
    public abstract class VeParticleGeneratorOp {
        protected string ReadParameterName(Util.Eq2Reader reader) {
            string name = null;

            bool isImmediateParameter = reader.ReadBoolean();
            if (!isImmediateParameter) {
                name = reader.ReadString(2);
            }

            return name;
        }


        protected void ReadUnaryScalarOp(Util.Eq2Reader reader,
                                         ref string destination,
                                         ref float value,
                                         ref string valueVariable) {
            // Read immediate parameters
            value = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            valueVariable = ReadParameterName(reader);
        }


        protected void ReadUnaryScalarOp(Util.Eq2Reader reader,
                                         ref string destination,
                                         ref uint value,
                                         ref string valueVariable) {
            // Read immediate parameters
            value = reader.ReadUInt32();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            valueVariable = ReadParameterName(reader);
        }


        protected void ReadBinaryScalarOp(Util.Eq2Reader reader,
                                          ref string destination,
                                          ref float arg0,
                                          ref float arg1,
                                          ref string arg0Variable,
                                          ref string arg1Variable) {
            // Read immediate parameters
            arg0 = reader.ReadSingle();
            arg1 = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            arg0Variable = ReadParameterName(reader);
            arg1Variable = ReadParameterName(reader);
        }


        protected void ReadUnaryVectorOp(Util.Eq2Reader reader,
                                         ref string destination,
                                         ref float[] value,
                                         ref string valueVariable) {
            #region Preconditions
            Debug.Assert(value.Length == 3);
            #endregion

            // Read immediate parameters
            value[0] = reader.ReadSingle();
            value[1] = reader.ReadSingle();
            value[2] = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            valueVariable = ReadParameterName(reader);
        }


        protected void ReadBinaryVectorOp(Util.Eq2Reader reader,
                                          ref string destination,
                                          ref float[] arg0,
                                          ref float[] arg1,
                                          ref string arg0Variable,
                                          ref string arg1Variable) {
            #region Preconditions
            Debug.Assert(arg0.Length == 3 && arg1.Length == 3);
            #endregion

            // Read immediate parameters
            arg0[0] = reader.ReadSingle();
            arg0[1] = reader.ReadSingle();
            arg0[2] = reader.ReadSingle();
            arg1[0] = reader.ReadSingle();
            arg1[1] = reader.ReadSingle();
            arg1[2] = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            arg0Variable = ReadParameterName(reader);
            arg1Variable = ReadParameterName(reader);
        }
    }


    // RAND <dest> <min> <max>
    public class VeParticleGeneratorRandOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorRandOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref min, ref max, ref minVariable, ref maxVariable);
        }

        public float min;
        public float max;
        public string destination;
        public string minVariable;
        public string maxVariable;
    }


    // RANDPITCH <dest> <min> <max>
    public class VeParticleGeneratorRandPitchOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorRandPitchOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref min, ref max, ref minVariable, ref maxVariable);
        }

        public float min;
        public float max;
        public string destination;
        public string minVariable;
        public string maxVariable;
    }


    // ADD <dest> <arg0> <arg1>
    public class VeParticleGeneratorAddOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorAddOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // SUB <dest> <arg0> <arg1>
    public class VeParticleGeneratorSubOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSubOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // MUL <dest> <arg0> <arg1>
    public class VeParticleGeneratorMulOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorMulOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // DIV <dest> <arg0> <arg1>
    public class VeParticleGeneratorDivOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorDivOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // MOD <dest> <arg0> <arg1>
    public class VeParticleGeneratorModOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorModOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // MIN <dest> <arg0> <arg1>
    public class VeParticleGeneratorMinOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorMinOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // MAX <dest> <arg0> <arg1>
    public class VeParticleGeneratorMaxOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorMaxOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // EQUAL <dest> <arg0> <arg1>
    public class VeParticleGeneratorEqualOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorEqualOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // NOTEQUAL <dest> <arg0> <arg1>
    public class VeParticleGeneratorNotEqualOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorNotEqualOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // LESS <dest> <arg0> <arg1>
    public class VeParticleGeneratorLessOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorLessOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // GREATER <dest> <arg0> <arg1>
    public class VeParticleGeneratorGreaterOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorGreaterOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryScalarOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float arg0;
        public float arg1;
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // SETRANDSEED <dest> <value>
    public class VeParticleGeneratorSetRandSeedOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSetRandSeedOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            seed = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
        }

        public float seed;
        public string destination;
    }


    // SET <dest> <value>
    public class VeParticleGeneratorSetOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSetOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // FLOOR <dest> <value>
    public class VeParticleGeneratorFloorOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorFloorOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // ROUND <dest> <value>
    public class VeParticleGeneratorRoundOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorRoundOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // NOT <dest> <value>
    public class VeParticleGeneratorNotOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorNotOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }

    // ABS <dest> <value>
    public class VeParticleGeneratorAbsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorAbsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // SIGN <dest> <value>
    public class VeParticleGeneratorSignOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSignOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // SQRT <dest> <value>
    public class VeParticleGeneratorSqrtOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSqrtOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // SIN <dest> <value>
    public class VeParticleGeneratorSinOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSinOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // COS <dest> <value>
    public class VeParticleGeneratorCosOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorCosOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }


    // SCALELS2WS <dest> <local space scale>
    public class VeParticleGeneratorScaleLs2WsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorScaleLs2WsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref localSpaceScale, ref localSpaceScaleVariable);
        }

        public float localSpaceScale;
        public string destination;
        public string localSpaceScaleVariable;
    }


    // GETSUBMESH <dest> <vertex>
    public class VeParticleGeneratorGetSubMeshOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorGetSubMeshOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref vertex, ref vertexVariable);
        }

        public uint vertex;
        public string destination;
        public string vertexVariable;
    }


    // CIRCLE <dest0> <dest1> <radius0> <radius1> <heading>
    public class VeParticleGeneratorCircleOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorCircleOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            radius0 = reader.ReadSingle();
            radius1 = reader.ReadSingle();
            heading = reader.ReadSingle();
            // Read destination variables
            destination0 = ReadParameterName(reader);
            destination1 = ReadParameterName(reader);
            // Read source variables
            radius0Variable = ReadParameterName(reader);
            radius1Variable = ReadParameterName(reader);
            headingVariable = ReadParameterName(reader);
        }

        public float radius0;
        public float radius1;
        public float heading;
        public string destination0;
        public string destination1;
        public string radius0Variable;
        public string radius1Variable;
        public string headingVariable;
    }


    // SPHERE <dest0> <dest1> <dest2> <radius0> <radius1> <radius2> <heading> <pitch>
    public class VeParticleGeneratorSphereOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSphereOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            radius0 = reader.ReadSingle();
            radius1 = reader.ReadSingle();
            radius2 = reader.ReadSingle();
            heading = reader.ReadSingle();
            pitch = reader.ReadSingle();
            // Read destination variables
            destination0 = ReadParameterName(reader);
            destination1 = ReadParameterName(reader);
            destination2 = ReadParameterName(reader);
            // Read source variables
            radius0Variable = ReadParameterName(reader);
            radius1Variable = ReadParameterName(reader);
            radius2Variable = ReadParameterName(reader);
            headingVariable = ReadParameterName(reader);
            pitchVariable = ReadParameterName(reader);
        }

        public float radius0;
        public float radius1;
        public float radius2;
        public float heading;
        public float pitch;
        public string destination0;
        public string destination1;
        public string destination2;
        public string radius0Variable;
        public string radius1Variable;
        public string radius2Variable;
        public string headingVariable;
        public string pitchVariable;
    }


    // CONV2WS
    public class VeParticleGeneratorConv2WsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorConv2WsOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // RENDERTARGETVOC
    public class VeParticleGeneratorRenderTargetVocOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorRenderTargetVocOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // DISABLETARGETVOC
    public class VeParticleGeneratorDisableTargetVocOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorDisableTargetVocOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // ENABLETARGETVOC
    public class VeParticleGeneratorEnableTargetVocOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorEnableTargetVocOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }

    // DISABLETARGETVOC_FORCELIGHTING
    public class VeParticleGeneratorDisableTargetVoc_ForceLightingOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorDisableTargetVoc_ForceLightingOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }

    // ENABLETARGETVOC_FORCELIGHTING
    public class VeParticleGeneratorEnableTargetVoc_ForceLightingOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorEnableTargetVoc_ForceLightingOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // IF <conditionalVariable> <unknown>
    public class VeParticleGeneratorIfOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorIfOp(Util.Eq2Reader reader, byte classVersion) {
            // Read condition variable
            condition = ReadParameterName(reader);
            // Read unknown
            unknown = reader.ReadUInt32();
        }

        public string condition;
        public uint unknown;
    }


    // IFNOT <conditionalVariable> <unknown>
    public class VeParticleGeneratorIfNotOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorIfNotOp(Util.Eq2Reader reader, byte classVersion) {
            // Read condition variable
            condition = ReadParameterName(reader);
            // Read unknown
            unknown = reader.ReadUInt32();
        }

        public string condition;
        public uint unknown;
    }


    // ENDIF
    public class VeParticleGeneratorEndIfOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorEndIfOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // PAUSE
    public class VeParticleGeneratorPauseOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorPauseOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }


    // GRAPH <dest> <source> <unk0> <minX> <maxX> <minY> <maxY> <count> [ ... ]
    public class VeParticleGeneratorGraphOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorGraphOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);
            // Read unknown values
            unk0 = reader.ReadSingle();
            minX = reader.ReadSingle();
            maxX = reader.ReadSingle();
            minY = reader.ReadSingle();
            maxY = reader.ReadSingle();

            uint count = reader.ReadUInt32();

            unk1 = new float[count, 2];
            for (uint i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
            }
        }

        public string destination;
        public string source;
        public float unk0;
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public float[,] unk1;
    }


    // VGRAPH <dest> <source> <unk0> <unk1> <unk2> <unk3> <unk4> <count> [ ... ]
    public class VeParticleGeneratorVGraphOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGraphOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);

            unk0 = reader.ReadSingle();

            min[0] = reader.ReadSingle();
            min[1] = reader.ReadSingle();
            min[2] = reader.ReadSingle();
            max[0] = reader.ReadSingle();
            max[1] = reader.ReadSingle();
            max[2] = reader.ReadSingle();

            uint count = reader.ReadUInt32();

            unk1 = new float[count, 4];
            for (uint i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
                unk1[i, 2] = reader.ReadSingle();
                unk1[i, 3] = reader.ReadSingle();
            }
        }

        public string destination;
        public string source;
        public float unk0;
        public float[] min = new float[3];
        public float[] max = new float[3];
        public float[,] unk1;
    }


    // VGRAPHSMOOTH <dest> <source> <unk0> <unk1> <unk2> <unk3> <unk4> <count> [ ... ]
    public class VeParticleGeneratorVGraphSmoothOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGraphSmoothOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);

            unk0 = reader.ReadSingle();

            min[0] = reader.ReadSingle();
            min[1] = reader.ReadSingle();
            min[2] = reader.ReadSingle();
            max[0] = reader.ReadSingle();
            max[1] = reader.ReadSingle();
            max[2] = reader.ReadSingle();

            uint count = reader.ReadUInt32();

            unk1 = new float[count, 4];
            for (uint i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
                unk1[i, 2] = reader.ReadSingle();
                unk1[i, 3] = reader.ReadSingle();
            }
        }

        public string destination;
        public string source;
        public float unk0;
        public float[] min = new float[3];
        public float[] max = new float[3];
        public float[,] unk1;
    }


    // VGRAPHSMOOTHEXT <dest> <source> <unk0> <unk1> <unk2> <unk3> <unk4> <count> [ ... ]
    public class VeParticleGeneratorVGraphSmoothExtOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGraphSmoothExtOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);

            unk0 = reader.ReadSingle();

            min[0] = reader.ReadSingle();
            min[1] = reader.ReadSingle();
            min[2] = reader.ReadSingle();
            max[0] = reader.ReadSingle();
            max[1] = reader.ReadSingle();
            max[2] = reader.ReadSingle();

            uint count = reader.ReadUInt32();

            unk1 = new float[count, 4];
            for (uint i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
                unk1[i, 2] = reader.ReadSingle();
                unk1[i, 3] = reader.ReadSingle();
            }
        }

        public string destination;
        public string source;
        public float unk0;
        public float[] min = new float[3];
        public float[] max = new float[3];
        public float[,] unk1;
    }


    // VGRAPHSMOOTHBONES <dest> <source> <unk0> <unk1> <unk2> <unk3> <unk4> <count> [ ... ]
    public class VeParticleGeneratorVGraphSmoothBonesOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGraphSmoothBonesOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);

            unk0 = reader.ReadSingle();

            min[0] = reader.ReadSingle();
            min[1] = reader.ReadSingle();
            min[2] = reader.ReadSingle();
            max[0] = reader.ReadSingle();
            max[1] = reader.ReadSingle();
            max[2] = reader.ReadSingle();

            uint count = reader.ReadUInt32();

            unk1 = new float[count, 4];
            for (uint i = 0; i < count; ++i) {
                unk1[i, 0] = reader.ReadSingle();
                unk1[i, 1] = reader.ReadSingle();
                unk1[i, 2] = reader.ReadSingle();
                unk1[i, 3] = reader.ReadSingle();
            }
        }

        public string destination;
        public string source;
        public float unk0;
        public float[] min = new float[3];
        public float[] max = new float[3];
        public float[,] unk1;
    }


    // OUTPUTGRAPH <dest> <source> <unk0> <minX> <maxX> <minY> <maxY> <count> [ ... ]
    public class VeParticleGeneratorOutputGraphOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorOutputGraphOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);
        }

        public string destination;
        public string source;
    }


    // VCAMSHAKE <center> <min radius, max radius, intensity>
    public class VeParticleGeneratorVCamShakeOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVCamShakeOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            center[0] = reader.ReadSingle();
            center[1] = reader.ReadSingle();
            center[2] = reader.ReadSingle();
            args[0] = reader.ReadSingle();
            args[1] = reader.ReadSingle();
            args[2] = reader.ReadSingle();
            // Read source variables
            centerVariable = ReadParameterName(reader);
            argsVariable = ReadParameterName(reader);
        }

        public float[] center = new float[3];
        public float[] args = new float[3];
        public string centerVariable;
        public string argsVariable;
    }


    // VRAND <dest> <min> <max>
    public class VeParticleGeneratorVRandOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVRandOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref min, ref max, ref minVariable, ref maxVariable);
        }

        public float[] min = new float[3];
        public float[] max = new float[3];
        public string destination;
        public string minVariable;
        public string maxVariable;
    }


    // VADD <dest> <arg0> <arg1>
    public class VeParticleGeneratorVAddOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVAddOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VSUB <dest> <arg0> <arg1>
    public class VeParticleGeneratorVSubOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVSubOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VMUL <dest> <arg0> <arg1>
    public class VeParticleGeneratorVMulOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVMulOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VDIV <dest> <arg0> <arg1>
    public class VeParticleGeneratorVDivOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDivOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VMOD <dest> <arg0> <arg1>
    public class VeParticleGeneratorVModOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVModOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VDIR <dest> <start> <end>
    public class VeParticleGeneratorVDirOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDirOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref start, ref end, ref startVariable, ref endVariable);
        }

        public float[] start = new float[3];
        public float[] end = new float[3];
        public string destination;
        public string startVariable;
        public string endVariable;
    }


    // VCROSS <dest> <arg0> <arg1>
    public class VeParticleGeneratorVCrossOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVCrossOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VRATE <dest> <start> <end>
    public class VeParticleGeneratorVRateOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVRateOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref start, ref end, ref startVariable, ref endVariable);
        }

        public float[] start = new float[3];
        public float[] end = new float[3];
        public string destination;
        public string startVariable;
        public string endVariable;
    }


    // VDIR2HPRU <dest> <forward> <up>
    public class VeParticleGeneratorVDir2HpruOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDir2HpruOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref forward, ref up, ref forwardVariable, ref upVariable);
        }

        public float[] forward = new float[3];
        public float[] up = new float[3];
        public string destination;
        public string forwardVariable;
        public string upVariable;
    }


    // VDIR2HPRF <dest> <forward> <up>
    public class VeParticleGeneratorVDir2HprfOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDir2HprfOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref forward, ref up, ref forwardVariable, ref upVariable);
        }

        public float[] forward = new float[3];
        public float[] up = new float[3];
        public string destination;
        public string forwardVariable;
        public string upVariable;
    }


    // VCONSTRAINELLIPSOID <dest> <offset> <radius vector>
    public class VeParticleGeneratorVConstrainEllipsoidOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVConstrainEllipsoidOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref offset, ref radius, ref offsetVariable, ref radiusVariable);
        }

        public float[] offset = new float[3];
        public float[] radius = new float[3];
        public string destination;
        public string offsetVariable;
        public string radiusVariable;
    }


    // VCONSTRAINELLIPSOIDAXIS <dest> <offset> <radius vector> <axis vector>
    public class VeParticleGeneratorVConstrainEllipsoidAxisOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVConstrainEllipsoidAxisOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            offset[0] = reader.ReadSingle();
            offset[1] = reader.ReadSingle();
            offset[2] = reader.ReadSingle();
            radius[0] = reader.ReadSingle();
            radius[1] = reader.ReadSingle();
            radius[2] = reader.ReadSingle();
            axis[0] = reader.ReadSingle();
            axis[1] = reader.ReadSingle();
            axis[2] = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            offsetVariable = ReadParameterName(reader);
            radiusVariable = ReadParameterName(reader);
            axisVariable = ReadParameterName(reader);
        }

        public float[] offset = new float[3];
        public float[] radius = new float[3];
        public float[] axis = new float[3];
        public string destination;
        public string offsetVariable;
        public string radiusVariable;
        public string axisVariable;
    }


    // VSET <dest> <value>
    public class VeParticleGeneratorVSetOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVSetOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VFLOOR <dest> <value>
    public class VeParticleGeneratorVFloorOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVFloorOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VROUND <dest> <value>
    public class VeParticleGeneratorVRoundOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVRoundOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VINTEGRATE <dest> <value>
    public class VeParticleGeneratorVIntegrateOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVIntegrateOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VNORMALIZE <dest> <value>
    public class VeParticleGeneratorVNormalizeOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVNormalizeOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VDIR2HPR <dest> <value>
    public class VeParticleGeneratorVDir2HprOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDir2HprOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VHPR2DIR <dest> <value>
    public class VeParticleGeneratorVHpr2DirOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVHpr2DirOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VPOSLS2WS <dest> <value>
    public class VeParticleGeneratorVPosLs2WsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVPosLs2WsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VHPRLS2WS <dest> <value>
    public class VeParticleGeneratorVHprLs2WsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVHprLs2WsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VDIRLS2WS <dest> <value>
    public class VeParticleGeneratorVDirLs2WsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDirLs2WsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VDIRWS2LS <dest> <value>
    public class VeParticleGeneratorVDirWs2LsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDirWs2LsOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VMAG <dest> <value>
    public class VeParticleGeneratorVMagOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVMagOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryVectorOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float[] value = new float[3];
        public string destination;
        public string valueVariable;
    }


    // VDOT <dest> <arg0> <arg1>
    public class VeParticleGeneratorVDotOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVDotOp(Util.Eq2Reader reader, byte classVersion) {
            ReadBinaryVectorOp(reader, ref destination, ref arg0, ref arg1, ref arg0Variable, ref arg1Variable);
        }

        public float[] arg0 = new float[3];
        public float[] arg1 = new float[3];
        public string destination;
        public string arg0Variable;
        public string arg1Variable;
    }


    // VSCALE <dest> <vector> <scale>
    public class VeParticleGeneratorVScaleOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVScaleOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            vector[0] = reader.ReadSingle();
            vector[1] = reader.ReadSingle();
            vector[2] = reader.ReadSingle();
            scale = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            vectorVariable = ReadParameterName(reader);
            scaleVariable = ReadParameterName(reader);
        }

        public float[] vector = new float[3];
        public float scale;
        public string destination;
        public string vectorVariable;
        public string scaleVariable;
    }


    // VCLAMP <dest> <vector> <minMagnitude> <maxMagnitude>
    public class VeParticleGeneratorVClampOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVClampOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            vector[0] = reader.ReadSingle();
            vector[1] = reader.ReadSingle();
            vector[2] = reader.ReadSingle();
            minMagnitude = reader.ReadSingle();
            maxMagnitude = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            vectorVariable = ReadParameterName(reader);
            minMagnitudeVariable = ReadParameterName(reader);
            maxMagnitudeVariable = ReadParameterName(reader);
        }

        public float[] vector = new float[3];
        public float minMagnitude;
        public float maxMagnitude;
        public string destination;
        public string vectorVariable;
        public string minMagnitudeVariable;
        public string maxMagnitudeVariable;
    }


    // VPATH <dest> <startPosition> <endPosition> <startControlPoint> <endControlPoint> <normalizedTime>
    public class VeParticleGeneratorVPathOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVPathOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            startPosition[0] = reader.ReadSingle();
            startPosition[1] = reader.ReadSingle();
            startPosition[2] = reader.ReadSingle();
            endPosition[0] = reader.ReadSingle();
            endPosition[1] = reader.ReadSingle();
            endPosition[2] = reader.ReadSingle();
            startControlPoint[0] = reader.ReadSingle();
            startControlPoint[1] = reader.ReadSingle();
            startControlPoint[2] = reader.ReadSingle();
            endControlPoint[0] = reader.ReadSingle();
            endControlPoint[1] = reader.ReadSingle();
            endControlPoint[2] = reader.ReadSingle();
            normalizedTime = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            startPositionVariable = ReadParameterName(reader);
            endPositionVariable = ReadParameterName(reader);
            startControlPointVariable = ReadParameterName(reader);
            endControlPointVariable = ReadParameterName(reader);
            normalizedTimeVariable = ReadParameterName(reader);
        }

        public float[] startPosition = new float[3];
        public float[] endPosition = new float[3];
        public float[] startControlPoint = new float[3];
        public float[] endControlPoint = new float[3];
        public float normalizedTime;
        public string destination;
        public string startPositionVariable;
        public string endPositionVariable;
        public string startControlPointVariable;
        public string endControlPointVariable;
        public string normalizedTimeVariable;
    }


    // VISECTCAPSULE <dest> <startPoint> <endPoint> <center0> <center1> <radius>
    public class VeParticleGeneratorVIsectCapsuleOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVIsectCapsuleOp(Util.Eq2Reader reader, byte classVersion) {
            // Read immediate parameters
            startPoint[0] = reader.ReadSingle();
            startPoint[1] = reader.ReadSingle();
            startPoint[2] = reader.ReadSingle();
            endPoint[0] = reader.ReadSingle();
            endPoint[1] = reader.ReadSingle();
            endPoint[2] = reader.ReadSingle();
            center0[0] = reader.ReadSingle();
            center0[1] = reader.ReadSingle();
            center0[2] = reader.ReadSingle();
            center1[0] = reader.ReadSingle();
            center1[1] = reader.ReadSingle();
            center1[2] = reader.ReadSingle();
            radius = reader.ReadSingle();
            // Read destination variables
            destination = ReadParameterName(reader);
            // Read source variables
            startPointVariable = ReadParameterName(reader);
            endPointVariable = ReadParameterName(reader);
            center0Variable = ReadParameterName(reader);
            center1Variable = ReadParameterName(reader);
            radiusVariable = ReadParameterName(reader);
        }

        public float[] startPoint = new float[3];
        public float[] endPoint = new float[3];
        public float[] center0 = new float[3];
        public float[] center1 = new float[3];
        public float radius;
        public string destination;
        public string startPointVariable;
        public string endPointVariable;
        public string center0Variable;
        public string center1Variable;
        public string radiusVariable;
    }


    // VOCNAME <filename>
    public class VeParticleGeneratorVocNameOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVocNameOp(Util.Eq2Reader reader, byte classVersion) {
            filename = reader.ReadString(2);
        }

        public string filename;
    }


    // FORCESHADER <filename>
    public class VeParticleGeneratorForceShaderOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorForceShaderOp(Util.Eq2Reader reader, byte classVersion) {
            filename = reader.ReadString(2);
        }

        public string filename;
    }


    // CALLOPS <filename>
    public class VeParticleGeneratorCallOpsOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorCallOpsOp(Util.Eq2Reader reader, byte classVersion) {
            filename = reader.ReadString(2);
        }

        public string filename;
    }


    // BONEPOS <destination> <boneName>
    public class VeParticleGeneratorBonePosOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorBonePosOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            boneName = reader.ReadString(2);
        }

        public string destination;
        public string boneName;
    }


    // WSBONEPOS <destination> <boneName>
    public class VeParticleGeneratorWsBonePosOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorWsBonePosOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            boneName = reader.ReadString(2);
        }

        public string destination;
        public string boneName;
    }


    // SRCBONEPOS <destination> <boneName>
    public class VeParticleGeneratorSrcBonePosOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSrcBonePosOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            boneName = reader.ReadString(2);
        }

        public string destination;
        public string boneName;
    }


    // WSSRCBONEPOS <destination> <boneName>
    public class VeParticleGeneratorWsSrcBonePosOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorWsSrcBonePosOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            boneName = reader.ReadString(2);
        }

        public string destination;
        public string boneName;
    }


    // GETVAR <destination> <variableName>
    public class VeParticleGeneratorGetVarOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorGetVarOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            variableName = reader.ReadString(2);
        }

        public string destination;
        public string variableName;
    }


    // VGETVAR <destination> <variableName>
    public class VeParticleGeneratorVGetVarOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGetVarOp(Util.Eq2Reader reader, byte classVersion) {
            destination = ReadParameterName(reader);
            variableName = reader.ReadString(2);
        }

        public string destination;
        public string variableName;
    }


    // SETVAR <destination> <value>
    public class VeParticleGeneratorSetVarOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorSetVarOp(Util.Eq2Reader reader, byte classVersion) {
            variableName = reader.ReadString(2);
            value = reader.ReadSingle();
            valueVariable = ReadParameterName(reader);
        }

        public string variableName;
        public float value;
        public string valueVariable;
    }


    // VSETVAR <destination> <value>
    public class VeParticleGeneratorVSetVarOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVSetVarOp(Util.Eq2Reader reader, byte classVersion) {
            variableName = reader.ReadString(2);
            value[0] = reader.ReadSingle();
            value[1] = reader.ReadSingle();
            value[2] = reader.ReadSingle();
            valueVariable = ReadParameterName(reader);
        }

        public string variableName;
        public float[] value = new float[3];
        public string valueVariable;
    }


    // COMMENT <comment>
    public class VeParticleGeneratorCommentOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorCommentOp(Util.Eq2Reader reader, byte classVersion) {
            comment = reader.ReadString(2);
        }

        public string comment;
    }

    public class VeParticleGeneratorNullOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorNullOp(Util.Eq2Reader reader, byte classVersion) {
        }
    }

    public class VeParticleGeneratorIntegrateOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorIntegrateOp(Util.Eq2Reader reader, byte classVersion) {
            ReadUnaryScalarOp(reader, ref destination, ref value, ref valueVariable);
        }

        public float value;
        public string destination;
        public string valueVariable;
    }

    public class VeParticleGeneratorVGraphTimeColorOp : VeParticleGeneratorOp {
        protected VeParticleGeneratorVGraphTimeColorOp(Util.Eq2Reader reader, byte classVersion) {
            // Read source and destination names
            destination = ReadParameterName(reader);
            source = ReadParameterName(reader);

            // Read unknown values
            unk0 = new float[7];
            for (int i = 0; i < 7; i++) {
                unk0[i] = reader.ReadSingle();
            }

            uint count = reader.ReadUInt32();
            unk1 = new float[count, 4];
            for (uint i = 0; i < count; ++i) {
                for (int j = 0; j < 4; j++) {
                    unk1[i, j] = reader.ReadSingle();
                }
            }

            unk2 = reader.ReadByte();
        }

        public string destination;
        public string source;
        public float[] unk0;
        public float[,] unk1;
        public byte unk2;
    }
}

/* EOF */
