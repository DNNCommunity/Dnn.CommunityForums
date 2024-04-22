<script type="text/javascript">

    $(document).ready(function() {

        $('.af-search-modify').button({ "icons": { "primary": "ui-icon-wrench" } }).click(function () {
            document.location.href = '<%=GetSearchUrl()%>';
            return false;
        });
    });

</script>
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
            <itemtemplate>
                <div class="af-post">
                    <div class="af-post-header">
                        <div class="af-stats">
                            <label>[RESX:SearchReplies]</label><span id="ReplyCount" runat="server"><%# Eval("ReplyCount") %></span><br />
                            <label>[RESX:SearchViews]</label><span><%# Eval("ViewCount") %></span>
                        </div>
                        <div class="af-forum">
                            <label>[RESX:SearchForum]</label>
                            <a class="af-forum-url" id="rptPostsForumUrl"><%# Eval("ForumName") %></a></div>
                        <div class="af-thread">
                            <label>[RESX:SearchTopic]</label>
                            <a class="af-thread-url" id="rptPostsTopicUrl"><%# Eval("Subject") %></a></div>
                        <div class="af-postinfo">
                            <label>[RESX:SearchPosted]</label><%# GetPostTime() %> [RESX:BY] <%# GetAuthor() %></div>
                    </div>
                    <div class="af-post-content">
                        <a class="af-post-url" id="rptPostsContentUrl"><%# Eval("PostSubject") %></a>
                        <div><%# GetPostSnippet() %></div>
                    </div>
                </div>
            </itemtemplate>
        </asp:repeater>

        <!-- Topic View -->
        <asp:repeater id="rptTopics" runat="server" visible="False">
            <headertemplate>
                <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
            </headertemplate>
            <itemtemplate>
                <tr>
                    <td colspan="0">
                        <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
                            <tr>
                                <td class="aftopicrow _hide" width="20"><a id="rptTopicsTopicUrl1">
                                    <asp:image runat="server" id="rptTopicsTopicIconImage" />
                                </a></td>
                                <%--<td class="aftopicrow af-content">
                                    <table>
                                        <tr>
                                            <td rowspan="2" class="afsubject">

                                                <span class="afhiddenstats"><%# Eval("ReplyCount") %> replies and <%# Eval("ViewCount") %> views</span>
                                                <span class="aftopictitle"><a class="af-thread-link" id="rptTopicsTopicUrl2"><%# Eval("Subject") %></a></span>
                                                <span class="aftopicsubtitle">[RESX:Started] <%# GetPostTime() %> [RESX:By] <%# GetAuthor() %></span>
                                        </tr>
                                    </table>
                                </td>--%>
                                <%--<td class="aftopicrow af-colstats af-colstats-replies"><%# Eval("ReplyCount") %></td>
                                <td class="aftopicrow af-colstats af-colstats-views"><%# Eval("ViewCount") %></td>
                                <td class="aftopicrow af-lastpost">
                                    <div class="af_lastpost" style="white-space: nowrap;">In: <a class="af-forum-url" id="rptTopicsForumUrl"><%# Eval("ForumName") %></a>
                                        <br />
                                        [RESX:SearchBy] <%# GetLastPostAuthor() %><br />
                                        <%# GetLastPostTime() %>
                                    </div>
                                </td>--%>
                            </tr>
                        </table>
                    </td>
                </tr>
				
            </itemtemplate>
            <footertemplate></table></footertemplate>
        </asp:repeater>

    </div>
    <div class="af-search-footer">
        <am:pagernav id="PagerBottom" runat="server" />
        <span class="af-search-recordCount">
            <asp:literal runat="server" id="litRecordCount" />
        </span>
    </div>
</div>
