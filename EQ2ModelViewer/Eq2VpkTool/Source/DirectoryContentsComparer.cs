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
using System.Collections;
using System.Windows.Forms;

using Eq2FileSystemInfo = Everquest2.IO.FileSystemInfo;
using Eq2FileInfo       = Everquest2.IO.FileInfo;
using Eq2DirectoryInfo  = Everquest2.IO.DirectoryInfo;

#endregion

namespace Eq2VpkTool
{
    /// <summary>
    /// Compares two file system items for sorting the list view.
    /// Directories always compare lower than files.
    /// Directories with directories and files with files compare lexicographically.
    /// </summary>
    public class DirectoryContentsComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            ListViewItem item1 = x as ListViewItem;
            ListViewItem item2 = y as ListViewItem;

            // The item tags could be null because some of the ListViewItem's passed to this function
            // are probably newly created items.
            if (item1.Tag == null) return  1;
            if (item2.Tag == null) return -1;

            Eq2FileSystemInfo child1 = item1.Tag as Eq2FileSystemInfo;
            Eq2FileSystemInfo child2 = item2.Tag as Eq2FileSystemInfo;

            if (child1 is Eq2DirectoryInfo)
            {
                if (child2 is Eq2DirectoryInfo)
                {
                    Eq2DirectoryInfo directory1 = child1 as Eq2DirectoryInfo;
                    Eq2DirectoryInfo directory2 = child2 as Eq2DirectoryInfo;

                    return directory1.Name.CompareTo(directory2.Name);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (child2 is Eq2DirectoryInfo)
                {
                    return 1;
                }
                else
                {
                    Eq2FileInfo file1 = child1 as Eq2FileInfo;
                    Eq2FileInfo file2 = child2 as Eq2FileInfo;

                    return file1.Name.CompareTo(file2.Name);
                }
            }
        }
    }
}
