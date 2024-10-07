// Copyright (c) 2013-2024 by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class TabCollection : IList<Tab>, IEnumerable<Tab>, ICollection<Tab>
    {
        private List<Tab> contents = new List<Tab>();
        private int count;
        private Tab tab;

        public Tab Tab
        {
            get
            {
                return this.tab;
            }

            set
            {
                this.tab = value;
            }
        }

        public TabCollection()
        {
            this.count = 0;

            // _contents = New ArrayList
        }

        public void Add(Tab item)
        {
            this.contents.Add(item);
            this.count = this.count + 1;
        }

        public void Clear()
        {
            this.contents = null;
            this.count = 0;
        }

        public bool Contains(Tab item)
        {
            bool inList = false;

            int i = 0;
            for (i = 0; i <= this.Count; i++)
            {
                if (((Tab)this.contents[i]).Text.ToLower() == item.Text.ToLower())
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
                array.SetValue(this.contents[i], j);
                j = j + 1;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
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
            return new TabEnum(this.contents);
        }

        public int IndexOf(Tab item)
        {
            int itemIndex = -1;

            int i = 0;
            for (i = 0; i <= this.Count; i++)
            {
                if (((Tab)this.contents[i]).Text.ToLower() == item.Text.ToLower())
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        public void Insert(int index, Tab item)
        {
            this.count = this.count + 1;

            int i = 0;
            for (i = this.Count - 1; i <= index; i++)
            {
                this.contents[i] = this.contents[i - 1];
            }

            this.contents[index] = item;
        }

        public Tab this[int index]
        {
            get
            {
                return (Tab)this.contents[index];
            }

            set
            {
                this.contents[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < this.Count)
            {
                int i = 0;
                for (i = index; i < this.Count; i++)
                {
                    this.contents[i] = this.contents[i + 1];
                }

                this.count = this.count - 1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.contents.GetEnumerator();
        }
    }

    public class TabEnum : IEnumerator<Tab>
    {
        public List<Tab> tabs;

        private int position = -1;

        public TabEnum(List<Tab> list)
        {
            this.tabs = list;
        }

        public bool MoveNext()
        {
            this.position = this.position + 1;
            return this.position < this.tabs.Count;
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
                    return this.tabs[this.position];
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
                return this.tabs[this.position];
            }
        }

        public void Dispose()
        {
            // this.Dispose();
        }
    }
}
