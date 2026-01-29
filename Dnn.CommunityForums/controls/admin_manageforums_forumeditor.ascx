<%@ control language="C#" autoeventwireup="false" codebehind="admin_manageforums_forumeditor.ascx.cs" inherits="DotNetNuke.Modules.ActiveForums.admin_manageforums_forumeditor" %>
<%@ register assembly="DotNetNuke.Modules.ActiveForums" namespace="DotNetNuke.Modules.ActiveForums.Controls" tagprefix="am" %>
<script type="text/javascript">
	var afShell = document.getElementById('amcpcontainer');
	var imgOn = new Image();
	imgOn.src = '<%=imgOn%>';
	var imgOff = new Image();
	imgOff.src = '<%=imgOff%>';
	var bSaveSettings = false;
	var currAction = '';
	var imgSpin = new Image();
	imgSpin.src = imgPath + 'spinner.gif';
	var tmpImg = new Image();
	var tmpSrc = '';

    function forumSave() {
        closeAllProp();
        var fgp = document.getElementById("<%=drpGroups.ClientID%>");
        if(fgp && !AMPage.IsGroupValid('afforum')){return false};
        af_showLoad();
        var fname = document.getElementById("<%=txtForumName.ClientID%>");
        if (fname) {
            fname = fname.value;
        } else {
            fname = '';
        };
        var fdesc = document.getElementById("<%=txtForumDesc.ClientID%>");
        if (fdesc) {
            fdesc = fdesc.value;
        } else {
            fdesc = '';
        };
        var a = document.getElementById("<%=chkActive.ClientID%>");
        if (a) {
            a = a.checked;
        } else {
            a = false;
        };
        var h = document.getElementById("<%=chkHidden.ClientID%>");
        if (h) {
            h = h.checked;
        } else {
            h = false;
        }
        var so = document.getElementById("<%=hidSortOrder.ClientID%>"); 
        if (so) {
            so = so.value;
        } else {
            so = '';
        }
        var purl = document.getElementById("<%=txtPrefixURL.ClientID%>"); 
        if (purl) {
            purl = purl.value;
        } else {
            purl = '';
        }

        var inheritModuleSecurity = document.getElementById("<%=chkInheritModuleSecurity.ClientID%>");
        var inheritModuleFeatures = document.getElementById("<%=chkInheritModuleFeatures.ClientID%>");
        var inheritGroupSecurity = document.getElementById("<%=chkInheritGroupSecurity.ClientID%>");
        var inheritGroupFeatures = document.getElementById("<%=chkInheritGroupFeatures.ClientID%>");

        var updatingGroup = (inheritModuleFeatures);
        var updatingForum = (inheritGroupFeatures);
        var updatingDefaults = (!updatingGroup && !updatingForum);

        var forumId = document.getElementById("<%=hidForumId.ClientID%>");
        if (forumId.value === '') {
            forumId = 0;
            bSaveSettings = false;
        } else {
            forumId = forumId.value;
            bSaveSettings = true;
        };
        if (updatingForum) {
            fgp = fgp.options[fgp.selectedIndex].value;
            currAction = 'forumsave';
        } else if (updatingGroup) {
            fgp = 0;
            currAction = 'groupsave';
        } else if (updatingDefaults) {
            fgp = 0;
            currAction = 'modulesave';
        };
        if (inheritModuleFeatures) {
            inheritModuleFeatures = inheritModuleFeatures.checked;
            if (inheritModuleFeatures === true) {
                bSaveSettings = true;
            };
        } else {
            inheritModuleFeatures = false;
            //bSaveSettings = true;
        };
        if (inheritGroupFeatures) {
            inheritGroupFeatures = inheritGroupFeatures.checked;
            if (inheritGroupFeatures === true) {
                bSaveSettings = true;
            };
        } else {
            inheritGroupFeatures = false;
            //bSaveSettings = true;
        };

        if (inheritModuleSecurity) {
            inheritModuleSecurity = inheritModuleSecurity.checked;
            if (inheritModuleSecurity === true) {
                bSaveSettings = true;
            };
        } else {
            inheritModuleSecurity = false;
            //bSaveSettings = true;
        };
        if (inheritGroupSecurity) {
            inheritGroupSecurity = inheritGroupSecurity.checked;
            if (inheritGroupSecurity === true) {
                bSaveSettings = true;
            };
        } else {
            inheritGroupSecurity = false;
            //bSaveSettings = true;
        };
        if ((currAction === 'modulesave') || 
            (currAction === 'groupsave' && ((inheritModuleFeatures === false) || (inheritModuleSecurity === false))) ||
            (currAction === 'forumsave' && ((inheritGroupFeatures === false) || (inheritGroupSecurity === false)))
            ) {
            bSaveSettings = true;
        } else {
            bSaveSettings = false;
        };
        if (currAction === 'groupsave') {
            <%=cbEditorAction.ClientID%>.Callback(currAction, forumId, fgp, fname, fdesc, a, h, so, inheritModuleFeatures, inheritModuleSecurity, purl);
        } else if (currAction === 'forumsave')  {
            <%=cbEditorAction.ClientID%>.Callback(currAction, forumId, fgp, fname, fdesc, a, h, so, inheritGroupFeatures, inheritGroupSecurity, purl);
        } else {
         <%=cbEditorAction.ClientID%>.Callback('modulesave', forumId, fgp, fname, fdesc, a, h, so, inheritGroupFeatures, inheritGroupSecurity, purl);
        };
    };

    function cbEditorAction_complete(){

		var forumId = document.getElementById("<%=hidEditorResult.ClientID%>").value;
		document.getElementById("<%=hidForumId.ClientID%>").value = forumId;

		switch (currAction){
			case 'modulesave':
                window.top.af_setCurrObj(forumId,'M');
				break;
			case 'forumsave':
                window.top.af_setCurrObj(forumId,'F');
				break;
			case 'groupsave':
                window.top.af_setCurrObj(forumId,'G');
				break;       
			case 'delforum':
				window.top.bSaveSettings = false;
                window.top.af_setCurrObj(0,'F');
				break;
			case 'delgroup':
				bSaveSettings = false;
                window.top.af_setCurrObj(0,'G');
				break;   

		}
		if(bSaveSettings === true){
			bSaveSettings = false;
			saveSettings();
		}else{
			window.top.af_refreshView();
		};
		af_clearLoad();
		switch (currAction){
			case 'modulesave':
				currAction = '';
				amcp.UI.ShowSuccess('[RESX:Actions:DefaultsSaved]');
				break;
			case 'forumsave':
				currAction = '';
				amcp.UI.ShowSuccess('[RESX:Actions:ForumSaved]');
				break;
			case 'groupsave':
				currAction = '';
				amcp.UI.ShowSuccess('[RESX:Actions:GroupSaved]');
				break;
			case 'delforum':
				currAction = '';
				amcp.UI.ShowSuccess('[RESX:Actions:ForumDeleted]');
				window.location.href = window.location.href;
				break;
			case 'delgroup':
				currAction = '';
				amcp.UI.ShowSuccess('[RESX:Actions:GroupDeleted]');
				window.location.href = window.location.href;
				break;
			};
	
		};

