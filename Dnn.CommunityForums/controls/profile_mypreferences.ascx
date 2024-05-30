<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="profile_mypreferences.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.profile_mypreferences" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>
<style>
    .afpref{min-width:inherit !important}
</style>
<h3 class="dcf-heading-3">
    <dnn:label id="lblHeader" cssclass="aftitlelg" runat="server" text="[RESX:MySettings]" />
</h3>
<div class="dnnForm afpref">

    <div class="dnnFormItem">
        <dnn:label controlname="drpPrefDefaultSort" text="[RESX:PrefDefaultSort]" suffix=":" runat="server" />
        <asp:dropdownlist id="drpPrefDefaultSort" runat="server">
            <asp:ListItem Value="ASC" Text="[RESX:OldestFirst]"></asp:listitem>
            <asp:ListItem Value="DESC" Text="[RESX:NewestFirst]"></asp:listitem>
        </asp:dropdownlist>
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="drpPrefPageSize" text="[RESX:PrefPageSize]" suffix=":" runat="server" />
        <asp:dropdownlist id="drpPrefPageSize" runat="server">
            <asp:listitem>5</asp:listitem>
            <asp:listitem>10</asp:listitem>
            <asp:listitem>25</asp:listitem>
            <asp:listitem>50</asp:listitem>
            <asp:listitem>100</asp:listitem>
            <asp:listitem>200</asp:listitem>
        </asp:dropdownlist>
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefJumpToLastPost" text="[RESX:PrefJumpToLastPost]" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefJumpToLastPost" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefTopicSubscribe" text="[RESX:PrefTopicSubscribe]" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefTopicSubscribe" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefBlockAvatars" text="[RESX:PrefBlockAvatars]" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefBlockAvatars" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefBlockSignatures" text="[RESX:PrefBlockSignatures]" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefBlockSignatures" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="txtSignature" text="[RESX:Signature]" suffix=":" runat="server" />
        <asp:textbox id="txtSignature" runat="server" textmode="MultiLine" />
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnSave" CssClass="dnnPrimaryAction" runat="server" Text="[RESX:Save]" />
        </li>
    </ul>
</div>
