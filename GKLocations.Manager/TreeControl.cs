/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using GKLocations.Core;

namespace GKLocations.Manager
{
    public partial class TreeControl : UserControl
    {
        private ICore fCore;


        public ICore Core
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
            InitializeComponent();
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
                var curNode = AddNode(ownerNode, item.Name, null);

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
