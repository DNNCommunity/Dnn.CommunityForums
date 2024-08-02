<%@ Control Language="C#" AutoEventWireup="false" Codebehind="af_quickreply.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_quickreplyform" %>
<script type="text/javascript">
<!--
	var isSubmitted = false;
function insertCode(sText) {
		var newMessage;
		var strMessage = document.getElementById('txtBody').value;
		newMessage = strMessage+sText;
		document.getElementById('txtBody').value=newMessage;
		document.getElementById('txtBody').focus();
		return;
	};
function nothing () {
		return;
	};	


function insertQuote() {
	var txt = "";
	if (document.getSelection) txt = document.getSelection();
	else if (document.selection && document.selection.createRange) txt = document.selection.createRange().text;
	else return;
	if (txt != "")	{
		var s = new String();
		s += "[quote]";
		s += txt + "[/quote]";
		insertCode(s);	
	}else
		insertCode('[quote] [/quote]');
};
function afQuickSubmit() {
	if (isSubmitted == false) {
		isSubmitted = true;
		var hid = document.getElementById('hidReply1');
		hid.value = 'true';
		document.forms[0].submit();
	}
};

$(document).on("keydown", "#txtBody", function (e) {
    if ((e.ctrlKey || e.metaKey) && (e.keyCode == 13 || e.keyCode == 13)) {
        // Ctrl + Enter pressed
        afQuickSubmit();
    }
});

//-->
</script>

		<div style="display:none;visibility:hidden;" >
		
            <label ID="contactByFaxOnly" runat="server"
               ControlName="ContactByFaxOnlyCheckBox"
               ResourceKey="ContactByFaxOnly"
               Suffix=":"
               TabIndex="-1" />
            <asp:CheckBox ID="contactByFaxOnlyCheckBox" runat="server"
               AutoPostBack="true"
               Checked="false"
               OnCheckedChanged="ContactByFaxOnlyCheckBox_CheckedChanged"
               TabIndex="-1" />
        </div>
<div id="qR" runat="server" />
<input type="hidden" name="hidReply1" id="hidReply1" value="" />
<input type="hidden" name="hidReply2" id="hidReply2" value="" />