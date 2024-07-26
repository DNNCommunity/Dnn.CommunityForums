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
    using System.ComponentModel;
    using System.Text;
    using System.Threading;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ValidationProperty("SelectedDate"), ToolboxData("<{0}:datepicker runat=server></{0}:datepicker>")]
    public class DatePicker : CompositeControl
    {
        #region Declarations
        private HiddenField labelHidden = new HiddenField();
        private string dateFormat;
        private string timeFormat;
        private string selectedDate = string.Empty;
        private string weekendstyle = "amothermonthday";
        private string weekdaystyle = "amothermonthday";
        private string monthstyle = "amcaltitle";
        private string calendarstyle = "amcalendar";
        private string selecteddaystyle = "amselectedday";
        private string currentdaystyle = "amcurrentday";
        private string dayheaderstyle = "amdayheader";
        private string currentmonthdaystyle = "amcurrentmonthday";
        private string othermonthdaystyle = "amothermonthday";
        private string calwidth = "150";
        private string calheight = "150";
        private string imageUrl = string.Empty;
        private string nullDate = "01/01/1900";
        private string defaultTime = "08:00 AM";
        private string selectedTime = string.Empty;
        private bool showDateBox = true;
        private string callbackFlag = string.Empty;
        private System.Globalization.Calendar cal;
        private System.Globalization.DateTimeFormatInfo dtFI;

        #endregion

        #region Properties

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string Text { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string SelectedDate
        {
            get
            {
                if (this.selectedDate != string.Empty)
                {
                    try
                    {
                        if (Convert.ToDateTime(this.selectedDate) <= Convert.ToDateTime(this.NullDate))
                        {
                            return string.Empty;
                        }

                        return this.selectedDate;
                    }
                    catch (Exception ex)
                    {
                        return string.Empty;
                    }

                }

                return string.Empty;
            }

            set
            {
                this.selectedDate = value;
            }
        }

        /// <summary>
        /// Default 1/1/1900.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string NullDate
        {
            get
            {
                return this.nullDate;
            }

            set
            {
                this.nullDate = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CalendarWidth
        {
            get
            {
                return this.calwidth;
            }

            set
            {
                this.calwidth = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CalendarHeight
        {
            get
            {
                return this.calheight;
            }

            set
            {
                this.calheight = value;
            }
        }

        /// <summary>
        /// Default amDayHeader.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssDayHeaderStyle
        {
            get
            {
                return this.dayheaderstyle;
            }

            set
            {
                this.dayheaderstyle = value;
            }
        }

        /// <summary>
        /// Default amOtherMonthDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssWeekendStyle
        {
            get
            {
                return this.weekendstyle;
            }

            set
            {
                this.weekendstyle = value;
            }
        }

        /// <summary>
        /// Default amOtherMonthDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssWeekdayStyle
        {
            get
            {
                return this.weekdaystyle;
            }

            set
            {
                this.weekdaystyle = value;
            }
        }

        /// <summary>
        /// Default amCalTitle.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssMonthStyle
        {
            get
            {
                return this.monthstyle;
            }

            set
            {
                this.monthstyle = value;
            }
        }

        /// <summary>
        /// Default amCalendar.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssCalendarStyle
        {
            get
            {
                return this.calendarstyle;
            }

            set
            {
                this.calendarstyle = value;
            }
        }

        /// <summary>
        /// Default amSelectedDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssSelectedDayStyle
        {
            get
            {
                return this.selecteddaystyle;
            }

            set
            {
                this.selecteddaystyle = value;
            }
        }

        /// <summary>
        /// Default amCurrentDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssCurrentDayStyle
        {
            get
            {
                return this.currentdaystyle;
            }

            set
            {
                this.currentdaystyle = value;
            }
        }

        /// <summary>
        /// Default amCurrentMonthDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssCurrentMonthDayStyle
        {
            get
            {
                return this.currentmonthdaystyle;
            }

            set
            {
                this.currentmonthdaystyle = value;
            }
        }

        /// <summary>
        /// Default amOtherMonthDay.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CssOtherMonthDayStyle
        {
            get
            {
                return this.othermonthdaystyle;
            }

            set
            {
                this.othermonthdaystyle = value;
            }
        }

        /// <summary>
        /// Default MM/dd/yyyy.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string DateFormat
        {
            get
            {
                if (this.dateFormat == string.Empty)
                {
                    return "MM/dd/yyyy";
                }

                return this.dateFormat;
            }

            set
            {
                this.dateFormat = value;
            }
        }

        /// <summary>
        /// Default MM/dd/yyyy.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string TimeFormat
        {
            get
            {
                if (this.dateFormat == string.Empty)
                {
                    return "h:nn tt";
                }

                return this.timeFormat;
            }

            set
            {
                this.timeFormat = value;
            }
        }

        /// <summary>
        /// URL to Calendar image if not using the built in one.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ImageUrl
        {
            get
            {
                return this.imageUrl;
            }

            set
            {
                this.imageUrl = value;
            }
        }

        /// <summary>
        /// URL for the next month image if not using the built in one.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ImgNext { get; set; }

        /// <summary>
        /// URL for the previous month image if not using the built in one.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string ImgPrev { get; set; }

        /// <summary>
        /// Option to display time in date picker
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public bool ShowTime { get; set; }

        /// <summary>
        /// Default Time.
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string DefaultTime
        {
            get
            {
                return this.defaultTime;
            }

            set
            {
                this.defaultTime = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string SelectedTime
        {
            get
            {
                if (this.selectedTime == string.Empty)
                {
                    if (this.SelectedDate == string.Empty)
                    {
                        return this.defaultTime;
                    }

                    if (Convert.ToDateTime(this.SelectedDate).Year == 1900)
                    {
                        return string.Empty;
                    }

                    return Convert.ToDateTime(this.SelectedDate).ToString(this.TimeFormat);
                }

                return this.selectedTime;
            }

            set
            {
                this.selectedTime = value;
            }
        }

        /// <summary>
        /// Option to display Textbox in date picker
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public bool ShowDateBox
        {
            get
            {
                return this.showDateBox;
            }

            set
            {
                this.showDateBox = value;
            }
        }

        /// <summary>
        /// Option to get CallbackFlag control
        /// </summary>
        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string CallbackFlag
        {
            get
            {
                return this.callbackFlag;
            }

            set
            {
                this.callbackFlag = value;
            }
        }

        public bool TimeRequired { get; set; }

        public string RelatedControl { get; set; }

        public bool IsEndDate { get; set; }

        #endregion

        #region Subs
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                DateTime tmpDate;
                try
                {
                    tmpDate = this.SelectedDate == string.Empty ? DateTime.UtcNow : Convert.ToDateTime(this.SelectedDate);
                }
                catch (Exception ex)
                {
                    tmpDate = DateTime.UtcNow;
                }

                string temp = this.CssClass;
                this.CssClass = string.Empty;
                if (temp == string.Empty)
                {
                    temp = "ampicker";
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                if (this.Text != string.Empty)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "white-space:nowrap");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(this.Text);
                    writer.RenderEndTag();
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute("class", temp);
                writer.AddAttribute("id", this.ClientID);
                writer.AddAttribute("name", this.ClientID);
                writer.AddAttribute("onblur", "return window." + this.ClientID + ".onblur(this);");
                writer.AddAttribute("onkeypress", "return window." + this.ClientID + ".onlyDateChars(event);");

                // writer.AddAttribute("onkeydown", "return window." & Me.ClientID & ".KeyPress(event);")
                // writer.AddAttribute("onclick", "return window." & Me.ClientID & ".Click(event);showalert();")
                if (this.Enabled == false)
                {
                    writer.AddAttribute("disabled", "disabled");
                }

                if (this.ShowDateBox)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                }

                this.dtFI = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                if (!string.IsNullOrEmpty(this.SelectedDate))
                {
                    DateTime dte = DateTime.Parse(this.SelectedDate);
                    this.SelectedDate = dte.ToString(this.dtFI.ShortDatePattern + " " + this.dtFI.ShortTimePattern);
                }

                writer.AddAttribute("type", "hidden");
                writer.AddAttribute("id", "hid_" + this.ClientID);
                writer.AddAttribute("name", "hid_" + this.ClientID);
                writer.AddAttribute("value", this.SelectedDate);
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
                writer.AddAttribute("id", "cal_" + this.ClientID);
                writer.AddAttribute("style", "display:none;position:absolute;");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (this.ImageUrl == string.Empty)
                {
                    this.ImageUrl = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.calendar.gif");
                }

                if (this.Enabled)
                {
                    writer.AddAttribute("src", this.ImageUrl);
                    writer.AddAttribute("onclick", "window." + this.ClientID + ".Toggle(event);");
                    writer.AddAttribute("id", "img_" + this.ClientID);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
                var str = new StringBuilder();
                str.Append("<script type=\"text/javascript\">");

                this.cal = new System.Globalization.GregorianCalendar();
                if (Thread.CurrentThread.CurrentCulture != null)
                {
                    this.cal = Thread.CurrentThread.CurrentCulture.Calendar;
                }

                this.DateFormat = this.dtFI.ShortDatePattern;
                this.TimeFormat = this.dtFI.ShortTimePattern;
                str.Append("window." + this.ClientID + "=new asDatePicker('" + this.ClientID + "');");
                str.Append("window." + this.ClientID + ".Locale='" + this.Context.Request.UserLanguages[0].Substring(0, 2).ToUpper() + "';");
                str.Append("window." + this.ClientID + ".SelectedDate='" + this.SelectedDate + "';");
                str.Append("window." + this.ClientID + ".Width='" + this.CalendarWidth + "';");
                str.Append("window." + this.ClientID + ".Height='" + this.CalendarHeight + "';");
                str.Append("window." + this.ClientID + ".DateFormat='" + this.dtFI.ShortDatePattern + "';");
                str.Append("window." + this.ClientID + ".TimeFormat='" + this.dtFI.ShortTimePattern + "';");
                str.Append("window." + this.ClientID + ".Year=" + tmpDate.Year + ";");
                str.Append("window." + this.ClientID + ".Month=" + (tmpDate.Month - 1) + ";");
                str.Append("window." + this.ClientID + ".Day=" + tmpDate.Day + ";");
                str.Append("window." + this.ClientID + ".SelectedYear=" + tmpDate.Year + ";");
                str.Append("window." + this.ClientID + ".SelectedMonth=" + (tmpDate.Month - 1) + ";");
                str.Append("window." + this.ClientID + ".SelectedDay=" + tmpDate.Day + ";");
                str.Append("window." + this.ClientID + ".ShowTime=" + this.ShowTime.ToString().ToLower() + ";");
                str.Append("window." + this.ClientID + ".DefaultTime='" + this.DefaultTime + "';");
                str.Append("window." + this.ClientID + ".CallbackFlag='" + this.CallbackFlag + "';");
                if (!string.IsNullOrEmpty(this.RelatedControl))
                {
                    Control ctl = this.Parent.FindControl(this.RelatedControl);
                    if (ctl == null)
                    {
                        ctl = this.Page.FindControl(this.RelatedControl);
                    }

                    if (ctl == null)
                    {
                        this.RelatedControl = string.Empty;
                    }
                    else
                    {
                        this.RelatedControl = ctl.ClientID;
                    }
                }

                str.Append("window." + this.ClientID + ".linkedControl='" + this.RelatedControl + "';");
                if (this.IsEndDate)
                {
                    str.Append("window." + this.ClientID + ".isEndDate=true;");
                }
                else
                {
                    str.Append("window." + this.ClientID + ".isEndDate=false;");
                }

                string sTime = string.Empty;
                this.SelectedTime = tmpDate.ToString(this.TimeFormat);
                if (this.ShowTime)
                {
                    if (this.SelectedTime != "12:00 AM")
                    {
                        sTime = this.SelectedTime;
                    }

                    if (this.TimeRequired)
                    {
                        str.Append("window." + this.ClientID + ".RequireTime=true;");
                    }
                    else
                    {
                        str.Append("window." + this.ClientID + ".RequireTime=false;");
                    }
                }
                else
                {
                    str.Append("window." + this.ClientID + ".RequireTime=false;");
                }

                str.Append("window." + this.ClientID + ".SelectedTime='" + sTime + "';");
                if (string.IsNullOrEmpty(this.ImgNext))
                {
                    str.Append("window." + this.ClientID + ".ImgNext='" + this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.cal_nextMonth.gif") + "';");
                }
                else
                {
                    str.Append("window." + this.ClientID + ".ImgNext='" + this.Page.ResolveUrl(this.ImgNext) + "';");
                }

                if (string.IsNullOrEmpty(this.ImgPrev))
                {
                    str.Append("window." + this.ClientID + ".ImgPrev='" + this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.cal_prevMonth.gif") + "';");
                }
                else
                {
                    str.Append("window." + this.ClientID + ".ImgPrev='" + this.Page.ResolveUrl(this.ImgPrev) + "';");
                }

                if (this.SelectedDate != string.Empty)
                {
                    try
                    {
                        if (this.ShowTime == false && sTime == string.Empty)
                        {
                            str.Append("window." + this.ClientID + ".textbox.value=new Date(" + tmpDate.Year + "," + (tmpDate.Month - 1) + "," + tmpDate.Day + ").formatDP('" + this.DateFormat + "','" + this.ClientID + "');");
                            str.Append("window." + this.ClientID + ".dateSel = new Date(" + tmpDate.Year + "," + (tmpDate.Month - 1) + "," + tmpDate.Day + ",0,0,0,0);");
                        }
                        else
                        {
                            str.Append("window." + this.ClientID + ".textbox.value=new Date(" + tmpDate.Year + "," + (tmpDate.Month - 1) + "," + tmpDate.Day + "," + tmpDate.Hour + "," + tmpDate.Minute + ",0).formatDP('" + this.DateFormat + " " + this.TimeFormat + "','" + this.ClientID + "');");
                            str.Append("window." + this.ClientID + ".dateSel = new Date(" + tmpDate.Year + "," + (tmpDate.Month - 1) + "," + tmpDate.Day + "," + tmpDate.Hour + "," + tmpDate.Minute + ",0);");
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }

                int xMonths = this.cal.GetMonthsInYear(this.cal.GetYear(tmpDate), this.cal.GetEra(tmpDate));
                int currMonth = this.cal.GetMonth(tmpDate);
                int currYear = this.cal.GetYear(tmpDate);
                int currDay = this.cal.GetDayOfMonth(tmpDate);

                str.Append("window." + this.ClientID + ".MonthDays = new Array(");
                for (int i = 0; i < xMonths; i++)
                {
                    str.Append(this.cal.GetDaysInMonth(currYear, i + 1));
                    if (i < (xMonths - 1))
                    {
                        str.Append(",");
                    }
                }

                str.Append(");");
                str.AppendLine();

                string[] mNames = this.dtFI.MonthNames;
                str.Append("window." + this.ClientID + ".MonthNames = new Array(");
                for (int i = 0; i < xMonths; i++)
                {
                    str.Append("'" + mNames[i] + "'");
                    if (i < (xMonths - 1))
                    {
                        str.Append(",");
                    }
                }

                str.Append(");");
                str.AppendLine();
                str.Append("window." + this.ClientID + ".ShortMonthNames = new Array(");
                string[] mAbbr = this.dtFI.AbbreviatedMonthNames;
                for (int i = 0; i < xMonths; i++)
                {
                    str.Append("'" + mAbbr[i] + "'");
                    if (i < (xMonths - 1))
                    {
                        str.Append(",");
                    }
                }

                str.Append(");");
                str.AppendLine();
                str.Append("window." + this.ClientID + ".ShortDayNames = new Array(");
                string[] dAbbr = this.dtFI.AbbreviatedDayNames;
                for (int i = 0; i <= 6; i++)
                {
                    str.Append("'" + dAbbr[i] + "'");
                    if (i < 6)
                    {
                        str.Append(",");
                    }
                }

                str.Append(");");
                str.AppendLine();

                str.Append("window." + this.ClientID + ".Class={");
                str.Append("CssCalendarStyle:'" + this.CssCalendarStyle + "',");
                str.Append("CssMonthStyle:'" + this.CssMonthStyle + "',");
                str.Append("CssWeekendStyle:'" + this.CssWeekendStyle + "',");
                str.Append("CssWeekdayStyle:'" + this.CssWeekdayStyle + "',");
                str.Append("CssSelectedDayStyle:'" + this.CssSelectedDayStyle + "',");
                str.Append("CssCurrentMonthDayStyle:'" + this.CssCurrentMonthDayStyle + "',");
                str.Append("CssOtherMonthDayStyle:'" + this.CssOtherMonthDayStyle + "',");
                str.Append("CssDayHeaderStyle:'" + this.CssDayHeaderStyle + "',");
                str.Append("CssCurrentDayStyle:'" + this.CssCurrentDayStyle + "'};");
                str.Append("window." + this.ClientID + ".selectedDate=window." + this.ClientID + ".textbox.value;");
                str.Append("window." + this.ClientID + ".timeLabel='[RESX:Time]';");

                str.Append("</script>");
                writer.Write(str);
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("AMDatePicker"))
            {
#if DEBUG
                this.Page.ClientScript.RegisterClientScriptInclude("AMDatePicker", this.Page.ResolveUrl("~/DesktopModules/activeforums/customcontrols/resources/datepicker.js"));

#else
                Page.ClientScript.RegisterClientScriptInclude("AMDatePicker", Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.datepicker.js"));
#endif

            }

            try
            {
                if (this.Page.IsPostBack)
                {
                    this.SelectedDate = this.Context.Request.Form[this.ClientID];
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

    }
}