function saveSettings(){
	var settingsAction = "modulesettingssave";
    var inheritModuleFeatures = document.getElementById("<%=chkInheritModuleFeatures.ClientID%>");
    var inheritGroupFeatures = document.getElementById("<%=chkInheritGroupFeatures.ClientID%>");
    if (inheritGroupFeatures != null){
        settingsAction = "forumsettingssave";
	}else if (inheritModuleFeatures != null) {
        settingsAction = "groupsettingssave";
    };

    var EmailAddress = document.getElementById("<%=txtEmailAddress.ClientID%>");
    if (EmailAddress) {
        EmailAddress = EmailAddress.value;
    } else {
        EmailAddress = '';
    };
    var EmailNotificationSubjectTemplate = document.getElementById("<%=txtEmailNotificationSubjectTemplate.ClientID%>");
    if (EmailNotificationSubjectTemplate) {
        EmailNotificationSubjectTemplate = EmailNotificationSubjectTemplate.value;
    } else {
        EmailNotificationSubjectTemplate = '';
    };
    var TemplateFileNameSuffix = document.getElementById("<%=txtTemplateFileNameSuffix.ClientID%>");
    if (TemplateFileNameSuffix) {
        TemplateFileNameSuffix = TemplateFileNameSuffix.value;
    } else {
        TemplateFileNameSuffix = '';
    };
    var CreatePostCount = document.getElementById("<%=txtCreatePostCount.ClientID%>");
    if (CreatePostCount) {
        CreatePostCount = CreatePostCount.value;
    } else {
        CreatePostCount = '';
    };
    var ReplyPostCount = document.getElementById("<%=txtReplyPostCount.ClientID%>");
    if (ReplyPostCount) {
        ReplyPostCount = ReplyPostCount.value;
    } else {
        ReplyPostCount = '';
    };
	var UseFilter = document.getElementById("<%=rdFilterOn.ClientID%>").checked;
	var AllowPostIcon = document.getElementById("<%=rdPostIconOn.ClientID%>").checked;
	var AllowEmoticons = document.getElementById("<%=rdEmotOn.ClientID%>").checked;
	var AllowScripts = document.getElementById("<%=rdScriptsOn.ClientID%>").checked;
	var IndexContent = document.getElementById("<%=rdIndexOn.ClientID%>").checked;
	var AllowRSS = document.getElementById("<%=rdRSSOn.ClientID%>").checked;
	var AllowAttach = document.getElementById("<%=rdAttachOn.ClientID%>").checked;
		
	var AttachCount = document.getElementById("<%=txtMaxAttach.ClientID%>").value;
	var AttachMaxSize = document.getElementById("<%=txtMaxAttachSize.ClientID%>").value;
	var AttachTypeAllowed = document.getElementById("<%=txtAllowedTypes.ClientID%>").value; 
	var AttachAllowBrowseSite = document.getElementById("<%=ckAllowBrowseSite.ClientID%>").checked;
	var MaxAttachWidth = document.getElementById("<%=txtMaxAttachWidth.ClientID%>").value;
	var MaxAttachHeight = document.getElementById("<%=txtMaxAttachHeight.ClientID%>").value;
	var AttachInsertAllowed = document.getElementById("<%=ckAttachInsertAllowed.ClientID%>").checked;
	var AttachConvertToJGPAllowed = document.getElementById("<%=ckConvertingToJpegAllowed.ClientID%>").checked;

	var AllowHtml = document.getElementById("<%=rdHTMLOn.ClientID%>").checked;
	var ed1 = document.getElementById("<%=drpEditorTypes.ClientID%>");
    if (ed1.selectedIndex >= 0) {
        var EditorType = ed1.options[ed1.selectedIndex].value;
    } else if (AllowHtml==true) {
        var EditorType = 3;/* default to forums-delivered CK Editor 4 */
    } else {
		var EditorType = 0;
	};
	var ed6 = document.getElementById("<%=drpPermittedRoles.ClientID%>");
    if (ed6.selectedIndex >= 0) {
        var EditorPermittedRoles = ed6.options[ed6.selectedIndex].value;
    } else {
        var EditorPermittedRoles = 0;
    };
		
	var IsModerated = document.getElementById("<%=rdModOn.ClientID%>").checked;
	var md1 = document.getElementById("<%=drpDefaultTrust.ClientID%>");
    if (md1.selectedIndex > 0) {
        var DefaultTrustLevel = md1.options[md1.selectedIndex].value;
    } else {
        var DefaultTrustLevel = 0;
    };
	var AutoTrustLevel = document.getElementById("<%=txtAutoTrustLevel.ClientID%>").value;

    var modNotifyApprove = document.getElementById("<%=chkModNotifyApprove.ClientID%>").checked;
    var modNotifyReject = document.getElementById("<%=chkModNotifyReject.ClientID%>").checked;
    var modNotifyDelete = document.getElementById("<%=chkModNotifyDelete.ClientID%>").checked;
    var modNotifyMove = document.getElementById("<%=chkModNotifyMove.ClientID%>").checked;
    var modNotifyAlert = document.getElementById("<%=chkModNotifyAlert.ClientID%>").checked;

    var as = document.getElementById("<%=rdAutoSubOn.ClientID%>");
	var as1 = document.getElementById('<%=hidRoles.ClientID%>');
	if (as != null){
		var AutoSubscribeEnabled = as.checked;
		var AutoSubscribeRoles = as1.value;
	}else{
		var AutoSubscribeEnabled = false;
		var AutoSubscribeRoles = ''
    };
	var AutoSubscribeNewTopicsOnly = document.getElementById("<%=chkAutoSubscribeNewTopicsOnly.ClientID%>").checked;
	if (AutoSubscribeNewTopicsOnly == null){AutoSubscribeNewTopicsOnly = false;};

    var AllowLikes = document.getElementById("<%=rdLikesOn.ClientID%>").checked;

	var UserMentions = document.getElementById("<%=rdMentionsOn.ClientID%>").checked;	
    var umvd1 = document.getElementById("<%=drpUserMentionVisibility.ClientID%>");
    if (umvd1.selectedIndex > 0) {
        var UserMentionVisibility = umvd1.options[umvd1.selectedIndex].value;
    } else {
        var UserMentionVisibility = 0;
    }

    var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
	<%=cbEditorAction.ClientID%>.Callback(settingsAction, forumid, EmailAddress, UseFilter, AllowPostIcon, AllowEmoticons, AllowScripts,
		IndexContent, AllowRSS, AllowAttach, AttachCount, AttachMaxSize, AttachTypeAllowed, AllowLikes, ReplyPostCount, AttachAllowBrowseSite, AttachInsertAllowed, MaxAttachWidth,
		MaxAttachHeight, AttachConvertToJGPAllowed, AllowHtml, EditorType, CreatePostCount, AutoSubscribeNewTopicsOnly, EditorPermittedRoles,
		AutoSubscribeRoles, IsModerated, DefaultTrustLevel, AutoTrustLevel, modNotifyApprove, modNotifyReject, modNotifyMove, modNotifyDelete,
        modNotifyAlert, AutoSubscribeEnabled, EmailNotificationSubjectTemplate, TemplateFileNameSuffix, UserMentions, UserMentionVisibility);


};
function toggleEditor(obj){
	closeAllProp();
	var editor = document.getElementById("<%=cfgHTML.ClientID%>");
	if (obj.value == '1'){
		editor.style.display = '';
	}else{
		editor.style.display = 'none';
		var winDiv = document.getElementById('edProp');
		winDiv.style.display = 'none';
	};
};

