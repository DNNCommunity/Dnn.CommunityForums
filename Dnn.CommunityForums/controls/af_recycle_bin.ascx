<%@ control language="C#" autoeventwireup="false" codebehind="af_recycle_bin.ascx.cs" inherits="DotNetNuke.Modules.ActiveForums.af_recycle_bin" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web" %>
<h3 class="dcf-heading-3">
    <asp:label id="lblRecycleBin" runat="server">[RESX:RecycleBin]</asp:label>
</h3>
<div>
    <asp:UpdatePanel ID="upOptions1" UpdateMode="Conditional" runat="server" ChildrenAsTriggers="True" >
        <contenttemplate>
            <asp:GridView ID="dgrdRestoreView" AutoGenerateColumns="false" AllowPaging="true" PageSize="25"
                Width="100%" CellPadding="4" GridLines="None" CssClass="dnnGrid" runat="server">
                <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Left" />
                <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
                <AlternatingRowStyle CssClass="dnnGridAltItem" />
                <EditRowStyle CssClass="dnnFormInput" />
                <SelectedRowStyle CssClass="dnnFormError" />
                <FooterStyle CssClass="dnnGridFooter" />
                <pagerstyle cssclass="dnnGridPager dcf-pager" />
                <Columns>
                    <asp:buttonfield buttontype="Button"
                                     headertext=""
                                     text="[RESX:Restore]" headerstyle-horizontalalign="Center" itemstyle-horizontalalign="Center" ControlStyle-CssClass="dnnPrimaryAction" />
                    <asp:boundfield datafield="TopicId" readonly="True" visible="false" />
                    <asp:boundfield datafield="ReplyId" readonly="True" visible="false" />
                    <asp:checkboxfield datafield="IsReply" readonly="True" headerstyle-horizontalalign="Center" itemstyle-horizontalalign="Center"  controlstyle-cssclass="dnnCheckbox" />
                    <asp:boundfield datafield="ForumName" readonly="True" />
                    <asp:boundfield datafield="Subject" readonly="True" />
                    <asp:boundfield datafield="AuthorName" readonly="True" />
                    <asp:boundfield datafield="DateCreated" readonly="True" />
                </Columns>
                <EmptyDataTemplate>
                    <div></div>
                </EmptyDataTemplate>
            </asp:GridView>
        </contenttemplate>
    </asp:UpdatePanel>
</div>
<script type="text/ecmascript">
function RemoveRow(item) {
    var table = document.getElementById('dgrdRestoreView');
    table.deleteRow(item.parentNode.parentNode.rowIndex);
    return false;
}
</script>
