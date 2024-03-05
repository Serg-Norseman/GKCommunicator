/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin.Editor
{
    public partial class TreeControl : UserControl
    {
        private TreeView treeView1;

        private GKLCore fCore;


        public GKLCore Core
        {
            get {
                return fCore;
            }
            set {
                fCore = value;
            }
        }


        public TreeControl()
        {
            treeView1 = new TreeView();
            treeView1.Dock = DockStyle.Fill;
            Controls.Add(treeView1);
        }

        public void UpdateContent(string lang)
        {
            var locations = fCore.Database.QueryLocationsEx(lang);

            FillNodes(null, locations, null);

            treeView1.ExpandAll();
        }

        private void FillNodes(TreeNode ownerNode, IList<QLocation> source, string ownerGUID)
        {
            var locItems = FindLocations(source, ownerGUID);
            foreach (var item in locItems) {
                string dateExt = string.IsNullOrEmpty(item.NameDate) ? string.Empty : string.Format(" [{0}]", item.NameDate);

                var curNode = AddNode(ownerNode, item.Name + dateExt, item.LocationGUID);

                FillNodes(curNode, source, item.LocationGUID);
            }
        }

        private IEnumerable<QLocation> FindLocations(IList<QLocation> source, string owner)
        {
            return source.Where(p => p.OwnerGUID == owner);
        }

        public TreeNode AddNode(TreeNode parent, string name, object tag)
        {
            var node = new TreeNode(name);
            node.Tag = tag;

            if (parent == null) {
                treeView1.Nodes.Add(node);
            } else {
                parent.Nodes.Add(node);
            }

            return node;
        }

        public void Expand(TreeNode node)
        {
            TreeNode treeNode = node as TreeNode;
            if (treeNode != null) {
                treeNode.ExpandAll();
            }
        }

        public object GetSelectedData()
        {
            TreeNode node = treeView1.SelectedNode as TreeNode;
            return (node == null) ? null : node.Tag;
        }

        public void Clear()
        {
            treeView1.Nodes.Clear();
        }

        public void BeginUpdate()
        {
            treeView1.BeginUpdate();
        }

        public void EndUpdate()
        {
            treeView1.EndUpdate();
        }
    }
}
