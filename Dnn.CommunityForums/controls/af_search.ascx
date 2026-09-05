<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_search.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_search" EnableViewState="false" %>
<%@ Register TagPrefix="am" TagName="SearchAdvanced" Src="~/DesktopModules/ActiveForums/controls/af_searchadvanced.ascx" %>

<am:SearchAdvanced ID="SearchAdvanced" runat="server" />
<div id="Search" runat="server"></div>
<script type="text/javascript">

    $(document).ready(function() {

        $('.af-search-modify').button().click(function () {
            document.location.href = '<%=GetSearchUrl()%>';
            return false;
        });
    });

</script>
