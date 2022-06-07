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
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

using Eq2FileSystem     = Everquest2.IO.FileSystem;
using Eq2FileSystemInfo = Everquest2.IO.FileSystemInfo;
using Eq2FileInfo       = Everquest2.IO.FileInfo;
using Eq2DirectoryInfo  = Everquest2.IO.DirectoryInfo;

#endregion

namespace Eq2VpkTool
{
    public class FileSystemViewController
    {
        #region Methods

        public void Open(string filename, TreeView treeview, ListView listview)
        {
            this.treeview = treeview;
            this.listview = listview;

            nodeDictionary.Clear();
            fileCount = 0;

            filesystem        = new Eq2FileSystem();
            extractionManager = new ExtractionManager();

            // Initialize treeview and listview
            treeview.Nodes.Clear();
            listview.Items.Clear();

            listview.Sorting            = SortOrder.Ascending;
            listview.ListViewItemSorter = new DirectoryContentsComparer();

            iconManager             = new IconManager(new ImageList());
            treeview.ImageList      = iconManager.ImageList;
            treeview.ImageIndex     = iconManager.GetDirectoryImageIndex();
            listview.SmallImageList = iconManager.ImageList;

            // We must first de-register the event handlers, in case they were registered in a previous call to Open().
            treeview.BeforeExpand   -= OnExpandNode;
            treeview.BeforeExpand   += OnExpandNode;
            treeview.BeforeCollapse -= OnCollapseNode;
            treeview.BeforeCollapse += OnCollapseNode;
            treeview.BeforeSelect   -= OnSelectNode;
            treeview.BeforeSelect   += OnSelectNode;
            listview.DoubleClick    -= OnListViewDoubleClick;
            listview.DoubleClick    += OnListViewDoubleClick;

            thread = new Thread(new ParameterizedThreadStart(OpenFileSystem));
            thread.Start(filename);
        }


        /// <summary>
        /// Start function for the processing thread.
        /// </summary>
        /// <param name="obj">String that represents the path to the VPL file.</param>
        private void OpenFileSystem(object obj)
        {
            filename = obj as string;

            try
            {
                filesystem.DirectoryAdded += OnRootDirectoryAdded;
                filesystem.FileAdded      += OnFileAdded;
                filesystem.Open(filename);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                MessageBox.Show("Error processing file.\n\n" + e, "Error");
            }
        }
        

        /// <summary>
        /// Stops the processing thread and deletes all generated temporary files.
        /// </summary>
        public void Close()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
                thread = null;

