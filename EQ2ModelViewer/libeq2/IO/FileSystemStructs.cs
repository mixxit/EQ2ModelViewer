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

#endregion

namespace Everquest2.IO
{
    public partial class FileSystem
    {
        protected struct VplHeader
        {
            public int  DirectoryOffset;
            public uint Unknown0x04;
            public uint Unknown0x08;
            public int  VpkNamesOffset;
            public uint Unknown0x10;
            public uint DirectoryEntryCount;
            public uint VpkDirectoryEntryCount;
            public int  VpkNamesInflatedSize;
            public int  VpkNamesDeflatedSize;
            public uint Unknown0x10Size;
            public uint Unknown0x28;
            public bool MustCheckVpkFiles;
            public int? VpkDirectoryOffset;

            #region Constructor
            public VplHeader(System.IO.BinaryReader reader)
            {
                DirectoryOffset         = reader.ReadInt32();
                Unknown0x04             = reader.ReadUInt32();
                Unknown0x08             = reader.ReadUInt32();
                VpkNamesOffset          = reader.ReadInt32();
                Unknown0x10             = reader.ReadUInt32();
                DirectoryEntryCount     = reader.ReadUInt32();
                VpkDirectoryEntryCount  = reader.ReadUInt32();
                VpkNamesInflatedSize    = reader.ReadInt32();
                VpkNamesDeflatedSize    = reader.ReadInt32();
                Unknown0x10Size             = reader.ReadUInt32();
                Unknown0x28             = reader.ReadUInt32();

                if (DirectoryOffset == 0x2C)
                {
                    MustCheckVpkFiles  = false;
                    VpkDirectoryOffset = null;
                }
                else
                {
                    MustCheckVpkFiles  = reader.ReadInt32() == 1 ? true : false;
                    VpkDirectoryOffset = reader.ReadInt32();
                }
            }
            #endregion
        }

        protected struct VpkDirectoryEntry
        {
            public uint TimeOfLastModification;
            public int  Size;

            #region Constructor
            public VpkDirectoryEntry(System.IO.BinaryReader reader)
            {
                TimeOfLastModification = reader.ReadUInt32();
                Size                   = reader.ReadInt32();
            }
            #endregion
        }

        protected struct VplFileEntry
        {
            public uint Key;
            public uint VpkIndex;
            public int  Offset;
            public int  Size;

            #region Constructor
            public VplFileEntry(System.IO.BinaryReader reader)
            {
                Key      = reader.ReadUInt32();
                VpkIndex = reader.ReadUInt32();
                Offset   = reader.ReadInt32();
                Size     = reader.ReadInt32();
            }
            #endregion
        }

        protected struct VplDirectoryEntry
        {
            public uint unk0;
            public uint unk1;
            public uint unk2;
            public uint unk3;
            public uint unk4;

            #region Constructor
            public VplDirectoryEntry(System.IO.BinaryReader reader)
            {
                unk0 = reader.ReadUInt32();
                unk1 = reader.ReadUInt32();
                unk2 = reader.ReadUInt32();
                unk3 = reader.ReadUInt32();
                unk4 = reader.ReadUInt32();
            }
            #endregion
        }

        protected struct VpkFileEntry
        {
            public int Offset;
            public int Size;
            public int NameLength;

            #region Constructor
            public VpkFileEntry(System.IO.BinaryReader reader)
            {
                Offset     = reader.ReadInt32();
                Size       = reader.ReadInt32();
                NameLength = reader.ReadInt32();
            }
            #endregion
        }
    }
}

/* EOF */