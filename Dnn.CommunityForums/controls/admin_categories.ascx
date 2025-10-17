<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="admin_categories.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.admin_categories" %>
<%@ Register  assembly="DotNetNuke.Modules.ActiveForums" namespace="DotNetNuke.Modules.ActiveForums.Controls" tagPrefix="am" %>
<script type="text/javascript">
var itemid = 0;
function renderDG(){
	<%=agCategories.ClientID%>.Callback();
};
function amaf_openDialog(row){
	itemid = 0;
	var itemname;
	var fid=-1;
	var fgid=-1;
	if (row != undefined){
		itemid = row.cells[0].firstChild.nodeValue;
		fid = row.cells[1].firstChild.nodeValue;
		fgid = row.cells[2].firstChild.nodeValue;
		itemname = row.cells[3].firstChild.nodeValue;
		var grpvalue = '';
		if (fid>0){
			grpvalue = 'FORUM' + fid;
		}else if (fgid >0) {
			grpvalue = 'GROUP' + fgid;
		}else{
			grpvalue = '-1';
		};
		var txt = document.getElementById('<%=txtCategory.ClientID%>');
		txt.value = itemname;
		var grp = document.getElementById('<%=drpForums.ClientID%>');
		for(var i=0; i<=grp.length; i++){
			if(grpvalue == grp[i].value){
				grp.selectedIndex = i;
				break;
			};
		};
	};
};
function amaf_saveCategory(){
	var objectName = document.getElementById('<%=txtCategory.ClientID%>').value;
	if (objectName != ''){
	   var grp = document.getElementById('<%=drpForums.ClientID%>');
	   grp = grp.options[grp.selectedIndex].value;
		<%=agCategories.ClientID%>.Params = 'save:' + objectName + ':' + itemid + ':' + grp;
		itemid = 0;
		document.getElementById('<%=txtCategory.ClientID%>').value = '';
		<%=agCategories.ClientID%>.Callback();
	};
};
function amaf_deleteCategory(row){
	if (confirm('[RESX:Actions:DeleteConfirm]')){
		var itemid;
		if (row != undefined){
			itemid = row.cells[0].firstChild.nodeValue;
		};

		<%=agCategories.ClientID%>.Params = 'delete:' + itemid;
		<%=agCategories.ClientID%>.Callback();
	};
 };
</script>
<div class="amcpsubnav"><div class="amcplnkbtn">&nbsp;</div></div>
<div class="amcpbrdnav">[RESX:Categories]</div>
<div class="amcpcontrols">
	  <table>
		<tr>
			<td></td><td class="amcpbold">[RESX:CategoryName]:</td><td><asp:TextBox ID="txtCategory" runat="server" CssClass="amcptxtbx" Font-Size="14px" Height="18" Width="125"/></td><td>
			<asp:DropDownList ID="drpForums" runat="server" CssClass="amcptxtbx" />
			</td>
			<td><am:imagebutton id="imgSave" runat="server" postback="false" clientsidescript="amaf_saveCategory();" cssclass="amsmallbtn" text="[RESX:Save]" height="18" width="50" imagelocation="LEFT" imageurl="~/DesktopModules/ActiveForums/images/save16.png" /></td>
			<td></td>
		</tr>
	</table>
	<div style="height:400px;overflow:auto;">
	<am:activegrid id="agCategories" runat="server" defaultcolumn="CategoryName" pagesize="20000" imagepath="~/DesktopModules/activeforums/images/">
		<HeaderTemplate>
			<table cellpadding="2" cellspacing="0" border="0" class="amGrid" style="width:100%;">
					<tr>
						<td ColumnName="TagId" style="display:none;width:0px;"></td>
						<td ColumnName="ForumId" style="display:none;width:0px;"></td>
						<td ColumnName="ForumGroupId" style="display:none;width:0px;"></td>
						<td class="amcptblhdr" ColumnName="TagName" style="height:16px;">
							<div class="amheadingcelltext">[RESX:CategoryName]</div></td>
						<td class="amcptblhdr" ColumnName="Items" style="height:16px;white-space:nowrap;width:50px;">
							<div class="amheadingcelltext">[RESX:Items]</div>
						</td>
						<td class="amcptblhdr" style="height:16px;white-space:nowrap;width:30px;">
							<div class="amheadingcelltext">&nbsp;</div>
						</td>
					</tr>
		</HeaderTemplate>
		<ItemTemplate>
			<tr style="display:none;" class="amdatarow">
				<td style="display:none;">##DataItem('CategoryId')##</td>
				<td style="display:none;">##DataItem('ForumId')##</td>
				<td style="display:none;">##DataItem('ForumGroupId')##</td>
				<td class="amcpnormal" resize="true" onclick="amaf_openDialog(this.parentNode);">##DataItem('CategoryName')##</td>
				<td class="amcpnormal" onclick="openDialog(this.parentNode);" style="white-space:nowrap;text-align:center;">##DataItem('Items')##</td>
				<td class="amcpnormal" onclick="amaf_deleteCategory(this.parentNode);">##DataItem('CategoryId')##</td>
		   </tr>
	   </ItemTemplate>
		<FooterTemplate></table></FooterTemplate>
	</am:ActiveGrid>  
	</div>
</div>