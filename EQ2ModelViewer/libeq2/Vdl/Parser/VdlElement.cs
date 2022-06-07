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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Everquest2.Vdl.Parser
{
    public abstract class VdlElement : IEnumerable<VdlElement>
    {
        internal abstract void                    AddElement    (VdlElement child);
        public   abstract void                    WriteXml      (System.Xml.XmlWriter writer);
        public   abstract IEnumerator<VdlElement> GetEnumerator ();


        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<VdlElement>).GetEnumerator();
        }

        
        public string Name
        {
            get { return name;  }
            set { name = value; }
        }

        
        private string name = null;
    }

}

/* EOF */