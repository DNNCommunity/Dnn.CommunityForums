<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_topicscripts.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.Controls.af_topicscripts" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.js" Priority="101" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Scripts/jquery/jquery.fileupload.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js" Priority="103" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/ActiveForums/scripts/splitmanager.js" Priority="104" />


<script type="text/javascript">
    $(document).ready(function() {

        // Wire up core private message buttons
        $.fn.dnnComposeMessage({
            openTriggerSelector: ".afcontainer .ComposeMessage",
            onPrePopulate: function (target) {    
                var recipientJSON = $(target).attr('data-recipient');              
                var recipient = $.parseJSON(recipientJSON);
                var prePopulatedRecipients = [recipient];
                return prePopulatedRecipients;
            },
            servicesFramework: $.ServicesFramework(<%=ModuleId%>)
        });
        
        // Create the user editor
        $.fn.afUserEditor({
            openTriggerSelector: ".af-button-edituser",
            servicesFramework: $.ServicesFramework(<%=ModuleId%>), 
            baseUrl: '<%=GetBaseUrl()%>',
            titleText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Title]")) %>",
            trustLevelText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:TrustLevel]")) %>",
            untrustedText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Untrusted]")) %>",
            neutralText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Neutral]")) %>",
            trustedText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Trusted]")) %>",
            userCaptionText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:UserCaption]")) %>",
            rewardPointsText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:RewardPoints]")) %>",
            signatureText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Signature]")) %>",
            updateText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Update]")) %>",
            updateBadges: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:UpdateBadges]")) %>",
            cancelText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Cancel]")) %>",
            getUserProfileErrorText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:GetProfileError]")) %>",
            updateUserProfileErrorText: "<%= System.Web.HttpUtility.JavaScriptStringEncode(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:UpdateProfileError]")) %>"
        });

        // Split selected posts
        $.fn.afSplitManager({
            openTriggerSelector: ".dcf-split-button-save",
            servicesFramework: $.ServicesFramework(<%=ModuleId%>)
        });        
    });

</script>