<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="profile_mysubscriptions.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.profile_mysubscriptions" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web" %>
<h3 class="dcf-heading-3">
    <span>[RESX:Subscriptions]</span>
</h3>
<div>
    <span>[RESX:Topic] [RESX:Subscriptions]:</span>
    <asp:updatepanel id="updatePanel1" updatemode="Conditional" runat="server" childrenastriggers="True">
        <contenttemplate>
            <asp:gridview id="dgrdTopicSubs" autogeneratecolumns="false" allowpaging="false"
                width="100%" cellpadding="4" gridlines="None" cssclass="dnnGrid" runat="server">
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
                    <div>[RESX:NoSubscriptions]</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </contenttemplate>
    </asp:UpdatePanel>
</div>
<div>
    <span>[RESX:Forum] [RESX:Subscriptions]:</span>
    <asp:updatepanel id="updatePanel2" updatemode="Conditional" runat="server" childrenastriggers="True">
        <contenttemplate>
            <asp:GridView ID="dgrdForumSubs" AutoGenerateColumns="false" AllowPaging="false"
                Width="100%" CellPadding="4" GridLines="None" CssClass="dnnGrid" runat="server">
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
                    <div>[RESX:NoSubscriptions]</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div style="float: right;">
        <asp:Button ID="btnSubscribeAll" CssClass="dnnPrimaryAction" runat="server" Text="[RESX:SubscribeAllForums]" />
    </div>
</div>
