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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Eq2FileSystemInfo = Everquest2.IO.FileSystemInfo;
using Eq2FileInfo       = Everquest2.IO.FileInfo;
using Eq2DirectoryInfo  = Everquest2.IO.DirectoryInfo;

#endregion

namespace Eq2VpkTool
{
    /// <summary>
    /// Manages the image list used to represent folder and file icons on the TreeView and ListView controls.
    /// </summary>
    public class IconManager
    {
        #region Methods

        #region Constructors
        public IconManager(ImageList imageList)
        {
            this.imageList = imageList;

            // Create the directory icon in advance.
            imageList.Images.Add(GetIcon("directory", true, IconSize.Small));
        }
        #endregion


        /// <summary>
        /// Returns the index of the directory icon.
        /// </summary>
        /// <returns>Index in the image list of the directory icon.</returns>
        public int GetDirectoryImageIndex()
        {
            return imageList.Images.IndexOfKey("directory");
        }


        /// <summary>
        /// Returns the index of the icon that best represents the provided item.
        /// </summary>
        /// <param name="item">File or directory to get the icon from.</param>
        /// <returns>Index in the image list of the appropriate icon.</returns>
        public int GetImageIndex(Eq2FileSystemInfo item)
        {
            bool isDirectory = item is Eq2DirectoryInfo;

            string name;

            if (isDirectory)
            {
                // All directories share the same key on the image list.
                name = "directory";
            }
            else
            {
                Eq2FileInfo file = item as Eq2FileInfo;

                // Get the image list key from the file name. The extension will be used for this purpose.
                name = System.IO.Path.GetExtension(file.Name);
            }

            int imageIndex;

            lock (imageList)
            {
                if (!imageList.Images.ContainsKey(name))
                {
                    // If the image list doesn't contain an icon for this item, get it from the OS and cache it.
                    imageList.Images.Add(name, GetIcon(name, isDirectory, IconSize.Small));
                }

                imageIndex = imageList.Images.IndexOfKey(name);
            }

            return imageIndex;
        }

        #endregion


        // The following code is borrowed from http://www.sliver.com/dotnet/FileSystemIcons/
        // Slightly modified to not require physical files on disk.
        #region Icon querying code

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int    iIcon;
            public int    dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        }

        private enum IconSize
        {
            Large,
            Small
        }

        private const uint SHGFI_LARGEICON          = 0x0;
        private const uint SHGFI_SMALLICON          = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES  = 0x10;
        private const uint SHGFI_ICON               = 0x100;
        private const uint SHGFI_SYSICONINDEX       = 0x4000;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL    = 0x80;

        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string path, uint fileAttributes, out SHFILEINFO psfi, uint fileInfo, uint flags);

        private static Icon GetIcon(string name, bool isDirectory, IconSize iconSize)
        {
            SHFILEINFO info = new SHFILEINFO();

            uint sizeFlags  = iconSize == IconSize.Large ? SHGFI_LARGEICON : SHGFI_SMALLICON;
            uint attributes = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;
            uint flags      = SHGFI_USEFILEATTRIBUTES | SHGFI_SYSICONINDEX | SHGFI_ICON | sizeFlags;

            int hTcdf = SHGetFileInfo(name, attributes, out info, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);

            return Icon.FromHandle(info.hIcon);
        }

        #endregion


        #region Properties
        public ImageList ImageList
        {
            get { lock (imageList) return imageList; }
        }
        #endregion


        #region Fields
        private ImageList imageList;
        #endregion
    }
}
