<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_searchadvanced.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_searchadvanced" %>
<div class="af-adv-search">
    <div class="af-adv-search-box">
        <span class='af-adv-search-header'><asp:Literal runat="server" ID="litOptions" Text="[RESX:SearchOptions]" /></span>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblSearch" Text="[RESX:SearchKeywords]" AssociatedControlID="txtSearch" />
            <asp:TextBox runat="server" ID="txtSearch" />
            <asp:DropDownList runat="server" ID="drpSearchColumns" >
                <asp:ListItem Text="[RESX:SearchSubjectAndTopic]" Value="0" />
                <asp:ListItem Text="[RESX:SearchSubjectOnly]" Value="1" />
                <asp:ListItem Text="[RESX:SearchTopicOnly]" Value="2" />
            </asp:DropDownList>
            <asp:DropDownList runat="server" ID="drpSearchType">
                <asp:ListItem Text="[RESX:SearchTypeANYKeywords]" Value="0" />
                <asp:ListItem Text="[RESX:SearchTypeAllKeywords]" Value="1" />
                <asp:ListItem Text="[RESX:SearchTypeExactMatch]" Value="2" />
            </asp:DropDownList>
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblUserName" Text="[RESX:SearchByUser]" AssociatedControlID="txtUserName" />
            <asp:TextBox ID="txtUserName" runat="server" />
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblTags" Text="[RESX:SearchByTag]" AssociatedControlID="txtTags" />
            <asp:TextBox runat="server" ID="txtTags" />
        </div>
        <div class="af-adv-search-footer">
            <span class="af-search-input-error"><asp:Literal runat="server" ID="litInputError" Text="[RESX:SearchInputError]" /></span>
            <asp:Button runat="server" ID="btnSearch" Text="[RESX:Search]" />
            <button runat="server" id="btnReset" type="reset">[RESX:Reset]</button>
        </div>
    </div>
    <div class="af-adv-search-box">
        <div class='af-adv-search-header af-adv-search-header-collapse'><asp:Literal runat="server" Text="[RESX:SearchOptionsAdditional]" />&nbsp;<span class="ui-icon ui-icon-triangle-1-n"></span></div>
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
                <asp:ListItem Text="[RESX:SearchByTopics]" Value="0" />
                <asp:ListItem Text="[RESX:SearchByPosts]" Value="1" />
            </asp:DropDownList>
        </div>
        <div class="af-adv-search-row">
            <asp:Label runat="server" ID="lblSortType" Text="[RESX:SearchSort]" AssociatedControlID="drpSort" />
            <asp:DropDownList runat="server" ID="drpSort">
                <asp:ListItem Text="[RESX:SearchSortTypeRelevance]" Value="0" />
                <asp:ListItem Text="[RESX:SearchSortTypePostDate]" Value="1" />
            </asp:DropDownList>
        </div>
        <div class="af-adv-search-footer">
            <asp:Button runat="server" CssClass="afbtn-b" ID="btnSearch2" Text="[RESX:Search]" />
            <button runat="server" class="afbtn" id="btnReset2" type="reset">[RESX:Reset]</button>
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