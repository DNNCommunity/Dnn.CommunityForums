<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="ActiveForumViewerSettings.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.ActiveForumViewerSettings" %>
<table>
	<tr>
		<td class="Normal">Select Forum Instance:</td>
		<td><asp:DropDownList id="drpForumInstance" runat="server" AutoPostBack="True"></asp:DropDownList></td>
	</tr>
	<tr>
		<td class="Normal">Select Forum:</td>
		<td><asp:DropDownList id="drpForum" runat="server"></asp:DropDownList></td>
	</tr>
</table>