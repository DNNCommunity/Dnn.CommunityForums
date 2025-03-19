<div class="dcf-search-results-wrap">
    <div class="af-search-header">

        <am:pagernav id="PagerTop" runat="server" />
        <h2 class="af-search-title">[RESX:Search]</h2>
        <br />
        <button class="af-search-modify">[RESX:SearchModify]</button>
        <br />
        <asp:PlaceHolder runat="server" ID="phKeywords">
            <span class="af-search-criteria">[RESX:SearchKeywords]<b>
                <asp:Literal runat="server" ID="litKeywords"></asp:Literal>
            </b>
            </span>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phUsername">
            <span class="af-search-criteria">[RESX:SearchByUser]<b>
                <asp:Literal runat="server" ID="litUserName"></asp:Literal>
            </b>
            </span>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phTag">
            <span class="af-search-criteria">[RESX:SearchByTag]<b>
                <asp:Literal runat="server" ID="litTag"></asp:Literal>
            </b>
            </span>
        </asp:PlaceHolder>
    </div>
    <div class="af-search-bar afgrouprow afgrouprow-f">
        <span class="af-search-duration">
            <asp:Literal runat="server" ID="litSearchDuration" />
            <asp:Literal runat="server" ID="litSearchAge" />
        </span>
        <span class="af-search-title">
            <label>[RESX:SearchTitle]</label>
        </span>
    </div>
    <asp:Panel ID="pnlMessage" runat="server" Visible="true" CssClass="af-search-noresults">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>
    <div class="af-search-results" style="position: relative;">

        <!-- Post View -->
        <asp:Repeater runat="server" ID="rptPosts" Visible="False">
            <HeaderTemplate>
                <div class="afgrid">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="aftopicrow af-content">
                    <div class="aftopicrow afsubject">
                        <span class="aftopictitle">[FORUMPOST:SUBJECTLINK|<a href="{0}" title="[FORUMPOST:BODYTITLE]" class="dcf-topic-link">[FORUMPOST:SUBJECT]</a>]
                        </span>
                        <span class="aftopicsubtitle">[RESX:SearchPosted] [FORUMPOST:DATECREATED] [FORUMPOST:AUTHORDISPLAYNAMELINK|[RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMPOST:AUTHORDISPLAYNAME]</a>]</span>
                        <span class="aftopicsubtitle">[FORUM:FORUMLINK|[RESX:IN] [RESX:SearchForum]<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]</span>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <!-- Topic View -->
        <asp:Repeater ID="rptTopics" runat="server" Visible="False">
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
                        <span class="aftopictitle">[FORUMTOPIC:SUBJECTLINK|<a href="{0}" title="[FORUMTOPIC:BODYTITLE]" class="dcf-topic-link">[FORUMTOPIC:SUBJECT]</a>]
                        </span>
                        <span class="aftopicsubtitle">[RESX:Started] [FORUMTOPIC:DATECREATED] [FORUMTOPIC:AUTHORDISPLAYNAMELINK|[RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:AUTHORDISPLAYNAME]</a>]</span>
                    </div>
                    <div class="aftopicrow af-colstats af-colstats-replies">[FORUMTOPIC:REPLYCOUNT]</div>
                    <div class="aftopicrow af-colstats af-colstats-views">[FORUMTOPIC:VIEWCOUNT]</div>
                    <div class="aftopicrow af-lastpost">
                        <div class="af_lastpost" style="white-space: nowrap;">
                            [FORUM:FORUMLINK|[RESX:IN] [RESX:SearchForum]<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]
                            [FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK|<br />
                            [RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]</a>
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
        </asp:Repeater>

    </div>
    <div class="af-search-footer">
        <am:pagernav id="PagerBottom" runat="server" />
        <span class="af-search-recordCount">
            <asp:Literal runat="server" ID="litRecordCount" />
        </span>
    </div>
</div>
