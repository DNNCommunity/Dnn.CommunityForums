<%@ control language="C#" autoeventwireup="false" codebehind="admin_badges.ascx.cs" inherits="DotNetNuke.Modules.ActiveForums.admin_badges" %>
<%@ register tagprefix="dnn" tagname="URL" src="~/controls/DnnUrlControl.ascx" %>
<%@ register assembly="DotNetNuke.Modules.ActiveForums" namespace="DotNetNuke.Modules.ActiveForums.Controls" tagprefix="am" %>
<script type="text/javascript">
function renderDG(){
	<%=agBadges.ClientID%>.Callback();
};
var badgeOptions = {};
badgeOptions.width = "500";
badgeOptions.height = "350";
badgeOptions.modtitle = "[RESX:Badge]";
function openDialog(row){
	var badgeid;
	if (row != undefined){
		badgeid = row.cells[0].firstChild.nodeValue;
		var data = {};
		data.action = 14;
		data.BadgeId = badgeid;
		afadmin_callback(JSON.stringify(data), loadEdit);
	} else {
		badge.BadgeId = -1;
        $('#txtBadgeName').val('');
        $('#<%=txtBadgeDescription.ClientID%>').val('');
		$('#txtSortOrder').val('');
        $('#<%=drpBadgeMetrics.ClientID%>').val('-1');
        $('#txtThreshold').val('');
        $('#txtFileId').val('-1');
        $('#<%=ctlBadgeImage.ClientID%>').Url = '';
		am.UI.LoadDiv('afBadgeEdit', badgeOptions);
	};
};

function loadEdit(data) {
	badge = data;
	$('#txtBadgeName').val(data.Name);
    $('#<%=txtBadgeDescription.ClientID%>').val(data.Description);
	$('#txtSortOrder').val(data.SortOrder);
    $('#<%=drpBadgeMetrics.ClientID%>').val(data.BadgeMetric);
	$('#txtThreshold').val(data.Threshold);
    $('#txtFileId').val(data.FileId);
    $('#<%=ctlBadgeImage.ClientID%>').Url = data.Url;
	am.UI.LoadDiv('afBadgeEdit', badgeOptions);
}
var badge = {};
badge.BadgeId = -1;
badge.Name = '';
badge.Description = '';
badge.FileId = -1;
badge.Url = '';
badge.SortOrder = 0;
badge.BadgeMetric = 0;
badge.Threshold = 0;
function saveBadge() {
	var isvalid = true;
	badge.action = 15;
    badge.Name = $('#txtBadgeName').val();
    if (badge.Name === '') {
        isvalid = false;
    }
    badge.Description = $('#<%=txtBadgeDescription.ClientID%>').val();
    if (badge.Description === '') {
        isvalid = false;
    }
    badge.SortOrder = $('#txtSortOrder').val();
	badge.Threshold = $('#txtThreshold').val();
	if (isvalid) {
		badge.Threshold = parseInt(badge.Threshold);
		badge.SortOrder = parseInt(badge.SortOrder);
	}
	badge.BadgeMetric = $('#<%=drpBadgeMetrics.ClientID%>').val();
	if (badge.BadgeMetric === -1) {
		isvalid = false;
	}
    badge.FileId = $('#txtFileId').val();
    if (badge.FileId === -1) {
        isvalid = false;
    }
    badge.Url = $('#<%=ctlBadgeImage.ClientID%>').Url;
    alert(badge.Url);
    if (badge.Url === '') {
        isvalid = false;
    }
	if (isvalid) {
		am.UI.CloseDiv('afBadgeEdit');
		afadmin_callback(JSON.stringify(badge), renderDG);
	}
}



</script>
<div class="amcpsubnav">
    <div onclick="openDialog();" class="amcplnkbtn">
        [RESX:BadgeNew]
    </div>
</div>
<div class="amcpbrdnav">
    [RESX:Badges]
