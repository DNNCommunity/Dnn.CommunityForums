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
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), Designer("DotNetNuke.Modules.ActiveForums.Controls.ActiveGridDesigner"), ToolboxData("<{0}:activegrid runat=server><headertemplate></headertemplate><itemtemplate></itemtemplate><footertemplate></footertemplate></{0}:activegrid>")]
    public class ActiveGrid : CompositeControl
    {

        #region Declarations
        private Literal lit;
        private System.Web.UI.HtmlControls.HtmlGenericControl div;
        protected Callback cb;
        private ItemTemplateContents itemTemplate;
        private ItemTemplateContents headerTemplate;
        private ItemTemplateContents footerTemplate;
        private IDataReader datasource;
        private string rowDelimiter = "^";
        private string colDelimiter = "|";
        private int pageSize = 10;
        private string itemStyle = "amdatarow";
        private string altItemStyle = "amrowalternating";
        private string selectedStyle = "amrowselected";
        private bool showPager = true;
        private string pagerText = "Page {0} of {1}";
        private string imagePath = string.Empty;
        private int pagerPages = 10;
        private string defaultColumn;
        private string defaultParams = string.Empty;
        private Sort defaultSort = Sort.Descending;
        private string cssPagerInfo = "ampagerinfo";
        private string cssPagerItem = "amPageritem";
        private string cssPagerItem2 = "ampagertem2";
        private string cssPagerCurrentNumber = "ampagernumber_current";
        private string cssPagerNumber = "ampagernumber";
        private string spacerImage = string.Empty;
        private string ascImage = string.Empty;
        private string descImage = string.Empty;
        private bool loadOnRender = true;

        public enum Sort : int
        {
            Ascending,
            Descending,
        }

        public event ItemBoundEventHandler ItemBound;

        public delegate void ItemBoundEventHandler(object sender, ItemBoundEventArgs e);
        #endregion

        #region Properties
        /// <summary>
        /// Item Template - Must include a table row.
        /// </summary>
        [Description("Initial content to render."), DefaultValue(null, ""), Browsable(false), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public ItemTemplateContents ItemTemplate
        {
            get
            {
                return this.itemTemplate;
            }

            set
            {
                this.itemTemplate = value;
            }
        }

        /// <summary>
        /// Header Template - Must include opening table tag and first table row.
        /// </summary>
        /// , DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)
        [Description("Initial content to render."), DefaultValue(null, ""), Browsable(false), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public ItemTemplateContents HeaderTemplate
        {
            get
            {
                return this.headerTemplate;
            }

            set
            {
                this.headerTemplate = value;
            }
        }

        /// <summary>
        /// Footer Template - Must include closing table tag.
        /// </summary>
        /// , DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)
        [Description("Initial content to render."), DefaultValue(null, ""), Browsable(false), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public ItemTemplateContents FooterTemplate
        {
            get
            {
                return this.footerTemplate;
            }

            set
            {
                this.footerTemplate = value;
            }
        }

        /// <summary>
        /// String delimiter to separate row data in callback.  Ensure none of your data will include this string.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string RowDelimiter
        {
            get
            {
                return this.rowDelimiter;
            }

            set
            {
                this.rowDelimiter = value;
            }
        }

        /// <summary>
        /// String delimiter to separate column data in callback.  Ensure none of your data will include this string.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ColDelimiter
        {
            get
            {
                return this.colDelimiter;
            }

            set
            {
                this.colDelimiter = value;
            }
        }

        /// <summary>
        /// Must be an IDataReader.  First SELECT is total row count.  Second is the data we display.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public IDataReader Datasource
        {
            get
            {
                return this.datasource;
            }

            set
            {
                this.datasource = value;
            }
        }

        /// <summary>
        /// Number of records to show per page.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public int PageSize
        {
            get
            {
                return this.pageSize;
            }

            set
            {
                this.pageSize = value;
            }
        }

        /// <summary>
        /// CSS class name added to Item Template table row.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ItemStyle
        {
            get
            {
                return this.itemStyle;
            }

            set
            {
                this.itemStyle = value;
            }
        }

        /// <summary>
        /// CSS class name added to Item Template every other table row.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string AltItemStyle
        {
            get
            {
                return this.altItemStyle;
            }

            set
            {
                this.altItemStyle = value;
            }
        }

        /// <summary>
        /// CSS class name added to Item Template table row when moused over.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string SelectedStyle
        {
            get
            {
                return this.selectedStyle;
            }

            set
            {
                this.selectedStyle = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public bool ShowPager
        {
            get
            {
                return this.showPager;
            }

            set
            {
                this.showPager = value;
            }
        }

        /// <summary>
        /// Fires a callback immediately after grid is rendered.  True by default.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public bool LoadOnRender
        {
            get
            {
                return this.loadOnRender;
            }

            set
            {
                this.loadOnRender = value;
            }
        }

        /// <summary>
        /// "Page {0} of {1}".
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string PagerText
        {
            get
            {
                return this.pagerText;
            }

            set
            {
                this.pagerText = value;
            }
        }

        /// <summary>
        /// Path to directory for Pager Images.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ImagePath
        {
            get
            {
                return this.imagePath;
            }

            set
            {
                this.imagePath = value;
            }
        }

        /// <summary>
        /// Number of pages.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public int PagerPages
        {
            get
            {
                return this.pagerPages;
            }

            set
            {
                this.pagerPages = value;
            }
        }

        /// <summary>
        /// Default Sort for first load.  Default Descending.
        /// </summary>
        public Sort DefaultSort
        {
            get
            {
                return this.defaultSort;
            }

            set
            {
                this.defaultSort = value;
            }
        }

        /// <summary>
        /// Default Column to sort by.
        /// </summary>
        public string DefaultColumn
        {
            get
            {
                return this.defaultColumn;
            }

            set
            {
                this.defaultColumn = value;
            }
        }

        /// <summary>
        /// Default Parameters to pass in to the callback on first load.
        /// </summary>
        public string DefaultParams
        {
            get
            {
                return this.defaultParams;
            }

            set
            {
                this.defaultParams = value;
            }
        }

        /// <summary>
        /// CSS class for the pager info.
        /// </summary>
        public string CssPagerInfo
        {
            get
            {
                return this.cssPagerInfo;
            }

            set
            {
                this.cssPagerInfo = value;
            }
        }

        /// <summary>
        /// CSS class for every page number except the first.
        /// </summary>
        public string CssPagerItem
        {
            get
            {
                return this.cssPagerItem;
            }

            set
            {
                this.cssPagerItem = value;
            }
        }

        /// <summary>
        /// CSS class for the first page number.
        /// </summary>
        public string CssPagerItem2
        {
            get
            {
                return this.cssPagerItem2;
            }

            set
            {
                this.cssPagerItem2 = value;
            }
        }

        /// <summary>
        /// CSS class for the current page.
        /// </summary>
        public string CssPagerCurrentNumber
        {
            get
            {
                return this.cssPagerCurrentNumber;
            }

            set
            {
                this.cssPagerCurrentNumber = value;
            }
        }

        /// <summary>
        /// CSS class for the page numbers.
        /// </summary>
        public string CssPagerNumber
        {
            get
            {
                return this.cssPagerNumber;
            }

            set
            {
                this.cssPagerNumber = value;
            }
        }

        /// <summary>
        /// URL to spacer image if not using the built in one.
        /// </summary>
        public string SpacerImage
        {
            get
            {
                return this.spacerImage;
            }

            set
            {
                this.spacerImage = value;
            }
        }

        /// <summary>
        /// URL to the Ascending sort image if not using the built in one.
        /// </summary>
        public string AscImage
        {
            get
            {
                return this.ascImage;
            }

            set
            {
                this.ascImage = value;
            }
        }

        /// <summary>
        /// URL to the Descending sort image if not using the built in one.
        /// </summary>
        public string DescImage
        {
            get
            {
                return this.descImage;
            }

            set
            {
                this.descImage = value;
            }
        }

        public string OnComplete
        {
            get
            {
                return this.cb.OnCallbackComplete;
            }

            set
            {
                this.EnsureChildControls();
                this.cb.OnCallbackComplete = value;
            }
        }
        #endregion

        #region Subs/Functions
        protected override void CreateChildControls()
        {
            this.div = new System.Web.UI.HtmlControls.HtmlGenericControl();
            this.div.ID = this.ClientID;
            this.lit = new Literal();
            this.lit.ID = "data_" + this.ClientID;
            this.cb = new Callback();
            if (HttpContext.Current.Request.Params["amagdebug"] == "true" || HttpContext.Current.Request.Params["amdebug"] == "true")
            {
                this.cb.Debug = true;
            }
            else
            {
                this.cb.Debug = false;
            }

            this.cb.ID = "CB_" + this.ClientID;
            this.cb.Attributes.Add("style", "display:none;");
            this.cb.OnCallbackComplete = "function(){" + this.ClientID + ".Build();}";
            this.cb.CallbackEvent += new Callback.CallbackEventHandler(this.RaiseCallback);
            this.ChildControlsCreated = true;
        }

        protected override void Render(HtmlTextWriter output)
        {
            try
            {
                output = new HtmlTextWriter(output, string.Empty);
                string sOutput = this.HeaderTemplate.Text + this.ItemTemplate.Text + this.FooterTemplate.Text;
                this.div.Controls.Add(this.Page.ParseControl(sOutput));
                this.div.RenderControl(output);
                if (this.ShowPager)
                {
                    output.AddAttribute("id", "pager_" + this.ClientID);
                    output.RenderBeginTag(HtmlTextWriterTag.Div);
                    output.RenderEndTag();
                }

                this.cb.RenderControl(output);
                StringBuilder str = new StringBuilder();
                str.Append("<script>");
                str.Append("window." + this.ClientID + "=new ActiveGrid('" + this.ClientID + "');");
                if (this.SpacerImage == string.Empty)
                {
                    this.SpacerImage = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.spacer.gif");
                }

                if (this.AscImage == string.Empty)
                {
                    this.AscImage = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.asc.gif");
                }

                if (this.DescImage == string.Empty)
                {
                    this.DescImage = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.desc.gif");
                }

                str.Append("window." + this.ClientID + ".SpacerImage='" + this.SpacerImage + "';");
                str.Append("window." + this.ClientID + ".AscImage='" + this.AscImage + "';");
                str.Append("window." + this.ClientID + ".DescImage='" + this.DescImage + "';");
                str.Append("window." + this.ClientID + ".SetColumns();");
                if (this.ShowPager)
                {
                    str.Append("window." + this.ClientID + ".Pager=document.getElementById('pager_" + this.ClientID + "');");
                    str.Append("window." + this.ClientID + ".PagerText='" + this.PagerText + "';");
                    str.Append("window." + this.ClientID + ".PagerPages=" + this.PagerPages + ";");
                    str.Append("window." + this.ClientID + ".CssPagerInfo='" + this.CssPagerInfo + "';");
                    str.Append("window." + this.ClientID + ".CssPagerItem='" + this.CssPagerItem + "';");
                    str.Append("window." + this.ClientID + ".CssPagerItem2='" + this.CssPagerItem2 + "';");
                    str.Append("window." + this.ClientID + ".CssPagerCurrentNumber='" + this.CssPagerCurrentNumber + "';");
                    str.Append("window." + this.ClientID + ".CssPagerNumber='" + this.CssPagerNumber + "';");
                }

                str.Append("window." + this.ClientID + ".Column='" + this.DefaultColumn + "';");
                if (this.DefaultSort == Sort.Ascending)
                {
                    str.Append("window." + this.ClientID + ".Sort='ASC';");
                    str.Append("window." + this.ClientID + ".DefaultSort='ASC';");
                }

                str.Append("window." + this.ClientID + ".Params='" + this.DefaultParams + "';");
                str.Append("window." + this.ClientID + ".Width='" + this.Width.ToString() + "';");
                str.Append("window." + this.ClientID + ".ImagePath='" + this.Page.ResolveUrl(this.ImagePath) + "';");
                str.Append("window." + this.ClientID + ".ItemStyle='" + this.ItemStyle + "';");
                str.Append("window." + this.ClientID + ".AltItemStyle='" + this.AltItemStyle + "';");
                str.Append("window." + this.ClientID + ".SelectedStyle='" + this.SelectedStyle + "';");
                str.Append("window." + this.ClientID + ".PageSize=" + this.PageSize.ToString() + ";");
                str.Append("window." + this.ClientID + ".RowDelimiter='" + this.RowDelimiter + "';");
                str.Append("window." + this.ClientID + ".ColDelimiter='" + this.ColDelimiter + "';");
                str.Append("window." + this.ClientID + ".ShowPager=" + this.ShowPager.ToString().ToLower() + ";");
                str.Append("window." + this.ClientID + ".CB='" + this.cb.ClientID + "';");
                if (this.LoadOnRender == true)
                {
                    str.Append("window." + this.ClientID + ".Callback();");
                }

                str.Append("</script>");
                output.Write(str.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Call in a callback to refresh the grid.  Pass in e.Output as the parameter.
        /// </summary>
        public void Refresh(HtmlTextWriter data)
        {
            this.lit.Text = this.HandleText();
            this.lit.RenderControl(data);
        }

        private string HandleText()
        {
            ArrayList order = new ArrayList();
            string template = this.ItemTemplate.Text;
            while (template.IndexOf("##DataItem(") > -1)
            {
                int endIndex = template.IndexOf("')##");
                int startIndex = template.IndexOf("##DataItem('") + 12;
                order.Add(template.Substring(startIndex, endIndex - startIndex));
                template = template.Substring(endIndex + 4);
            }

            if (!this.Page.IsPostBack)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Empty);
                IDataReader dr = this.Datasource;
                dr.Read();
                int iCount = Convert.ToInt32(dr[0]);
                int pageCount = Convert.ToInt32(Math.Ceiling(iCount / (double)this.PageSize));
                dr.NextResult();
                while (dr.Read())
                {
                    ArrayList itemArray = new ArrayList();
                    foreach (string item in order)
                    {
                        itemArray.Add(dr[item].ToString());
                    }

                    ItemBoundEventArgs e = new ItemBoundEventArgs(itemArray);
                    if (this.ItemBound != null)
                    {
                        this.ItemBound(this, e);
                    }

                    foreach (string item in e.Item)
                    {
                        try
                        {
                            sb.Append(item.ToString().Replace("#", string.Empty).Replace("\"", string.Empty).Replace("\\", "&#92;").Replace("|", "&#124;").Replace("^", "&#94;").Replace("'", "\\'") + this.ColDelimiter);
                        }
                        catch
                        {
                        }

                    }

                    sb.Remove(sb.Length - this.ColDelimiter.Length, this.ColDelimiter.Length);
                    sb.Append(this.RowDelimiter);
                }

                dr.Close();
                return "<script>window." + this.ClientID + ".data='" + sb.ToString() + "';window." + this.ClientID + ".PageCount=" + pageCount + ";</script>";
            }
            else
            {
                return string.Empty;
            }
        }

        public delegate void CallbackEventHandler(object sender, CallBackEventArgs e);

        public event CallbackEventHandler Callback;

        public void RaiseCallback(object sender, CallBackEventArgs e) // Implements ICallbackEventHandler.RaiseCallback
        {
            this.OnCallback(e);
        }

        protected virtual void OnCallback(CallBackEventArgs e)
        {
            if (this.Callback != null)
            {
                this.Callback(this.cb, e);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("AMActiveGrid"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMActiveGrid", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.ActiveGrid.js"));
            }
        }
        #endregion

    }

    public class ItemBoundEventArgs : EventArgs
    {
        public ArrayList Item;

        internal ItemBoundEventArgs(ArrayList sParam)
        {
            this.Item = sParam;
        }
    }

    [ToolboxItem(false)]
    public class ItemTemplateContents : System.Web.UI.Control
    {
        [DefaultValue(""), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Text
        {
            get
            {
                if (this.Controls.Count > 0 && this.Controls[0] is LiteralControl)
                {
                    return ((LiteralControl)this.Controls[0]).Text;
                }

                return string.Empty;
            }

            set
            {
                this.Controls.Clear();
                this.Controls.Add(new LiteralControl(value));
            }
        }
    }

    public class ActiveGridDesigner : ControlDesigner
    {

        public override string GetDesignTimeHtml()
        {
            return "<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>DNN Community Forums Grid Control<br />Please use source view to make changes</td></tr></table>";
        }

    }

}
