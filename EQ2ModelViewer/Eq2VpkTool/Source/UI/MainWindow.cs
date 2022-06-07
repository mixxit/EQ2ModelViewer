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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

using Eq2FileSystem     = Everquest2.IO.FileSystem;
using Eq2FileSystemInfo = Everquest2.IO.FileSystemInfo;
using Eq2FileInfo       = Everquest2.IO.FileInfo;
using Eq2DirectoryInfo  = Everquest2.IO.DirectoryInfo;

#endregion

namespace Eq2VpkTool
{
    public partial class MainWindow : Form
    {
        #region Methods

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            Text = ApplicationName;

            if (File.Exists(ConfigurationFileName))
            {
                try                 { Configuration.Instance.Load(new FileStream(ConfigurationFileName, FileMode.Open, FileAccess.Read)); } 
                catch (Exception e) { MessageBox.Show("Error loading configuration file '" + ConfigurationFileName + "'\n\n" + e); }
            }

            loadingUpdateTimer           = new System.Windows.Forms.Timer();
            loadingUpdateTimer.Interval  = 200;
            loadingUpdateTimer.Tick     += OnLoadingUpdateTimerTick;

            fileSystemViewController = new FileSystemViewController();
            extractionManager        = new ExtractionManager();

            extractToolStripContextMenuItem.Click += extractToolStripContextMenuItem_Click;
            extractWithPathInformationToolStripContextMenuItem.Click += extractWithPathInformationToolStripContextMenuItem_Click;
            decryptToolStripContextMenuItem.Click += decryptToolStripContextMenuItem_Click;
        }
        #endregion


