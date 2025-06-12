<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="admin_securitygrid.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.Controls.admin_securitygrid" %>
<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<script type="text/javascript">
    
    function addObject(){
        currAction = 'addobject';
        var ddlRole = document.getElementById("drpSecRoles");
        var objectName = ddlRole.options[ddlRole.selectedIndex].text;
        var objectId = ddlRole.options[ddlRole.selectedIndex].value;
        if (objectId !=''){
            ddlRole.selectedIndex = 0;
            securityAddObject(<%=PermissionsId%>,objectId,objectName,0);
        };
    };

var rebuild = false;

function securityDelObject(obj,oid,pid){
    if(confirm('[RESX:Actions:DeleteConfirm]')){
        rebuild = true;
        af_showLoad();
        securityCallback('delete', -1, pid, oid, '', '', securityToggleComplete);
    };
};

function securityAddObject(pid,secId,secName){
    af_showLoad();
    selectedTab = 'divSecurity';
    securityCallback('addobject', -1, pid, secId, secName, '', securityToggleComplete);      
    rebuild = true;
};

function securityToggleComplete(data){
    if (data.length > 2){
        var cellId = document.getElementById(data.split('|')[1]);
        var action = data.split('|')[0];
        var img = cellId.firstChild;
        if (action == 'remove'){
            img.src = imgOff.src
            img.setAttribute('alt','Disabled');
        }else{
            img.src = imgOn.src
            img.setAttribute('alt','Enabled');
        };
    };
    if (rebuild){
        rebuild = false;
        var currview = getQueryString()["cpview"];
        var currparms = getQueryString()["params"];
        LoadView(currview,currparms,'divSecurity');
    };
};

function secGridComplete(){
    af_clearLoad();
};

function securityCallback(action, returnId, pid, secId,secName, accessReq, callback) {
    var data = {};
    data.ModuleId = <%=ModuleId%>;
    data.Action = action;
    data.PermissionsId = pid;
    data.SecurityId = secId;
    data.SecurityAccessRequested = accessReq;
    data.ReturnId = returnId;
    var sf = $.ServicesFramework(<%=ModuleId%>);
    //sf.getAntiForgeryProperty(data);
    
    $.ajax({
        type: "POST",
        url: sf.getServiceRoot('ActiveForums') + "AdminService/ToggleSecurity",
        beforeSend: sf.setModuleHeaders,
        data: data,
        success: function (data) {
            callback(data);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
};

function securityToggle(obj,pid,secId,secName,key) {
    var returnId = obj.id;
    var img = obj.firstChild;
    if (img.src == imgOn.src){
        currAction = 'remove';
    }else{
        currAction = 'add';
    };
    img.src = imgSpin.src;
    img.setAttribute('alt','Please Wait');
    securityCallback(currAction, returnId, pid, secId, secName, key, securityToggleComplete);           
};

</script>
<div id="gridActions" runat="server">
    <table cellpadding="0" cellspacing="0" width="100%" style="padding-bottom: 4px;">

        <tr>
            <td class="amroles">
                <table cellpadding="0" cellspacing="2">
                    <tr>
                        <td style="width: 12px;">
                            <img id="Img43" src="~/DesktopModules/ActiveForums/images/tooltip.png" runat="server" onmouseover="amShowTip(this, '[RESX:Tips:AddRoles]');" onmouseout="amHideTip(this);" /></td>
                        <td>[RESX:Roles]:</td>
                        <td style="width: 150px;">
                            <asp:Literal ID="litRoles" runat="server" /></td>
                        <td style="width: 16px;">
                            <div class="amcpimgbtn" style="width: 16px;" onclick="addObject(0);">
                                <img id="Img41" src="~/DesktopModules/ActiveForums/images/add.png" runat="server" alt="[RESX:AddRole]" />
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</div>
<div class="afsecgrid">

    <am:callback id="cbSecGrid" runat="server" oncallbackcomplete="secGridComplete">
					<Content>
						<asp:Literal ID="litSecGrid" runat="server" />
					</Content>
				</am:callback>

</div>

<div style="display: none;">
    <am:callback id="cbSecurityToggle" runat="server" oncallbackcomplete="securityToggleComplete">
				<Content></Content>
			</am:callback>
</div>

