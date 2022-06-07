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
using System.Reflection;
using System.Collections.Generic;
using Everquest2.Visualization;
using Everquest2.Visualization.ParticleGenerator;

#endregion

namespace Everquest2.Util
{
    public class Eq2Reader : BinaryReader
    {
        #region Methods
        public Eq2Reader(Stream stream) : base(stream, System.Text.Encoding.ASCII)
        {
        }

        #region Disposable semantics
        public void Dispose()
        {
            base.Dispose(true);
        }

        ~Eq2Reader()
        {
            base.Dispose(false);
        }
        #endregion

        /// <summary>
        /// Reads an Everquest 2 object from the stream.
        /// </summary>
        /// <remarks>
        /// If the object is a node object, none of its children (if any) are read.
        /// To read a node object and all its children use <see cref="ReadNodeObject()"/>.
        /// </remarks>
        /// <exception cref="DeserializationException">Error encountered while deserializing the object.</exception>
        /// <returns>Deserialized object.</returns>
        public virtual VeBase ReadObject()
        {
            long startPos = BaseStream.Position;

            // Read class name
            string className = ReadString();

            if (className.Length < 1)
                return null;

            ConstructorInfo constructor = null;

            // Lookup class in cache
            if (classCache.ContainsKey(className))
            {
                constructor = classCache[className];
            }
            else
            {
                // Image(2020): ClassNames no longer include "Ve"
                    if (!className.StartsWith("Ve"))
                className = "Ve" + className;

                // Find class in current assembly
                Type classType = GetType().Assembly.GetType("Everquest2.Visualization." + className, false);

                string filename = null;
                if (typeof(FileStream).IsInstanceOfType(BaseStream))
                {
                    filename = (BaseStream as System.IO.FileStream).Name;
                }

                //Debug.Assert(classType != null, "Invalid class name!", "Error getting class type at index {0}{1}",
                //    BaseStream.Position,
                //    filename != null ? "\n in file " + filename : "");

                // Find deserializing constructor
                constructor = classType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                                                       null, 
                                                       new Type[] { typeof(Eq2Reader), typeof(StreamingContext) }, 
                                                       null);
                Debug.Assert(constructor != null, "Deserializing constructor not found on class " + classType.Name);

                classCache[className] = constructor;
            }

            // Create streaming context
            StreamingContext context = new StreamingContext();

            // Read parent chain count
            byte parentChainCount     = ReadByte();
            byte realParentChainCount = 0;
            
            Type type = constructor.DeclaringType;
            while (type != typeof(Object)) 
            {
                type = type.BaseType;
                ++realParentChainCount;
            }

            Debug.Assert(parentChainCount == realParentChainCount, "Parent chain count changed for class " + className + " (is " + parentChainCount + ", should be " + realParentChainCount + ")");

            // Read class versions. They are stored from derived to base.
            byte[]  classVersions = ReadBytes(parentChainCount);
            Type    curClassType  = constructor.DeclaringType;
            uint    curClassIndex = 0;

            context.ClassVersions = new Dictionary<Type, byte>(parentChainCount);
            // Traverse the class hierarchy from derived to base setting the class version info
            while (curClassIndex < parentChainCount)
            {
                context.ClassVersions[curClassType] = classVersions[curClassIndex];
                // TODO: Enforce correct class versions. From the time being just skip those class version bytes.

                curClassType = curClassType.BaseType;
                ++curClassIndex;
            }

            // Invoke deserialization
            object eq2Object = constructor.Invoke(new object[] { this, context });

            // Forces an exception to be thrown if the object is not a VeBase
            return (VeBase)eq2Object;
        }


        /// <summary>
        /// Reads an Everquest 2 node hierarchy from the stream.
        /// </summary>
        /// <remarks>
        /// This methods deserializes a hierarchy of Everquest 2 nodes.
        /// An exception is thrown if any of the objects read is not a node.
        /// </remarks>
        /// <exception cref="InvalidCastException">One of the objects from the hierarchy is not an Everquest 2 node.</exception>
        /// <exception cref="DeserializationException">Error encountered while deserializing the object.</exception>
        /// <returns>Deserialized hierarchy of objects.</returns>
        public virtual VeNode ReadNodeObject()
        {
            // Read root node. If the object is not a VeNode the cast will throw an exception.
            VeNode node = (VeNode)ReadObject();

            if (node == null)
                return null;

            // Read children count
            uint childrenCount = ReadUInt32();

            // Read children
            for (uint i = 0; i < childrenCount; ++i)
            {
                VeNode child = ReadNodeObject();

                node.AddChild(child);
            }

            return node;
        }


