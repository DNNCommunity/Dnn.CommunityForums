<%@ control language="C#" autoeventwireup="false" codebehind="admin_badges.ascx.cs" inherits="DotNetNuke.Modules.ActiveForums.admin_badges" %>
<%@ Register Assembly="DotNetNuke.Modules.ActiveForums" Namespace="DotNetNuke.Modules.ActiveForums.Controls" TagPrefix="am" %>
<script type="text/javascript">
function renderDG(){
	<%=agBadges.ClientID%>.Callback();
};
var badgeOptions = {};
badgeOptions.width = "500";
badgeOptions.height = "350";
badgeOptions.modtitle = "[RESX:Badges]";
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
        $('#txtBadgeDescription').val('');
		$('#txtSortOrder').val('');
        $('#<%=drpBadgeMetrics.ClientID%>').val('-1');
		$('#txtThreshold').val('');
        $('#<%=drpBadgeImages.ClientID%>').val('-1');
		am.UI.LoadDiv('afBadgeEdit', badgeOptions);
	};
};

function loadEdit(data) {
	badge = data;
	$('#txtBadgeName').val(data.Name);
    $('#txtBadgeDescription').val(data.Description);
	$('#txtSortOrder').val(data.SortOrder);
    $('#<%=drpBadgeMetrics.ClientID%>').val(data.BadgeMetric);
	$('#txtThreshold').val(data.Threshold);
    $('#<%=drpBadgeImages.ClientID%>').val(data.FileId);
	am.UI.LoadDiv('afBadgeEdit', badgeOptions);
}
var badge = {};
badge.BadgeId = -1;
badge.Name = '';
badge.Description = '';
badge.FileId = 0;
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
    badge.FileId = $('#<%=drpBadgeImages.ClientID%>').val();
    if (badge.FileId === -1) {
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
		[RESX:BadgeNew]</div>
</div>
<div class="amcpbrdnav">
	[RESX:Badges]</div>
<div class="amcpcontrols">
	<am:ActiveGrid ID="agBadges" runat="server" ImagePath="<%=ImagePath%>">
		<headertemplate><table cellpadding="2" cellspacing="0" border="0" class="amGrid" style="width:100%;">
						<tr><td ColumnName="RankId" style="display:none;width:0px;"></td><td class="amcptblhdr" ColumnName="Name" style="height:16px;"><div class="amheadingcelltext">[RESX:BadgeName]</div></td><td class="amcptblhdr" ColumnName="SortOrder" style="width:120px;height:16px;"><div class="amheadingcelltext">[RESX:SortOrder]</div></td><td class="amcptblhdr" ColumnName="Threshold" style="height:16px;white-space:nowrap;width:120px;"><div class="amheadingcelltext">[RESX:BadgeThreshold]</div></td><td class="amcptblhdr" columnname="BadgeMetric" style="height: 16px; white-space: nowrap; width: 120px;"><div class="amheadingcelltext">[RESX:BadgeMetric]</div></td></tr>
			</headertemplate>
		<itemtemplate><tr style="display:none;" class="amdatarow"><td style="display:none;">##DataItem('BadgeId')##</td><td class="amcpnormal" resize="true" onclick="openDialog(this.parentNode);">##DataItem('Name')##</td><td class="amcpnormal" onclick="openDialog(this.parentNode);">##DataItem('SortOrder')##</td><td class="amcpnormal" onclick="openDialog(this.parentNode);" style="white-space:nowrap;">##DataItem('Threshold')##</td><td class="amcpnormal" onclick="openDialog(this.parentNode);" style="white-space:nowrap;">##DataItem('BadgeMetric')##</td></tr></itemtemplate>
		<footertemplate></table></footertemplate>
	</am:ActiveGrid>
</div>
<div id="afBadgeEdit" style="width:500px;height:350px;display:none;" title="[RESX:Badges]">
	<div class="dnnForm">
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeName]:</label>
            <input type="text" id="txtBadgeName" class="dnnFormRequired" />

        </div>
        <<div class="dnnFormItem">
            <label>
                [RESX:BadgeDescription]:</label>
            <input type="text" textmode="MultiLine" id="txtBadgeDescription" class="dnnFormRequired" />

        </div>
        <div class="dnnFormItem">
			<label>
				[RESX:SortOrder]:</label>
				<input type="text" id="txtSortOrder" class="dnnFormRequired" onkeypress="return onlyNumbers(event);" width="50" />

		</div>
		<div class="dnnFormItem">
			<label>
				[RESX:BadgeMetric]:</label>
			<asp:DropDownList ID="drpBadgeMetrics" runat="server" Width="150" />
		</div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeThreshold]:</label>
            <input type="text" id="txtThreshold" class="dnnFormRequired" onkeypress="return onlyNumbers(event);" width="50" />

        </div>
        <div class="dnnFormItem">
            <label>
                [RESX:BadgeImage]:</label>
            <asp:dropdownlist id="drpBadgeImages" runat="server" width="150" />
        </div>
        <ul class="dnnActions dnnClear">
			<li><a href="#" onclick="saveBadge(); return false;" class="dnnPrimaryAction">[RESX:Button:Save]</a></li>
			<li><a href="#" class="confirm dnnSecondaryAction">[RESX:Button:Delete]</a></li>
			<li><a href="#" onclick="am.UI.CloseDiv('afBadgeEdit'); return false;" class="dnnSecondaryAction">
				[RESX:Button:Cancel]</a></li>
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