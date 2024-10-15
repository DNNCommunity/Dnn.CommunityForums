<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="af_grid.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.af_grid" %>
<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<div class="dcf-filter-results-wrap">
    <div class="af-search-header">
        <am:pagernav id="PagerTop" runat="server" />
        <h1 class="af-search-title"><asp:Label ID="lblHeader" runat="server" /></h1>
        <button type="submit" runat="server" ID="btnMarkRead" class="af-markread" Visible="false" onclick="if(!af_confirmMarkAllRead()) return false;" />
        <asp:DropDownList ID="drpTimeFrame" runat="server" AutoPostBack="true" Visible="false" >
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
    <%--<div class="af-search-bar">
        <span class="af-search-title"><%= GetSharedResource("[RESX:SearchByTopics]") %></span>
    </div>--%>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="af-search-noresults">
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


    /* 40 Part of the Theme  but never added to the theme :) */
    .af-colstats_responsive,
    .af_lastpost_responsive {
	    display: none;
	    white-space: nowrap;
    }
    </style>
    <div class="af-search-results" style="position: relative;">
        <asp:repeater id="rptTopics" runat="server">
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
												    <span class="aftopictitle"><a class="af-thread-link" href='<%# GetThreadUrl() %>'><%# Eval("Subject") %></a> <a href='<%# GetLastRead() %>'><img border="0" class="afminiarrow" src='<%# GetArrowPath() %>' style="vertical-align: middle;" /></a>  </span>
                                                    <span class="af-colstats_responsive"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;<%# Eval("ReplyCount") %> <i class="fa fa-eye fa-fw fa-grey"></i>&nbsp;<%# Eval("ViewCount") %></span>
												    <span class="aftopicsubtitle"><%= GetSharedResource("Started") %> <%# GetPostTime() %> <%= GetSharedResource("By") %> <%# GetAuthor() %></span>
												    <div class="af_lastpost_responsive" style="white-space:nowrap;">In: <a class="af-forum-url" href='<%# GetForumUrl() %>'><%# Eval("ForumName") %></a> <br /><%= GetSharedResource("SearchBy") %> <%# GetLastPostAuthor() %><br /><%# GetLastPostTime() %></div>
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
			    <hr />
			    </td>
		    </tr>
				    
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
    $(document).ready(function () {
        $('.af-markread').button({ "icons": { "primary": "ui-icon-check" } });
    });
    
    function af_confirmMarkAllRead() {
        return confirm('<%= GetSharedResource("MarkAllReadConfirm") %>');
    }
</script>
