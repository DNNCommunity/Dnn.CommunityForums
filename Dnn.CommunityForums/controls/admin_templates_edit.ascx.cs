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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using static DotNetNuke.Modules.ActiveForums.Templates;

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

            Utilities.BindEnum(this.drpTemplateType, typeof(Templates.TemplateTypes), string.Empty, false, true, 0);
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

        private void LoadForm(int templateId)
        {
            TemplateInfo ti = null;
            TemplateController tc = new TemplateController();
            ti = tc.Template_Get(templateId);
            if (ti != null)
            {
                this.txtTitle.Text = ti.Title;
                this.txtSubject.Text = ti.Subject;

                SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(ti.ModuleId);
                this.txtFileName.Text = Utilities.MapPath(moduleSettings.TemplatePath + ti.FileName);
                this.txtEditor.Text = this.Server.HtmlDecode(ti.Template.Replace("[RESX:", "[TRESX:"));
                this.drpTemplateType.SelectedIndex = this.drpTemplateType.Items.IndexOf(this.drpTemplateType.Items.FindByValue(Convert.ToString(Convert.ToInt32(Enum.Parse(typeof(Templates.TemplateTypes), ti.TemplateType.ToString())))));
                this.hidTemplateId.Value = Convert.ToString(ti.TemplateId);
                if (ti.IsSystem)
                {
                    this.btnDelete.Visible = false;
                    this.txtTitle.ReadOnly = true;
                    this.drpTemplateType.Enabled = false;
                }
            }
        }

        private void cbAction_Callback(object sender, Controls.CallBackEventArgs e)
        {
            string sMsg = string.Empty;
            switch (e.Parameters[0].ToLower())
            {
                case "save":
                    {
                        try
                        {
                            // save template
                            TemplateInfo ti = null;
                            TemplateController tc = new TemplateController();
                            int templateId = 0;
                            if (e.Parameters[1].ToString() != string.Empty)
                            {
                                templateId = Convert.ToInt32(e.Parameters[1]);
                                ti = tc.Template_Get(templateId);
                            }
                            else
                            {
                                ti = new TemplateInfo();
                                ti.IsSystem = false;
                                ti.TemplateType = (Templates.TemplateTypes)Convert.ToInt32(e.Parameters[5]);
                                ti.PortalId = this.PortalId;
                                ti.ModuleId = this.ModuleId;
                            }

                            ti.Title = e.Parameters[2].ToString();
                            ti.Subject = e.Parameters[3].ToString();
                            ti.Template = e.Parameters[4];
                            ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
                            templateId = tc.Template_Save(ti);
                            DataCache.SettingsCacheClear(this.ModuleId, string.Format(CacheKeys.Template, this.ModuleId, templateId, ti.TemplateType));
                            sMsg = "Template saved successfully!";
                        }
                        catch (Exception ex)
                        {
                            sMsg = "Error saving template.";

                        }

                        break;
                    }

                case "delete":
                    {
                        try
                        {
                            // delete template
                            TemplateInfo ti = null;
                            TemplateController tc = new TemplateController();
                            int templateid = 0;
                            if (e.Parameters[1].ToString() != string.Empty)
                            {
                                templateid = Convert.ToInt32(e.Parameters[1]);
                                ti = tc.Template_Get(templateid);
                                if (!(ti.IsSystem == true))
                                {
                                    tc.Template_Delete(templateid, this.PortalId, this.ModuleId);
                                    sMsg = "Template deleted successfully!";
                                }
                                else
                                {
                                    sMsg = "Enable to delete system templates";
                                }
                            }

                            DataCache.CacheClearPrefix(this.ModuleId, string.Format(CacheKeys.TemplatePrefix, this.ModuleId));
                        }
                        catch (Exception ex)
                        {
                            sMsg = "Error deleting template.";
                        }

                        break;
                    }
            }

            this.cbActionMessage.InnerText = sMsg;
            this.cbActionMessage.RenderControl(e.Output);
        }
    }
}
