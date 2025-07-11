// Copyright (c) by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using DotNetNuke.Modules.ActiveForums.Entities;

    public partial class admin_templates_edit : ActiveAdminBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbAction.CallbackEvent += this.cbAction_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Utilities.BindEnum(this.drpTemplateType, typeof(Enums.TemplateType), string.Empty, false, true, 0);
            this.drpTemplateType.Attributes.Add("onchange", "toggleTextTab()");
            if (!(this.Params == string.Empty) && !(this.Params == "undefined"))
            {
                try
                {
                    this.LoadForm(Convert.ToInt32(this.Params));
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                this.btnDelete.Visible = false;
            }
        }

        private void LoadForm(int templateId) => throw new NotImplementedException();
        //{
        //    var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TemplateController().Template_Get(templateId);
        //    if (ti != null)
        //    {
        //        SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(this.ModuleId);
        //        this.txtFileName.Text = string.Empty;// Utilities.MapPath(moduleSettings.TemplatePath + ti.FileName);
        //        this.txtEditor.Text = this.Server.HtmlDecode(ti.Replace("[RESX:", "[TRESX:"));
        //        this.drpTemplateType.SelectedIndex = this.drpTemplateType.Items.IndexOf(this.drpTemplateType.Items.FindByValue(Convert.ToString(Convert.ToInt32(Enum.Parse(typeof(Enums.TemplateType), ti.TemplateType.ToString())))));
        //        this.hidTemplateId.Value = Convert.ToString(ti.TemplateId);
        //    }
        //}

        private void cbAction_Callback(object sender, Controls.CallBackEventArgs e) => throw new NotImplementedException();
        //{
        //    string sMsg = string.Empty;
        //    switch (e.Parameters[0].ToLower())
        //    {
        //        case "save":
        //            {
        //                try
        //                {
        //                    // save template
        //                    DotNetNuke.Modules.ActiveForums.Entities.TemplateInfo ti = null;
        //                    int templateId = 0;
        //                    if (e.Parameters[1].ToString() != string.Empty)
        //                    {
        //                        templateId = Convert.ToInt32(e.Parameters[1]);
        //                        ti = new DotNetNuke.Modules.ActiveForums.Controllers.TemplateController().Template_Get(templateId);
        //                    }
        //                    else
        //                    {
        //                        ti = new TemplateInfo();
        //                        ti.TemplateType = (Enums.TemplateType)Convert.ToInt32(e.Parameters[5]);
        //                    }

        //                    ti.Template = e.Parameters[4];
        //                    ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
        //                    new DotNetNuke.Modules.ActiveForums.Controllers.TemplateController().Template_Save(this.ModuleId, ti);
        //                    DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.TemplatePrefix, this.ModuleId));
        //                    sMsg = "Template saved successfully!";
        //                }
        //                catch (Exception ex)
        //                {
        //                    sMsg = "Error saving template.";
        //                }

        //                break;
        //            }

        //        case "delete":
        //            {
        //                try
        //                {
        //                    // delete template
        //                    int templateid = 0;
        //                    if (e.Parameters[1].ToString() != string.Empty)
        //                    {
        //                        templateid = Convert.ToInt32(e.Parameters[1]);
        //                        new DotNetNuke.Modules.ActiveForums.Controllers.TemplateController().Template_Delete(this.ModuleId, templateid, string.Empty );
        //                        sMsg = "Template deleted successfully!";
        //                    }

        //                    DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.TemplatePrefix, this.ModuleId));
        //                }
        //                catch (Exception ex)
        //                {
        //                    sMsg = "Error deleting template.";
        //                }

        //                break;
        //            }
        //    }

        //    this.cbActionMessage.InnerText = sMsg;
        //    this.cbActionMessage.RenderControl(e.Output);
        //}
    }
}
