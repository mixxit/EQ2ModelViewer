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
using System.Collections.ObjectModel;
using System.Diagnostics;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using Sys = System.IO;

#endregion

namespace Everquest2.IO
{
    public partial class FileSystem
    {
        #region Methods

        #region Constructors
        public FileSystem()
        {
        }


        public FileSystem(string path) : this(path, Sys.FileMode.Open)
        {
        }


        public FileSystem(string path, Sys.FileMode mode) : this(path, mode, Sys.FileAccess.Read)
        {
        }


        public FileSystem(string path, Sys.FileMode mode, Sys.FileAccess access)
        {
            Open(path, mode, access);
        }


        public void Open(string path)
        {
            Open(path, Sys.FileMode.Open);
        }


        public void Open(string path, Sys.FileMode mode)
        {
            Open(path, mode, Sys.FileAccess.Read);
        }

        
        public void Open(string path, Sys.FileMode mode, Sys.FileAccess access)
        {
            #region Preconditions
            if (path   == null)                throw new ArgumentNullException("path", "Path must not be null.");
            if (mode   != Sys.FileMode.Open)   throw new NotSupportedException("Only 'open' mode is implemented.");
            if (access != Sys.FileAccess.Read) throw new NotSupportedException("Write access is not implemented. Please specify read-only access.");
            #endregion

            rootDirectory = null;
            directories.Clear();
            files.Clear();

            // Extract the base path from the specified file name. This path will be used to locate the referenced VPK files.
            basePath = System.IO.Path.GetDirectoryName(path) + Sys.Path.DirectorySeparatorChar;
            if (!Sys.Directory.Exists(basePath)) throw new ArgumentException("The specified path is invalid. The '" + basePath + "' directory doesn't exist.", "path");

            // Create root directory.
            rootDirectory = new DirectoryInfo(this, string.Empty);
            directories.Add(string.Empty, rootDirectory);
            OnDirectoryAdded(rootDirectory);

            // The System.IO.FileStream constructor may throw the following exceptions:
            //      - ArgumentOutOfRangeException: mode, access, or share contain an invalid value.
            //      - FileNotFoundException: The file cannot be found.
            //      - IOException: An I/O error occurs.
            //      - SecurityException: The caller does not have the required permission. 
            //      - UnauthorizedAccessException: The access requested is not permitted by the operating system for the specified path. 
            // None of these exceptions are caught here.
            Sys.FileStream stream = new Sys.FileStream(path, mode, access);

            Initialize(stream);
        }
        #endregion


        #region Creation methods
        public static FileSystem Create(string path)
        {
            Sys.FileStream stream = new Sys.FileStream(path, Sys.FileMode.Create, Sys.FileAccess.ReadWrite);

            return Create(stream);
        }


        public static FileSystem Create(Sys.Stream stream)
        {
            #region Preconditions
            throw new NotImplementedException("File system creation not supported.");
            #endregion
        }
        #endregion


        private void Initialize(Sys.Stream stream)
        {
            // The System.IO.BinaryReader constructor may throw the following exceptions:
            //      - ArgumentException: The stream does not support reading, the stream is null, or the stream is already closed. 
            // This exception is not caught here.
            Sys.BinaryReader reader = new Sys.BinaryReader(stream);

            // The VplHeader constructor may throw the following exceptions:
            //      - EndOfStreamException: The end of the stream is reached. 
            //      - ObjectDisposedException: The stream is closed. 
            //      - IOException: An I/O error occurs. 
            // None of these exceptions are caught here.
            header = new VplHeader(reader);


            stream.Seek(header.DirectoryOffset, Sys.SeekOrigin.Begin);
            VplFileEntry[] directory = new VplFileEntry[header.DirectoryEntryCount];
            for (int i = 0; i < directory.Length; ++i) directory[i] = new VplFileEntry(reader);


            stream.Seek(header.Unknown0x04, Sys.SeekOrigin.Begin);

            uint[] unk4 = new uint[header.DirectoryEntryCount];
            for (int i  =0; i < header.DirectoryEntryCount; ++i) unk4[i] = reader.ReadUInt32();

            VplDirectoryEntry[] dirs = new VplDirectoryEntry[(header.Unknown0x10 - header.Unknown0x08)/ 20];
            for (int i = 0; i < (header.Unknown0x10 - header.Unknown0x08)/ 20; ++i) dirs[i] = new VplDirectoryEntry(reader);

            
            // We don't need this reader anymore
            reader = null;

            // If header.VpkDirectoryEntryCount is zero there are no referenced VPK files in this filesystem.
            if (header.VpkDirectoryEntryCount > 0)
            {
                ProcessVplFile(stream, header);
            }
        }


