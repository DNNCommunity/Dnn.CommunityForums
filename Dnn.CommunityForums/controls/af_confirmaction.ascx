<%@ Control Language="C#" AutoEventWireup="false" Codebehind="af_confirmaction.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_confirmaction_new" %>
<div class="dcf-confirm-action">
	<table class="dcf-table AFGrid">
		<tr>
			<td colspan="2" class="afgrouprow">
				<asp:Label CssClass="dcf-confirm-title" id="lblTitle" runat="server" resourcekey="Title"></asp:Label></td>
		</tr>
		<tr>
			<td class="Normal" colSpan="2">
				<asp:Label CssClass="dcf-confirm-message" id="lblMessage" Runat="server"></asp:Label></td>
		</tr>
		<tr class="dcf-confirm-buttons">
			<td class="Normal">
				<asp:HyperLink id="hypForums" runat="server" resourcekey="Forum" CssClass="CommandButton"></asp:HyperLink></td>
			<td class="Normal">
				<asp:HyperLink id="hypPost" runat="server" resourcekey="Topic" CssClass="CommandButton"></asp:HyperLink></td>
		</tr>
		<tr>
			<td class="Normal" colSpan="2">
				<asp:HyperLink id="hypHome" runat="server" resourcekey="Home" CssClass="CommandButton"></asp:HyperLink></td>
		</tr>
	</table>
</div>
