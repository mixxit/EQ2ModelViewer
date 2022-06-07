#region License information
// ----------------------------------------------------------------------------
//
//          Eq2VpkTool - A tool to extract Everquest II VPK files
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
using System.Threading;
using System.Collections.Generic;

using Eq2FileSystem     = Everquest2.IO.FileSystem;
using Eq2FileSystemInfo = Everquest2.IO.FileSystemInfo;
using Eq2FileInfo       = Everquest2.IO.FileInfo;
using Eq2DirectoryInfo  = Everquest2.IO.DirectoryInfo;
using Eq2FileStream     = Everquest2.IO.FileStream;

#endregion

namespace Eq2VpkTool
{
    /// <summary>
    /// Manages a set of extractor threads.
    /// </summary>
    public class ExtractionManager
    {
        #region Methods
        /// <summary>
        /// Begins an asynchronous extraction.
        /// </summary>
        /// <param name="items">Files and directories to extract.</param>
        /// <param name="outputPath">Path to extract the items to. Must end with a directory separator character.</param>
        /// <param name="userCallback">Callback method to invoke every time a file is extracted.</param>
        /// <param name="stateObject">User defined object.</param>
        /// <returns>IAsyncResult object that can be used to query the state of the extraction.</returns>
        public IAsyncResult BeginExtract(Eq2FileSystemInfo[] items, string outputPath, AsyncCallback userCallback, object stateObject)
        {
            Thread      thread = new Thread(new ParameterizedThreadStart(Extract));
            AsyncResult result = new AsyncResult(thread, items, outputPath, userCallback, stateObject);

            lock (currentExtractions) currentExtractions.Add(result);
            thread.Start(result);

            return result;
        }


        public void Extract(object obj)
        {
            AsyncResult info = obj as AsyncResult;

            try
            {
                try 
                { 
                    if (!Directory.Exists(info.outputPath))
                    {
                        Directory.CreateDirectory(info.outputPath);
                    }

                    foreach (Eq2FileSystemInfo item in info.items)
                    {
                        if (item is Eq2DirectoryInfo)
                        {
                            ExtractDirectory(item as Eq2DirectoryInfo, info.outputPath, info);
                        }
                        else if (item is Eq2FileInfo)
                        {
                            FileStream stream = ExtractFile(item as Eq2FileInfo, info.outputPath, info);
                            // Close the stream so the file gets written to disk immediately.
                            stream.Close();
                        }
                    }
                }
                catch (UnauthorizedAccessException) {}
                catch (IOException) {}
            }
            finally
            {
                lock (currentExtractions) currentExtractions.Remove(info);
            }
        }


        public FileStream ExtractFile(Eq2FileInfo file, string outputPath)
        {
            AsyncResult dummy = new AsyncResult();

            return ExtractFile(file, outputPath, dummy);
        }


        private FileStream ExtractFile(Eq2FileInfo file, string outputPath, AsyncResult info)
        {
            string     filename = outputPath + file.Name;
            FileStream stream   = new FileStream(filename, FileMode.Create, FileAccess.Write);

            using (Eq2FileStream eq2Stream = file.OpenRead())
            {
                long   size = eq2Stream.Length;
                byte[] data = new byte[size];

                eq2Stream.Read(data, 0, (int)size);
                stream.Write(data, 0, (int)size);
            }

            if (info.userCallback != null) info.userCallback(info);

            return stream;
        }


        public void ExtractDirectory(Eq2DirectoryInfo directory, string outputPath)
        {
            AsyncResult dummy = new AsyncResult();

            ExtractDirectory(directory, outputPath, dummy);
        }


        private void ExtractDirectory(Eq2DirectoryInfo directory, string outputPath, AsyncResult info)
        {
            string directoryPath = outputPath + directory.Name + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(directoryPath); 

            Eq2FileSystemInfo[] children = directory.GetFileSystemInfos();
            foreach (Eq2FileSystemInfo child in children)
            {
                if (child is Eq2DirectoryInfo)
                {
                    ExtractDirectory(child as Eq2DirectoryInfo, directoryPath, info);
                }
                else if (child is Eq2FileInfo)
                {
                    FileStream stream = ExtractFile(child as Eq2FileInfo, directoryPath, info);
                    // Close the stream so the file gets written to disk immediately.
                    stream.Close();
                }
            }
        }


        /// <summary>
        /// Blocks the calling thread until the specified extraction completes.
        /// </summary>
        /// <param name="asyncResult">Object representing the state, as returned from BeginExtract.</param>
        /// <returns>Always returns zero.</returns>
        public int EndExtract(IAsyncResult asyncResult)
        {
            AsyncResult result = asyncResult as AsyncResult;

            result.thread.Join();
            result.CompletedSynchronously = true;
            lock (currentExtractions) currentExtractions.Remove(result);

            return 0;
        }


        /// <summary>
        /// Aborts all ongoing extractions.
        /// </summary>
        public void Close()
        {
            AsyncResult[] extractionInfos;
            lock (currentExtractions)
            {
                extractionInfos = new AsyncResult[currentExtractions.Count];
                currentExtractions.CopyTo(extractionInfos, 0);
            }

            foreach (AsyncResult extractionInfo in extractionInfos)
            {
                extractionInfo.thread.Abort();
                extractionInfo.thread.Join();
            }
        }
        #endregion


        #region Types
        private class AsyncResult : IAsyncResult
        {
            public AsyncResult()
            {
            }


            public AsyncResult(Thread thread, Eq2FileSystemInfo[] items, string outputPath, AsyncCallback userCallback, object stateObject)
            {
                this.thread       = thread;
                this.items        = items;
                this.outputPath   = outputPath;
                this.userCallback = userCallback;
                this.stateObject  = stateObject;

                completedSynchronously = false;
            }


            public object AsyncState
            {
                get { return stateObject; }
            }


            public bool IsCompleted
            {
                get { return !thread.IsAlive; }
            }


            public bool CompletedSynchronously
            {
                get          { return completedSynchronously; }
                internal set { completedSynchronously = value; }
            }


            public WaitHandle AsyncWaitHandle
            {
                // Not implemented. Always return null.
                get { return null; }
            }


            public  Thread              thread;
            public  Eq2FileSystemInfo[] items;
            public  string              outputPath;
            public  AsyncCallback       userCallback;
            private object              stateObject;
            private bool                completedSynchronously;
        }
        #endregion


        #region Fields
        private IList<AsyncResult> currentExtractions = new List<AsyncResult>();
        #endregion
    }
}
