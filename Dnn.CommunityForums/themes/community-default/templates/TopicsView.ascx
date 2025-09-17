<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<%@ Register TagPrefix="am" TagName="MarkForumRead" Src="~/DesktopModules/ActiveForums/controls/af_markallread.ascx"%>
<%@ Register TagPrefix="am" TagName="MiniSearch" Src="~/DesktopModules/ActiveForums/controls/af_searchquick.ascx"%>
<div class="dcf-topics-view">
	<div class="dcf-breadcrumb"><i class="fa fa-home"></i>  [FORUM:FORUMMAINLINK|<a href="{0}" class="dcf-forums-link">[RESX:ForumMain]</a>] <i class="fa fa-chevron-right"></i> [FORUMGROUP:GROUPLINK|<a href="{0}" class="dcf-forumgroup-link">[FORUMGROUP:GROUPNAME]</a>]</div>
	<div class="dcf-actions dcf-actions-top dcf-cols">
		
		<div class="dcf-forum-title-wrap">
		<h2 class="dcf-forum-title">[TRESX:Forum]: [FORUM:FORUMLINK|<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]</h2>
		<div class="dcf-buttons">[ADDTOPIC]</div>
		</div>
		
		
		<div class="dcf-forum-search-subscribe">
			<div class="dcf-forum-search">[MINISEARCH]</div>
			<div class="dcf-forum-subscribers"><i class="fa fa-envelope fa-fw fa-grey"></i><span id="af-topicsview-forumsubscribercount">[FORUM:SUBSCRIBERCOUNT]</span>&nbsp;[RESX:FORUMSUBSCRIBERCOUNT]</div>
			<div class="dcf-subscribe-forum">
                <span class="afnormal">
					[FORUM:SUBSCRIBEONCLICK|<input type="button" class="dcf-btn-subs [FORUM:SUBSCRIBE-UNSUBSCRIBE-CSSCLASS]" value="[FORUM:SUBSCRIBE-UNSUBSCRIBE-LABEL]" onclick="{0}" />]
                </span>
			</div>
		</div>
		
		
		

	</div>

[ANNOUNCEMENTS]
	<div class="dcf-topics dcf-announcements">
		
			<!-- Render the sub Announcements -->
				<table class="dcf-table dcf-table-100">
					<thead>
						<tr class="dcf-table-head-row">
							<th scope="col" class="dcf-th dcf-col-title" colspan="2">
								<h3 class="dcf-title dcf-title-3">[RESX:Announcements]</h3>
							</th>
							<th scope="col" class="dcf-th dcf-col-replies">
								<div class="dcf-icon-text"><i class="fa fa-reply"></i><span class="dcf-link-text">[RESX:REPLIESHEADER]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-views">
								<div class="dcf-icon-text"><i class="fa fa-eye"></i><span class="dcf-link-text">[RESX:Views]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-subscribers">
								<div class="dcf-icon-text"><i class="fa fa-envelope-o"></i><span class="dcf-link-text">[RESX:SUBSCRIBERS]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-last-post">
								<div class="dcf-icon-text"><i class="fa fa-clock-o"></i><span class="dcf-link-text">[RESX:LASTPOSTHEADER]</span></div>
							</th>
						</tr>
					</thead>
					[ANNOUNCEMENT]
					<tbody>
						<tr class="dcf-table-body-row">
							<td class="dcf-col dcf-col-icon">[FORUMTOPIC:POSTICON|<div><i class="{0}"></i></div>
                                ]</td>
							<td class="dcf-col dcf-col-subject" title="[FORUMTOPIC:BODYTITLE]">
								<div class="dcf-subject">
										<h4 class="dcf-title dcf-title-4">[FORUMTOPIC:SUBJECTLINK|<a href="{0}" class="dcf-topic-link">[FORUMTOPIC:SUBJECT]</a>]</h4>
										<div class="dcf-topic-started">[RESX:StartedHeader] <i class="fa fa-user fa-blue"></i>&nbsp;[FORUMTOPIC:AUTHORDISPLAYNAMELINK|<a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:AUTHORDISPLAYNAME]</a>|[FORUMTOPIC:AUTHORDISPLAYNAME]][AF:UI:MINIPAGER]</div>

									<div class="dcf-forum-description">[FORUMTOPIC:BODYTITLE]</div>

									<div class="dcf-topic-tools">
										[DCF:TEMPLATE-TOPICACTIONS]
									</div>

									</div>
										<div>[FORUMTOPIC:RATING|<span class="fa-rater fa-rate{0}"><i class="fa fa-star1"></i><i class="fa fa-star2"></i><i class="fa fa-star3"></i><i class="fa fa-star4"></i><i class="fa fa-star5"></i></span>]
										</div>
										<div rowspan="2" class="dcf-status">[FORUMTOPIC:STATUS|<div><i class="fa fa-status{0} fa-red fa-2x"></i></div>]</div>

							</td>
							<td class="dcf-col dcf-col-replies">[FORUMTOPIC:LASTREADURL|<a href="{0}" rel="nofollow">[FORUMTOPIC:REPLYCOUNT]</a>]</td>
							<td class="dcf-col dcf-col-views">[FORUMTOPIC:VIEWCOUNT]</td>
							<td class="dcf-col dcf-col-views">[FORUMTOPIC:SUBSCRIBERCOUNT]</td>
							<td class="dcf-col dcf-col-last-post"><div class="dcf-_lastpost">
								[LASTPOST]
									<div class="dcf-last-reply">
									[FORUMTOPIC:LASTREPLYURL|<a class="dcf-last-reply-link" href="{0}" rel="nofollow" title="[RESX:JumpToLastReply]">[FORUMTOPIC:LASTPOSTDATE]</a>]
									</div>
									<div class="dcf-last-profile">
									[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK|[RESX:BY]<i class="fa fa-user fa-fw fa-blue"></i><a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]</a>|[RESX:BY]<i class="fa fa-user fa-fw fa-blue"></i>[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]]
									</div>
								[/LASTPOST]
                            </div></td>
						</tr>
					</tbody>
					[/ANNOUNCEMENT]
				</table>
	</div>
