<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="Classic.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.Classic" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<!-- 
// We do not need to use BS3 for now,
// for the time being, we will do all
// of our responsive w/o a framework.
 -->

<dnn:DnnCssInclude runat="server" FilePath="https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" Priority="10" Name="fontawesome" Version="4.7.0" />

<asp:placeholder ID="plhToolbar" runat="server" />
<div class="afcontainer" id="afcontainer">
	<div id="amnotify" class="amnotify"><div><i></i><span id="amnotify-message"></span></div></div>
	<asp:PlaceHolder ID="plhLoader" runat="server" />
</div>
<asp:Literal ID="litOutput" runat="server" />