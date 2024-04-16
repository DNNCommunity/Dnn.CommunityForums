<%@ Control Language="C#" AutoEventWireup="false" Codebehind="af_modban.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_modban" %>
<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" assembly="DotNetNuke.Modules.ActiveForums" %>
<div class="afcrumb">[AF:LINK:FORUMMAIN] > [AF:LINK:FORUMGROUP] > [AF:LINK:FORUMNAME]</div>
<div class="aftitlelg">[RESX:BanUser]</div>
<div style="text-align: center; padding-top: 10px;">
    <div style="width: 450px; margin-left: auto; margin-right: auto; padding-top: 5px;">
		 <div class="afeditor">
             <table>
                 <tr>
                     <td align="center" colspan="2">
                         <ul class="dnnActions dnnClear">
                             <li><asp:LinkButton ID="btnBan" CssClass="dnnPrimaryAction" runat="server" Text="[RESX:Ban]" /></li>
                             <li><asp:LinkButton ID="btnCancel" CssClass="dnnSecondaryAction" runat="server" Text="[RESX:Cancel]" /></li>
                         </ul>
                     </td>
                     <td></td>
                 </tr>
             </table>
		   </div>
	  </div>			
</div>