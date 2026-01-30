<%@ Control Language="C#" AutoEventWireup="false" Codebehind="af_searchquick.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_searchquick" %>
<div class="dcf-quick-search-input">
	<asp:textbox id="txtSearch" maxlength="255" placeholder="[RESX:SearchFor]" runat="server" cssclass="afminisearchbox" />
    <asp:linkbutton id="lnkSearch" runat="server" text=""><i class="fa fa-search fa-fw fa-blue dcf-search-icon-button"></i></asp:linkbutton>                
</div>