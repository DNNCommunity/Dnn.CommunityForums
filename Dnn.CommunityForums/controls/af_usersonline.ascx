﻿<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_usersonline.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_usersonline" %>
<div class="afgrid">
    <div class="afgrid-inner">

        <div class="afgroupsection afgroupsectiononline" id="tblWhosOnline">
            <table width="100%">
                <tr>
                    <td class="afgrouprow af-groupname">
                        <div class="afgroupsectiontitle">[RESX:WhosOnline]</div>
                        <span class="dcf-collapsible" id="dcf-collapsible-groupWHOS" onclick="dcf_collapsible_toggle('groupWHOS');">
	                        <i class="fa"></i>
                        </span>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <div class="afgroup afgrouponline" id="groupWHOS" <%=DisplayMode%>>
                            <asp:Literal ID="litGuestsOnline" runat="server" /><br />
                            <div id="af-usersonline">
                                <asp:Literal ID="litUsersOnline" runat="server"></asp:Literal>&nbsp;
                            </div>
                        </div>

                    </td>
                </tr>
            </table>
        </div>

    </div>
</div>