        /// <summary>
        /// Reads an Everquest 2 particle generator operation from the stream.
        /// </summary>
        /// <remarks>
        /// This method is usually called during the deserialization of a VeParticleGeneratorNode object.
        /// An exception is the particle generator operation is unknown.
        /// </remarks>
        /// <param name="classVersion">Class version of the calling particle generator class.</param>
        /// <exception cref="DeserializationException">Error encountered while deserializing the operation.</exception>
        /// <returns>Deserialized particle generator operation.</returns>
        public virtual VeParticleGeneratorOp ReadParticleGeneratorOp(byte classVersion)
        {
            // Read operation name
            string name = ReadString(2);

            ConstructorInfo constructor = null;

            // Lookup op name in cache
            if (particleOpCache.ContainsKey(name))
            {
                constructor = particleOpCache[name];
            }
            else
            {
                // Find the operation class given its name as read from the stream
                Type[] opClasses = GetType().Module.FindTypes
                                   (
                                        delegate(Type type, object opName)
                                        {
                                            if (type.Namespace == "Everquest2.Visualization.ParticleGenerator" &&
                                                type.Name.StartsWith("VeParticleGenerator")                    && 
                                                type.Name.EndsWith("Op")                                       &&
                                                type.Name != "VeParticleGeneratorOp")
                                            {
                                                // Note: 19 == "VeParticleGenerator".Length and 21 == "VeParticleGeneratorOp".Length
                                                string extractedOpName = type.Name.Substring(19, type.Name.Length - 21);

                                                String tmpOpName = opName.ToString();
                                                String endName = Char.ToString(tmpOpName[0]);
                                                for (int i = 1; i < tmpOpName.ToString().Length; i++)
                                                {
                                                    endName += Char.ToLower(tmpOpName[i]);
                                                }

                                                return String.Compare(endName, extractedOpName, true) == 0;
                                            }

                                            return false;
                                        },
                                        name
                                   );

                string filename = null;
                if (typeof(FileStream).IsInstanceOfType(BaseStream))
                {
                    filename = (BaseStream as FileStream).Name;
                }

                //Debug.Assert(opClasses.Length > 0, "Error deserializing ParticleGenOp", "'{0}' unknown\nat index {1}{2}",
                //    name, BaseStream.Position, filename != null ? "\nin file " + filename : "");

                // Find deserializing constructor
                constructor = opClasses[0].GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                                                          null, 
                                                          new Type[] { typeof(Eq2Reader), typeof(byte) }, 
                                                          null);
                Debug.Assert(constructor != null, "Deserializing constructor not found on class " + opClasses[0].Name);

                particleOpCache[name] = constructor;
            }

            // Invoke deserialization
            object particleGeneratorOp = constructor.Invoke(new object[] { this, classVersion });

            // Forces an exception to be thrown if the object is not a VeBase
            return (VeParticleGeneratorOp)particleGeneratorOp;
        }

        
        /// <summary>
        /// Reads a string in Everquest 2 format from the stream.
        /// </summary>
        /// <remarks>
        /// Calling this method is equivalent to calling ReadString(1).
        /// </remarks>
        /// <exception cref="DeserializationException">Error encountered while deserializing the string.</exception>
        /// <returns>String read from the stream.</returns>
        public override string ReadString()
        {
            return ReadString(1);
        }


        /// <summary>
        /// Reads a string in Everquest 2 format from the stream using the specified length scale.
        /// </summary>
        /// <remarks>
        /// The Everquest 2 binary format stores the length of a string directly preceding the string contents.
        /// This length has a variable size of 1 to 4 bytes and is stored in little-endian format.
        /// </remarks>
        /// <param name="lengthSize">Size in bytes of string length.</param>
        /// <exception cref="DeserializationException">Error encountered while deserializing the string.</exception>
        /// <returns>String read from the stream.</returns>
        public virtual string ReadString(uint lengthSize)
        {
            #region Preconditions
            Debug.Assert(lengthSize >= 1 && lengthSize <= 4, "lengthSize must be between 1 and 4 bytes");
            #endregion

            // Read string length
            uint length = 0;

            string str = "";
            if ( lengthSize > 1 )
            {
                for (int i = 0; i < lengthSize; ++i)
                {
                    length += (uint)ReadByte() << 8 * i;
                }

                // Read string contents
                str = new string(ReadChars((int)length));
                return str;
            }
            
            bool override_ = false;
            if (this.BaseStream.Position > 0)
                override_ = true;
            do
            {
                long pos = this.BaseStream.Position;
                char curChar = (char)PeekChar();
                if (this.BaseStream.Position+1 >= this.BaseStream.Length)
                    break;

                if (curChar == 0)
                {
                    byte val = ReadByte();
                    curChar = (char)PeekChar();
                }
                bool isStr = Char.IsLetterOrDigit(curChar);
                if (!isStr || override_)
                {
                    byte val = ReadByte();
                    char[] chars_ = ReadChars(val);
                    for(int i=0;i<chars_.Length;i++)
                    {
                        if (i == 0 && chars_[i] != '/' && chars_[i] != '.' && chars_[i] != '_' && !Char.IsLetterOrDigit(chars_[i]))
                        {
                            this.BaseStream.Position = pos;
                            break;
                        }
                        else
                            str += chars_[i];
                    }
                    break;
                }
                else
                    str += ReadChar();
            } while (true);

            return str;
        }
        #endregion


        #region Fields
        private IDictionary<string, ConstructorInfo> classCache      = new Dictionary<string, ConstructorInfo>();
        private IDictionary<string, ConstructorInfo> particleOpCache = new Dictionary<string, ConstructorInfo>();
        #endregion
    }
}