                // Delete temporary files
                foreach (string path in temporaryFiles)
                {
                    try                                 { File.Delete(path); }
                    catch (UnauthorizedAccessException) {}
                    catch (IOException)                 {}
                }
            }
        }


        /// <summary>
        /// Callback invoked every time a file is added to the file system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileAdded(object sender, Eq2FileSystem.FileAddedEventArgs e)
        {
            lock (this) ++fileCount;
        }


        /// <summary>
        /// Callback invoked when the first directory (the root directory) is added to the file system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRootDirectoryAdded(object sender, Eq2FileSystem.DirectoryAddedEventArgs e)
        {
            filesystem.DirectoryAdded  -= OnRootDirectoryAdded;
            e.directory.DirectoryAdded += OnDirectoryAdded;

            treeview.Invoke(new Action<Eq2DirectoryInfo>(AddRootDirectory), e.directory);
        }


        /// <summary>
        /// Adds the root directory node to the TreeView.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="rootDirectory">Root directory.</param>
        private void AddRootDirectory(Eq2DirectoryInfo rootDirectory)
        {
            string rootNodeText = System.IO.Path.GetFileName(filename);

            TreeNode node = new TreeNode(rootNodeText);
            node.Tag = rootDirectory;

            treeview.Nodes.Add(node);
            // No need to acquire the lock on nodeDictionary because we're sure 
            // no other thread will be accessing it at this point.
            nodeDictionary.Add(rootDirectory, new NodeInfo(node));
        }


        /// <summary>
        /// Callback invoked every time a new directory is added to an existing directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDirectoryAdded(object sender, Eq2FileSystem.DirectoryAddedEventArgs e)
        {
            Eq2DirectoryInfo directory       = e.directory;
            Eq2DirectoryInfo parentDirectory = directory.Parent;

            NodeInfo parentDirectoryNodeInfo;
            lock (nodeDictionary) parentDirectoryNodeInfo = nodeDictionary[parentDirectory];
                
            if (parentDirectoryNodeInfo.Expanded)
            {
                // The parent directory is expanded, so we must reflect the addition of the new directory.
                // This new directory doesn't have any subdirectories yet, so we will install a listener on it.
                TreeNode directoryNode = treeview.Invoke(new AddDirectoryDelegate(AddDirectoryToExpandedDirectory), 
                                                         parentDirectoryNodeInfo.Node, 
                                                         directory) as TreeNode;

                lock (nodeDictionary) nodeDictionary.Add(directory, new NodeInfo(directoryNode));
                directory.DirectoryAdded += OnDirectoryAdded;
            }
            else
            {
                // The parent directory didn't have any subdirectories, but it now has one.
                // We'll add a dummy child node so the plus icon appears and the node can be expanded.
                // Note: More than one thread can invoke this method before we deregister from the DirectoryAdded event.
                //       The means more than one dummy subnode can be added to this directory.
                parentDirectory.DirectoryAdded -= OnDirectoryAdded;

                treeview.Invoke(new Action<TreeNode>(AddDirectoryToCollapsedDirectory), parentDirectoryNodeInfo.Node);
            }
        }


        /// <summary>
        /// Adds a new tree node as a child of an expanded directory node.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="parentNode"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private TreeNode AddDirectoryToExpandedDirectory(TreeNode parentNode, Eq2DirectoryInfo directory)
        {
            string directoryName = directory.Name;

            TreeNode node = parentNode.Nodes.Add(directoryName, directoryName);
            node.Tag = directory;

            return node;
        }


        /// <summary>
        /// Adds a dummy tree node as a child of a collapsed directory node.
        /// This dummy node makes the treeview render a plus sign used to expand the directory node.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="directoryNode"></param>
        private void AddDirectoryToCollapsedDirectory(TreeNode directoryNode)
        {
            directoryNode.Nodes.Add(new TreeNode());
        }


        /// <summary>
        /// Callback invoked when a tree node is expanded.
        /// This method must create the nodes for the subdirectories of the expanded directory.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExpandNode(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode         node      = e.Node;
            Eq2DirectoryInfo directory = node.Tag as Eq2DirectoryInfo;

            // Remove dummy nodes if present
            if (node.Nodes[0].Text.Length == 0) node.Nodes.Clear();

            lock (nodeDictionary)
            {
                treeview.BeginUpdate();

                // Add new subdirectories as nodes to the treeview and to the node dictionary.
                Eq2DirectoryInfo[] subdirectories = directory.GetDirectories();
                foreach (Eq2DirectoryInfo subdirectory in subdirectories)
                {
                    if (!node.Nodes.ContainsKey(subdirectory.Name))
                    {
                        TreeNode subdirectoryNode = AddDirectoryToExpandedDirectory(node, subdirectory);

                        nodeDictionary.Add(subdirectory, new NodeInfo(subdirectoryNode));

                        // Check whether this subdirectory has subdirectories of its own or not.
                        // If it does, we will add a dummy tree node to it so the plus icon appears.
                        // If it does not, we will track any changes to it.
                        if (subdirectory.DirectoryCount > 0)
                        {
                            subdirectoryNode.Nodes.Add(new TreeNode());
                        }
                        else
                        {
                            subdirectory.DirectoryAdded += OnDirectoryAdded;
                        }
                    }
                }

                treeview.EndUpdate();

                // Mark this directory as expanded
                nodeDictionary[directory].Expanded = true;

                directory.DirectoryAdded += OnDirectoryAdded;
            }
        }


        /// <summary>
        /// Callback invoked when a tree node is collapsed.
        /// We won't delete the tree nodes already added to the collapsed directory, 
        /// but we won't add any more until the node is expanded again.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCollapseNode(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode         node      = e.Node;
            Eq2DirectoryInfo directory = node.Tag as Eq2DirectoryInfo;

            directory.DirectoryAdded -= OnDirectoryAdded;

            // Mark this directory as collapsed
            lock (nodeDictionary) nodeDictionary[directory].Expanded = false;
        }


        /// <summary>
        /// Callback invoked when a tree node is selected.
        /// Updates the list view with the directory contents.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectNode(object sender, TreeViewCancelEventArgs e)
        {
            TreeView         treeview     = sender as TreeView;
            TreeNode         node         = e.Node;
            Eq2DirectoryInfo newDirectory = node.Tag as Eq2DirectoryInfo;

            if (treeview.SelectedNode != null)
            {
                // Deregister from the old directory
                Eq2DirectoryInfo oldDirectory = treeview.SelectedNode.Tag as Eq2DirectoryInfo;
                oldDirectory.ChildAdded -= OnDirectoryChildAdded;
            }

            // FIXME: There is a race condition here. It is possible that after having deregistered from
            //        the old directory there is still a child of that directory in the process of being
            //        added. In that case, it will be added to the listview erroneously as if it were a
            //        child of the newly selected directory.

            // Clear the old items from the listview
            listview.Items.Clear();
            listview.BeginUpdate();
            // Add the current children of the new directory to the listview
            Eq2FileSystemInfo[] children = newDirectory.GetFileSystemInfos();
            foreach (Eq2FileSystemInfo child in children)
            {
                ListViewItem item = listview.Items.Add(child.Name);

                item.Tag = child;
                item.ImageIndex = iconManager.GetImageIndex(child);
            }
            listview.EndUpdate();

            // Register to receive child added events in the future to the new directory
            newDirectory.ChildAdded += OnDirectoryChildAdded;
        }


        /// <summary>
        /// Callback invoked when a file or directory is added to an existing directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDirectoryChildAdded(object sender, Eq2DirectoryInfo.ChildAddedEventArgs e)
        {
            listview.Invoke(new AddChildToDirectoryDelegate(AddChildToDirectory), sender as Eq2DirectoryInfo, e.child);
        }


        /// <summary>
        /// Adds a file or directory to the list view.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="directory"></param>
        /// <param name="child"></param>
        private void AddChildToDirectory(Eq2DirectoryInfo directory, Eq2FileSystemInfo child)
        {
            // Note: The second condition is probably not needed.
            if (treeview.SelectedNode != null && treeview.SelectedNode.Tag == directory)
            {
                ListViewItem item = listview.Items.Add(child.Name);

                item.Tag = child;
                item.ImageIndex = iconManager.GetImageIndex(child);
            }
        }


        /// <summary>
        /// Callback invoked when the user double-clicks on the list view.
        /// This operation opens the selected directory or extracts and opens the selected file.
        /// </summary>
        /// <remarks>This method is called from the UI thread.</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListViewDoubleClick(object sender, EventArgs e)
        {
            ListView listview = sender as ListView;
            
            if (listview.SelectedItems.Count != 1) return;

            Eq2FileSystemInfo child = listview.SelectedItems[0].Tag as Eq2FileSystemInfo;

            if (child is Eq2DirectoryInfo)
            {
                OnListViewDoubleClickDirectory(child as Eq2DirectoryInfo);
            }
            else if (child is Eq2FileInfo)
            {
                OnListViewDoubleClickFile(child as Eq2FileInfo);
            }
        }


        private void OnListViewDoubleClickDirectory(Eq2DirectoryInfo directory)
        {
            Stack<Eq2DirectoryInfo> directoryHierarchy = new Stack<Eq2DirectoryInfo>();
            directoryHierarchy.Push(directory);

            Eq2DirectoryInfo parent = directory.Parent;
            while (parent != null)
            {
                directoryHierarchy.Push(parent);
                parent = parent.Parent;
            }
            // Pop root directory
            directoryHierarchy.Pop();

            TreeNode rootNode = treeview.Nodes[0];
            rootNode.Expand();

            TreeNodeCollection directoryNodes = rootNode.Nodes;
            TreeNode           nodeToExpand   = null;
            while (directoryHierarchy.Count > 0)
            {
                Eq2DirectoryInfo directoryToExpand = directoryHierarchy.Pop();
                
                nodeToExpand = directoryNodes[directoryToExpand.Name];
                if (!nodeToExpand.IsExpanded) nodeToExpand.Expand();

                directoryNodes = nodeToExpand.Nodes;
            }

            treeview.SelectedNode = nodeToExpand;
        }


        private void OnListViewDoubleClickFile(Eq2FileInfo file)
        {
            OpenFile(file);
        }


        private void OpenFile(Eq2FileInfo file)
        {
            string path     = Path.GetTempPath() + file.DirectoryName.Replace('/', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string filename = path + file.Name;

            try
            {
                Directory.CreateDirectory(path);
                FileStream stream = extractionManager.ExtractFile(file, path);
                stream.Close();
            }
            catch (UnauthorizedAccessException) { return; }
            catch (IOException)                 { return; }

            try
            {
                temporaryFiles.Add(filename);
                System.Diagnostics.Process.Start(filename);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // There is no associated program for this file type.
                // Ignore the error.
            }
        }

        #endregion


        #region Types
        /// <summary>
        /// Contains information about a tree view node. This information is used by the processing thread,
        /// not the UI thread. It's a way of reducing the number of cross-thread invocations.
        /// </summary>
        private class NodeInfo
        {
            public NodeInfo(TreeNode node)
            {
                this.node     = node;
                this.expanded = false;
            }


            public TreeNode Node
            { 
                get { return node; }
            }


            public bool Expanded 
            { 
                get { return expanded; } 
                set { expanded = value; } 
            }


            private TreeNode node;
            private bool     expanded;
        }
        #endregion


        #region Delegates
        private delegate TreeNode AddDirectoryDelegate        (TreeNode parentNode, Eq2DirectoryInfo directory);
        private delegate void     AddChildToDirectoryDelegate (Eq2DirectoryInfo directory, Eq2FileSystemInfo child);
        #endregion


        #region Properties
        public int FileCount
        {
            get { lock (this) return fileCount; }
        }


        public Eq2FileSystem FileSystem
        {
            get { return filesystem; }
        }
        #endregion


        #region Fields
        private IDictionary<Eq2DirectoryInfo, NodeInfo> nodeDictionary = new Dictionary<Eq2DirectoryInfo, NodeInfo>();
        private IList<string> temporaryFiles = new List<string>();

        private int               fileCount;
        private string            filename;
        private Thread            thread;
        private Eq2FileSystem     filesystem;
        private TreeView          treeview;
        private ListView          listview;
        private IconManager       iconManager;
        private ExtractionManager extractionManager;
        #endregion
    }
}
