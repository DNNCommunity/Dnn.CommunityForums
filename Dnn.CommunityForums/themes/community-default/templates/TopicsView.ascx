<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<%@ Register TagPrefix="am" TagName="MarkForumRead" Src="~/DesktopModules/ActiveForums/controls/af_markallread.ascx"%>
<%@ Register TagPrefix="am" TagName="MiniSearch" Src="~/DesktopModules/ActiveForums/controls/af_searchquick.ascx"%>
<div class="dcf-topics-view">
	<div class="dcf-breadcrumb"><i class="fa fa-home"></i>  [FORUMMAINLINK] <i class="fa fa-chevron-right"></i> [FORUM:GROUPLINK|<a href="{0}" class="dcf-forumgroup-link">[FORUM:GROUPNAME]</a>]</div>
	<div class="dcf-actions dcf-actions-top dcf-cols">
		
		<div class="dcf-forum-title-wrap">
		<h2 class="dcf-forum-title">[TRESX:Forum]: [FORUM:FORUMLINK|<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]</h2>
		<div class="dcf-buttons">[ADDTOPIC]</div>
		</div>
		
		
		<div class="dcf-forum-search-subscribe">
			<div class="dcf-forum-search">[MINISEARCH]</div>
			<div class="dcf-forum-subscribers"><i class="fa fa-envelope fa-fw fa-grey"></i><span id="af-topicsview-forumsubscribercount">[FORUMSUBSCRIBERCOUNT]</span>&nbsp;[RESX:FORUMSUBSCRIBERCOUNT]</div>
			<div class="dcf-subscribe-forum">[FORUMSUBSCRIBE]</div>
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
							<td class="dcf-col dcf-col-icon">[POSTICON]</td>
							<td class="dcf-col dcf-col-subject" title="[BODYTITLE]">
								<div class="dcf-subject">
										<h4 class="dcf-title dcf-title-4">[SUBJECTLINK][AF:ICONLINK:LASTREAD]</h4>
										<div class="dcf-topic-started">[RESX:StartedHeader] <i class="fa fa-user fa-blue"></i>&nbsp;[STARTEDBY][AF:UI:MINIPAGER]</div>

									<div class="dcf-forum-description">[BODYTITLE]</div>

									<div class="dcf-topic-tools">
										[ACTIONS:EDIT]
										[ACTIONS:MOVE]
										[ACTIONS:LOCK]
										[ACTIONS:PIN]
										[ACTIONS:DELETE]
									</div>

									</div>
										<div>[POSTRATINGDISPLAY]</div>
										<div rowspan="2" class="dcf-status">[STATUS]</div>

							</td>
							<td class="dcf-col dcf-col-replies">[FORUMTOPIC:REPLYCOUNT]</td>
							<td class="dcf-col dcf-col-views">[FORUMTOPIC:VIEWCOUNT]</td>
							<td class="dcf-col dcf-col-views">[FORUMTOPIC:SUBSCRIBERCOUNT]</td>
							<td class="dcf-col dcf-col-last-post"><div class="dcf-_lastpost" style="white-space:nowrap;">[LASTPOST][RESX:BY] <i class="fa fa-user fa-blue"></i>&nbsp;[LASTPOSTDISPLAYNAME][AF:ICONLINK:LASTREPLY]<br />[LASTPOSTDATE][/LASTPOST]</div></td>
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
						[POSTICON]
					</td>
					<td class="dcf-col dcf-col-subject">
						<div class="dcf-subject" title="[BODYTITLE]">

							<h4 class="dcf-title dcf-title-4">[SUBJECTLINK][AF:ICONLINK:LASTREAD][ICONPIN][ICONLOCK]</h4>

							<div class="dcf-topic-started">[RESX:StartedHeader] <i class="fa fa-user fa-blue"></i>&nbsp;[STARTEDBY][AF:UI:MINIPAGER]</div>

							<div class="dcf-topic-description">[BODYTITLE]</div>

							<div class="dcf-topic-tools">
								[AF:QUICKEDITLINK]
								[ACTIONS:DELETE]
								[ACTIONS:EDIT]
								[ACTIONS:MOVE]
								[ACTIONS:LOCK]
								[ACTIONS:PIN]
							</div>


							<div>
								[AF:PROPERTIES]
								[AF:PROPERTY:LABEL]:[AF:PROPERTY:VALUE]
								[/AF:PROPERTIES]
							</div>
						</div>
					</td>
					<td class="dcf-col dcf-col-replies">[FORUMTOPIC:REPLYCOUNT]</td>
					<td class="dcf-col dcf-col-ratings">[POSTRATINGDISPLAY]</td>
					<td class="dcf-col dcf-col-status">[STATUS]</td>
					<td class="dcf-col dcf-col-views">[FORUMTOPIC:VIEWCOUNT]</td>
					<td class="dcf-col dcf-col-subscribers">[FORUMTOPIC:SUBSCRIBERCOUNT]</td>
					<td class="dcf-col dcf-col-last-post"><div class="dcf-last-post">[LASTPOST][RESX:BY] <i class="fa fa-user fa-blue"></i>&nbsp;[LASTPOSTDISPLAYNAME][AF:ICONLINK:LASTREPLY]<br />[LASTPOSTDATE][/LASTPOST]</div></td>
				</tr>
			</tbody>
			[/TOPICS]
		</table>



		<div class="dcf-actions dcf-actions-bottom">
			<div class="dcf-buttons">[ADDTOPIC] [MARKFORUMREAD]</div>
			<div class="dcf-topics-pager">[PAGER2]<br />[JUMPTO]<br />[RSSLINK]</div>
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
								<td class="dcf-col dcf-col-icon">[FORUMICONCSS]</td>
								<td class="dcf-col dcf-col-subject">
									<h4 class="dcf-forum-title">[FORUM:FORUMNAME]</h4>
									<span class="dcf-forum-description">[FORUM:FORUMDESCRIPTION]</span>
								</td>
								<td class="dcf-col dcf-col-topics">[FORUM:TOTALTOPICS] </td>
								<td class="dcf-col dcf-col-replies">[FORUM:TOTALREPLIES]</td>
								<td class="dcf-col dcf-col-subscribers">[FORUM:SUBSCRIBERCOUNT]</td>
								<td class="dcf-col dcf-col-last-post">
									<span class="dcf-lastpost-subject">[LASTPOSTSUBJECT:25]</span>
									<span class="dcf-lastpost-author">[RESX:BY] <i class="fa fa-user fa-blue"></i>&nbsp;[DISPLAYNAME]</span>
									<span class="dcf-lastpost-date">[LASTPOSTDATE]</span>
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

