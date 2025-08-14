<%@ control language="C#" autoeventwireup="false" codebehind="af_assign_user_badges.ascx.cs" inherits="DotNetNuke.Modules.ActiveForums.af_assign_user_badges" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web" %>
<h3 class="dcf-heading-3">
    <asp:label id="lblBadgesAssigned" runat="server">[RESX:UserBadgesAssigned]</asp:label>
</h3>
<div>
    
    <asp:UpdatePanel ID="upOptions1" UpdateMode="Conditional" runat="server" ChildrenAsTriggers="True" >
        <contenttemplate>
            <asp:GridView ID="dgrdUserBadges" AutoGenerateColumns="false" AllowPaging="true" PageSize="25"
                Width="100%" CellPadding="4" GridLines="None" CssClass="dnnGrid" runat="server">
                <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Left" />
                <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
                <AlternatingRowStyle CssClass="dnnGridAltItem" />
                <EditRowStyle CssClass="dnnFormInput" />
                <SelectedRowStyle CssClass="dnnFormError" />
                <FooterStyle CssClass="dnnGridFooter" />
                <pagerstyle cssclass="dnnGridPager dcf-pager" />
                <Columns>
                    <asp:CheckBoxField DataField="Assigned" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    <asp:boundfield datafield="UserBadgeId" readonly="True" visible="false" />
                    <asp:boundfield datafield="BadgeId" readonly="True" visible="false" />
                    <asp:BoundField DataField="BadgeName" ReadOnly="True" />
                    <asp:boundfield datafield="DateAssigned" readonly="True" />
                </Columns>
                <EmptyDataTemplate>
                    <div></div>
                </EmptyDataTemplate>
            </asp:GridView>
        </contenttemplate>
    </asp:UpdatePanel>
</div>
