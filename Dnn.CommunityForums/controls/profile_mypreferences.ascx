<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="profile_mypreferences.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.profile_mypreferences" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>
<style>
    .afpref{min-width:inherit !important}
</style>
<h3 class="dcf-heading-3">
    <dnn:label id="lblHeader" cssclass="aftitlelg" runat="server" resourcekey="[RESX:MySettings]" />
</h3>
<div class="dnnForm afpref">

    <div class="dnnFormItem">
        <dnn:label controlname="drpPrefDefaultSort" resourcekey="[RESX:PrefDefaultSort]" text="Default Sort" suffix=":" runat="server" />
        <asp:dropdownlist id="drpPrefDefaultSort" runat="server">
            <asp:listitem value="ASC" resourcekey="[RESX:OldestFirst]"></asp:listitem>
            <asp:listitem value="DESC" resourcekey="[RESX:NewestFirst]"></asp:listitem>
        </asp:dropdownlist>
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="drpPrefPageSize" resourcekey="[RESX:PrefPageSize]" text="Page Size" suffix=":" runat="server" />
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
        <dnn:label controlname="chkPrefJumpToLastPost" resourcekey="[RESX:PrefJumpToLastPost]" text="Jump to last post" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefJumpToLastPost" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefTopicSubscribe" resourcekey="[RESX:PrefTopicSubscribe]" text="Subscribe" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefTopicSubscribe" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefBlockAvatars" resourcekey="[RESX:PrefBlockAvatars]" text="Block Avatars" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefBlockAvatars" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="chkPrefBlockSignatures" resourcekey="[RESX:PrefBlockSignatures]" text="Block Signatures" suffix=":" runat="server" />
        <asp:checkbox id="chkPrefBlockSignatures" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label controlname="txtSignature" resourcekey="[RESX:Signature]" text="Signature" suffix=":" runat="server" />
        <asp:textbox id="txtSignature" runat="server" textmode="MultiLine" />
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:linkbutton id="btnSave" cssclass="dnnPrimaryAction" runat="server" text="Save" resourcekey="[RESX:Save]" />
        </li>
    </ul>
</div>