        private void ProcessVplFile(Sys.Stream stream, VplHeader header)
        {
            ReadVpkDirectory(stream, header);
            ReadFileDirectory(stream, header);
        }


        private void ReadVpkDirectory(Sys.Stream stream, VplHeader header)
        {
            InflaterInputStream inflater = new InflaterInputStream(stream);

            stream.Seek(header.VpkNamesOffset, Sys.SeekOrigin.Begin);

            byte[] rawByteNames = new byte[header.VpkNamesInflatedSize];

            // Decompress the referenced VPK file names
            // The DeflateStream.Read method may throw the following exceptions:
            //      - ZipException: Inflater needs a dictionary.
            // This exception is rethrown inside an Sys.InvalidDataException exception.
            try                    
            {
                int bytesRead = 0;

                while (inflater.Available == 1 && bytesRead < header.VpkNamesInflatedSize)
                {
                    bytesRead += inflater.Read(rawByteNames, bytesRead, header.VpkNamesInflatedSize - bytesRead);
                }
            }
            catch (ZipException e) 
            { 
                throw new Sys.InvalidDataException("Error reading VPK names. The compressed data is invalid.", e); 
            }

            // Transform the bytes to a string. The strings are stored using the ASCII encoding.
            Sys.MemoryStream memoryStream = new Sys.MemoryStream(rawByteNames, false);
            Sys.BinaryReader reader       = new Sys.BinaryReader(memoryStream, System.Text.Encoding.ASCII);
            string rawNames               = new string(reader.ReadChars(header.VpkNamesInflatedSize));

            // The names are stored as a single chunk, separated by null characters.
            // Here we chop the string into the individual names.
            vpkFiles = rawNames.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            if (vpkFiles.Length != header.VpkDirectoryEntryCount) throw new Sys.InvalidDataException("The number of referenced VPK file names (" + vpkFiles.Length + ") doesn't match the info in the header (" + header.VpkDirectoryEntryCount + ")");
            // Convert absolute VPK file paths to relative file paths.
            // Note: In some systems this step isn't necessary because the VPK file paths 
            // are already relative to the base EQ2 installation directory.
            for (int i = 0; i < vpkFiles.Length; ++i)
            {
                string path = vpkFiles[i].Replace('/', Sys.Path.DirectorySeparatorChar);

                if (Sys.Path.IsPathRooted(path))
                {
                    if (!Sys.File.Exists(path)) throw new Sys.FileNotFoundException("The referenced VPK file '" + path + "' doesn't exist.");
                    // Remove the base path from the VPK file path
                    vpkFiles[i] = path.Remove(0, basePath.Length);
                }
                else
                {
                    string absolutePath = basePath + path; 
                    if (!Sys.File.Exists(absolutePath)) throw new Sys.FileNotFoundException("The referenced VPK file '" + absolutePath + "' doesn't exist.");
                    // Leave the path as a relative path
                    vpkFiles[i] = path;
                }
            }

            // Read the VPK directory. We don't process this information at the moment.
            Debug.Assert(header.VpkDirectoryOffset.HasValue);
            stream.Seek(header.VpkDirectoryOffset.Value, Sys.SeekOrigin.Begin);
            reader = new Sys.BinaryReader(stream);

            for (uint i = 0; i < header.VpkDirectoryEntryCount; ++i)
            {
                VpkDirectoryEntry entry = new VpkDirectoryEntry(reader);
            }
        }


        private class FileDirectoryState
        {
            public byte[]         Data;
            public Sys.FileStream Stream;
            public string         VpkFile;
        }


        private void ReadFileDirectoryCallback(IAsyncResult result)
        {
            FileDirectoryState state = result.AsyncState as FileDirectoryState;

            state.Stream.EndRead(result);
            state.Stream.Close();

            Sys.MemoryStream    deflatedDirectoryStream   = new Sys.MemoryStream(state.Data);
            InflaterInputStream deflatedDirectoryInflater = new InflaterInputStream(deflatedDirectoryStream);

            byte[]              rawInt                    = new byte[4];
            Sys.MemoryStream    intMemoryStream           = new Sys.MemoryStream(rawInt, 0, rawInt.Length);
            Sys.BinaryReader    intReader                 = new Sys.BinaryReader(intMemoryStream);
            
            // Uncompress file directory inflated size.
            try                    { deflatedDirectoryInflater.Read(rawInt, 0, 4); }
            catch (ZipException e) { throw new Sys.InvalidDataException("Error reading file directory inflated size of pak '" + state.VpkFile + "'. The compressed data is invalid.", e); }

            intMemoryStream.Seek(0, Sys.SeekOrigin.Begin);
            int inflatedDirectorySize = intReader.ReadInt32() - 4;

            intReader.Close();
            intReader       = null;
            intMemoryStream = null;
            rawInt          = null;

            Sys.BinaryReader directoryReader = new Sys.BinaryReader(deflatedDirectoryInflater, System.Text.Encoding.ASCII);

            // Read file directory header.
            uint directoryEntryCount = directoryReader.ReadUInt32();

            // Process file directory.
            for (uint i = 0; i < directoryEntryCount; ++i)
            {
                VpkFileEntry fileEntry = new VpkFileEntry(directoryReader);
                string       filename  = new string(directoryReader.ReadChars(fileEntry.NameLength));

                AddFile(filename, fileEntry, state.VpkFile);
            }

            state.Data = null;
        }


