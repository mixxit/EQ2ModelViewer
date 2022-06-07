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

using Sys = System.IO;

#endregion

namespace Everquest2.IO
{
    public class FileInfo : FileSystemInfo
    {
        internal FileInfo(FileSystem fileSystem, string fileName, long size, string vpkFile, int offset)
        {
            #region Preconditions
            if (fileName == null) throw new ArgumentNullException("fileName");
            #endregion

            this.fileSystem = fileSystem;
            this.fileName   = fileName;
            this.size       = size;
            this.vpkFile    = vpkFile;
            this.offset     = offset;
        }


        public override bool Exists
        {
            get { return fileSystem.FileExists(FullName); }
        }


        public override string FullName
        {
            get { return fileName; }
        }


        public override string Name
        {
            get
            {
                string name = FullName;

                int nameStart = name.LastIndexOfAny(FileSystem.directorySeparators, name.Length - 1);

                return nameStart == -1 ? name : name.Substring(nameStart + 1);
            }
        }


        public DirectoryInfo Directory
        {
            get
            {
                int    index         = fileName.LastIndexOfAny(FileSystem.directorySeparators);
                string directoryName = index == -1 ? string.Empty : fileName.Substring(0, index);

                DirectoryInfo directory = fileSystem.GetDirectoryInfo(directoryName);

                if (directory == null) throw new Sys.DirectoryNotFoundException("The directory '" + directoryName + "' was not found.");

                return directory;
            }
        }


        public string DirectoryName
        {
            get
            {
                string name = FullName;

                int nameEnd = name.LastIndexOfAny(FileSystem.directorySeparators, name.Length - 1);

                return nameEnd == -1 ? string.Empty : name.Substring(0, nameEnd);
            }
        }


        public long Length
        {
            get
            {
                if (!Exists) throw new System.IO.FileNotFoundException("File '" + FullName + "' not found", FullName);

                return size;
            }
        }


        /// <summary>
        /// Gets the relative path to the VPK file that contains this file.
        /// </summary>
        /// <value>Relative path to the VPK file.</value> 
        internal string VpkFile
        {
            get { return vpkFile; }
        }


        /// <summary>
        /// Gets the offset to this file's packed block in its VPK container.
        /// </summary>
        /// <value>Offset to this file's packed block.</value>
        internal int Offset
        {
            get { return offset; }
        }


        public FileStream OpenRead()
        {
            return Open(Sys.FileMode.Open, Sys.FileAccess.Read);
        }


        public FileStream OpenWrite()
        {
            return Open(Sys.FileMode.Open, Sys.FileAccess.Write);
        }


        public FileStream Open(Sys.FileMode mode)
        {
            return Open(mode, Sys.FileAccess.Read);
        }


        public FileStream Open(Sys.FileMode mode, Sys.FileAccess access)
        {
            return new FileStream(fileSystem.BasePath + VpkFile, Offset, mode, access);
        }


        #region Fields
        private FileSystem fileSystem;
        private string     fileName;
        private long       size;
        private string     vpkFile;
        private int        offset;
        #endregion
    }
}

/* EOF */