</div>
<div class="amcpcontrols">
    <am:activegrid id="agBadges" runat="server" imagepath="<%=ImagePath%>">
        <headertemplate>
            <table cellpadding="2" cellspacing="0" border="0" class="amGrid" style="width: 100%;">
                <tr>
                    <td columnname="BadgeId" style="display: none; width: 0px;"></td>
                    <td columnname="Description" style="display: none; width: 0px;"></td>
                    <td class="amcptblhdr" columnname="Name" style="height: 16px;">
                        <div class="amheadingcelltext">[RESX:BadgeName]</div>
                    </td>
                    <td class="amcptblhdr" columnname="SortOrder" style="width: 120px; height: 16px;">
                        <div class="amheadingcelltext">[RESX:SortOrder]</div>
                    </td>
                    <td class="amcptblhdr" columnname="BadgeMetric" style="display: none;"></td>
                    <td class="amcptblhdr" columnname="BadgeMetricEnumName" style="height: 16px; white-space: nowrap; width: 120px;">
                        <div class="amheadingcelltext">[RESX:BadgeMetric]</div>
                    </td>
                    <td class="amcptblhdr" columnname="Threshold" style="height: 16px; white-space: nowrap; width: 120px;">
                        <div class="amheadingcelltext">[RESX:BadgeThreshold]</div>
                    </td>
                    <td class="amcptblhdr" columnname="FileId" style="display: none;"></td>
                    <td class="amcptblhdr" columnname="ImageUrl" style="height: 16px; white-space: nowrap; width: 16px;"></td>
                </tr>
        </headertemplate>
        <itemtemplate>
            <tr style="display: none;" class="amdatarow">
                <td style="display: none;">##DataItem('BadgeId')##</td>
                <td style="display: none;">##DataItem('Description')##</td>
                <td class="amcpnormal" resize="true" onclick="openDialog(this.parentNode);">##DataItem('Name')##</td>
                <td class="amcpnormal" onclick="openDialog(this.parentNode);">##DataItem('SortOrder')##</td>
                <td class="amcpnormal" onclick="openDialog(this.parentNode);" style="display: none;">##DataItem('BadgeMetric')##</td>
                <td class="amcpnormal" onclick="openDialog(this.parentNode);" style="white-space: nowrap;">##DataItem('BadgeMetricEnumName')##</td>
                <td class="amcpnormal" onclick="openDialog(this.parentNode);" style="white-space: nowrap;">##DataItem('Threshold')##</td>
                <td style="display: none;">##DataItem('FileId')##</td>
                <td style="white-space: nowrap;">##DataItem('ImageUrl')##</td>
            </tr>
        </itemtemplate>
        <footertemplate>
            </table>
        </footertemplate>
    </am:activegrid>
</div>
<div id="afBadgeEdit" style="width: 500px; height: 350px; display: none;" title="[RESX:Badge]">
    <div class="dnnForm">
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeName]:</label>
            <input type="text" id="txtBadgeName" class="dnnFormRequired" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:Description]:</label>
            <asp:textbox type="text" runat="server" textmode="MultiLine" id="txtBadgeDescription" class="dnnFormRequired" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:SortOrder]:</label>
            <input type="text" id="txtSortOrder" class="dnnFormRequired" onkeypress="return onlyNumbers(event);" width="50" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeMetric]:</label>
            <asp:dropdownlist id="drpBadgeMetrics" runat="server" width="150" />
        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeThreshold]:</label>
            <input type="text" id="txtThreshold" class="dnnFormRequired" onkeypress="return onlyNumbers(event);" width="50" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeFileId]:</label>
            <input type="text" id="txtFileId" class="dnnFormRequired" onkeypress="return onlyNumbers(event);" width="50" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeImage]:</label>
            <dnn:url id="ctlBadgeImage" runat="server" width="300" showimages="true" showfiles="True" showurls="True" shownewwindow="false" showtrack="False" showlog="False" urltype="F" showtabs="False">
            </dnn:url>
        </div>
        <ul class="dnnActions dnnClear">
            <li><a href="#" onclick="saveBadge(); return false;" class="dnnPrimaryAction">[RESX:Button:Save]</a></li>
            <li><a href="#" class="confirm dnnSecondaryAction">[RESX:Button:Delete]</a></li>
            <li><a href="#" onclick="am.UI.CloseDiv('afBadgeEdit'); return false;" class="dnnSecondaryAction">[RESX:Button:Cancel]</a></li>
        </ul>
    </div>
</div>
<script type="text/javascript">
	jQuery(function ($) {
		var opts = {};
		opts.callbackTrue = function () {
			$(this).dialog("close");
			var data = {};
			data.action = 16;
			data.BadgeId = badge.BadgeId;
			am.UI.CloseDiv('afBadgeEdit');
			afadmin_callback(JSON.stringify(data), renderDG);
		};
		$('#afBadgeEdit .confirm').dnnConfirm(opts);
	});
</script>
