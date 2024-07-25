//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class TabCollection : IList<Tab>, IEnumerable<Tab>, ICollection<Tab>
    {
        private List<Tab> _contents = new List<Tab>();
        private int _count;
        private Tab _tab;

        public Tab Tab
        {
            get
            {
                return this._tab;
            }

            set
            {
                this._tab = value;
            }
        }

        public TabCollection()
        {
            this._count = 0;
            // _contents = New ArrayList

        }

        public void Add(Tab item)
        {
            this._contents.Add(item);
            this._count = this._count + 1;
        }

        public void Clear()
        {
            this._contents = null;
            this._count = 0;

        }

        public bool Contains(Tab item)
        {
            bool inList = false;

            int i = 0;
            for (i = 0; i <= this.Count; i++)
            {

                if (((Tab)this._contents[i]).Text.ToLower() == item.Text.ToLower())
                {
                    inList = true;
                    break;
                }
            }

            return inList;
        }

        public void CopyTo(Tab[] array, int arrayIndex)
        {
            int j = arrayIndex;
            int i = 0;
            for (i = 0; i <= this.Count; i++)
            {
                array.SetValue(this._contents[i], j);
                j = j + 1;
            }

        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(Tab item)
        {
            this.RemoveAt(this.IndexOf(item));
            return true;
        }

        IEnumerator<Tab> IEnumerable<Tab>.GetEnumerator()
        {
            return new TabEnum(this._contents);
        }

        public int IndexOf(Tab item)
        {
            int itemIndex = -1;

            int i = 0;
            for (i = 0; i <= this.Count; i++)
            {

                if (((Tab)this._contents[i]).Text.ToLower() == item.Text.ToLower())
                {

                    itemIndex = i;
                    break;

                }

            }

            return itemIndex;

        }

        public void Insert(int index, Tab item)
        {
            this._count = this._count + 1;

            int i = 0;
            for (i = this.Count - 1; i <= index; i++)
            {
                this._contents[i] = this._contents[i - 1];
            }

            this._contents[index] = item;

        }

        public Tab this[int index]
        {
            get
            {
                return (Tab)this._contents[index];
            }

            set
            {
                this._contents[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < this.Count)
            {

                int i = 0;
                for (i = index; i < this.Count; i++)
                {

                    this._contents[i] = this._contents[i + 1];
                }

                this._count = this._count - 1;

            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._contents.GetEnumerator();
        }
    }

    public class TabEnum : IEnumerator<Tab>
    {

        public List<Tab> _tabs;

        private int position = -1;

        public TabEnum(List<Tab> list)
        {
            this._tabs = list;
        }

        public bool MoveNext()
        {
            this.position = this.position + 1;
            return this.position < this._tabs.Count;
        }

        public void Reset()
        {
            this.position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                try
                {
                    return this._tabs[this.position];
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        Tab IEnumerator<Tab>.Current
        {
            get
            {
                return this._tabs[this.position];
            }
        }

        public void Dispose()
        {
            // this.Dispose();
        }
    }

}