        private void ReadFileDirectory(Sys.Stream stream, VplHeader header)
        {
            foreach (string vpkFile in vpkFiles)
            {
				string absoluteVpkFilePath = basePath + vpkFile;

				Sys.FileStream vpkStream = null;

                try
				{
                    // The System.IO.FileStream constructor may throw the following exceptions:
                    //      - FileNotFoundException: The file cannot be found.
                    //      - IOException: An I/O error occurs.
                    //      - SecurityException: The caller does not have the required permission. 
                    //      - UnauthorizedAccessException: The access requested is not permitted by the operating system for the specified path. 
                    // The SecurityException exception is caught and rethrown. The others fall through.
					vpkStream = new Sys.FileStream(absoluteVpkFilePath, Sys.FileMode.Open, Sys.FileAccess.Read, Sys.FileShare.Read, 4, true);
                }
                catch (System.Security.SecurityException e)
                {
                    throw new System.Security.SecurityException("No permission to open '" + absoluteVpkFilePath + "' for reading.", e);
                }

                Sys.BinaryReader vpkReader = new Sys.BinaryReader(vpkStream);

                // Seek to the end of file minus 8 bytes
                vpkStream.Seek(-8, Sys.SeekOrigin.End);
                // Read file directory offset.
                int directoryOffset = vpkReader.ReadInt32();
                // Seek to file directory.
                vpkStream.Seek(directoryOffset, Sys.SeekOrigin.Begin);

				// Read file directory deflated size.
                int deflatedDirectorySize = vpkReader.ReadInt32();

                FileDirectoryState state = new FileDirectoryState();
                state.Data    = new byte[deflatedDirectorySize];
                state.Stream  = vpkStream;
                state.VpkFile = vpkFile;

                vpkStream.BeginRead(state.Data, 0, deflatedDirectorySize, ReadFileDirectoryCallback, state);
            }
        }


        private void AddFile(string filename, VpkFileEntry entry, string vpkFile)
        {
            // We won't add the same file twice
            lock (files)
            {
                if (files.ContainsKey(filename)) return;
            }

            // We won't use Path.GetDirectoryName here because we want to preserve the forward slashes
            // for the directory name, as we will use it as a key in the dictionary.
            int pathEnd = Math.Max(0, filename.LastIndexOfAny(directorySeparators, filename.Length - 1, filename.Length));

            string        directory        = filename.Substring(0, pathEnd);
            DirectoryInfo currentDirectory = null;

			List<DirectoryInfo> directoriesToNotify = new List<DirectoryInfo>();

            // We will hold the lock on the dictionary until we finish creating all the needed directories.
			lock (directories)
			{
				directories.TryGetValue(directory, out currentDirectory);

				if (currentDirectory == null)
				{
					currentDirectory = RootDirectory;

					int directoryStart = 0;
					int directoryEnd   = directory.IndexOfAny(directorySeparators, 0);

                    if (directoryEnd == -1) directoryEnd = directory.Length;

					while (directoryStart < directory.Length)
					{
						string        subdirectory  = directory.Substring(directoryStart, directoryEnd - directoryStart);
						DirectoryInfo nextDirectory = currentDirectory.GetDirectory(subdirectory);

						if (nextDirectory == null)
						{
							// Create the directory.
							string newDirectoryName = directory.Substring(0, directoryEnd);

							nextDirectory = new DirectoryInfo(this, newDirectoryName);

							currentDirectory.AddChild(nextDirectory);
							directories.Add(newDirectoryName, nextDirectory);
							// We don't notify the addition of the directory here so as to prevent a deadlock
							// in case the invoked delegate calls a function on the filesystem that tries to
							// acquire the lock on 'directories'.
							directoriesToNotify.Add(nextDirectory);
						}

						currentDirectory = nextDirectory;

						directoryStart = directoryEnd + 1;
						if (directoryStart < directory.Length)
						{
							directoryEnd = directory.IndexOfAny(directorySeparators, directoryStart);
                            if (directoryEnd == -1) directoryEnd = directory.Length;
						}
					}
				}
			}

            // Notify the addition of the directories now that we have released the lock on 'directories'.
			foreach (DirectoryInfo dir in directoriesToNotify) OnDirectoryAdded(dir);

			FileInfo file = new FileInfo(this, filename, entry.Size, vpkFile, entry.Offset);
            currentDirectory.AddChild(file);
            lock (files) files.Add(filename, file);
            OnFileAdded(file);
        }