function toggleMod(obj){
	closeAllProp();
	var mod = document.getElementById("<%=cfgMod.ClientID%>");
	if (obj.value == '1'){
		mod.style.display = '';
	}else{
		mod.style.display = 'none';
		var winDiv = document.getElementById('modProp');
		mod.style.display = 'none';
	};
};

function toggleAttach(obj){
		closeAllProp();
		var attach = document.getElementById("<%=cfgAttach.ClientID%>");
	if (obj.value == '1'){
		attach.style.display = '';
	}else{
		attach.style.display = 'none';
		var winDiv = document.getElementById('attachProp');
		attach.style.display = 'none';
	};
};

function toggleMentions(obj){
	closeAllProp();
	var mentions = document.getElementById("<%=cfgMentions.ClientID%>");
	if (obj.value == '1'){
		mentions.style.display = '';
	}else{
		mentions.style.display = 'none';
		var winDiv = document.getElementById('mentionsProp');
		mentions.style.display = 'none';
	};
};

	function showProp(obj,win){
        var propertyWindow = document.getElementById(win);
        if (propertyWindow.style.display == '' || propertyWindow.style.display == 'block') {
            propertyWindow.style.display = 'none';
            hideSelectBoxes();
        } else {
            propertyWindow.style.display = '';
            var $propertyWindow = $(propertyWindow);
            var elem = $(obj);
            var position = elem.position();
            //position.left += 15;
            //position.top = (position.top - ($propertyWindow.height() / 2));
            position.left += 650;
            position.top = (position.top + ($propertyWindow.height() / 2));
            position.width = $propertyWindow.innerWidth() + 20;
            position.height = $propertyWindow.innerHeight();

            $propertyWindow.css(position);
            closeAllProp();
            displaySelectBoxes();
            propertyWindow.style.display = '';
        }
	};


function addRole(){
	var drp = document.getElementById('<%=drpRoles.ClientID%>');
	var hidRoles = document.getElementById('<%=hidRoles.ClientID%>');
	if (drp.selectedIndex > 0){
		var tb = document.getElementById('tblRoles');
		var lastRow = tb.rows.length;
		var iteration = lastRow;
		var row = tb.insertRow(lastRow);
		var cellLeft = row.insertCell(0);
		var textNode = document.createTextNode(iteration);
		textNode.nodeValue = drp.options[drp.selectedIndex].text;
		cellLeft.appendChild(textNode)
		var cellRight = row.insertCell(1);
		var img = document.createElement('img');
        img.src = imgPath + "delete16.png";
		img.onclick = function() {removeRole(this,drp.options[drp.selectedIndex].value)};
		cellRight.appendChild(img);        
		var hidRoles = document.getElementById('<%=hidRoles.ClientID%>');
		var newRole = drp.options[drp.selectedIndex].value;
		var arRoles = hidRoles.value.split(';');
		var addRole = true;
		for ( var i=0, len=arRoles.length; i<len; ++i ){
			if (arRoles[i] == newRole){
				addRole = false;
				break;
			};
		};
		if (addRole){
			hidRoles.value = hidRoles.value + newRole + ';';
		};
	};
};
function removeRole(item,roleid){
	var tb = document.getElementById('tblRoles');
	tb.deleteRow(item.parentNode.parentNode.rowIndex);
	var hidRoles = document.getElementById('<%=hidRoles.ClientID%>');
	var arRoles = hidRoles.value.split(';');
	hidRoles.value = '';
	var newRoles = '';
	for ( var i=0, len=arRoles.length; i<len; ++i ){
		if (arRoles[i] != roleid){
			if (arRoles[i] != ''){
				newRoles += arRoles[i] + ';';
			}
		};
	};
	hidRoles.value = newRoles;
};
function deleteForum(){
	currAction = 'delforum';
	if (confirm('[RESX:Actions:FinalConfirm]')){
		var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
		<%=cbEditorAction.ClientID%>.Callback('deleteforum',forumid);
	};
};
function deleteGroup(){
	currAction = 'delgroup';
	if (confirm('[RESX:Actions:FinalConfirm]')){
		var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
		<%=cbEditorAction.ClientID%>.Callback('deletegroup',forumid);
	};
};
function amaf_toggleInheritFeatures() {
    var chk1 = document.getElementById('<%=chkInheritModuleFeatures.ClientID%>');
    var chk2 = document.getElementById('<%=chkInheritGroupFeatures.ClientID%>');
	var trTmp = document.getElementById('<%=trTemplates.ClientID%>');
    var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
	var divSet = document.getElementById('divSettings');
    if (chk1 && chk1.checked) {
        if (trTmp) {
            trTmp.style.display = 'none';
        }
        if (divSet) {
            divSet.style.display = 'none';
        };

    } else if (chk2 && chk2.checked) {
		if (trTmp) {
            trTmp.style.display = 'none';
        }
        if (divSet) {
            divSet.style.display = 'none';
        };
    } else {
		forumSave();
		trTmp.style.display = '';
		if (forumid !== '') {
			divSet.style.display = '';
		};
	};
};

function amaf_toggleInheritSecurity() {
    var chk1 = document.getElementById('<%=chkInheritModuleSecurity.ClientID%>');
    var chk2 = document.getElementById('<%=chkInheritGroupSecurity.ClientID%>');
    var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
	var divSec = document.getElementById('divSecurity');
    if (chk1 && chk1.checked) {
        if (divSec != null) {
            divSec.style.display = 'none';
        };

    } else if (chk2 && chk2.checked) {
        if (divSec != null) {
            divSec.style.display = 'none';
        };

    } else {
        forumSave();
        if (forumid != '') {
            divSec.style.display = '';
        };
    };
};

function maintRun(opt){
	var topicsOlderThan = 0;
	var topicsByUser = 0;
	var activityOlderThan = 0;
	var chkTopicsOlderThan = document.getElementById("<%=chkTopicsOlderThan.ClientID%>").checked;
	var chkUserId = document.getElementById("<%=chkTopicsByUser.ClientID%>").checked;
	var chkActivityOlderThan = document.getElementById("<%=chkActivityOlderThan.ClientID%>").checked;
	var noReplies = document.getElementById("<%=chkNoReplies.ClientID%>").checked;
	var canGo = true;
	var selected = false;
	if (chkTopicsOlderThan && document.getElementById("<%=txtOlderThan.ClientID%>").value != ''){
		topicsOlderThan = document.getElementById("<%=txtOlderThan.ClientID%>").value;
		selected = true;
	}else if (chkTopicsOlderThan){

		canGo = false;
	};
	if (chkUserId && document.getElementById("<%=txtUserId.ClientID%>").value != ''){
		topicsByUser = document.getElementById("<%=txtUserId.ClientID%>").value;
			selected = true;
		}else if (chkUserId ){

			canGo = false;
		};
		if (chkActivityOlderThan && document.getElementById("<%=txtReplyOlderThan.ClientID%>").value != ''){
		activityOlderThan = document.getElementById("<%=txtReplyOlderThan.ClientID%>").value;
			selected = true;
		}else if (chkActivityOlderThan){
			canGo = false;
		};
		var auth = false;
		if (opt == 0){
			if (confirm('[RESX:Warning:Maint]')){
				auth = true;
			};
		}else{
			auth = true;
		};
		if (noReplies) {
			selected = true;
		};
		if (auth && canGo == true && selected == true){
			var dryRun = false;
			if (opt == 1) {
				dryRun = true;
			}
			var forumid = document.getElementById("<%=hidForumId.ClientID%>").value;
			var data = {};
			data.ModuleId = <%=ModuleId %>;
	data.ForumId = forumid;
	data.olderThan = topicsOlderThan;
	data.byUserId = topicsByUser;
	data.lastActive = activityOlderThan;
	data.withNoReplies = noReplies;
	data.dryRun = dryRun;
	var sf = $.ServicesFramework(<%=ModuleId%>);
		$.ajax({
			type: "POST",
			url: sf.getServiceRoot('ActiveForums') + "AdminService/RunMaintenance",
			beforeSend: sf.setModuleHeaders,
			data: data,
			success: function (data) {
				alert(data.Result);
			},
			error: function (xhr, status, error) {
				alert(error);
			}
		});
	}else if (canGo == false || selected == false){
		alert('[RESX:Maint:RequiredCriteria]');
	};
};