[/ANNOUNCEMENTS]

	<!-- Render regular Forum post -->
	<div class="dcf-topics">

		<div class="dcf-actions dcf-actions-top">
			<div class="dcf-topics-pager">
				[PAGER1]
			</div>
		</div>

		<table class="dcf-table dcf-table-100">
			<thead>
				<tr class="dcf-table-head-row">
					<th scope="col" colspan="2" class="dcf-th dcf-col-title">
						<h3 class="dcf-title dcf-title-3">[RESX:TOPICSHEADER]</h3>
					</th>
					<th scope="col" class="dcf-th dcf-col-replies">
						<div class="dcf-icon-text"><i class="fa fa-reply"></i><span class="dcf-link-text">[RESX:REPLIESHEADER]</span></div>
					</th>
					<th scope="col" class="dcf-th dcf-col-ratings">
						<div class="dcf-icon-text"><i class="fa fa-star"></i><span class="dcf-link-text">[RESX:Rating]</span></div>
					</th>
					<th scope="col" class="dcf-th dcf-col-status">
						<div class="dcf-icon-text"><i class="fa fa-star"></i><span class="dcf-link-text">[RESX:Status]</span></div>
					</th>
					<th scope="col" class="dcf-th dcf-col-views">
						<div class="dcf-icon-text"><i class="fa fa-eye"></i><span class="dcf-link-text">[RESX:Views]</span></div>
					</th>

					<th scope="col" class="dcf-th dcf-col-subscribers">
						<div class="dcf-icon-text"><i class="fa fa-envelope-o"></i><span class="dcf-link-text">[RESX:SUBSCRIBERS]</span></div>
					</th>
					<th scope="col" class="dcf-th dcf-col-last-post">
						<div class="dcf-icon-text"><i class="fa fa-file-o"></i><span class="dcf-link-text">[RESX:LASTPOSTHEADER]</span></div>
					</th>
				</tr>
			</thead>
			[TOPICS]
			<tbody>
				<tr class="dcf-table-body-row">
					<td class="dcf-col dcf-col-icon">
                        [FORUMTOPIC:POSTICON|<div class="dcf-post-icon"><i class="{0}"></i></div>]
					</td>
					<td class="dcf-col dcf-col-subject">
						<div class="dcf-subject" title="[FORUMTOPIC:BODYTITLE]">

							<h4 class="dcf-title dcf-title-4">[FORUMTOPIC:SUBJECTLINK|<a href="{0}" title="[FORUMTOPIC:BODYTITLE]" class="dcf-topic-link">[FORUMTOPIC:SUBJECT]</a>]
                                [FORUMTOPIC:ICONPINNED|&nbsp;&nbsp;<i id="af-topicsview-pin-{0}" class="fa fa-thumb-tack fa-fw fa-red"></i>]
                                [FORUMTOPIC:ICONUNPINNED|&nbsp;&nbsp;<i id="af-topicsview-pin-{0}" class="fa fa-fw fa-red"></i>]
                                [FORUMTOPIC:ICONLOCKED|&nbsp;&nbsp;<i id="af-topicsview-lock-{0}" class="fa fa-lock fa-fw fa-red"></i>]
                                [FORUMTOPIC:ICONUNLOCKED|&nbsp;&nbsp;<i id="af-topicsview-lock-{0}" class="fa fa-fw fa-red"></i>]
							</h4>

							<div class="dcf-topic-started">[RESX:StartedHeader] <i class="fa fa-user fa-blue"></i>&nbsp;[FORUMTOPIC:AUTHORDISPLAYNAMELINK|<a href="{0}" class="af-profile-link" rel="nofollow">[FORUMTOPIC:AUTHORDISPLAYNAME]</a>|[FORUMTOPIC:AUTHORDISPLAYNAME]][AF:UI:MINIPAGER]</div>

							<div class="dcf-topic-description">[FORUMTOPIC:BODYTITLE]</div>

							<div class="dcf-topic-tools">
                                [DCF:TEMPLATE-TOPICACTIONS]
                            </div>

							<div>
								[AF:PROPERTIES]
								    [AF:PROPERTY:LABEL]:[AF:PROPERTY:VALUE]
								[/AF:PROPERTIES]
							</div>
						</div>
					</td>
					<td class="dcf-col dcf-col-replies">[FORUMTOPIC:LASTREADURL|<a href="{0}" rel="nofollow">[FORUMTOPIC:REPLYCOUNT]</a>]</td>
					<td class="dcf-col dcf-col-ratings">[FORUMTOPIC:RATING|<span class="fa-rater fa-rate{0}"><i class="fa fa-star1"></i><i class="fa fa-star2"></i><i class="fa fa-star3"></i><i class="fa fa-star4"></i><i class="fa fa-star5"></i></span>
                        ]

					</td>
					<td class="dcf-col dcf-col-status">[FORUMTOPIC:STATUS]</td>
					<td class="dcf-col dcf-col-views">[FORUMTOPIC:VIEWCOUNT]</td>
					<td class="dcf-col dcf-col-subscribers">[FORUMTOPIC:SUBSCRIBERCOUNT]</td>
					<td class="dcf-col dcf-col-last-post">
						<div class="dcf-last-post">
								[LASTPOST]
									<div class="dcf-last-reply">
									[FORUMTOPIC:LASTREPLYURL|<a class="dcf-last-reply-link" href="{0}" rel="nofollow" title="[RESX:JumpToLastReply]">[FORUMTOPIC:LASTPOSTDATE]</a>]
									</div>
									<div class="dcf-last-profile">
									[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAMELINK|[RESX:BY]<i class="fa fa-user fa-fw fa-blue"></i><a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]</a>|[RESX:BY]<i class="fa fa-user fa-fw fa-blue"></i>[FORUMTOPIC:LASTPOSTAUTHORDISPLAYNAME]]
									</div>
								[/LASTPOST]
                    	</div>
					</td>
				</tr>
			</tbody>
			[/TOPICS]
		</table>



		<div class="dcf-actions dcf-actions-bottom">
			<div class="dcf-buttons">[ADDTOPIC] [MARKFORUMREAD]</div>
			<div class="dcf-topics-pager">[PAGER2]<br />[JUMPTO]<br />[FORUM:RSSLINK|<a href="{0}" target="_blank"><img src="[FORUM:THEMELOCATION]images/rss.png" border="0" alt="[RESX:RSS]" /></a>]</div>
		</div>
	</div>


	<!-- Render the sub Forums -->
	<div class="dcf-sub-forums-wrap">

		<table class="dcf-table dcf-table-100">
			<tr class="dcf-sub-forums">
				<td colspan="2">
				[SUBFORUMS] 

				<table class="dcf-table dcf-table-100">
					<thead>
						<tr class="dcf-table-head-row">
							<th scope="col" colspan="2" class="dcf-th dcf-col-title">
								<h3 class="dcf-title dcf-title-3">[RESX:FORUMHEADER]</h3>
							</th>
							<th scope="col" class="dcf-th dcf-col-topics">
								<div class="dcf-icon-text"><i class="fa fa-files-o"></i><span class="dcf-link-text">[RESX:TOPICSHEADER]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-replies">
								<div class="dcf-icon-text"><i class="fa fa-reply"></i><span class="dcf-link-text">[RESX:REPLIESHEADER]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-subscribers">
								<div class="dcf-icon-text"><i class="fa fa-envelope-o"></i><span class="dcf-link-text">[RESX:SUBSCRIBERS]</span></div>
							</th>
							<th scope="col" class="dcf-th dcf-col-last-post">
								<div class="dcf-icon-text"><i class="fa fa-file-o"></i><span class="dcf-link-text">[RESX:LASTPOSTHEADER]</span></div>
							</th>
						</tr>
					</thead>
						[FORUMS]
						<tbody>

							<tr class="dcf-table-body-row">
								<td class="dcf-col dcf-col-icon">[FORUM:FORUMICONCSS|<div class="dcf-forum-icon"><i class="fa {0} fa-2x"></i></div>]
								</td>
								<td class="dcf-col dcf-col-subject">
									<h4 class="dcf-forum-title">[FORUM:FORUMLINK|
                                        <a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>
                                        ]

									</h4>
									<span class="dcf-forum-description">[FORUM:FORUMDESCRIPTION]</span>
								</td>
								<td class="dcf-col dcf-col-topics">[FORUM:TOTALTOPICS] </td>
								<td class="dcf-col dcf-col-replies">[FORUM:TOTALREPLIES]</td>
								<td class="dcf-col dcf-col-subscribers">[FORUM:SUBSCRIBERCOUNT]</td>
								<td class="dcf-col dcf-col-last-post">
									<span class="dcf-lastpost-subject">[FORUM:LASTPOSTSUBJECT:25]</span>
									<span class="dcf-lastpost-author">[FORUM:DISPLAYNAME| [RESX:BY] <i class="fa fa-user fa-blue"></i>&nbsp;{0}]</span>
									<span class="dcf-lastpost-date">[FORUM:LASTPOSTDATE]</span>
								</td>
							</tr>

					</tbody>
						[/FORUMS]
					</table>
				[/SUBFORUMS]
				</td>
			</tr>
		</table>
	</div>
</div>

