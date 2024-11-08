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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls;

    using DotNetNuke.Security.Roles;

    using AFSettings = DotNetNuke.Modules.ActiveForums.Settings;

    public partial class admin_manageforums_forumeditor : ActiveAdminBase
    {
        public string imgOn;
        public string imgOff;
        public string editorType = "G"; // "F"
        public int recordId = 0;
        protected Controls.admin_securitygrid ctlSecurityGrid = new Controls.admin_securitygrid();

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbEditorAction.CallbackEvent += this.cbEditorAction_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.litTopicPropButton.Text = "<div><a href=\"\" onclick=\"afadmin_LoadPropForm();return false;\" class=\"btnadd afroundall\">[RESX:AddProperty]</a></div>";
            this.litPropLoad.Text = Utilities.GetFileResource("DotNetNuke.Modules.ActiveForums.scripts.afadmin.properties.js");

            this.BindRoles();

            var sepChar = '|';
            if (this.Params != null && !string.IsNullOrEmpty(this.Params))
            {
                if (this.Params.Contains("!"))
                {
                    sepChar = '!';
                }

                this.editorType = this.Params.Split(sepChar)[1];
                this.recordId = Utilities.SafeConvertInt(this.Params.Split(sepChar)[0]);
            }

            this.span_Parent.Visible = false;
            if (this.editorType == "M")
            {
                this.trGroups.Visible = false;
                this.trName.Visible = false;
                this.trPrefix.Visible = false;
                this.trActive.Visible = false;
                this.trHidden.Visible = false;
                this.reqGroups.Enabled = false;
                this.trDesc.Visible = false;
                this.trInheritModuleFeatures.Visible = false;
                this.trInheritModuleSecurity.Visible = false;
                this.trInheritGroupFeatures.Visible = false;
                this.trInheritGroupSecurity.Visible = false;
                this.btnDelete.ClientSideScript = string.Empty;
                this.btnDelete.Enabled = false;
            }
            else if (this.editorType == "G")
            {
                this.trGroups.Visible = false;
                this.reqGroups.Enabled = false;
                this.trDesc.Visible = false;
                this.trInheritModuleFeatures.Visible = true;
                this.trInheritModuleSecurity.Visible = true;
                this.chkInheritModuleFeatures.Attributes.Add("onclick", "amaf_toggleInheritFeatures();");
                this.chkInheritModuleSecurity.Attributes.Add("onclick", "amaf_toggleInheritSecurity();");
                this.trInheritGroupFeatures.Visible = false;
                this.trInheritGroupSecurity.Visible = false;
                this.lblForumGroupName.Text = this.GetSharedResource("[RESX:GroupName]");
                this.btnDelete.ClientSideScript = "deleteGroup();";
            }
            else if (this.editorType == "F")
            {
                this.lblForumGroupName.Text = this.GetSharedResource("[RESX:ForumName]");
                this.trInheritModuleFeatures.Visible = false;
                this.trInheritModuleSecurity.Visible = false;
                this.trInheritGroupFeatures.Visible = true;
                this.trInheritGroupSecurity.Visible = true;
                this.chkInheritGroupFeatures.Attributes.Add("onclick", "amaf_toggleInheritFeatures();");
                this.chkInheritGroupSecurity.Attributes.Add("onclick", "amaf_toggleInheritSecurity();");
                this.btnDelete.ClientSideScript = "deleteForum();";
                if (this.recordId != 0)
                {
                    this.span_Parent.Visible = true;
                    string parent = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Parent]", true);
                    var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.recordId, this.ModuleId);
                    if (fi.ParentForumId != 0)
                    {
                        this.span_Parent.Attributes.Add("onclick", $"LoadView('manageforums_forumeditor','{fi.ParentForumId}|F');");
                        this.span_Parent.InnerText = "| " + parent + " " + fi.ParentForumName;
                    }
                    else
                    {
                        this.span_Parent.InnerText = "| " + parent + " " + fi.GroupName;
                        this.span_Parent.Attributes.Add("onclick", $"LoadView('manageforums_forumeditor','{fi.ForumGroupId}|G');");
                    }
                }
            }

            if (this.recordId == 0)
            {
                this.btnDelete.Visible = false;
            }

            this.imgOn = this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "admin_check.png");
            this.imgOff = this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "admin_stop.png");
            this.reqForumName.Text = "<img src=\"" + this.Page.ResolveUrl(RequiredImage) + "\" />";
            this.reqGroups.Text = "<img src=\"" + this.Page.ResolveUrl(RequiredImage) + "\" />";
            var propImage = "<img src=\"" + this.Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath + "properties16.png") + "\" alt=\"[RESX:ConfigureProperties]\" />";

            this.rdAttachOn.Attributes.Add("onclick", "toggleAttach(this);");
            this.rdAttachOn.Attributes.Add("value", "1");
            this.rdAttachOff.Attributes.Add("onclick", "toggleAttach(this);");
            this.rdAttachOff.Attributes.Add("value", "0");
            this.cfgAttach.InnerHtml = propImage;

            this.rdHTMLOn.Attributes.Add("onclick", "toggleEditor(this);document.getElementById('" + this.drpEditorTypes.ClientID + "').value = '2';document.getElementById('" + this.drpEditorMobile.ClientID + "').value ='2';");
            this.rdHTMLOff.Attributes.Add("onclick", "toggleEditor(this); document.getElementById('" + this.drpEditorTypes.ClientID + "').value = '0'; document.getElementById('" + this.drpEditorMobile.ClientID + "').value = '0'; ");
            this.rdHTMLOn.Attributes.Add("value", "1");
            this.rdHTMLOff.Attributes.Add("value", "0");
            this.cfgHTML.Attributes.Add("style", "display:none;");
            this.cfgHTML.InnerHtml = propImage;
            this.rdModOn.Attributes.Add("onclick", "toggleMod(this);");
            this.rdModOff.Attributes.Add("onclick", "toggleMod(this);");
            this.rdModOn.Attributes.Add("value", "1");
            this.rdModOff.Attributes.Add("value", "0");
            this.cfgMod.Attributes.Add("style", "display:none;");
            this.cfgMod.InnerHtml = propImage;

            this.trAutoSub.Visible = true;
            this.rdAutoSubOn.Attributes.Add("onclick", "toggleAutoSub(this);");
            this.rdAutoSubOff.Attributes.Add("onclick", "toggleAutoSub(this);");
            this.rdAutoSubOn.Attributes.Add("value", "1");
            this.rdAutoSubOff.Attributes.Add("value", "0");
            this.cfgAutoSub.Attributes.Add("style", "display:none;");
            this.cfgAutoSub.InnerHtml = propImage;
            this.txtOlderThan.Attributes.Add("onkeypress", "return onlyNumbers(event)");
            this.txtReplyOlderThan.Attributes.Add("onkeypress", "return onlyNumbers(event)");
            this.txtUserId.Attributes.Add("onkeypress", "return onlyNumbers(event)");

            if (this.MainSettings.DeleteBehavior == 1)
            {
                this.lblMaintWarn.Text = string.Format(this.GetSharedResource("[RESX:MaintenanceWarning]"), this.GetSharedResource("[RESX:MaintenanceWarning:Recycle]"), this.GetSharedResource("[RESX:MaintenanceWarning:Recycle:Desc]"));
            }
            else
            {
                this.lblMaintWarn.Text = string.Format(this.GetSharedResource("[RESX:MaintenanceWarning]"), this.GetSharedResource("[RESX:MaintenanceWarning:Remove]"), this.GetSharedResource("[RESX:MaintenanceWarning:Remove:Desc]"));
            }

            // drpEditorTypes.Attributes.Add("onchange", "toggleEditorFields();");
            if (this.cbEditorAction.IsCallback)
            {
                return;
            }

            this.BindGroups();
            this.BindTemplates();

            if (this.recordId > 0)
            {
                switch (this.editorType)
                {
                    case "M":
                        this.LoadDefaults(this.recordId);
                        break;
                    case "F":
                        this.LoadForum(this.recordId);
                        break;
                    case "G":
                        this.LoadGroup(this.recordId);
                        break;
                }

                this.cfgHTML.Attributes.Add("onclick", "showProp(this,'edProp')");

                this.cfgMod.Attributes.Add("onclick", "showProp(this,'modProp')");

                this.cfgAutoSub.Attributes.Add("onclick", "showProp(this,'subProp')");

                this.cfgAttach.Attributes.Add("onclick", "showProp(this,'attachProp')");

                var sb = new StringBuilder();
                sb.Append("<script type=\"text/javascript\">");
                sb.Append("function toggleAutoSub(obj){");
                sb.Append("    closeAllProp();");
                sb.Append("    var mod = document.getElementById('" + this.cfgAutoSub.ClientID + "');");
                sb.Append("    if (obj.value == '1'){");
                sb.Append("        mod.style.display = '';");
                sb.Append("    }else{");
                sb.Append("        mod.style.display = 'none';");
                sb.Append("        var winDiv = document.getElementById('subProp');");
                sb.Append("        mod.style.display = 'none';");
                sb.Append("    };");
                sb.Append("};");

                sb.Append("</script>");
                this.litScripts.Text = sb.ToString();
            }

            this.BindTabs();
        }

        private void cbEditorAction_Callback(object sender, Controls.CallBackEventArgs e)
        {
            switch (e.Parameters[0].ToLowerInvariant())
            {
                case "forumsave":
                    {
                        var fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
                        var bIsNew = false;
                        int forumGroupId;
                        var forumSettingsKey = string.Empty;

                        if (Utilities.SafeConvertInt(e.Parameters[1]) <= 0)
                        {
                            bIsNew = true;
                            fi.ForumID = -1;
                        }
                        else
                        {
                            fi = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ModuleId, Utilities.SafeConvertInt(e.Parameters[1]), false, -1);
                        }

                        fi.ModuleId = this.ModuleId;
                        fi.PortalId = this.PortalId;
                        var sParentValue = e.Parameters[2];
                        var parentForumId = 0;

                        if (sParentValue.Contains("GROUP"))
                        {
                            sParentValue = sParentValue.Replace("GROUP", string.Empty);
                            forumGroupId = Utilities.SafeConvertInt(sParentValue);
                        }
                        else
                        {
                            parentForumId = Utilities.SafeConvertInt(sParentValue.Replace("FORUM", string.Empty));
                            forumGroupId = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: parentForumId, moduleId: this.ModuleId).ForumGroupId;
                        }

                        fi.ForumGroupId = forumGroupId;
                        fi.ParentForumId = parentForumId;
                        fi.ForumName = e.Parameters[3];
                        fi.ForumDesc = e.Parameters[4];
                        fi.Active = Utilities.SafeConvertBool(e.Parameters[5]);
                        fi.Hidden = Utilities.SafeConvertBool(e.Parameters[6]);

                        fi.SortOrder = string.IsNullOrWhiteSpace(e.Parameters[7]) ? 0 : Utilities.SafeConvertInt(e.Parameters[7]);

                        var fkey = string.IsNullOrEmpty(fi.ForumSettingsKey) ? string.Empty : fi.ForumSettingsKey;

                        bool inheritFeatures = Utilities.SafeConvertBool(e.Parameters[8]);
                        bool inheritSecurity = Utilities.SafeConvertBool(e.Parameters[9]);

                        fi.PrefixURL = e.Parameters[10];
                        if (!string.IsNullOrEmpty(fi.PrefixURL))
                        {
                            var db = new Data.Common();
                            if (!db.CheckForumURL(this.PortalId, this.ModuleId, fi.PrefixURL, fi.ForumID, fi.ForumGroupId))
                            {
                                fi.PrefixURL = string.Empty;
                            }
                        }

                        this.recordId = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(this.PortalId, fi, bIsNew, inheritFeatures, inheritSecurity);

                        this.hidEditorResult.Value = this.recordId.ToString();
                        break;
                    }

                case "groupsave":
                    {
                        var bIsNew = false;
                        var groupId = Utilities.SafeConvertInt(e.Parameters[1]);
                        var gi = (groupId > 0) ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(groupId, this.ModuleId) : new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();

                        var settingsKey = string.Empty;
                        if (groupId == 0)
                        {
                            bIsNew = true;
                        }
                        else
                        {
                            settingsKey = "G:" + groupId;
                        }

                        gi.ModuleId = this.ModuleId;
                        gi.ForumGroupId = groupId;
                        gi.GroupName = e.Parameters[3];
                        gi.Active = Utilities.SafeConvertBool(e.Parameters[5]);
                        gi.Hidden = Utilities.SafeConvertBool(e.Parameters[6]);

                        gi.SortOrder = string.IsNullOrWhiteSpace(e.Parameters[7]) ? 0 : Utilities.SafeConvertInt(e.Parameters[7]);

                        bool inheritFeatures = Utilities.SafeConvertBool(e.Parameters[8]);
                        bool inheritSecurity = Utilities.SafeConvertBool(e.Parameters[9]);

                        gi.PrefixURL = e.Parameters[10];
                        if (!string.IsNullOrEmpty(gi.PrefixURL))
                        {
                            var db = new Data.Common();
                            if (!db.CheckGroupURL(this.PortalId, this.ModuleId, gi.PrefixURL, gi.ForumGroupId))
                            {
                                gi.PrefixURL = string.Empty;
                            }
                        }

                        gi.GroupSettingsKey = settingsKey;
                        groupId = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Groups_Save(this.PortalId, gi, bIsNew, inheritFeatures, inheritSecurity);
                        this.recordId = groupId;
                        this.hidEditorResult.Value = groupId.ToString();

                        break;
                    }

                case "modulesettingssave":
                    {
                        var sKey = "M:" + this.ModuleId;
                        this.SaveSettings(sKey, e.Parameters);

                        this.hidEditorResult.Value = this.ModuleId.ToString();

                        break;
                    }

                case "forumsettingssave":
                    {
                        var forumId = Utilities.SafeConvertInt(e.Parameters[1]);
                        var sKey = "F:" + forumId;
                        this.SaveSettings(sKey, e.Parameters);

                        this.hidEditorResult.Value = forumId.ToString();

                        break;
                    }

                case "groupsettingssave":
                    {
                        var forumId = Utilities.SafeConvertInt(e.Parameters[1]);
                        var sKey = "G:" + forumId;
                        this.SaveSettings(sKey, e.Parameters);

                        this.hidEditorResult.Value = forumId.ToString();

                        break;
                    }

                case "deleteforum":
                    {
                        var forumId = Utilities.SafeConvertInt(e.Parameters[1]);
                        new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Delete(this.PortalId, this.ModuleId, forumId);
                        break;
                    }

                case "deletegroup":
                    {
                        var groupId = Utilities.SafeConvertInt(e.Parameters[1]);
                        new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Groups_Delete(this.ModuleId, groupId);
                        break;
                    }
            }

            DataCache.ClearAllCache(this.ModuleId);
            DataCache.ClearAllCacheForTabId(this.TabId);

            this.hidEditorResult.RenderControl(e.Output);
        }

        #endregion

        #region Private Methods

        private void SaveSettings(string sKey, string[] parameters)
        {
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.TopicsTemplateId, parameters[2]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.TopicTemplateId, parameters[3]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EmailAddress, parameters[4]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.UseFilter, parameters[5]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowPostIcon, parameters[6]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowEmoticons, parameters[7]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowScript, parameters[8]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.IndexContent, parameters[9]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowRSS, parameters[10]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowAttach, parameters[11]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AttachCount, parameters[12]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AttachMaxSize, parameters[13]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AttachTypeAllowed, parameters[14]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EditorMobile, parameters[15]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowLikes, parameters[16]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ReplyPostCount, parameters[17]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AttachAllowBrowseSite, parameters[18]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AttachInsertAllowed, parameters[19]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.MaxAttachWidth, parameters[20]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.MaxAttachHeight, parameters[21]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ConvertingToJpegAllowed, parameters[22]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AllowHTML, parameters[23]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EditorType, parameters[24]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EditorHeight, parameters[25]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EditorWidth, parameters[26]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.CreatePostCount, parameters[27]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AutoSubscribeNewTopicsOnly, parameters[28]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.EditorPermittedUsers, parameters[29]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.TopicFormId, parameters[30]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ReplyFormId, parameters[31]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AutoSubscribeRoles, parameters[32]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ProfileTemplateId, parameters[33]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.IsModerated, parameters[34]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.DefaultTrustLevel, parameters[35]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AutoTrustLevel, parameters[36]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ModApproveTemplateId, parameters[37]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ModRejectTemplateId, parameters[38]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ModMoveTemplateId, parameters[39]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ModDeleteTemplateId, parameters[40]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.ModNotifyTemplateId, parameters[41]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.AutoSubscribeEnabled, parameters[42]);
            AFSettings.SaveSetting(this.ModuleId, sKey, ForumSettingKeys.QuickReplyFormId, parameters[43]);
        }

        private void LoadForum(int forumId)
        {
            var fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, this.ModuleId);

            if (fi == null)
            {
                return;
            }

            var newForum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, this.ModuleId);

            this.ctlSecurityGrid = this.LoadControl(virtualPath: this.Page.ResolveUrl(Globals.ModulePath + "/controls/admin_securitygrid.ascx")) as Controls.admin_securitygrid;
            if (this.ctlSecurityGrid != null)
            {
                this.ctlSecurityGrid.Perms = newForum.Security;
                this.ctlSecurityGrid.PermissionsId = newForum.PermissionsId;
                this.ctlSecurityGrid.ModuleConfiguration = this.ModuleConfiguration;
                this.ctlSecurityGrid.ReadOnly = newForum.InheritSecurity;

                this.plhGrid.Controls.Clear();
                this.plhGrid.Controls.Add(this.ctlSecurityGrid);
            }

            this.txtForumName.Text = fi.ForumName;
            this.txtForumDesc.Text = fi.ForumDesc;
            this.chkActive.Checked = fi.Active;
            this.chkHidden.Checked = fi.Hidden;
            this.hidForumId.Value = fi.ForumID.ToString();
            this.txtPrefixURL.Text = fi.PrefixURL;
            this.chkInheritGroupSecurity.Checked = fi.InheritSecurity;

            var groupValue = (fi.ParentForumId > 0) ? "FORUM" + fi.ParentForumId : "GROUP" + fi.ForumGroupId;

            Utilities.SelectListItemByValue(this.drpGroups, groupValue);

            if (fi.ForumSettingsKey == "G:" + fi.ForumGroupId)
            {
                this.chkInheritGroupFeatures.Checked = true;
                this.trTemplates.Attributes.Add("style", "display:none;");
            }

            this.LoadFeatureSettings(fi.FeatureSettings);
        }

        private void LoadGroup(int groupId)
        {
            var gc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
            var gi = gc.GetById(groupId, this.ModuleId);

            if (gi == null)
            {
                return;
            }

            var newGroup = gc.GetById(groupId, this.ModuleId);

            this.ctlSecurityGrid = this.LoadControl(this.Page.ResolveUrl(Globals.ModulePath + "controls/admin_securitygrid.ascx")) as Controls.admin_securitygrid;
            if (this.ctlSecurityGrid != null)
            {
                this.ctlSecurityGrid.Perms = newGroup.Security;
                this.ctlSecurityGrid.PermissionsId = newGroup.PermissionsId;
                this.ctlSecurityGrid.ModuleConfiguration = this.ModuleConfiguration;

                this.plhGrid.Controls.Clear();
                this.plhGrid.Controls.Add(this.ctlSecurityGrid);
            }

            this.trGroups.Visible = false;
            this.reqGroups.Enabled = false;
            this.txtForumName.Text = gi.GroupName;
            this.chkActive.Checked = gi.Active;
            this.chkHidden.Checked = gi.Hidden;
            this.hidForumId.Value = gi.ForumGroupId.ToString();
            this.hidSortOrder.Value = gi.SortOrder.ToString();
            this.txtPrefixURL.Text = gi.PrefixURL;
            this.chkInheritModuleSecurity.Checked = gi.InheritSecurity;

            if (gi.GroupSettingsKey == "M:" + this.ModuleId)
            {
                this.chkInheritModuleFeatures.Checked = true;
                this.trTemplates.Attributes.Add("style", "display:none;");
            }
            this.LoadFeatureSettings(gi.FeatureSettings);
        }

        private void LoadDefaults(int groupId)
        {
            this.ctlSecurityGrid = this.LoadControl(this.Page.ResolveUrl(Globals.ModulePath + "controls/admin_securitygrid.ascx")) as Controls.admin_securitygrid;
            if (this.ctlSecurityGrid != null)
            {
                var permissions = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(permissionId: this.MainSettings.DefaultPermissionId, moduleId: this.ModuleId);
                if (permissions == null)
                {
                    permissions = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(this.PortalId).ToString(), this.ModuleId);
                    DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(this.PortalId, this.ModuleId, permissions.PermissionsId);
                    DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(this.ModuleId, SettingKeys.DefaultPermissionId, permissions.PermissionsId.ToString());
                }

                this.ctlSecurityGrid.Perms = permissions;
                this.ctlSecurityGrid.PermissionsId = permissions.PermissionsId;
                this.ctlSecurityGrid.ModuleConfiguration = this.ModuleConfiguration;

                this.plhGrid.Controls.Clear();
                this.plhGrid.Controls.Add(this.ctlSecurityGrid);
            }

            this.trGroups.Visible = false;
            this.reqGroups.Enabled = false;

            this.LoadFeatureSettings(this.MainSettings.ForumFeatureSettings);
        }

        private void LoadFeatureSettings(DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings featureSettings)
        {
            Utilities.SelectListItemByValue(this.drpTopicsTemplate, featureSettings.TopicsTemplateId);
            Utilities.SelectListItemByValue(this.drpTopicTemplate, featureSettings.TopicTemplateId);
            Utilities.SelectListItemByValue(this.drpTopicForm, featureSettings.TopicFormId);
            Utilities.SelectListItemByValue(this.drpReplyForm, featureSettings.ReplyFormId);
            Utilities.SelectListItemByValue(this.drpQuickReplyForm, featureSettings.QuickReplyFormId);
            Utilities.SelectListItemByValue(this.drpProfileDisplay, featureSettings.ProfileTemplateId);
            Utilities.SelectListItemByValue(this.drpModApprovedTemplateId, featureSettings.ModApproveTemplateId);
            Utilities.SelectListItemByValue(this.drpModRejectTemplateId, featureSettings.ModRejectTemplateId);
            Utilities.SelectListItemByValue(this.drpModDeleteTemplateId, featureSettings.ModDeleteTemplateId);
            Utilities.SelectListItemByValue(this.drpModMoveTemplateId, featureSettings.ModMoveTemplateId);
            Utilities.SelectListItemByValue(this.drpModNotifyTemplateId, featureSettings.ModNotifyTemplateId);
            Utilities.SelectListItemByValue(this.drpDefaultTrust, (int)featureSettings.DefaultTrustValue);
            Utilities.SelectListItemByValue(this.drpEditorTypes, (int)featureSettings.EditorType);
            Utilities.SelectListItemByValue(this.drpEditorMobile, (int)featureSettings.EditorMobile);
            Utilities.SelectListItemByValue(this.drpPermittedRoles, (int)featureSettings.EditorPermittedUsers);

            this.txtAutoTrustLevel.Text = featureSettings.AutoTrustLevel.ToString();
            this.txtEmailAddress.Text = featureSettings.EmailAddress;
            this.txtCreatePostCount.Text = featureSettings.CreatePostCount.ToString();
            this.txtReplyPostCount.Text = featureSettings.ReplyPostCount.ToString();

            this.rdFilterOn.Checked = featureSettings.UseFilter;
            this.rdFilterOff.Checked = !featureSettings.UseFilter;

            this.rdPostIconOn.Checked = featureSettings.AllowPostIcon;
            this.rdPostIconOff.Checked = !featureSettings.AllowPostIcon;

            this.rdEmotOn.Checked = featureSettings.AllowEmoticons;
            this.rdEmotOff.Checked = !featureSettings.AllowEmoticons;

            this.rdScriptsOn.Checked = featureSettings.AllowScript;
            this.rdScriptsOff.Checked = !featureSettings.AllowScript;

            this.rdIndexOn.Checked = featureSettings.IndexContent;
            this.rdIndexOff.Checked = !featureSettings.IndexContent;

            this.rdRSSOn.Checked = featureSettings.AllowRSS;
            this.rdRSSOff.Checked = !featureSettings.AllowRSS;

            this.rdAttachOn.Checked = featureSettings.AllowAttach;
            this.rdAttachOff.Checked = !featureSettings.AllowAttach;

            if (featureSettings.AllowAttach)
            {
                this.cfgAttach.Attributes.Remove("style");
            }
            else
            {
                this.cfgAttach.Attributes.Add("style", "display:none;");
            }

            this.txtMaxAttach.Text = featureSettings.AttachCount.ToString();
            this.txtMaxAttachSize.Text = featureSettings.AttachMaxSize.ToString();
            this.txtAllowedTypes.Text = featureSettings.AttachTypeAllowed;
            this.ckAllowBrowseSite.Checked = featureSettings.AttachAllowBrowseSite;
            this.txtMaxAttachWidth.Text = featureSettings.MaxAttachWidth.ToString();
            this.txtMaxAttachHeight.Text = featureSettings.MaxAttachHeight.ToString();
            this.ckAttachInsertAllowed.Checked = featureSettings.AttachInsertAllowed;
            this.ckConvertingToJpegAllowed.Checked = featureSettings.ConvertingToJpegAllowed;

            // if switching from HTML off to HTML on, switch editor to HTML editor, or vice versa
            if (this.rdHTMLOff.Checked && featureSettings.AllowHTML)
            {
                Utilities.SelectListItemByValue(this.drpEditorTypes, (int)EditorTypes.HTMLEDITORPROVIDER);
                Utilities.SelectListItemByValue(this.drpEditorMobile, (int)EditorTypes.HTMLEDITORPROVIDER);
            }

            if (this.rdHTMLOn.Checked && !featureSettings.AllowHTML)
            {
                Utilities.SelectListItemByValue(this.drpEditorTypes, (int)EditorTypes.TEXTBOX);
                Utilities.SelectListItemByValue(this.drpEditorMobile, (int)EditorTypes.TEXTBOX);
            }

            this.rdHTMLOn.Checked = featureSettings.AllowHTML;
            this.rdHTMLOff.Checked = !featureSettings.AllowHTML;

            if (featureSettings.AllowHTML)
            {
                this.cfgHTML.Attributes.Remove("style");
            }
            else
            {
                this.cfgHTML.Attributes.Add("style", "display:none;");
            }

            this.rdModOn.Checked = featureSettings.IsModerated;
            this.rdModOff.Checked = !featureSettings.IsModerated;

            if (featureSettings.IsModerated)
            {
                this.cfgMod.Attributes.Remove("style");
            }
            else
            {
                this.cfgMod.Attributes.Add("style", "display:none;");
            }

            this.rdAutoSubOn.Checked = featureSettings.AutoSubscribeEnabled;
            this.rdAutoSubOff.Checked = !featureSettings.AutoSubscribeEnabled;

            if (featureSettings.AutoSubscribeEnabled)
            {
                this.cfgAutoSub.Attributes.Remove("style");
            }
            else
            {
                this.cfgAutoSub.Attributes.Add("style", "display:none;");
            }

            this.rdLikesOn.Checked = featureSettings.AllowLikes;
            this.rdLikesOff.Checked = !featureSettings.AllowLikes;

            this.chkAutoSubscribeNewTopicsOnly.Checked = featureSettings.AutoSubscribeNewTopicsOnly;

            this.txtEditorHeight.Text = (featureSettings.EditorHeight == string.Empty) ? "400" : featureSettings.EditorHeight;
            this.txtEditorWidth.Text = (featureSettings.EditorWidth == string.Empty) ? "99%" : featureSettings.EditorWidth;

            this.hidRoles.Value = featureSettings.AutoSubscribeRoles;
            this.BindAutoSubRoles(featureSettings.AutoSubscribeRoles);
        }


        private void BindAutoSubRoles(string roles)
        {
            var sb = new StringBuilder();
            sb.Append("<table id=\"tblRoles\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\">");
            sb.Append("<tr><td width=\"99%\"></td><td></td></tr>");
            if (roles != null)
            {
                var arr = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: this.PortalId);
                foreach (RoleInfo ri in from RoleInfo ri in arr let sRoles = roles.Split(';') from role in sRoles.Where(role => role == ri.RoleID.ToString()) select ri)
                {
                    sb.Append("<tr><td class=\"amcpnormal\">" + ri.RoleName + "</td><td><img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/delete16.png") + "\" onclick=\"removeRole(this," + ri.RoleID + ");\" /></td></tr>");
                }
            }

            sb.Append("</table>");
            this.tbRoles.Text = sb.ToString();
        }

        private void BindGroups()
        {
            var dr = DataProvider.Instance().Forums_List(this.PortalId, this.ModuleId, -1, -1, false);
            this.drpGroups.Items.Add(new ListItem(Utilities.GetSharedResource("DropDownSelect"), "-1"));

            var tmpGroupId = -1;
            while (dr.Read())
            {
                var groupId = dr.GetInt("ForumGroupId");
                if (tmpGroupId != groupId)
                {
                    this.drpGroups.Items.Add(new ListItem(dr.GetString("GroupName"), "GROUP" + groupId));
                    tmpGroupId = groupId;
                }

                var forumId = dr.GetInt("ForumId");
                if (forumId == 0)
                {
                    continue;
                }

                if (dr.GetInt("ParentForumID") == 0)
                {
                    this.drpGroups.Items.Add(new ListItem(" - " + dr.GetString("ForumName"), "FORUM" + forumId));
                }
            }

            dr.Close();
        }

        private void BindTabs()
        {
            var sb = new StringBuilder();
            var sLabel = (this.editorType == "M") ? "[RESX:DefaultSettings]" : (this.editorType == "F") ? "[RESX:Forum]" : "[RESX:Group]";

            sb.Append("<div id=\"divForum\" onclick=\"toggleTab(this);\" class=\"amtabsel\" style=\"margin-left:10px;\"><div id=\"divForum_text\" class=\"amtabseltext\">" + sLabel + "</div></div>");

            if (this.recordId > 0)
            {
                if (this.editorType == "G" && this.chkInheritModuleSecurity.Checked)
                {
                    sb.Append("<div id=\"divSecurity\" onclick=\"toggleTab(this);\" class=\"amtab\"style=\"display:none;\"><div id=\"divSecurity_text\" class=\"amtabtext\">[RESX:Security]</div></div>");
                }
                else if (this.editorType == "F" && this.chkInheritGroupSecurity.Checked)
                {
                    sb.Append("<div id=\"divSecurity\" onclick=\"toggleTab(this);\" class=\"amtab\"style=\"display:none;\"><div id=\"divSecurity_text\" class=\"amtabtext\">[RESX:Security]</div></div>");
                }
                else
                {
                    sb.Append("<div id=\"divSecurity\" onclick=\"toggleTab(this);\" class=\"amtab\"><div id=\"divSecurity_text\" class=\"amtabtext\">[RESX:Security]</div></div>");
                }

                if (this.editorType == "G" && this.chkInheritModuleFeatures.Checked)
                {
                    sb.Append("<div id=\"divSettings\" onclick=\"toggleTab(this);\" class=\"amtab\" style=\"display:none;\"><div id=\"divSettings_text\" class=\"amtabtext\">[RESX:Features]</div></div>");
                }
                else if (this.editorType == "F" && this.chkInheritGroupFeatures.Checked)
                {
                    sb.Append("<div id=\"divSettings\" onclick=\"toggleTab(this);\" class=\"amtab\" style=\"display:none;\"><div id=\"divSettings_text\" class=\"amtabtext\">[RESX:Features]</div></div>");
                }
                else
                {
                    sb.Append("<div id=\"divSettings\" onclick=\"toggleTab(this);\" class=\"amtab\"><div id=\"divSettings_text\" class=\"amtabtext\">[RESX:Features]</div></div>");
                }

                if (this.editorType == "F")
                {
                    sb.Append("<div id=\"divClean\" onclick=\"toggleTab(this);\" class=\"amtab\"><div id=\"divClean_text\" class=\"amtabtext\">[RESX:Maintenance]</div></div>");
                    sb.Append("<div id=\"divProperties\" onclick=\"toggleTab(this);\" class=\"amtab\"><div id=\"divProperties_text\" class=\"amtabtext\">[RESX:TopicProperties]</div></div>");
                }
            }

            this.litTabs.Text = sb.ToString();
        }

        private void BindRoles()
        {
            var rc = new RoleController();
            this.drpRoles.DataTextField = "RoleName";
            this.drpRoles.DataValueField = "RoleId";
            this.drpRoles.DataSource = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: this.PortalId);
            this.drpRoles.DataBind();
            this.drpRoles.Items.Insert(0, new ListItem("[RESX:DropDownDefault]", string.Empty));
        }

        private void BindTemplates()
        {
            this.BindTemplateDropDown(this.drpTopicsTemplate, Templates.TemplateTypes.TopicsView, "[RESX:Default]", "0");
            this.BindTemplateDropDown(this.drpTopicTemplate, Templates.TemplateTypes.TopicView, "[RESX:Default]", "0");
            this.BindTemplateDropDown(this.drpTopicForm, Templates.TemplateTypes.TopicForm, "[RESX:Default]", "0");
            this.BindTemplateDropDown(this.drpReplyForm, Templates.TemplateTypes.ReplyForm, "[RESX:Default]", "0");
            this.BindTemplateDropDown(this.drpQuickReplyForm, Templates.TemplateTypes.QuickReplyForm, "[RESX:Default]", "0");

            this.BindTemplateDropDown(this.drpProfileDisplay, Templates.TemplateTypes.PostInfo, "[RESX:Default]", "0");
            this.BindTemplateDropDown(this.drpModApprovedTemplateId, Templates.TemplateTypes.ModEmail, "[RESX:DropDownDisabled]", "0");
            this.BindTemplateDropDown(this.drpModDeleteTemplateId, Templates.TemplateTypes.ModEmail, "[RESX:DropDownDisabled]", "0");
            this.BindTemplateDropDown(this.drpModMoveTemplateId, Templates.TemplateTypes.ModEmail, "[RESX:DropDownDisabled]", "0");
            this.BindTemplateDropDown(this.drpModRejectTemplateId, Templates.TemplateTypes.ModEmail, "[RESX:DropDownDisabled]", "0");
            this.BindTemplateDropDown(this.drpModNotifyTemplateId, Templates.TemplateTypes.ModEmail, "[RESX:DropDownDisabled]", "0");
        }

        #endregion

    }
}