var asprop = {};
var asproplist = {};
function afadmin_saveProperty() {
	var tmp = afadmin_getPropertyInput();
	if (typeof (tmp) != 'undefined') {
		tmp.action = 2;
		afadmin_callback(JSON.stringify(tmp), afadmin_savePropertyComplete);
	};
};
function afadmin_savePropertyComplete(sender, result) {
	afadmin_cancelPropForm();
	af_setSession();
	afadmin_getProperties();
};
function afadmin_getPropertyInput() {
	var pname = afGet('txtPropertyName');
	if (pname.trim() == '') {
		return;
	};
	var dtype = afGet('drpDataType').value;
	if (dtype.indexOf('list')>=0){
		dtype += '|' + afGet('drpLists').value;
	};
	var phid = false;
	if (afGet('propHiddenYes').checked) {
		phid = true;
	};
	var pro = false;
	if (afGet('propReadOnlyYes').checked) {
		pro = true;
	};
	var preq = false;
	if (afGet('propReqYes').checked) {
		preq = true;
	};
	var propid = -1;
	var psort = -1;
	if (afGet('hidPropertyId') != '') {
		propid = afGet('hidPropertyId');
	};
	var lbl = afGet('txtLabel');
	var prex = afGet('txtValExp');
	var petemp = afGet('txtEditTemplate');
	var pvtemp = afGet('txtViewTemplate');
	psort = afGet('hidSortOrder');
	var pdv = afGet('txtDefaultValue');
	var objId = afGet('<%=hidForumId.ClientID%>');
	asprop.Name = pname;
	asprop.DataType = dtype;
	asprop.IsHidden = phid;
	asprop.IsReadOnly = pro;
	asprop.IsRequired = preq;
	asprop.ValidationExpression = encodeURI(escape(prex));
	asprop.EditTemplate = petemp;
	asprop.ViewTemplate = pvtemp;
	asprop.DefaultAccessControl = 1;
	asprop.ObjectType = 1;
	asprop.ObjectOwnerId = objId;
	asprop.PropertyId = propid;
	asprop.SortOrder = psort;
	asprop.DefaultValue = pdv;
	asprop.Label = lbl;
	return asprop;

};
function afadmin_getProperties() {
	var ul = document.getElementById('proplist');
	ul.style.display = 'block';
	afadmin_cancelPropForm()
	var req = {};
	req.action = 4;
	req.ObjectType = 1;
	var objId = afGet('<%=hidForumId.ClientID%>');
    req.ObjectOwnerId = objId;
	afadmin_callback(JSON.stringify(req), afadmin_buildProperties);
	};
	function afadmin_buildProperties(result) {
		asproplist = result;
		var ul = document.getElementById('proplist');
		var licur = amcp.Utils.GetElementsByClassName('candrag');

		if (typeof(licur) != 'undefined'){
			var cn = licur.length;

			for (var x = 0; x < cn; x++) {
				var el = licur[x];
				var p = el.parentNode;

				if (typeof (p) != 'undefined') {
					p.removeChild(el);
				};
			};
		};

		var p = {};
		for (var i = 0; i < asproplist.length; i++) {
			p = asproplist[i];
			var l = createLI(p);
			ul.appendChild(l);

		};
		var options = {
			itemHoverClass: 'myItemHover',
			dragTargetClass: 'myDragTarget',
			dropTargetClass: 'myDropTarget',
			useDefaultDragHandle: true
		};
		var lists = jQuery('#proplist').ListReorder(options);
		lists.bind('listorderchanged', function (evt, jqList, listOrder) {
			var props = '';
			for (var i = 0; i < listOrder.length; i++) {
				var idx = listOrder[i];
				var asprop = asproplist[idx];
				if (asprop.SortOrder != i) {
					asproplist[idx]['SortOrder'] = i;
					props += asprop.PropertyId + '|' + i + '^';
				};
			};
			if (props.length > 0) {
				var tmp = {};
				tmp.action = 5;
				tmp.props = props;
				afadmin_callback(JSON.stringify(tmp), afadmin_sortSaveComplete);
			};

		});
	};
	function afadmin_sortSaveComplete(result) {
		if (typeof (result.message) != 'undefined') {
			// alert(result.message);
		};
	};

	function afadmin_cancelPropForm() {
		asprop = {};
		afadmin_resetForm('ampropform');
		if (document.getElementById('propeditor').style.display == 'block') {
			amcp.UI.CloseDiv('propeditor');
		};

	};
	function afadmin_resetForm(id) {
		var container = document.getElementById(id);
		if (typeof (container) != 'undefined') {
			var elements = container.getElementsByTagName('input');
			for (var i = 0; i < elements.length; i++) {
				if (elements[i].type == 'text') {
					elements[i].value = '';
				} else if (elements[i].type == 'hidden'){
					elements[i].value = '';
				} else if (elements[i].type == 'radio') {
					if (elements[i].getAttribute('isdefault')) {
						elements[i].checked = true;
					} else {
						elements[i].checked = false;
					};
				} else if (elements[i].type == 'checkbox') {
					if (elements[i].getAttribute('isdefault')) {
						elements[i].checked = true;
					} else {
						elements[i].checked = false;
					};
				};
			};
			var elements = container.getElementsByTagName('select');
			for (var i = 0; i < elements.length; i++) {
				elements[i].selectedIndex = 0;
			};


		};
	};
	function createLI(prop) {
		var l = document.createElement('li');
		l.setAttribute('id', prop.PropertyId);
		l.setAttribute('class', 'afclear candrag');
		l.className = 'afclear candrag';
		var dr = document.createElement('div');
		dr.className = 'propname afclear';
		//dr.setAttribute('class', 'propname afclear');
		var a = document.createElement('a');
        var img = document.createElement('img');
        img.src = imgPath + "delete16.png";
        img.onclick = function() { afadmin_deleteProp(prop.PropertyId); };
        a.appendChild(img);
		a.onclick = function () { afadmin_deleteProp(prop.PropertyId); };
		dr.appendChild(a);
		a = document.createElement('a');
		a.appendChild(document.createTextNode(prop.Name));
		a.onclick = function () { afadmin_loadPropForm(this); };
		dr.appendChild(a);
		var ul = document.createElement('ul');
		//ul.setAttribute('class', 'aflistflat afclear');
		ul.className = 'aflistflat afclear';
		var la = document.createElement('li');
		la.appendChild(document.createTextNode('[RESX:IsHidden]'));
		if (prop.IsHidden == true) {
			la.className = 'checked';
			//la.setAttribute('class', 'checked');
		};
		ul.appendChild(la);
		la = document.createElement('li');
		la.appendChild(document.createTextNode('[RESX:IsRequired]'));
		if (prop.IsRequired == true) {
			//la.setAttribute('class', 'checked');
			la.className = 'checked';
		};
		ul.appendChild(la);
		la = document.createElement('li');
		la.appendChild(document.createTextNode('[RESX:IsReadOnly]'));
		if (prop.IsReadOnly == true) {
			//la.setAttribute('class', 'checked');
			la.className = 'checked';
		};
		ul.appendChild(la);
		dr.appendChild(ul);
		l.appendChild(dr);
		dr = document.createElement('div');
		//dr.setAttribute('class', 'acl');
		dr.className = 'acl';
		return l;
	};
	function afadmin_deleteProp(propertyid) {
		if (confirm('[RESX:WARN:DeleteProperty]')) {
			var tmp = {};
			tmp.action = 3;
			tmp.propertyid = propertyid;
			afadmin_callback(JSON.stringify(tmp), afadmin_getProperties);
		};
	};
	function afadmin_loadPropForm(obj) {

		var objNode = obj.parentNode;
		var count = 1;
		while (objNode.tagName != 'LI' && count < 100) {
			objNode = objNode.parentNode;
			count++;
		};
		for (var prop in asproplist) {
			if (asproplist[prop].PropertyId == objNode.id) {
				asprop = asproplist[prop];

			};
		};

		if (typeof (asprop) != 'undefined') {
			afV('hidPropertyId').value = asprop.PropertyId;
			afV('txtPropertyName').value = asprop.Name;

			afV('txtValExp').value = decodeURIComponent(asprop.ValidationExpression);

			afV('txtDefaultValue').value = asprop.DefaultValue;
			afV('hidSortOrder').value = asprop.SortOrder;
			afV('txtLabel').value = asprop.Label;
			if(asprop.DataType.indexOf("|")>0){
				var datatype = asprop.DataType.split('|')[0];
				var list = asprop.DataType.split('|')[1];
				amcp.Utils.SetSelected('drpDataType', datatype);
				afadmin_propDataTypeChange(list);

			}else{
				amcp.Utils.SetSelected('drpDataType', asprop.DataType);
			};


			if (asprop.IsHidden == true) {
				afV('propHiddenNo').checked = false;
				afV('propHiddenYes').checked = true;
			};
			if (asprop.IsReadOnly == true) {
				afV('propReadOnlyNo').checked = false;
				afV('propReadOnlyYes').checked = true;
			};
			if (asprop.IsRequired == true) {
				afV('propReqNo').checked = false;
				afV('propReqYes').checked = true;
			};

			amcp.UI.LoadDiv('propeditor', '[RESX:AddProperty]');
		};
	};
	function afadmin_LoadPropForm(){
		afadmin_resetForm('propeditor');
		amcp.UI.LoadDiv('propeditor','[RESX:AddProperty]')
	};
	function afadmin_propDataTypeChange(lname){
		var dtype = afGet('drpDataType');
		var drp = afV('drpDataType');
		var li = amcp.Utils.GetParentByTagName(drp,'TR');
		var tr = li.nextSibling; //.parentNode.nextSibling;
		if (typeof(tr.tagName) == 'undefined'){
			tr = li.nextSibling.nextSibling;
		};
		if(dtype.value.indexOf('list')>=0){
			tr.style.display = '';
			var tmp = {};
			tmp.action = 6;
			afadmin_callback(JSON.stringify(tmp), afadmin_loadLists);
		}else{
			tr.style.display = 'none';
		};
	};
	function afadmin_loadLists(result){
		if (result != ''){
			var drp = afV('drpLists');
			if (drp.options.length > 0){

				if (drp.hasChildNodes()) {
					while (drp.childNodes.length >= 1) {
						drp.removeChild(drp.firstChild);
					};
				};

			};

			for (var i = 0; i < result.length; i++) {
				p = result[i];
				amcp.Utils.addOption('',p.listid, p.listname, drp);
			};
			if(asprop!=null){
				if (typeof(asprop.DataType) != 'undefined'){
					if (asprop.DataType.indexOf('|')>0){
						var list = asprop.DataType.split('|')[1];
						amcp.Utils.SetSelected('drpLists', list);
					};
				};

			};
		};
	};