        public bool FileExists(string file)
        {
            lock (files) return files.ContainsKey(file);
        }


        public bool DirectoryExists(string directory)
        {
			lock (directories) return directories.ContainsKey(directory);
		}


        public ReadOnlyCollection<string> GetDirectories(string directory)
        {
            ReadOnlyCollection<string> directoryNames = null;
            
            DirectoryInfo parentDirectory = null;
            lock (directories) directories.TryGetValue(directory, out parentDirectory);

            if (parentDirectory != null)
            {
                DirectoryInfo[] subdirectories = parentDirectory.GetDirectories();
                string[]        names          = new string[subdirectories.Length];

                for (int i = 0; i < subdirectories.Length; ++i) names[i] = subdirectories[i].Name;
                
                directoryNames = new ReadOnlyCollection<string>(names);
            }

            return directoryNames;
        }


        public ReadOnlyCollection<string> GetFiles(string directory)
        {
            ReadOnlyCollection<string> fileNames = null;

            DirectoryInfo parentDirectory = null;
            lock (directories) directories.TryGetValue(directory, out parentDirectory);

            if (parentDirectory != null)
            {
                FileInfo[] directoryFiles = parentDirectory.GetFiles();
                string[]   names          = new string[directoryFiles.Length];

                for (int i = 0; i < directoryFiles.Length; ++i) names[i] = directoryFiles[i].Name;
                
                fileNames = new ReadOnlyCollection<string>(names);
            }

            return fileNames;
        }


        public DirectoryInfo GetDirectoryInfo(string directory)
        {
            DirectoryInfo result = null;

            lock (directories) directories.TryGetValue(directory, out result);

            return result;
        }


        public FileInfo GetFileInfo(string file)
        {
            FileInfo result = null;

            lock (files) files.TryGetValue(file, out result);

            return result;
        }


        private void OnDirectoryAdded(DirectoryInfo directory)
        {
            if (DirectoryAdded != null) DirectoryAdded(this, new DirectoryAddedEventArgs(directory));
        }


        private void OnFileAdded(FileInfo file)
        {
            if (FileAdded != null) FileAdded(this, new FileAddedEventArgs(file));
        }
        #endregion


        #region Properties
        public DirectoryInfo RootDirectory
        {
            get { return rootDirectory; }
        }


        // Return the file count, as published on the VPL file header. 
        /// <summary>
        /// Gets the number of files in this file system, as specified on the VPL file header. 
        /// </summary>
        /// <remarks>Might not be the real number of files contained in the referenced VPK files.</remarks>
        /// <value>Number of files in this file system.</value>
        public int FileCount
        {
            get { return (int)header.DirectoryEntryCount; }
        }


        internal string BasePath
        {
            get { return basePath; }
        }
        #endregion


        #region Events
        public class DirectoryAddedEventArgs : EventArgs
        {
            public DirectoryAddedEventArgs(DirectoryInfo directory)
            {
                this.directory = directory;
            }

            public DirectoryInfo directory;
        }


        public class FileAddedEventArgs : EventArgs
        {
            public FileAddedEventArgs(FileInfo file)
            {
                this.file = file;
            }

            public FileInfo file;
        }


        public event EventHandler<DirectoryAddedEventArgs> DirectoryAdded;
        public event EventHandler<FileAddedEventArgs>      FileAdded;
        #endregion


        #region Fields
        private VplHeader                          header;

        private string[]                           vpkFiles;
        private string                             basePath;

        private DirectoryInfo                      rootDirectory;

        private Dictionary<string, DirectoryInfo>  directories = new Dictionary<string, DirectoryInfo>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, FileInfo>       files       = new Dictionary<string, FileInfo>(StringComparer.CurrentCultureIgnoreCase);

        internal static char[]                     directorySeparators = new char[] { Sys.Path.DirectorySeparatorChar, Sys.Path.AltDirectorySeparatorChar };
        #endregion
    }
}

/* EOF */