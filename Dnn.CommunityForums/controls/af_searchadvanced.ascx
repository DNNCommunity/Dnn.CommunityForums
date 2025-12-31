<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_searchadvanced.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_searchadvanced" %>
<div class="af-adv-search">
    <div class="af-adv-search-box">
        <h2><asp:Literal runat="server" ID="litOptions" Text="[RESX:SearchOptions]" /></h2>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblSearch" Text="[RESX:SearchKeywords]" AssociatedControlID="txtSearch" />
            <asp:textbox runat="server" columns="50" maxlength="255" id="txtSearch" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblUserName" Text="[RESX:SearchByUser]" AssociatedControlID="txtUserName" />
            <asp:TextBox ID="txtUserName" runat="server" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblTags" Text="[RESX:SearchByTag]" AssociatedControlID="txtTags" />
            <asp:TextBox runat="server" ID="txtTags" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblForums" Text="[RESX:SearchInForums]" AssociatedControlID="lbForums" />
            <asp:ListBox runat="server" ID="lbForums" CssClass="af-adv-search-list" SelectionMode="Multiple" Rows="6" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblSearchDays" AssociatedControlID="drpSearchDays" Text="[RESX:SearchTimeFrame]" />
            <asp:DropDownList ID="drpSearchDays" runat="server" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblResultType" Text="[RESX:SearchResultType]" AssociatedControlID="drpResultType" />
            <asp:DropDownList runat="server" ID="drpResultType">
            </asp:DropDownList>
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblSortType" Text="[RESX:SearchSort]" AssociatedControlID="drpSort" />
            <asp:DropDownList runat="server" ID="drpSort">
            </asp:DropDownList>
        </div>
        <div class="af-adv-search-footer">
            <asp:button runat="server" cssclass="dnnPrimaryAction" id="btnSearch" text="[RESX:Search]" />
            <button runat="server" class="dnnSecondaryAction" id="btnReset" type="reset">[RESX:Reset]</button>
        </div>
    </div>
</div>

<script type="text/javascript">

    $(document).ready(function() {
        $('#<%= lbForums.ClientID %>').afForumSelector();
        $('.af-adv-search-footer').find('input:submit').each(function() { $(this).replaceWith('<button type="submit" name="' + $(this).attr('name') + '" class="' + $(this).attr('class') + '" id="' + $(this).attr('id') + '" >' + $(this).val() + '</button>');});
        $('.af-adv-search-footer :submit').button({ icons: { primary: "ui-icon-search" } }).click(function (e) {
            if (!$('#<%=txtSearch.ClientID%>').val() && !$('#<%=txtUserName.ClientID%>').val() && !$('#<%=txtTags.ClientID%>').val()) {
                $('.af-search-input-error').show().delay(1500).fadeOut('slow');
                return false;
            } 
        });
        $('.af-adv-search :text').keypress(function (event) { if (event.keyCode == 13) { $('#<%= btnSearch.ClientID%>').click(); } });
        $('.af-adv-search-footer :reset').button({ icons: { primary: "ui-icon-refresh" } });
        $('.af-adv-search-header-collapse').click(function() { $(this).siblings().toggle();  $(this).find(".ui-icon").toggleClass("ui-icon-triangle-1-n ui-icon-triangle-1-s"); });
    });

</script>