</script>
<asp:literal id="litScripts" runat="server" />
<div class="amcpsubnav">
    <div class="amcplnkbtn">&nbsp;</div>
</div>
<div class="amcpbrdnav"><span class="amcpbrdnavitem" onclick="LoadView('manageforums');">[RESX:ForumsGroups]</span> > [RESX:Details]&nbsp;<span runat="server" id="span_Parent" class="amcpbrdnavitem"></span></div>
<div class="amcpcontrolstab" id="amcpcontrolstab">
    <asp:literal id="litTabs" runat="server" />
    <div class="amtabcontent" id="amTabContent">
        <div style="width: 400px; margin-left: auto; margin-right: auto; padding-top: 5px;">
            <div id="divForum_afcontent" style="display: block; width: auto; min-height: 400px;" class="amtabdisplay">
                <table width="100%">
                    <tr id="trGroups" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:ForumGroup]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:ForumGroup]:</td>
                        <td width="100%">
                            <asp:dropdownlist id="drpGroups" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td style="width: 50px;">
                            <am:requiredfieldvalidator id="reqGroups" runat="server" text="*" controltovalidate="drpGroups" defaultvalue="-1" validationgroup="afforum" />
                        </td>
                    </tr>
                    <tr id="trName" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:ForumName]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">
                            <asp:label id="lblForumGroupName" runat="server" text="[RESX:ForumName]" />
                            :</td>
                        <td width="100%">
                            <asp:textbox id="txtForumName" runat="server" width="100%" cssclass="amcptxtbx" maxlength="255" />
                        </td>
                        <td>
                            <am:requiredfieldvalidator id="reqForumName" runat="server" text="*" controltovalidate="txtForumName" validationgroup="afforum" />
                        </td>
                    </tr>
                    <tr id="trDesc" runat="server">
                        <td valign="top">
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:ForumDesc]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" valign="top">[RESX:ForumDesc]:</td>
                        <td width="100%">
                            <asp:textbox id="txtForumDesc" runat="server" cssclass="amcptxtbx" textmode="MultiLine" />
                        </td>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/spacer.gif")%>" width="20" /></td>
                    </tr>
                    <tr id="trPrefix" runat="server">
                        <td valign="top">
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:VanityName]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" valign="top">[RESX:VanityName]:</td>
                        <td width="100%">
                            <asp:textbox id="txtPrefixURL" runat="server" width="100%" cssclass="amcptxtbx" maxlength="50" onkeypress="return filterVanity(this,event);" />
                        </td>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/spacer.gif")%>" width="20" height="" /></td>
                    </tr>
                </table>
                <table id="trActive" runat="server" width="100%">
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Active]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold">[RESX:Active]:</td>
                        <td width="100%">
                            <asp:checkbox id="chkActive" runat="server" checked="true" />
                        </td>
                        <td></td>
                    </tr>
                    <tr id="trHidden" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Hidden]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold">[RESX:Hidden]:</td>
                        <td>
                            <asp:checkbox id="chkHidden" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <table width="100%" id="trInheritModuleFeatures" runat="server">
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:InheritModuleFeatures]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">[RESX:InheritModuleFeatures]:</td>
                        <td width="100%">
                            <asp:checkbox id="chkInheritModuleFeatures" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <table width="100%" id="trInheritModuleSecurity" runat="server">
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:InheritModuleSecurity]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">[RESX:InheritModuleSecurity]:</td>
                        <td width="100%">
                            <asp:checkbox id="chkInheritModuleSecurity" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <table width="100%" id="trInheritGroupFeatures" runat="server">
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:InheritGroupFeatures]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">[RESX:InheritGroupFeatures]:</td>
                        <td width="100%">
                            <asp:checkbox id="chkInheritGroupFeatures" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <table width="100%" id="trInheritGroupSecurity" runat="server">
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:InheritGroupSecurity]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">[RESX:InheritGroupSecurity]:</td>
                        <td width="100%">
                            <asp:checkbox id="chkInheritGroupSecurity" runat="server" />
                        </td>
                        <td></td>
                    </tr>
                </table>
                <table width="100%" id="trTemplates" runat="server">
                    <tr>
                        <td colspan="4" class="amcpbold">[RESX:Templates]</td>
                    </tr>
                    <tr id="trEmail" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:EmailAddress]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:EmailAddress]:</td>
                        <td width="100%">
                            <asp:textbox id="txtEmailAddress" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td></td>
                    </tr>
                    <tr id="trEmailNotificationSubjectTemplate" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:EmailSubjectTemplate]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:EmailSubjectTemplate]:</td>
                        <td width="100%">
                            <asp:textbox id="txtEmailNotificationSubjectTemplate" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td></td>
                    </tr>
                    <tr id="trTemplateFileNameSuffix" runat="server">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:TemplateFileNameSuffix]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:TemplateFileNameSuffix]:</td>
                        <td width="100%">
                            <asp:textbox id="txtTemplateFileNameSuffix" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:CreatePostCount]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:CreatePostCount]:</td>
                        <td width="100%">
                            <asp:textbox id="txtCreatePostCount" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:ReplyPostCount]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:ReplyPostCount]:</td>
                        <td width="100%">
                            <asp:textbox id="txtReplyPostCount" runat="server" cssclass="amcptxtbx" />
                        </td>
                        <td></td>
                    </tr>
                </table>


                <asp:hiddenfield id="hidForumId" runat="server" />
                <asp:hiddenfield id="hidSortOrder" runat="server" />

            </div>
        </div>
        <div id="divSecurity_afcontent" style="display: none; width: auto;" class="amtabdisplay">
            <asp:placeholder id="plhGrid" runat="server" />


        </div>

        <div id="divSettings_afcontent" style="display: none;" class="amtabdisplay">
            <div style="width: 300px; margin-left: auto; margin-right: auto; padding-top: 5px;">
                <table>
                    <tr>
                        <td class="amcpbold" colspan="5" align="left">[RESX:Status]</td>
                    </tr>
                    <tr>
                        <td></td>
                        <td class="amcpbold"></td>
                        <td class="amcpbold">[RESX:Enabled]</td>
                        <td class="amcpbold">[RESX:Disabled]</td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Moderated]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:Moderated]</td>
                        <td align="center">
                            <asp:radiobutton id="rdModOn" groupname="Moderated" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdModOff" groupname="Moderated" runat="server" checked="true" />
                        </td>
                        <td width="100%">
                            <div id="cfgMod" runat="server" class="amcfgbtn" style="display: none;"></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Filters]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:EnableFilter]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdFilterOn" groupname="Filter" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdFilterOff" groupname="Filter" runat="server" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:PostIcon]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:AllowPostIcon]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdPostIconOn" groupname="PostIcon" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdPostIconOff" groupname="PostIcon" runat="server" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Emoticons]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:Emoticons]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdEmotOn" groupname="Emot" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdEmotOff" groupname="Emot" runat="server" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Scripts]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:AllowScripts]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdScriptsOn" groupname="Scripts" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdScriptsOff" groupname="Scripts" runat="server" checked="true" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:IndexContent]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:IndexContent]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdIndexOn" groupname="Index" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdIndexOff" groupname="Index" runat="server" checked="true" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:RSS]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:AllowRSS]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdRSSOn" groupname="RSS" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdRSSOff" groupname="RSS" runat="server" checked="true" />
                        </td>
                        <td width="100%"></td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:Attachments]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:AllowAttach]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdAttachOn" groupname="Attach" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdAttachOff" groupname="Attach" runat="server" checked="true" />
                        </td>
                        <td width="100%">
                            <div id="cfgAttach" runat="server" class="amcfgbtn" style="display: none;"></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:AllowHTML]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap">[RESX:AllowHTML]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdHTMLOn" groupname="HTML" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdHTMLOff" groupname="HTML" runat="server" />
                        </td>
                        <td width="100%">
                            <div id="cfgHTML" runat="server" class="amcfgbtn" style="display: none;"></div>
                        </td>
                    </tr>

                    <tr id="trAutoSub" runat="server" visible="false">
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:AutoSubscribe]');" onmouseout="amHideTip(this);" /></td>
                        <td class="amcpbold" style="white-space: nowrap;">[RESX:AutoSubscribe]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdAutoSubOn" groupname="AutoSubscribe" runat="server" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdAutoSubOff" groupname="AutoSubscribe" runat="server" checked="true" />
                        </td>
                        <td width="100%">
                            <div id="cfgAutoSub" runat="server" class="amcfgbtn" style="display: none;"></div>
                        </td>
                    </tr>

                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:AllowLikes]');" onmouseout="amHideTip(this);" /></td>

                        <td class="amcpbold" style="white-space: nowrap;">[RESX:AllowLikes]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdLikesOn" groupname="AllowLikes" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdLikesOff" groupname="AllowLikes" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:UserMentions]');" onmouseout="amHideTip(this);" /></td>

                        <td class="amcpbold" style="white-space: nowrap;">[RESX:UserMentions]:</td>
                        <td align="center">
                            <asp:radiobutton id="rdMentionsOn" groupname="UserMentions" runat="server" checked="true" />
                        </td>
                        <td align="center">
                            <asp:radiobutton id="rdMentionsOff" groupname="UserMentions" runat="server" />
                        </td>
                        <td width="100%">
                            <div id="cfgMentions" runat="server" class="amcfgbtn" style="display: none;"></div>
                        </td>
                    </tr>
                    
                </table>
            </div>
        </div>

        <div id="divClean_afcontent" style="display: none;" class="amtabdisplay">
            <div style="width: 500px; margin-left: auto; margin-right: auto; padding-top: 5px;">
                <table>

                    <tr>
                        <td class="amcpbold">
                            <asp:label id="lblMaintWarn" runat="server" text="[RESX:MaintenanceWarning]" />
                        </td>
                    </tr>
                    <tr>
                        <td>[RESX:MaintenanceInstruct]</td>
                    </tr>
                    <tr>
                        <td align="center">
                            <div style="width: 100%; padding-top: 1em;">
                                <table>
                                    <tr>
                                        <td>
                                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:MaintOlderThan]');" onmouseout="amHideTip(this);" /></td>
                                        <td>
                                            <asp:checkbox id="chkTopicsOlderThan" runat="server" />
                                        </td>
                                        <td>[RESX:Maint:TopicsOlderThan]</td>
                                        <td>
                                            <asp:textbox id="txtOlderThan" runat="server" class="amcptxtbx" width="40" />
                                            ([RESX:Maint:Days])</td>

                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:MaintTopicsByUser]');" onmouseout="amHideTip(this);" /></td>
                                        <td>
                                            <asp:checkbox id="chkTopicsByUser" runat="server" />
                                        </td>
                                        <td>[RESX:Maint:TopicsByUser]</td>
                                        <td>
                                            <asp:textbox id="txtUserId" runat="server" class="amcptxtbx" width="40" />
                                        </td>

                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:MaintWithOutReplies]');" onmouseout="amHideTip(this);" /></td>
                                        <td>
                                            <asp:checkbox id="chkNoReplies" runat="server" />
                                        </td>
                                        <td>[RESX:Maint:WithoutReplies]</td>
                                        <td></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/tooltip.png")%>" onmouseover="amShowTip(this, '[RESX:Tips:MaintActivityOlderThan]');" onmouseout="amHideTip(this);" /></td>
                                        <td>
                                            <asp:checkbox id="chkActivityOlderThan" runat="server" />
                                        </td>
                                        <td>[RESX:Maint:NoActivityPast]</td>
                                        <td>
                                            <asp:textbox id="txtReplyOlderThan" runat="server" class="amcptxtbx" width="40" />
                                            ([RESX:Maint:Days])</td>

                                    </tr>
                                    <tr>
                                        <td colspan="3" align="center">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <am:imagebutton runat="server" postback="false" clientsidescript="maintRun(1);" cssclass="amsmallbtn" text="[RESX:Maint:TestRun]" height="18" width="85" imagelocation="LEFT" imageurl="~/DesktopModules/ActiveForums/images/testrun16.png" />
                                                    </td>
                                                    <td>
                                                        <am:imagebutton runat="server" postback="false" clientsidescript="maintRun(0);" cssclass="amsmallbtn" text="[RESX:Maint:Execute]" height="18" width="85" imagelocation="LEFT" imageurl="~/DesktopModules/ActiveForums/images/execute16.png" />
                                                    </td>
                                                </tr>
                                            </table>

                                        </td>
                                    </tr>
                                </table>



                            </div>
                        </td>

                    </tr>
                </table>
            </div>
        </div>
        <div id="divProperties_afcontent" style="display: none;" class="amtabdisplay">
            <div class="amcphd">[RESX:TopicProperties]</div>
            <div class="amcpintro">[RESX:TopicProperties:Intro]</div>
            <asp:literal id="litTopicPropButton" runat="server" />
            <ul id="proplist" style="display: none;">
                <li class="afclear prophead">
                    <div style="float: left;">[RESX:Properties]</div>
                    <div style="width: 150px; float: right;">[RESX:Visibility]</div>
                </li>

            </ul>
        </div>
        <div class="amtbwrapper">
            <div class="amcpmdtoolbarbtm" id="amtoolbar">
                <am:imagebutton id="btnSave" runat="server" postback="False" clientsidescript="forumSave();" imagelocation="TOP" text="[RESX:Button:Save]" imageurl="~/DesktopModules/ActiveForums/images/save32.png" />
                <am:imagebutton id="btnDelete" confirm="true" confirmmessage="[RESX:Actions:ForumDeleteConfirm]" imagelocation="TOP" runat="server" postback="false" clientsidescript="deleteForum();" text="[RESX:Button:Delete]" imageurl="~/DesktopModules/ActiveForums/images/delete32.png" />
                <am:imagebutton id="btnClose" runat="server" postback="false" imagelocation="TOP" clientsidescript="LoadView('manageforums');" text="[RESX:Button:Cancel]" imageurl="~/DesktopModules/ActiveForums/images/cancel32.png" />
            </div>
        </div>
    </div>