        private void mainHelpWebsiteMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(ApplicationWebsite);
        }


        private void mainFileExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        
        private void mainFileOpenMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadingUpdateTimer.Enabled = false;
                extractionManager.Close();
                fileSystemViewController.Close();
                extractionProgress = null;

                fileSystemViewController.Open(openFileDialog.FileName, directoryTreeView, fileListView);
                loadingUpdateTimer.Enabled = true;
            }
        }


        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ExtractFromMainMenu(extractFolderDialog.SelectedPath, false);
            }
        }

        
        private void extractWithPathInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ExtractFromMainMenu(extractFolderDialog.SelectedPath, true);
            }
        }


        private void extractToolStripContextMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ExtractFromContextMenu(extractFolderDialog.SelectedPath, false);
            }
        }

        
        private void extractWithPathInformationToolStripContextMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                ExtractFromContextMenu(extractFolderDialog.SelectedPath, true);
            }
        }

        
        private void decryptToolStripContextMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                DecryptFromContextMenu(extractFolderDialog.SelectedPath, false);
            }
        }

        
        private void decryptWithPathInformationToolStripContextMenuItem_Click(object sender, EventArgs e)
        {
            if (extractFolderDialog.ShowDialog() == DialogResult.OK)
            {
                DecryptFromContextMenu(extractFolderDialog.SelectedPath, true);
            }
        }


        private void DecryptFromContextMenu(string path, bool withPathInfo)
        {
            if (fileListView.SelectedItems.Count != 1) return;

            Decrypt(path, withPathInfo, fileListView.SelectedItems[0].Tag as Eq2FileInfo);
        }


        private void Decrypt(string outputPath, bool withPathInfo, Eq2FileInfo file)
        {
            if (!TextureDecryptor.CanDecrypt(file)) return;

            // Make sure the path ends with a directory separator char.
            if (outputPath[outputPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                outputPath += Path.DirectorySeparatorChar;
            }

            try
            {
                using (FileStream stream = new FileStream(outputPath + file.Name, FileMode.Create, FileAccess.Write))
                {
                    byte[] decryptedData = TextureDecryptor.Decrypt(file);

                    stream.Write(decryptedData, 0, decryptedData.Length);
                }

                statusMainPanel.Text = file.Name + " decrypted successfully.";
            }
            catch (Exception e)
            {
                MessageBox.Show("Error saving decrypted file:\n\n" + e, "Error");
            }
        }


        private void ExtractFromMainMenu(string path, bool withPathInfo)
        {
            if (directoryTreeView.Focused)
            {
                ExtractTreeViewSelection(path, withPathInfo);
            }
            else if (fileListView.Focused)
            {
                ExtractListViewSelection(path, withPathInfo);
            }
        }

        
        private void ExtractFromContextMenu(string path, bool withPathInfo)
        {
            if (contextMenu.Tag == directoryTreeView)
            {
                ExtractTreeViewSelection(path, withPathInfo);
            }
            else if (contextMenu.Tag == fileListView)
            {
                ExtractListViewSelection(path, withPathInfo);
            }
        }

        
        private void OnLoadingUpdateTimerTick(object sender, EventArgs args)
        {
            if (extractionProgress != null)
            {
                ExtractionProgress progress = extractionProgress.AsyncState as ExtractionProgress;

                if (extractionProgress.IsCompleted)
                {
                    statusMainPanel.Text = progress.extractedFileCount + " files extracted.";

                    System.Windows.Forms.Timer timer = sender as System.Windows.Forms.Timer;
                    timer.Enabled = false;
                    extractionProgress = null;
                }
                else
                {
                    statusMainPanel.Text = progress.extractedFileCount + " of " + progress.totalFileCount + " files extracted.";
                }
            }
            else
            {
                int fileCount      = fileSystemViewController.FileCount;
                int totalFileCount = fileSystemViewController.FileSystem.FileCount;

                if (totalFileCount > 0 && fileCount >= totalFileCount) 
                {
                    statusMainPanel.Text = totalFileCount + " files processed.";

                    System.Windows.Forms.Timer timer = sender as System.Windows.Forms.Timer;
                    timer.Enabled = false;
                }
                else
                {
                    statusMainPanel.Text = fileCount + " of " + totalFileCount + " files processed.";
                }
            }
        }


        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            loadingUpdateTimer.Enabled = false;
            fileSystemViewController.Close();
            extractionManager.Close();
        }

           
        private void directoryTreeView_Enter(object sender, EventArgs e)
        {
            UpdateExtractionMenus(directoryTreeView);
        }


        private void directoryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateExtractionMenus(directoryTreeView);
        }


        private void fileListView_Enter(object sender, EventArgs e)
        {
            UpdateExtractionMenus(fileListView);
        }


        private void fileListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateExtractionMenus(fileListView);
        }


        private void UpdateExtractionMenus(object sender)
        {
            bool enabled;
            string text = string.Empty;

            if (sender == directoryTreeView)
            {
                enabled = directoryTreeView.SelectedNode != null && extractionProgress == null;
                if (enabled)
                {
                    text = directoryTreeView.SelectedNode.Text;
                }
            }
            else
            {
                int selectedItemCount = fileListView.SelectedItems.Count;

                enabled = selectedItemCount > 0 && extractionProgress == null;
                if (enabled)
                {
                    if (selectedItemCount == 1)
                    {
                        text = fileListView.SelectedItems[0].Text;
                    }
                    else
                    {
                        text = selectedItemCount + " items";
                    }
                }
            }

            if (enabled)
            {
                extractToolStripMenuItem.Text = "Extract " + text + "...";
                extractToolStripContextMenuItem.Text = "Extract " + text + "...";
                extractWithPathInformationToolStripMenuItem.Text = "Extract " + text + " with path information...";
                extractWithPathInformationToolStripContextMenuItem.Text = "Extract " + text + " with path information...";
            }
            else
            {
                extractToolStripMenuItem.Text = "Extract...";
                extractToolStripContextMenuItem.Text = "Extract...";
                extractWithPathInformationToolStripMenuItem.Text = "Extract with path information...";
                extractWithPathInformationToolStripContextMenuItem.Text = "Extract with path information...";
            }

            extractToolStripMenuItem.Enabled = enabled;
            extractToolStripContextMenuItem.Enabled = enabled;
            extractWithPathInformationToolStripMenuItem.Enabled = enabled;
            extractWithPathInformationToolStripContextMenuItem.Enabled = enabled;


            bool visible;

            if (sender == directoryTreeView)
            {
                enabled = false;
                visible = false;
            }
            else
            {
                enabled = false;
                visible = false;

                if (fileListView.SelectedItems.Count == 1)
                {
                    Eq2FileSystemInfo item = fileListView.SelectedItems[0].Tag as Eq2FileSystemInfo;

                    if (item is Eq2FileInfo)
                    {
                        Eq2FileInfo file = item as Eq2FileInfo;

                        if (TextureDecryptor.CanDecrypt(file))
                        {
                            enabled = true;
                            text    = file.Name;
                        }

                        visible = StringComparer.InvariantCultureIgnoreCase.Equals(file.DirectoryName, "nrvobm");
                    }
                }
            }

            if (enabled)
            {
                decryptToolStripContextMenuItem.Text = "Decrypt and extract " + text + "...";
            }
            else
            {
                decryptToolStripContextMenuItem.Text = "Decrypt and extract...";
            }

            decryptToolStripContextMenuItem.Enabled = enabled;
            decryptToolStripContextMenuItem.Visible = visible;
        }

        
        private void ExtractTreeViewSelection(string outputPath, bool withPathInfo)
        {
            if (directoryTreeView.SelectedNode == null) return;

            Eq2FileSystemInfo[] children = null;

            children    = new Eq2FileSystemInfo[1];
            children[0] = directoryTreeView.SelectedNode.Tag as Eq2DirectoryInfo;

            // Make sure the path ends with a directory separator char.
            if (outputPath[outputPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                outputPath += Path.DirectorySeparatorChar;
            }

            string newOutputPath = outputPath;
            if (withPathInfo)
            {
                Eq2DirectoryInfo directory           = directoryTreeView.SelectedNode.Tag as Eq2DirectoryInfo;
                Eq2DirectoryInfo parentDirectory     = directory.Parent;
                string           parentDirectoryName = parentDirectory != null ? parentDirectory.FullName : string.Empty;

                newOutputPath = outputPath + parentDirectoryName.Replace('/', Path.DirectorySeparatorChar);
            }

            if (newOutputPath[newOutputPath.Length-1] != Path.DirectorySeparatorChar)
            {
                newOutputPath += Path.DirectorySeparatorChar;
            }

            loadingUpdateTimer.Enabled = true;
            extractionProgress = extractionManager.BeginExtract(children, newOutputPath, OnFileExtracted, new ExtractionProgress(children));

            UpdateExtractionMenus(directoryTreeView);
        }


        private void ExtractListViewSelection(string outputPath, bool withPathInfo)
        {
            if (fileListView.SelectedItems.Count == 0) return;

            Eq2FileSystemInfo[] children = null;

            children = new Eq2FileSystemInfo[fileListView.SelectedItems.Count];
            for (int i = 0; i < children.Length; ++i)
            {
                children[i] = fileListView.SelectedItems[i].Tag as Eq2FileSystemInfo;
            }

            // Make sure the path ends with a directory separator char.
            if (outputPath[outputPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                outputPath += Path.DirectorySeparatorChar;
            }

            string newOutputPath = outputPath;
            if (withPathInfo)
            {
                Eq2DirectoryInfo parentDirectory = null;
    
                if (children[0] is Eq2FileInfo)
                {
                    Eq2FileInfo file = children[0] as Eq2FileInfo;
                    parentDirectory = file.Directory;
                }
                else if (children[0] is Eq2DirectoryInfo)
                {
                    Eq2DirectoryInfo directory = children[0] as Eq2DirectoryInfo;
                    parentDirectory = directory.Parent;
                }

                newOutputPath = outputPath + parentDirectory.FullName.Replace('/', Path.DirectorySeparatorChar);
            }

            if (newOutputPath[newOutputPath.Length-1] != Path.DirectorySeparatorChar)
            {
                newOutputPath += Path.DirectorySeparatorChar;
            }

            loadingUpdateTimer.Enabled = true;
            extractionProgress = extractionManager.BeginExtract(children, newOutputPath, OnFileExtracted, new ExtractionProgress(children));

            UpdateExtractionMenus(fileListView);
        }


        private void OnFileExtracted(IAsyncResult result)
        {
            ExtractionProgress progress = result.AsyncState as ExtractionProgress;

            ++progress.extractedFileCount;
        }


        private void fileListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenu.Tag = fileListView;
                UpdateExtractionMenus(fileListView);
            }
        }


        private void directoryTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = directoryTreeView.GetNodeAt(e.Location);
                if (node != null) directoryTreeView.SelectedNode = node;

                contextMenu.Tag = directoryTreeView;
                UpdateExtractionMenus(directoryTreeView);
            }
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ApplicationName + "\n\n" + "Send any feedback to blaz@blazlabs.com", "About...");
        }
        #endregion


        #region Types
        private class ExtractionProgress
        {
            public ExtractionProgress(Eq2FileSystemInfo[] children)
            {
                totalFileCount = 0;

                foreach (Eq2FileSystemInfo child in children)
                {
                    totalFileCount += GetChildrenCount(child);
                }
            }


            private int GetChildrenCount(Eq2FileSystemInfo parent)
            {
                int count = 0;

                if (parent is Eq2FileInfo)
                {
                    ++count;    
                }
                else if (parent is Eq2DirectoryInfo)
                {
                    Eq2DirectoryInfo directory = parent as Eq2DirectoryInfo;

                    count += directory.FileCount;

                    Eq2DirectoryInfo[] subdirectories = directory.GetDirectories();
                    foreach (Eq2DirectoryInfo subdirectory in subdirectories)
                    {
                        count += GetChildrenCount(subdirectory);
                    }
                }

                return count;
            }


            public int extractedFileCount;
            public int totalFileCount;
        }
        #endregion


        #region Fields
        private System.Windows.Forms.Timer  loadingUpdateTimer;
        private FileSystemViewController    fileSystemViewController;
        private ExtractionManager           extractionManager;
        private IAsyncResult                extractionProgress;

        #region Constants
        private static string ApplicationName       = "Eq2VpkTool v1.2.3";
        private static string ApplicationWebsite    = "http://eq2.blazlabs.com";
        private static string ConfigurationFileName = "Configuration.xml";
        #endregion

        #endregion
    }
}