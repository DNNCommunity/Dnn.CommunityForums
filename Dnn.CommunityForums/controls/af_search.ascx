<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_search.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_search" EnableViewState="false" %>
<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" assembly="DotNetNuke.Modules.ActiveForums" %>
<div class="dcf-search-results-wrap">
    <div class="af-search-header">

        <am:pagernav id="PagerTop" runat="server" />
        <h1 class="af-search-title"><%= GetSharedResource("Search") %> </h1>
        <br/>
        <button class="af-search-modify"><%= GetSharedResource("SearchModify") %></button><br/>  
        <asp:PlaceHolder runat="server" ID="phKeywords" Visible="False">
            <span class="af-search-criteria">
                <%= GetSharedResource("[RESX:SearchKeywords]") %> 
                <asp:Repeater runat="server" ID="rptKeywords">
                    <ItemTemplate> <b><%# Eval("Value") %></b></ItemTemplate>
                    <SeparatorTemplate>, </SeparatorTemplate>
                </asp:Repeater>
            </span>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phUsername" Visible="False">
            <span class="af-search-criteria">
                <%= GetSharedResource("[RESX:SearchByUser]") %> <b><asp:Literal runat="server" ID="litUserName"></asp:Literal></b>
            </span>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phTag" Visible="False">
            <span class="af-search-criteria">
               <%= GetSharedResource("[RESX:SearchByTag]") %> <b><asp:Literal runat="server" ID="litTag"></asp:Literal></b>
            </span>
        </asp:PlaceHolder>
    </div>
    <div class="af-search-bar afgrouprow afgrouprow-f">
        <span class="af-search-duration"><asp:Literal runat="server" ID="litSearchDuration" /> <asp:Literal runat="server" ID="litSearchAge" /></span>
        <span class="af-search-title"><asp:Literal runat="server" ID="litSearchTitle" /></span>
    </div>
    <asp:Panel ID="pnlMessage" runat="server" Visible="true" CssClass="af-search-noresults">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>


    <style>
	    td.af-colstats {
		    width: 10%;
		    font-size: 14px;
		    font-weight: 300;
		    color: #333;
	    }

    td.af-lastpost {
	    width: 20%;
	    white-space: nowrap;
	    font-size: 14px;
	    font-weight: 400;
	    color: #888;
    }

    td.af-lastpost div {
	    text-align: left;
	    width: 175px;
	    overflow: hidden;
	    white-space: normal !important;
    }

    td.af-lastpost div a:link,
    td.af-lastpost div a:visited {
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

    td.af-content td.afsubject {
	    vertical-align: top;
	    width: 100%;
	    padding-right: 10px;
    }

	    td.af-content td.afsubject > a {
		    color: #333;
		    font-weight: 700;
		    font-size: 13px;
	    }


    td.af-content td.afsubject .aftopicstarted a {
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
    <div class="af-search-results" style="position: relative;">
        
        <!-- Post View -->
        <asp:Repeater runat="server" ID="rptPosts" Visible="False">
            <ItemTemplate>
                <div class="af-post">
                    <div class="af-post-header">
                        <div class="af-stats">
                            <label><%= GetSharedResource("SearchReplies") %></label><span><%# Eval("ReplyCount") %></span><br/>
                            <label><%= GetSharedResource("SearchViews") %></label><span><%# Eval("ViewCount") %></span>
                        </div>
                        <div class="af-forum"><label><%= GetSharedResource("SearchForum") %></label> <a class="af-forum-url" href='<%# GetForumUrl() %>'><%# Eval("ForumName") %></a></div>
                        <div class="af-thread"><label><%= GetSharedResource("SearchTopic") %></label> <a class="af-thread-url" href='<%# GetThreadUrl() %>'><%# Eval("Subject") %></a></div>
                        <div class="af-postinfo"><label><%= GetSharedResource("SearchPosted") %></label><%# GetPostTime() %> <%= GetSharedResource("By") %> <%# GetAuthor() %></div>   
                    </div>
                    <div class="af-post-content">
                        <a class="af-post-url" href='<%# GetPostUrl() %>'><%# Eval("PostSubject") %></a>
                        <div><%# GetPostSnippet() %></div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <!-- Topic View -->
        <asp:repeater id="rptTopics" runat="server" Visible="False">
            <HeaderTemplate>
                <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
                    <tr>
                        <td class="aftopicrow afsubject">[RESX:Subject]</td>
                        <td class="aftopicrow af-colstats af-colstats-replies">[RESX:REPLIESHEADER]</td>
                        <td class="aftopicrow af-colstats af-colstats-views">[RESX:Views]</td>
                        <td class="aftopicrow af-lastpost">[RESX:LASTPOSTHEADER]</td>
                    </tr>
                </table>
        	    <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
            </HeaderTemplate>
            <ItemTemplate>
					    <tr>
						    <td colspan="0">
							     <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
								    <tr>
									    <td class="aftopicrow _hide" width="20"><a href='<%# GetThreadUrl() %>'><asp:Image runat="server" ImageUrl='<%#GetIcon()%>' /></a></td>
									    <td class="aftopicrow af-content">
										    <table>
											    <tr>
												    <td rowspan="2" class="afsubject">
												    
												    <span class="afhiddenstats"><%# Eval("ReplyCount") %> replies and <%# Eval("ViewCount") %> views</span>
												    <span class="aftopictitle"><a class="af-thread-link" href='<%# GetThreadUrl() %>'><%# Eval("Subject") %></a></span> 
												    <span class="aftopicsubtitle"><%= GetSharedResource("Started") %> <%# GetPostTime() %> <%= GetSharedResource("By") %> <%# GetAuthor() %></span>
												    
											    </tr>
										    </table>
									    </td>
									    <td class="aftopicrow af-colstats af-colstats-replies"><%# Eval("ReplyCount") %></td>
									    <td class="aftopicrow af-colstats af-colstats-views"><%# Eval("ViewCount") %></td>
									    <td class="aftopicrow af-lastpost"><div class="af_lastpost" style="white-space:nowrap;">In: <a class="af-forum-url" href='<%# GetForumUrl() %>'><%# Eval("ForumName") %></a> <br /><%= GetSharedResource("SearchBy") %> <%# GetLastPostAuthor() %><br /><%# GetLastPostTime() %></div></td>
								    </tr>
							    </table>	
									    </td>
								    </tr>
							    </table>
						    </td>
					    </tr>
			    <hr />
				    
            </ItemTemplate>
            <FooterTemplate></table></FooterTemplate>
        </asp:Repeater>

    </div>
    <div class="af-search-footer">
        <am:pagernav id="PagerBottom" runat="server" />
        <span class="af-search-recordCount"><asp:Literal runat="server" ID="litRecordCount" /></span>  
    </div>
</div>
<script type="text/javascript">

    $(document).ready(function() {

        $('.af-search-modify').button({ "icons": { "primary": "ui-icon-wrench" } }).click(function () {
            document.location.href = '<%=GetSearchUrl()%>';
            return false;
        });
    });

</script>