</div>

<am:callback id="cbEditorAction" runat="server" oncallbackcomplete="cbEditorAction_complete">
    <content>
        <asp:hiddenfield id="hidEditorResult" runat="server" />
    </content>
</am:callback>

<!-- Forum Features HTML Input -->
<div class="ammodalpop" style="display: none; position: absolute;" id="edProp">
	<div style="margin: 0px; padding: 10px;">
		<table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px;">
			<tr>
				<td></td>
				<td class="amcpbold" style="white-space: nowrap">[RESX:PermittedRoles]:</td>
				<td>
					<asp:DropDownList ID="drpPermittedRoles" runat="server" CssClass="amcptxtbx">
						<asp:ListItem Value="0">[RESX:AllUsers]</asp:ListItem>
						<asp:ListItem Value="1">[RESX:RegisteredUsers]</asp:ListItem>
						<asp:ListItem Value="2">[RESX:TrustedUsers]</asp:ListItem>
						<asp:ListItem Value="3">[RESX:Moderators]</asp:ListItem>
						<asp:ListItem Value="4">[RESX:Administrators]</asp:ListItem>
					</asp:DropDownList></td>
				<td></td>
			</tr>
			<tr>
				<td></td>
				<td class="amcpbold" style="white-space: nowrap">[RESX:EditorType]:</td>
				<td>
					<asp:DropDownList ID="drpEditorTypes" runat="server" CssClass="amcptxtbx" >
					</asp:DropDownList>
				</td>
				<td></td>
			</tr>
		</table>
	</div>
