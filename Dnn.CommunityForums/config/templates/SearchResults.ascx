<script type="text/javascript">

    $(document).ready(function() {

        $('.af-search-modify').button({ "icons": { "primary": "ui-icon-wrench" } }).click(function () {
            document.location.href = '<%=GetSearchUrl()%>';
            return false;
        });
    });

</script>
<style>
    .divTable{
        display: table;
        width: 100%;
    }
    .divTableRow {
        display: table-row;
    }
    .divTableHeading {
        background-color: #EEE;
        display: table-header-group;
    }
    .divTableCell, .divTableHead {
        border: 1px solid #999999;
        display: table-cell;
        padding: 3px 10px;
    }
    .divTableHeading {
        background-color: #EEE;
        display: table-header-group;
        font-weight: bold;
    }
    .divTableFoot {
        background-color: #EEE;
        display: table-footer-group;
        font-weight: bold;
    }
    .divTableBody {
        display: table-row-group;
    }
</style>
<div class="dcf-search-results-wrap">
    <div class="af-search-header">

        <am:pagernav id="PagerTop" runat="server" />
        <h1 class="af-search-title">[RESX:Search]</h1>
        <br />
        <button class="af-search-modify">[RESX:SearchModify]</button><br />
        <asp:placeholder runat="server" id="phKeywords" visible="False">
            <span class="af-search-criteria">[RESX:SearchKeywords]
                <asp:repeater runat="server" id="rptKeywords">
                    <itemtemplate><b><%--<%# Eval("Value") %>--%></b></itemtemplate>
                    <separatortemplate>, </separatortemplate>
                </asp:repeater>
            </span>
        </asp:placeholder>
        <asp:placeholder runat="server" id="phUsername" visible="False">
            <span class="af-search-criteria">[RESX:SearchByUser]<b>
                    <asp:literal runat="server" id="litUserName"></asp:literal>
                </b>
            </span>
        </asp:placeholder>
        <asp:placeholder runat="server" id="phTag" visible="False">
            <span class="af-search-criteria">[RESX:SearchByTag]<b>
                    <asp:literal runat="server" id="litTag"></asp:literal>
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
                            <label>[RESX:SearchReplies]</label><span id="ReplyCount" runat="server"><%# Eval("ReplyCount") %></span><br />
                            <label>[RESX:SearchViews]</label><span><%# Eval("ViewCount") %></span>
                        </div>
                        <div class="af-forum">
                            <label>[RESX:SearchForum]</label>
                            <a class="af-forum-url" id="rptPostsForumUrl"><%# Eval("ForumName") %></a>
                        </div>
                        <div class="af-thread">
                            <label>[RESX:SearchTopic]</label>
                            <a class="af-thread-url" id="rptPostsTopicUrl"><%# Eval("Subject") %></a>
                        </div>
                        <div class="af-postinfo">
                            <label>[RESX:SearchPosted]</label><%# GetPostTime() %> [RESX:BY] <%# GetAuthor() %>
                        </div>
                    </div>
                    <div class="af-post-content">
                        <a class="af-post-url" id="rptPostsContentUrl"><%# Eval("PostSubject") %></a>
                        <div><%# GetPostSnippet() %></div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate></FooterTemplate>
        </asp:repeater>
        
        <!-- Topic View -->
        <asp:repeater id="rptTopics" runat="server" visible="False">
            <HeaderTemplate></HeaderTemplate>
            <ItemTemplate>
                <div class="divTable">
                    <div class="divTableBody">
                        <div class="divTableRow">
                            <div class="divTableCell">
                                <div class="divTable">
                                    <div class="divTableBody">
                                        <div class="divTableRow">
                                            <div class="divTableCell">&nbsp;</div>
                                            <div class="divTableCell">
                                                <div class="divTable">
                                                    <div class="divTableBody">
                                                        <div class="divTableRow">
                                                            <div class="divTableCell">
                                                                <span class="afhiddenstats">&lt;%# Eval("ReplyCount") %&gt; replies and &lt;%# Eval("ViewCount") %&gt; views</span> 
                                                                <span class="aftopictitle">
                                                                    <a id="rptTopicsTopicUrl2" class="af-thread-link"></a>&lt;%# Eval("Subject") %&gt;
                                                                </span> 
                                                                <span class="aftopicsubtitle">[RESX:Started] &lt;%# GetPostTime() %&gt; [RESX:By] &lt;%# GetAuthor() %&gt;</span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="divTableCell">&lt;%# Eval("ReplyCount") %&gt;</div>
                                            <div class="divTableCell">&lt;%# Eval("ViewCount") %&gt;</div>
                                            <div class="divTableCell">
                                                <div class="af_lastpost" style="white-space: nowrap;">
                                                    In: <a id="rptTopicsForumUrl" class="af-forum-url"></a>&lt;%# Eval("ForumName") %&gt;
                                                    <br />
                                                    [RESX:SearchBy] &lt;%# GetLastPostAuthor() %&gt;<br />
                                                    &lt;%# GetLastPostTime() %&gt;
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate></FooterTemplate>
        </asp:repeater>

    </div>
    <div class="af-search-footer">
        <am:pagernav id="PagerBottom" runat="server" />
        <span class="af-search-recordCount">
            <asp:literal runat="server" id="litRecordCount" />
        </span>
    </div>
</div>
