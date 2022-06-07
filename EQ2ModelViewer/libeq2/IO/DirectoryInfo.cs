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

namespace Everquest2.IO
{
    public class DirectoryInfo : FileSystemInfo
    {
        internal DirectoryInfo(FileSystem fileSystem, string path)
        {
            #region Preconditions
            if (path == null) throw new ArgumentNullException("path");
            #endregion

            this.fileSystem = fileSystem;
            this.path       = path.TrimEnd('/');

            // If path is empty then this is the root directory
            if (path.Length == 0)
            {
                parent = null;
            }
            else
            {
                int parentNameEnd = path.LastIndexOfAny(FileSystem.directorySeparators, path.Length - 1);                
                string parentName = parentNameEnd == -1 ? string.Empty : path.Substring(0, parentNameEnd);

                parent = fileSystem.GetDirectoryInfo(parentName);
            }
        }


        public override bool Exists
        {
            get { return fileSystem.DirectoryExists(FullName); }
        }


        public override string FullName
        {
            get { return path; }
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


        public DirectoryInfo Parent
        {
            get { return parent; }
        }


        public int ChildrenCount
        {
            get { lock (contents) return contents.Count; }
        }


        public int DirectoryCount
        {
            get { lock (directories) return directories.Count; }
        }


        public int FileCount
        {
            get { lock (files) return files.Count; }
        }


        public DirectoryInfo[] GetDirectories()
        {
            DirectoryInfo[] subdirectories;

            lock (directories)
            {
                subdirectories = new DirectoryInfo[directories.Count];
                directories.Values.CopyTo(subdirectories, 0);
            }

            return subdirectories;
        }


        public DirectoryInfo[] GetDirectories(string pattern)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            DirectoryInfo[] subdirectories = GetDirectories();

            return Array.FindAll<DirectoryInfo>(subdirectories, delegate(DirectoryInfo entry) { return regex.IsMatch(entry.Name); });
        }

        
        public DirectoryInfo GetDirectory(string directory)
        {
            DirectoryInfo result = null;

            lock (directories) directories.TryGetValue(directory, out result);

            return result;
        }


        public FileInfo[] GetFiles()
        {
            FileInfo[] directoryFiles;
            
            lock (files)
            {
                directoryFiles = new FileInfo[files.Count];
                files.Values.CopyTo(directoryFiles, 0);
            }

            return directoryFiles;
        }


        public FileInfo[] GetFiles(string pattern)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            FileInfo[] directoryFiles = GetFiles();

            return Array.FindAll<FileInfo>(directoryFiles, delegate(FileInfo entry) { return regex.IsMatch(entry.Name); });
        }

        
        public FileInfo GetFile(string file)
        {
            FileInfo result = null;

            lock (files) files.TryGetValue(file, out result);

            return result;
        }


        public FileSystemInfo[] GetFileSystemInfos()
        {
            FileSystemInfo[] directoryContents;
            
            lock (contents)
            {
                directoryContents = new FileSystemInfo[contents.Count];
                contents.CopyTo(directoryContents, 0);
            }

            return directoryContents;            
        }


        internal void AddChild(FileSystemInfo child)
        {
            lock (contents) contents.Add(child);
            OnChildAdded(child);

            if (child is DirectoryInfo)
            {
                lock (directories) directories.Add(child.Name, child as DirectoryInfo);
                OnDirectoryAdded(child as DirectoryInfo);
            }
            if (child is FileInfo)
            {
                lock (files) files.Add(child.Name, child as FileInfo);
                OnFileAdded(child as FileInfo);
            }
        }

        
        private void OnChildAdded(FileSystemInfo child)
        {
            if (ChildAdded != null) ChildAdded(this, new ChildAddedEventArgs(child));
        }

        
        private void OnDirectoryAdded(DirectoryInfo directory)
        {
            if (DirectoryAdded != null) DirectoryAdded(this, new FileSystem.DirectoryAddedEventArgs(directory));
        }


        private void OnFileAdded(FileInfo file)
        {
            if (FileAdded != null) FileAdded(this, new FileSystem.FileAddedEventArgs(file));
        }


        #region Events
        public class ChildAddedEventArgs : EventArgs
        {
            public ChildAddedEventArgs(FileSystemInfo child)
            {
                this.child = child;
            }

            public FileSystemInfo child;
        }


        public event EventHandler<ChildAddedEventArgs>                ChildAdded;
        public event EventHandler<FileSystem.DirectoryAddedEventArgs> DirectoryAdded;
        public event EventHandler<FileSystem.FileAddedEventArgs>      FileAdded;
        #endregion


        #region Fields
        FileSystem    fileSystem;
        DirectoryInfo parent;
        string        path;

        IList<FileSystemInfo> contents = new List<FileSystemInfo>();

        Dictionary<string, DirectoryInfo> directories = new Dictionary<string, DirectoryInfo>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<string, FileInfo>      files       = new Dictionary<string, FileInfo>(StringComparer.CurrentCultureIgnoreCase);
        #endregion
    }
}

/* EOF */