</div>


<!-- Forum Features User Mentions -->
<div class="ammodalpop" style="display: none; position: absolute;" id="mentionsProp">
    <div style="margin: 0px; padding: 10px;">
        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px;">
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap">[RESX:UserMentionVisibility]:</td>
                <td>
                    <asp:dropdownlist id="drpUserMentionVisibility" runat="server" cssclass="amcptxtbx">
                    </asp:dropdownlist>
                </td>
                <td></td>
            </tr>
        </table>
    </div>
</div>

<div class="ammodalpop" style="display: none; position: absolute;" id="modProp">
    <div style="margin: 0px; padding: 10px;">

        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px; vertical-align: middle;">
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap">[RESX:DefaultTrust]:</td>
                <td>
                    <asp:dropdownlist id="drpDefaultTrust" runat="server">
                        <asp:listitem value="0" text="[RESX:NotTrusted]" />
                        <asp:listitem value="1" text="[RESX:Trusted]" />
                    </asp:dropdownlist>
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap">[RESX:AutoTrustLevel]:</td>
                <td>
                    <asp:textbox id="txtAutoTrustLevel" runat="server" width="50px" cssclass="amcptxtbx" text="0" />
                </td>
                <td></td>
            </tr>
        </table>
        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px; vertical-align: middle">
            <tr>
                <td colspan="4" class="amcpbold">[RESX:EmailTemplates]</td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:Notify]:</td>
                <td>
                    <asp:checkbox id="chkModNotifyAlert" runat="server" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:Approved]:</td>
                <td>
                    <asp:checkbox id="chkModNotifyApprove" runat="server" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:Rejected]:</td>
                <td>
                    <asp:checkbox id="chkModNotifyReject" runat="server" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:Moved]:</td>
                <td>
                    <asp:checkbox id="chkModNotifyMove" runat="server" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:Deleted]:</td>
                <td>
                    <asp:checkbox id="chkModNotifyDelete" runat="server" />
                </td>
                <td></td>
            </tr>

        </table>
    </div>
