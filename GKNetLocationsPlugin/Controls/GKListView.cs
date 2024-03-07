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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace GKNetLocationsPlugin.Controls
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class GKListItem : ListViewItem
    {
        protected object fValue;

        public GKListItem(object itemValue, object tag)
        {
            fValue = itemValue;
            Text = ToString();
            Tag = tag;
        }

        public override string ToString()
        {
            return (fValue == null) ? string.Empty : fValue.ToString();
        }

        public int CompareTo(object obj)
        {
            GKListItem otherItem = obj as GKListItem;
            if (otherItem == null) {
                return -1;
            }

            IComparable cv1 = fValue as IComparable;
            IComparable cv2 = otherItem.fValue as IComparable;

            int compRes;
            if (cv1 != null && cv2 != null) {
                compRes = cv1.CompareTo(cv2);
            } else if (cv1 != null) {
                compRes = -1;
            } else if (cv2 != null) {
                compRes = 1;
            } else {
                compRes = 0;
            }
            return compRes;
        }

        public void SetSubItem(int index, object value)
        {
            SubItems[index] = new GKListSubItem(value);
        }
    }


    public class GKListSubItem : ListViewItem.ListViewSubItem, IComparable
    {
        protected object fValue;

        public GKListSubItem(object itemValue)
        {
            fValue = itemValue;
            Text = ToString();
        }

        public override string ToString()
        {
            return (fValue == null) ? string.Empty : fValue.ToString();
        }

        public int CompareTo(object obj)
        {
            var otherItem = obj as GKListSubItem;
            if (otherItem == null) {
                return -1;
            }

            if (fValue is string && otherItem.fValue is string) {
                return GKListView.StrCompareEx((string)fValue, (string)otherItem.fValue);
            }

            IComparable cv1 = fValue as IComparable;
            IComparable cv2 = otherItem.fValue as IComparable;

            int compRes;
            if (cv1 != null && cv2 != null) {
                compRes = cv1.CompareTo(cv2);
            } else if (cv1 != null) {
                compRes = -1;
            } else if (cv2 != null) {
                compRes = 1;
            } else {
                compRes = 0;
            }
            return compRes;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GKListView : ListView
    {
        private class LVColumnSorter : IComparer
        {
            private readonly GKListView fOwner;

            public LVColumnSorter(GKListView owner)
            {
                fOwner = owner;
            }

            public int Compare(object x, object y)
            {
                int result = 0;

                int sortColumn = fOwner.fSortColumn;
                SortOrder sortOrder = fOwner.fSortOrder;

                if (sortOrder != SortOrder.None && sortColumn >= 0) {
                    ListViewItem item1 = (ListViewItem)x;
                    ListViewItem item2 = (ListViewItem)y;

                    if (sortColumn == 0) {
                        if (item1 is IComparable && item2 is IComparable) {
                            IComparable eitem1 = (IComparable)x;
                            IComparable eitem2 = (IComparable)y;
                            result = eitem1.CompareTo(eitem2);
                        } else {
                            result = StrCompareEx(item1.Text, item2.Text);
                        }
                    } else if (sortColumn < item1.SubItems.Count && sortColumn < item2.SubItems.Count) {
                        ListViewItem.ListViewSubItem subitem1 = item1.SubItems[sortColumn];
                        ListViewItem.ListViewSubItem subitem2 = item2.SubItems[sortColumn];

                        if (subitem1 is IComparable && subitem2 is IComparable) {
                            IComparable sub1 = (IComparable)subitem1;
                            IComparable sub2 = (IComparable)subitem2;
                            result = sub1.CompareTo(sub2);
                        } else {
                            result = StrCompareEx(subitem1.Text, subitem2.Text);
                        }
                    }

                    if (sortOrder == SortOrder.Descending) {
                        result = -result;
                    }
                }

                return result;
            }
        }

        public static int StrCompareEx(string str1, string str2)
        {
            double val1, val2;
            bool v1 = double.TryParse(str1, out val1);
            bool v2 = double.TryParse(str2, out val2);

            int result;
            if (v1 && v2) {
                if (val1 < val2) {
                    result = -1;
                } else if (val1 > val2) {
                    result = +1;
                } else {
                    result = 0;
                }
            } else {
                result = string.Compare(str1, str2, false);
                if (str1 != "" && str2 == "") {
                    result = -1;
                } else if (str1 == "" && str2 != "") {
                    result = +1;
                }
            }
            return result;
        }

        private readonly LVColumnSorter fColumnSorter;

        private GKListItem[] fCache;
        private int fCacheFirstItem;
        private int fSortColumn;
        private SortOrder fSortOrder;
        private int fUpdateCount;


        public int SelectedIndex
        {
            get {
                return Items.IndexOf(GetSelectedItem());
            }
            set {
                SelectItem(value);
            }
        }

        public int SortColumn
        {
            get { return fSortColumn; }
            set { fSortColumn = value; }
        }

        public SortOrder SortOrder
        {
            get { return fSortOrder; }
            set { fSortOrder = value; }
        }


        public event EventHandler ItemsUpdated;


        public GKListView()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            // Enable the OnNotifyMessage event so we get a chance to filter out
            // Windows messages before they get to the form's WndProc
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            OwnerDraw = true;
            HideSelection = false;
            LabelEdit = false;
            FullRowSelect = true;
            View = View.Details;

            fSortColumn = 0;
            fSortOrder = SortOrder.None;
            fColumnSorter = new LVColumnSorter(this);

            ListViewItemSorter = fColumnSorter;
        }

        public void Activate()
        {
            Select();
        }

        public new void BeginUpdate()
        {
            if (fUpdateCount == 0) {
                #if !MONO
                ListViewItemSorter = null;
                #endif
                base.BeginUpdate();
            }
            fUpdateCount++;
        }

        public new void EndUpdate()
        {
            fUpdateCount--;
            if (fUpdateCount == 0) {
                base.EndUpdate();
                #if !MONO
                ListViewItemSorter = fColumnSorter;
                #endif
            }
        }

        protected SortOrder GetColumnSortOrder(int columnIndex)
        {
            return (fSortColumn == columnIndex) ? fSortOrder : SortOrder.None;
        }

        public void SetSortColumn(int sortColumn, bool checkOrder = true)
        {
            int prevColumn = fSortColumn;
            if (prevColumn == sortColumn && checkOrder) {
                SortOrder prevOrder = GetColumnSortOrder(sortColumn);
                fSortOrder = (prevOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }

            fSortColumn = sortColumn;
            SortContents(true);
        }

        public void Sort(int sortColumn, SortOrder sortOrder)
        {
            fSortOrder = sortOrder;
            SetSortColumn(sortColumn, false);
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            SetSortColumn(e.Column);

            // we use Refresh() because only Invalidate() isn't update header's area
            Refresh();

            base.OnColumnClick(e);
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = false;

            using (var sf = new StringFormat()) {
                Graphics gfx = e.Graphics;
                Rectangle rt = e.Bounds;

                if ((e.State & ListViewItemStates.Selected) == ListViewItemStates.Selected) {
                    DrawHeaderBackground(gfx, Color.Gray, rt);
                } else {
                    DrawHeaderBackground(gfx, Color.Silver, rt);
                }

                switch (e.Header.TextAlign) {
                    case HorizontalAlignment.Left:
                        sf.Alignment = StringAlignment.Near;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                }

                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                int w = TextRenderer.MeasureText(" ", Font).Width;
                rt.Inflate(-(w / 5), 0);

                using (var brush = new SolidBrush(Color.Black)) {
                    gfx.DrawString(e.Header.Text, Font, brush, rt, sf);
                }

                switch (GetColumnSortOrder(e.ColumnIndex)) {
                    case SortOrder.Ascending:
                        DrawSortArrow(gfx, rt, "▲");
                        break;
                    case SortOrder.Descending:
                        DrawSortArrow(gfx, rt, "▼");
                        break;
                }
            }

            base.OnDrawColumnHeader(e);
        }

        private void DrawSortArrow(Graphics gfx, Rectangle rt, string arrow)
        {
            using (var fnt = new Font(Font.FontFamily, Font.SizeInPoints * 0.6f, FontStyle.Regular)) {
                float aw = gfx.MeasureString(arrow, fnt).Width;
                float x = rt.Left + (rt.Width - aw) / 2.0f;
                gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
                gfx.DrawString(arrow, fnt, Brushes.Black, x, rt.Top);
            }
        }

        private static void DrawHeaderBackground(Graphics g, Color backColor, Rectangle bounds, bool classic3d = false)
        {
            using (Brush brush = new SolidBrush(backColor)) {
                g.FillRectangle(brush, bounds);
            }

            Rectangle rect = bounds;
            if (classic3d) {
                rect.Width--;
                rect.Height--;
                g.DrawRectangle(SystemPens.ControlDarkDark, rect);
                rect.Width--;
                rect.Height--;
                g.DrawLine(SystemPens.ControlLightLight, rect.X, rect.Y, rect.Right, rect.Y);
                g.DrawLine(SystemPens.ControlLightLight, rect.X, rect.Y, rect.X, rect.Bottom);
                g.DrawLine(SystemPens.ControlDark, rect.X + 1, rect.Bottom, rect.Right, rect.Bottom);
                g.DrawLine(SystemPens.ControlDark, rect.Right, rect.Y + 1, rect.Right, rect.Bottom);
            } else {
                rect.Width--;
                rect.Height--;
                g.DrawLine(SystemPens.ControlDark, rect.Right, rect.Y + 1, rect.Right, rect.Bottom - 1);
            }
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawItem(e);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawSubItem(e);
        }

        #region Virtual mode with ListSource

        private void SortContents(bool restoreSelected)
        {
            Sort();
        }

        private void DoItemsUpdated()
        {
            var eventHandler = ItemsUpdated;
            if (eventHandler != null) eventHandler(this, new EventArgs());
        }

        #endregion

        #region Public methods

        public new void Clear()
        {
            // identical clearing of columns and items
            base.Clear();
        }

        public void ClearColumns()
        {
            Columns.Clear();
        }

        public void AddCheckedColumn(string caption, int width, bool autoSize = false)
        {
            CheckBoxes = true;
            AddColumn(caption, width, autoSize);
        }

        public void AddColumn(string caption, int width, bool autoSize = false)
        {
            if (autoSize) width = -1;
            Columns.Add(caption, width, HorizontalAlignment.Left);
        }

        public void AddColumn(string caption, int width, bool autoSize, HorizontalAlignment textAlign)
        {
            if (autoSize) width = -1;
            Columns.Add(caption, width, textAlign);
        }

        public void SetColumnCaption(int index, string caption)
        {
            Columns[index].Text = caption;
        }

        public void ResizeColumn(int columnIndex)
        {
            try {
                if (columnIndex >= 0 && Items.Count > 0) {
                    AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.ColumnContent);

                    if (Columns[columnIndex].Width < 20) {
                        AutoResizeColumn(columnIndex, ColumnHeaderAutoResizeStyle.HeaderSize);
                    }
                }
            } catch (Exception ex) {
            }
        }

        public void ResizeColumns()
        {
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public GKListItem AddItem(object rowData, bool isChecked, params object[] columnValues)
        {
            var item = AddItem(rowData, columnValues);
            if (CheckBoxes) {
                ((GKListItem)item).Checked = isChecked;
            }
            return item;
        }

        public GKListItem AddItem(object rowData, params object[] columnValues)
        {
            var result = new GKListItem(columnValues[0], rowData);
            Items.Add(result);

            int num = columnValues.Length;
            if (num > 1) {
                for (int i = 1; i < num; i++) {
                    object val = columnValues[i];
                    result.SubItems.Add(new GKListSubItem(val));
                }
            }

            return result;
        }

        public IList<object> GetSelectedItems()
        {
            try {
                var result = new List<object>();

                if (!VirtualMode) {
                    int num = SelectedItems.Count;
                    for (int i = 0; i < num; i++) {
                        var lvItem = SelectedItems[i] as GKListItem;
                        result.Add(lvItem.Tag);
                    }
                } else {
                }

                return result;
            } catch (Exception ex) {
                return null;
            }
        }

        public GKListItem GetSelectedItem()
        {
            GKListItem result;

            if (SelectedItems.Count <= 0) {
                result = null;
            } else {
                result = (SelectedItems[0] as GKListItem);
            }

            return result;
        }

        public object GetSelectedData()
        {
            try {
                object result = null;

                if (!VirtualMode) {
                    GKListItem item = GetSelectedItem();
                    if (item != null) result = item.Tag;
                } else {
                }

                return result;
            } catch (Exception ex) {
                return null;
            }
        }

        private void SelectItem(int index, GKListItem item)
        {
            if (item != null) {
                SelectedIndices.Clear();
                item.Selected = true;

                // in Mono `item.EnsureVisible()` doesn't work
                EnsureVisible(index);
            }
        }

        public void SelectItem(int index)
        {
            if (index == -1) {
                index = Items.Count - 1;
            }

            if (index >= 0 && index < Items.Count) {
                var item = (GKListItem)Items[index];
                SelectItem(index, item);
            }
        }

        public void SelectItem(object rowData)
        {
            if (rowData == null)
                return;

            try {
                int num = Items.Count;
                for (int i = 0; i < num; i++) {
                    var item = (GKListItem)Items[i];
                    if (item.Tag == rowData) {
                        SelectItem(i, item);
                        return;
                    }
                }
            } catch (Exception ex) {
            }
        }

        #endregion
    }
}
