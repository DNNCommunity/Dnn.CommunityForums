<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_usersonline.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_usersonline" %>
<div class="afgrid">
    <div class="afgrid-inner">

        <div class="afgroupsection afgroupsectiononline" id="tblWhosOnline">
            <table width="100%">
                <tr>
                    <td class="afgrouprow af-groupname">
                        <div class="afgroupsectiontitle">Who's Online</div>
                        <img class="afarrow" id="imgGroupWHOS" onclick="toggleGroup('WHOS');" src="<%=ImagePath + "/images/arrows_down.png"%>" alt="-" />
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
