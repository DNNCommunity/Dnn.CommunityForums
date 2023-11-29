<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="profile_mysubscriptions.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.profile_mysubscriptions" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web" %>
<h3 class="dcf-group-title">
    <asp:Label ID="Label26" runat="server" resourcekey="Subscriptions" />
</h3>
<p>
    <asp:Label ID="Label27" runat="server" resourcekey="ThreadsTracked" />
    <asp:UpdatePanel ID="upOptions1" UpdateMode="Conditional" runat="server" ChildrenAsTriggers="True" >
        <contenttemplate>
    <asp:GridView ID="dgrdTopicSubs" AutoGenerateColumns="false" AllowPaging="true" PageSize="25"
        Width="96%" CellPadding="4" GridLines="None" CssClass="dnnGrid" runat="server">
        <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Left" />
        <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
        <AlternatingRowStyle CssClass="dnnGridAltItem" />
        <EditRowStyle CssClass="dnnFormInput" />
        <SelectedRowStyle CssClass="dnnFormError" />
        <FooterStyle CssClass="dnnGridFooter" />
        <pagerstyle cssclass="dnnGridPager dcf-pager" />
        <Columns>
            <asp:CheckBoxField DataField="Subscribed" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="Id" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="ForumId" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="ForumGroupName" ReadOnly="True" />
            <asp:BoundField DataField="ForumName" ReadOnly="True" />
            <asp:BoundField DataField="Subject" ReadOnly="True" />
            <asp:BoundField DataField="LastPostDate" ReadOnly="True" />
        </Columns>
        <EmptyDataTemplate>
            <asp:Label ID="Label2" runat="server" resourcekey="[RESX:NoSubscriptions]" />
        </EmptyDataTemplate>
    </asp:GridView>
        </contenttemplate>
        </asp:UpdatePanel>
</p>
<p>
    <asp:Label ID="Label28" runat="server" resourcekey="ForumsTracked" />
    <asp:UpdatePanel ID="UpdatePanel2" UpdateMode="Conditional" runat="server" ChildrenAsTriggers="True">
        <contenttemplate>
    <asp:GridView ID="dgrdForumSubs" AutoGenerateColumns="false" AllowPaging="true" PageSize="25"
        Width="96%" CellPadding="4" GridLines="None" CssClass="dnnGrid" runat="server">
        <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Left" />
        <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
        <AlternatingRowStyle CssClass="dnnGridAltItem" />
        <EditRowStyle CssClass="dnnFormInput" />
        <SelectedRowStyle CssClass="dnnFormError" />
        <FooterStyle CssClass="dnnGridFooter" />
        <pagerstyle cssclass="dnnGridPager dcf-pager" />
        <Columns>
            <asp:CheckBoxField DataField="Subscribed" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
            <asp:BoundField DataField="Id" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="ForumId" ReadOnly="True" Visible="false" />
            <asp:BoundField DataField="ForumGroupName" ReadOnly="True" />
            <asp:BoundField DataField="ForumName" ReadOnly="True" />
            <asp:BoundField DataField="LastPostDate" ReadOnly="True" Visible="false" />

        </Columns>
        <EmptyDataTemplate>
            <asp:Label ID="Label1" runat="server" resourcekey="[RESX:NoSubscriptions]" />
        </EmptyDataTemplate>
    </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
</p>
