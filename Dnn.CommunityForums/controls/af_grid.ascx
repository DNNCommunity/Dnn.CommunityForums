<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_grid.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_grid" %>
<div id="FilteredTopics" runat="server"></div>
<script type="text/javascript">
    $(document).ready(function () {
        $('.af-markread').button({ "icons": { "primary": "ui-icon-check" } });
    });
    
    function af_confirmMarkAllRead() {
        return confirm('<%= GetSharedResource("MarkAllReadConfirm") %>');
    }
</script>
