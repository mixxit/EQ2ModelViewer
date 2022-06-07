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
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using Sys = System.IO;

#endregion

namespace Everquest2.IO
{
    public class FileStream : Sys.Stream
    {
        public FileStream(FileSystem fileSystem, string path, Sys.FileMode mode) : this(fileSystem, path, mode, Sys.FileAccess.Read)
        {
        }


        public FileStream(FileSystem fileSystem, string path, Sys.FileMode mode, Sys.FileAccess access)
        {
            #region Preconditions
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");
            if (path       == null) throw new ArgumentNullException("path");
            
            string directory = Sys.Path.GetDirectoryName(path);

            if (!fileSystem.DirectoryExists(directory)) throw new Sys.DirectoryNotFoundException("Directory '" + directory + "' not found in this file system.");

            if (mode   != Sys.FileMode.Open)   throw new NotSupportedException("Only 'open' mode is supported.");
            if (access != Sys.FileAccess.Read) throw new NotSupportedException("Write access is not supported. Please specify read-only access.");
            #endregion

            FileInfo file = fileSystem.GetFileInfo(path);
            if (file == null)
            {
                if (mode == Sys.FileMode.Open) throw new Sys.FileNotFoundException("File '" + path + "' not found in this file system.", path);
            }

            Initialize(fileSystem.BasePath + file.VpkFile, file.Offset, mode, access);
        }


        internal FileStream(string vpkPath, int offset, Sys.FileMode mode, Sys.FileAccess access)
        {
            Initialize(vpkPath, offset, mode, access);
        }


        private void Initialize(string vpkPath, int offset, Sys.FileMode mode, Sys.FileAccess access)
        {
            try
            {
                using (Sys.FileStream baseStream = new Sys.FileStream(vpkPath, mode, access))
                {
                    InflaterInputStream inflaterStream = new InflaterInputStream(baseStream);
                    Sys.BinaryReader    reader         = new Sys.BinaryReader(baseStream);

                    baseStream.Seek(offset, Sys.SeekOrigin.Begin);

                    byte[]           rawInt    = new byte[4];
                    Sys.MemoryStream intStream = new Sys.MemoryStream(rawInt);
                    Sys.BinaryReader intReader = new Sys.BinaryReader(intStream);

                    byte[]           rawName;
                    Sys.MemoryStream nameStream = new Sys.MemoryStream();
                    Sys.BinaryReader nameReader = new Sys.BinaryReader(nameStream, System.Text.Encoding.ASCII);

                    int deflatedDataSize = reader.ReadInt32();

                    try                    { inflaterStream.Read(rawInt, 0, 4); }
                    catch (ZipException e) { throw new Sys.InvalidDataException("Error uncompressing filename length. The compressed data is invalid.", e); }

                    intStream.Seek(0, Sys.SeekOrigin.Begin);
                    int fileNameLength = intReader.ReadInt32();

                    try                    { inflaterStream.Read(rawInt, 0, 4); }
                    catch (ZipException e) { throw new Sys.InvalidDataException("Error uncompressing file size. The compressed data is invalid.", e); }

                    intStream.Seek(0, Sys.SeekOrigin.Begin);
                    int fileSize = intReader.ReadInt32();

                    rawName = new byte[fileNameLength];

                    try                    { inflaterStream.Read(rawName, 0, fileNameLength); }
                    catch (ZipException e) { throw new Sys.InvalidDataException("Error uncompressing filename. The compressed data is invalid.", e); }

                    nameStream.Seek(0, Sys.SeekOrigin.Begin);
                    nameStream.Write(rawName, 0, fileNameLength);
                    nameStream.Seek(0, Sys.SeekOrigin.Begin);
                    filename = new string(nameReader.ReadChars(fileNameLength - 1));

                    byte[] fileData = new byte[fileSize];

                    try
                    {
                        int bytesRead = 0;
                        while (inflaterStream.Available == 1 && bytesRead < fileSize)
                        {
                            bytesRead += inflaterStream.Read(fileData, bytesRead, (int)fileSize - bytesRead);
                        }
                    }
                    catch (ZipException e)
                    {
                        throw new Sys.InvalidDataException("Error uncompressing file data. The compressed data is invalid.", e); 
                    }

                    bool writable = (access & Sys.FileAccess.Write) != 0;

                    dataStream = new Sys.MemoryStream(fileData, writable);
                }
            }
            catch (System.Security.SecurityException e)
            {
                throw new System.Security.SecurityException("No permission to open '" + vpkPath + "' with " + mode + " mode and " + access + " access.", e);
            }
        }


        public override bool CanRead
        {
            get { return dataStream.CanRead; }
        }


        public override bool CanSeek
        {
            get { return dataStream.CanSeek; }
        }

    
        public override bool CanWrite
        {
            get { return dataStream.CanWrite; }
        }


        public override long Length
        {
            get { return dataStream.Length; }
        }

    
        public override long Position
        {
            get { return dataStream.Position;  }
            set { dataStream.Position = value; }
        }


        public string Name
        {
            get { return filename; }
        }


        public override void Close()
        {
            dataStream.Close();
        }


        public override void Flush()
        {
            dataStream.Flush();
        }


        public override long Seek(long offset, Sys.SeekOrigin origin)
        {
            return dataStream.Seek(offset, origin);
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            return dataStream.Read(buffer, offset, count);
        }


        public override int ReadByte()
        {
            return dataStream.ReadByte();
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            dataStream.Write(buffer, offset, count);
        }


        public override void SetLength(long value)
        {
            dataStream.SetLength(value);
        }


        #region Fields
        Sys.MemoryStream dataStream;
        string           filename;
        #endregion
    }
}

/* EOF */