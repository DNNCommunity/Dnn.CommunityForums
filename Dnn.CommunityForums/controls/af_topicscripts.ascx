<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_topicscripts.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.Controls.af_topicscripts" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.js" Priority="101" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Scripts/jquery/jquery.fileupload.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js" Priority="103" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/ActiveForums/scripts/splitmanager.js" Priority="104" />

<span id="afUserEditorResources<%=ModuleId%>" hidden
      data-title="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Edit]") + " " + DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("User") + ":" %>"
      data-trust-level="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:TrustLevel]") %>"
      data-untrusted="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:NotTrusted]") %>"
      data-neutral="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:Neutral]") %>"
      data-trusted="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Trusted]") %>"
      data-user-caption="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserCaption]") %>"
      data-reward-points="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:RewardPoints]") %>"
      data-signature="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Signature]") %>"
      data-update="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("Update") + " " + DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("User") %>"
      data-update-badges="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:UpdateBadges]") %>"
      data-cancel="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Cancel]") %>"
      data-get-user-profile-error="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:GetProfileError]") %>"
      data-update-user-profile-error="<%: DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:UserEditor:UpdateProfileError]") %>"></span>

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
        var userEditorResources = document.getElementById("afUserEditorResources<%=ModuleId%>").dataset;
        $.fn.afUserEditor({
            openTriggerSelector: ".af-button-edituser",
            servicesFramework: $.ServicesFramework(<%=ModuleId%>), 
            baseUrl: '<%=GetBaseUrl()%>',
            titleText: userEditorResources.title,
            trustLevelText: userEditorResources.trustLevel,
            untrustedText: userEditorResources.untrusted,
            neutralText: userEditorResources.neutral,
            trustedText: userEditorResources.trusted,
            userCaptionText: userEditorResources.userCaption,
            rewardPointsText: userEditorResources.rewardPoints,
            signatureText: userEditorResources.signature,
            updateText: userEditorResources.update,
            updateBadges: userEditorResources.updateBadges,
            cancelText: userEditorResources.cancel,
            getUserProfileErrorText: userEditorResources.getUserProfileError,
            updateUserProfileErrorText: userEditorResources.updateUserProfileError
        });

        // Split selected posts
        $.fn.afSplitManager({
            openTriggerSelector: ".dcf-split-button-save",
            servicesFramework: $.ServicesFramework(<%=ModuleId%>)
        });        
    });

</script>