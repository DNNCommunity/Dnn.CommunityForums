<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="_default.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums._default" %>
<div class="dcf-init-screen">

    <div class="dcf-init-logo-wrap">
        <img class="dcf-init-logo" runat="server" src="~/DesktopModules/ActiveForums/images/branding/logo/DNN-Community-Forums-Logo-Horizontal.png" alt="DNN Community Forums" />
    </div>

    [RESX:InitConfigScreen]
    <p>
        [RESX:ClickContinueToStart]<br />

    <div class="dcf-buttons">
        <asp:Button ID="btnContinue" runat="server" CssClass="dnnPrimaryAction" Text="[RESX:Continue]" />
    </div>
    </p>
</div>
