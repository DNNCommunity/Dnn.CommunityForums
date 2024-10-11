<style>
    div.afgrid {
        width: 100%;
        display: table;
        font-size: 14px;
        font-weight: 300;
        color: #333;
    }
    div.aftopicrow {
        width: 100%;
        display: table-row;
    }
    div.aftopicrow  div.afsubject {
        width: 60%;
        display: table-cell;
    }
    div.af-colstats {
        display: table-cell;
        width: 10%;
    }
    div.af-lastpost {
        display: table-cell;
        width: 20%;
        white-space: nowrap;
        font-weight: 400;
        color: #888;
    }

    div.af-lastpost div {
        text-align: left;
        overflow: hidden;
        white-space: normal !important;
    }

    div.af-lastpost div a:link,
    div.af-lastpost div a:visited {
        text-decoration: none;
        font-size: 12px;
        font-weight: 700;
        color: #333;
        word-break: break-word;
    }

    .aftopictitle,
    .aftopictitle a  {
        font-size: 15px;
        /*display: block;*/
        font-weight: 700;
        color: #333 !important;
        letter-spacing: -0.5px;
        line-height: 1.25;
    }

    div.af-content div.afsubject {
        vertical-align: top;
        padding-right: 10px;
    }

    div.af-content div.afsubject > a {
        color: #333;
        font-weight: 700;
        font-size: 13px;
    }

    div.af-content div.afsubject .aftopicstarted a {
        color: #333;
        font-weight: 700;
        font-size: 14px;
        display: inline-block;
        margin-top: 4px;
        margin-bottom: 4px;
    }

    .aftopicstarted {
        display: block !important;
        color: #888;
        font-size: 14px;
    }
    .aftopicsubtitle {
        display: inline-block;
        font-size: 15px;
        font-weight: 400;
        display: block;
        color: #888;
        word-break: break-word;
        overflow: hidden;
    }

</style>
<div class="dcf-search-results-wrap">
    <div class="af-search-header">
        <am:pagernav id="PagerTop" runat="server" />
        <h1 class="af-search-title">
            <asp:Label ID="lblHeader" runat="server" /></h1>
        <button type="submit" runat="server" id="btnMarkRead" class="af-markread" visible="false" onclick="if(!af_confirmMarkAllRead()) return false;" />
        <asp:DropDownList ID="drpTimeFrame" runat="server" AutoPostBack="true" Visible="false">
            <asp:ListItem Value="15"></asp:ListItem>
            <asp:ListItem Value="30"></asp:ListItem>
            <asp:ListItem Value="45"></asp:ListItem>
            <asp:ListItem Value="60"></asp:ListItem>
            <asp:ListItem Value="120"></asp:ListItem>
            <asp:ListItem Value="360"></asp:ListItem>
            <asp:ListItem Value="720"></asp:ListItem>
            <asp:ListItem Value="1440"></asp:ListItem>
            <asp:ListItem Value="2880"></asp:ListItem>
            <asp:ListItem Value="10080"></asp:ListItem>
            <asp:ListItem Value="20160"></asp:ListItem>
            <asp:ListItem Value="40320"></asp:ListItem>
            <asp:ListItem Value="80640"></asp:ListItem>
        </asp:DropDownList>
    </div>
    <asp:panel id="pnlMessage" runat="server" visible="true" cssclass="af-search-noresults">
        <asp:literal id="litMessage" runat="server" />
    </asp:panel>
    <div class="af-search-results" style="position: relative;">
        
        <!-- Topic View -->
        <asp:repeater id="rptTopics" runat="server" visible="False">
            <HeaderTemplate>
                <div class="afgrid">
                    <div class="aftopicrow af-content">
                        <div class="aftopicrow afsubject">[RESX:Subject]</div>
                        <div class="aftopicrow af-colstats af-colstats-replies">[RESX:REPLIESHEADER]</div>
                        <div class="aftopicrow af-colstats af-colstats-views">[RESX:Views]</div>
                        <div class="aftopicrow af-lastpost">[RESX:LASTPOSTHEADER]</div>
                    </div>
            </HeaderTemplate>
            <ItemTemplate>
                <div class="aftopicrow af-content">
                    <div class="aftopicrow afsubject">
                        <span class="aftopictitle">
                            [FORUMTOPIC:SUBJECTLINK|<a class="dcf-title dcf-title-4">{0}</a>]
                        </span> 
                        <span class="aftopicsubtitle">[RESX:Started] [FORUMTOPIC:DATECREATED] [FORUMTOPIC:AUTHORDISPLAYNAMELINK|[RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:AUTHORDISPLAYNAME]</a>]</span>
                    </div>
                    <div class="aftopicrow af-colstats af-colstats-replies">[FORUMTOPIC:REPLYCOUNT]</div>
                    <div class="aftopicrow af-colstats af-colstats-views">[FORUMTOPIC:VIEWCOUNT]</div>
                    <div class="aftopicrow af-lastpost">
                        <div class="af_lastpost" style="white-space: nowrap;">
                            [FORUM:FORUMLINK|In: <a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]
                            [FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK|<br /> [RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]</a>
                            <br />
                            ]
                            [FORUMTOPIC:LASTPOSTDATE]
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:repeater>

    </div>
    <div class="af-search-footer">
        <am:pagernav id="PagerBottom" runat="server" />
        <span class="af-search-recordCount">
            <asp:literal runat="server" id="litRecordCount" />
        </span>
    </div>
</div>
