<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_search.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_search" EnableViewState="false" %>

<div id="Search" runat="server"></div>
<script type="text/javascript">

    $(document).ready(function() {

        $('.af-search-modify').button({ "icons": { "primary": "ui-icon-wrench" } }).click(function () {
            document.location.href = '<%=GetSearchUrl()%>';
            return false;
        });
    });

</script>
