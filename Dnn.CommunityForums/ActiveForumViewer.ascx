<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="ActiveForumViewer.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.ActiveForumViewer" %>
<%@ Register TagPrefix="active" Namespace="DotNetNuke.Modules.ActiveForums.Controls" assembly="DotNetNuke.Modules.ActiveForums" %>
<div class="dnn-community-forums activeForums"> <!-- Provides CSS Scope -->
    <div id="dcf-content-loading" class="dcf-content-loading">
        <active:forumloader id="ctlForumLoader" runat="server" />
    </div>
</div>