</div>
<div class="ammodalpop" style="display: none; position: absolute;" id="attachProp">
    <div style="margin: 0px; padding: 10px;">
        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px; vertical-align: middle">
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:MaxAttach]:</td>
                <td>
                    <asp:textbox id="txtMaxAttach" runat="server" cssclass="amcptxtbx" text="0" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:MaxFileSize]:</td>
                <td>
                    <asp:textbox id="txtMaxAttachSize" runat="server" cssclass="amcptxtbx" text="0" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:AllowedTypes]:</td>
                <td>
                    <asp:textbox id="txtAllowedTypes" runat="server" cssclass="amcptxtbx" text="" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:AllowBrowseSite]:</td>
                <td>
                    <asp:checkbox runat="server" id="ckAllowBrowseSite" checked="True" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:ConvertingToJpegAllowed]:</td>
                <td>
                    <asp:checkbox runat="server" id="ckConvertingToJpegAllowed" checked="True" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:AttachInsertAllowed]:</td>
                <td>
                    <asp:checkbox runat="server" id="ckAttachInsertAllowed" checked="True" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:MaxAttachWidth]:</td>
                <td>
                    <asp:textbox id="txtMaxAttachWidth" runat="server" cssclass="amcptxtbx" text="" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold">[RESX:MaxAttachHeight]:</td>
                <td>
                    <asp:textbox id="txtMaxAttachHeight" runat="server" cssclass="amcptxtbx" text="" />
                </td>
                <td></td>
            </tr>
        </table>
    </div>
</div>

<!-- Forums Features Auto Subscriptions -->
<div class="ammodalpop" style="display: none; position: absolute;" id="subProp">
    <div style="margin: 0px; padding: 10px;">
        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px;">
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap" colspan="3">[RESX:NewTopicsOnly]:</td>
                <td>
                    <asp:checkbox id="chkAutoSubscribeNewTopicsOnly" runat="server" />
                </td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap">[RESX:SelectRoles]:</td>
                <td>
                    <asp:dropdownlist id="drpRoles" runat="server" cssclass="amcptxtbx" />
                </td>
                <td>
                    <div onclick="addRole();">
                        <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/add.png")%>" border="0" align="absmiddle" alt="[RESX:ClickToAdd]" /></div>
                </td>
            </tr>
            <tr>
                <td colspan="4" class="amcpnormal">
                    <asp:literal id="tbRoles" runat="server" />
                </td>
            </tr>
        </table>
        <asp:hiddenfield id="hidRoles" runat="server" />
    </div>
</div>

<div class="ammodalpop" style="display: none; position: absolute;" id="socialProp">
    <div style="margin: 0px; padding: 10px;">
        <table cellpadding="0" cellspacing="2" border="0" style="margin: 0px; padding: 0px;">
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap" colspan="3">[RESX:NewTopicsOnly]:<asp:checkbox id="chkSocialTopicsOnly" runat="server" /></td>
            </tr>
            <tr>
                <td></td>
                <td class="amcpbold" style="white-space: nowrap">[RESX:SecurityOption]:</td>
                <td width="100%">
                    <asp:dropdownlist id="drpSocialSecurityOption" runat="server" cssclass="amcptxtbx">
                        <asp:listitem value="1" text="[RESX:Everyone]" />
                        <asp:listitem value="2" text="[RESX:Community]" />
                        <asp:listitem value="3" text="[RESX:FriendsOnly]" />
                        <asp:listitem value="6" text="[RESX:ViewRoles]" />
                    </asp:dropdownlist>
                </td>
                <td></td>
            </tr>

        </table>

    </div>
</div>


<div style="display: none; width: 500px; height: 350px; position: absolute; padding: 0; margin: 0;" id="propeditor" class="afmodalform afroundall">
    <div class="amcp-mod-inner" style="height: 348px;">
        <div class="amcp-mod-hd">
            <img src="<%=Page.ResolveUrl("~/DesktopModules/activeforums/images/close.gif")%>" onclick="afadmin_cancelPropForm();" border="0" />
            <div id="propeditor_header" style="float: left; padding: 3px; padding-top: 8px; padding-left: 10px; font-weight: bold;">[RESX:PropertyEditor]</div>
            <div class="afclear"></div>
        </div>
        <table class="amcpformlist afclear ampropeditor" id="ampropform">
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:PropertyName]:</label></td>
                <td>
                    <input type="text" id="txtPropertyName" title="[RESX:PropertyName]" required="true" value="" onkeyup="updateShadow(this,event,'txtLabel');" onkeypress="return filterInput(this,event,'txtLabel');" style="width: 225px;" /></td>
            </tr>
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:Label]:</label></td>
                <td>
                    <input type="text" id="txtLabel" title="[RESX:Label]" required="true" value="" style="width: 225px;" /></td>
            </tr>
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:DataType]:</label></td>
                <td>
                    <select id="drpDataType" onchange="afadmin_propDataTypeChange();" style="width: 225px;">
                        <option value="text">[RESX:Text]</option>
                        <option value="list">[RESX:ListDropDown]</option>
                        <option value="list-multi">[RESX:ListCheckBoxes]</option>
                        <option value="yesno">[RESX:YesNo]</option>
                    </select></td>
            </tr>
            <tr style="display: none;">
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:ListName]:</label></td>
                <td>
                    <select id="drpLists" style="width: 225px;"></select></td>
            </tr>
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:IsHidden]:</label></td>
                <td>
                    <div class="amcpradiorow"><span>[RESX:No]</span><input type="radio" name="prophidden" isdefault="true" value="false" id="propHiddenNo" checked="checked" /><span>[RESX:Yes]</span><input type="radio" name="prophidden" value="true" id="propHiddenYes" /></div>
                </td>
            </tr>
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:IsRequired]:</label></td>
                <td>
                    <div class="amcpradiorow"><span>[RESX:No]</span><input type="radio" name="propreq" isdefault="true" value="false" id="propReqNo" checked="checked" /><span>[RESX:Yes]</span><input type="radio" name="propreq" value="true" id="propReqYes" /></div>
                </td>
            </tr>
            <tr>
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:ValidationExpression]:</label></td>
                <td>
                    <input type="text" id="txtValExp" value="" style="width: 225px;" /></td>
            </tr>
            <tr style="display: none;">
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:DefaultValue]:</label></td>
                <td>
                    <input type="text" id="txtDefaultValue" value="" /></td>
            </tr>
            <tr style="display: none;">
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:EditTemplate]:</label></td>
                <td>
                    <input type="text" id="txtEditTemplate" value="" /></td>
            </tr>
            <tr style="display: none;">
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:ViewTemplate]:</label></td>
                <td>
                    <input type="text" id="txtViewTemplate" value="" /></td>
            </tr>
            <tr style="display: none;">
                <td style="text-align: right; width: 150px;">
                    <label>[RESX:IsReadOnly]:</label></td>
                <td>
                    <div class="amcpradiorow"><span>[RESX:No]</span><input type="radio" name="propreadonly" isdefault="true" value="false" id="propReadOnlyNo" checked="checked" /><span>[RESX:Yes]</span><input type="radio" name="propreadonly" value="true" id="propReadOnlyYes" /></div>
                </td>
            </tr>
        </table>


        <div class="afbuttonarea">
            <input type="button" title="[RESX:Save]" value="[RESX:Save]" id="btnSave" class="afbtn act" onclick="afadmin_saveProperty();" />
            <input type="button" title="[RESX:Cancel]" value="[RESX:Cancel]" onclick="afadmin_cancelPropForm();" id="btnCancel" class="afbtn dim" />
        </div>
        <input type="hidden" id="hidPropertyId" value="" />
        <input type="hidden" id="hidSortOrder" value="" />

    </div>
</div>

