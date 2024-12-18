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
        </asp:placeholder>
        <asp:placeholder runat="server" id="phUsername">
            <span class="af-search-criteria">[RESX:SearchByUser]<b>
                    <asp:literal runat="server" id="litUserName"></asp:literal>
                </b>
            </span>
        </asp:placeholder>
        <asp:PlaceHolder runat="server" ID="phTag">
            <span class="af-search-criteria">[RESX:SearchByTag]<b>
                    <asp:Literal runat="server" ID="litTag"></asp:literal>
                </b>
            </span>
        </asp:placeholder>
    </div>
    <div class="af-search-bar afgrouprow afgrouprow-f">
        <span class="af-search-duration">
            <asp:literal runat="server" id="litSearchDuration" />
            <asp:literal runat="server" id="litSearchAge" />
        </span>
        <span class="af-search-title">
            <label>[RESX:SearchTitle]</label>
        </span>
    </div>
    <asp:panel id="pnlMessage" runat="server" visible="true" cssclass="af-search-noresults">
        <asp:literal id="litMessage" runat="server" />
    </asp:panel>
    <div class="af-search-results" style="position: relative;">

        <!-- Post View -->
        <asp:repeater runat="server" id="rptPosts" visible="False">
            <HeaderTemplate></HeaderTemplate>
            <ItemTemplate>
                <div class="af-post">
                    <div class="af-post-header">
                        <div class="af-stats">
                            <label>[RESX:SearchReplies]</label><span id="ReplyCount" runat="server">[FORUMTOPIC:REPLYCOUNT]</span><br />
                            <label>[RESX:SearchViews]</label><span>[FORUMTOPIC:VIEWCOUNT]</span>
                        </div>
                        <div class="af-forum">
                            <label>[RESX:SearchForum]</label>
                            [FORUM:FORUMLINK|<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]
                        </div>
                        <div class="af-thread">
                            <label>[RESX:SearchTopic]</label>
                            [FORUMTOPIC:SUBJECTLINK|<a class="dcf-title dcf-title-4">{0}</a>]
                        </div>
                        <div class="af-postinfo">
                            <label>[RESX:SearchPosted]</label>[FORUMPOST:DATECREATED] [FORUMPOST:AUTHORDISPLAYNAMELINK|[RESX:BY] <a href="{0}" class="af-profile-link" rel="nofollow">[FORUMPOST:AUTHORDISPLAYNAME]</a>]
                        </div>
                    </div>
                    <div class="af-post-content">
                        <a class="af-post-url" id="rptPostsContentUrl">[FORUMPOST:LINK|<a class="dcf-title dcf-title-4">{0}</a>]
                        <div>[FORUMPOST:BODY:255]</div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate></FooterTemplate>
        </asp:repeater>
        
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
