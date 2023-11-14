<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="Classic.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.Classic" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<asp:placeholder ID="plhToolbar" runat="server" />
<div class="afcontainer" id="afcontainer">
	<div id="amnotify" class="amnotify"><div><i></i><span id="amnotify-message"></span></div></div>
	<asp:PlaceHolder ID="plhLoader" runat="server" />
</div>
<asp:Literal ID="litOutput" runat="server" />