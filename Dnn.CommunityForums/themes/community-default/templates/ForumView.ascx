<%@ Register TagPrefix="af" Assembly="DotNetNuke.Modules.ActiveForums" Namespace="DotNetNuke.Modules.ActiveForums.Controls"%>
<div class="dcf-forum-view">
[BREADCRUMB]
[GROUPSECTION]
<div class="dcf-forums">
		<div class="dcf-group-title-wrap">
			<h3 class="dcf-group-title">[RESX:Group]: [GROUPNAME]</h3>
			<span class="dcf-group-collapse">[GROUPCOLLAPSE]</span>
		</div>
		
			[GROUP]
			<div class="dcf-forums-group">
				<table class="dcf-table dcf-table-100">
					<thead>
						<tr class="dcf-table-head-row">
							<th scope="col" class="dcf-th dcf-col-text">
								<div class="dcf-th-text">[RESX:FORUMHEADER]</div>
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
					<tr class="dcf-table-body-row dcf-main-forums">
						
						<td class="dcf-col dcf-col-text">
							<div class="dcf-col-text-inner">
								[FORUMICONCSS]
								<div class="dcf-forum-title-text"><h4 class="dcf-forum-title">[FORUMNAME]</h4>
								<div class="dcf-forum-description">[FORUMDESCRIPTION]</div>
								</div>
							</div>
						</td>
						<td class="dcf-col dcf-col-topics">[TOTALTOPICS] </td>
						<td class="dcf-col dcf-col-replies">[TOTALREPLIES]</td>
						<td class="dcf-col dcf-col-subscribers">[FORUMSUBSCRIBERCOUNT]</td>
						<td class="dcf-col dcf-col-last-post">
							<span class="dcf-lastpost-subject">[LASTPOSTSUBJECT:25]</span>
							<span class="dcf-lastpost-author">[RESX:BY] [DISPLAYNAME]</span>
							<span class="dcf-lastpost-date">[LASTPOSTDATE]</span>
						</td>
					</tr>

					[SUBFORUMS]
					<!-- <tr class="dcf-table-body-row dcf-sub-forums">
						<td class="dcf-col dcf-col-icon"></td>
						<td class="dcf-col" colspan="5">
							<h5 class="dcf-sub-forum-title">Child Forums</h5>
						</td>
					</tr> -->
					
					<tr class="dcf-table-body-row dcf-sub-forums">


									<td class="dcf-col dcf-col-icon"></td>
									<td class="dcf-col dcf-col-text">
												<span class="aftopictitle">[FORUMNAME]</span>
												<span class="aftopicsubtitle">[FORUMDESCRIPTION]</span>
									</td>
									<td class="dcf-col dcf-col-topics">[TOTALTOPICS]</td>
									<td class="dcf-col dcf-col-replies">[TOTALREPLIES]</td>
									<td class="dcf-col dcf-col-subscribers">[FORUMSUBSCRIBERCOUNT]</td>
									<td class="dcf-col dcf-col-last-post"><div class="af_lastpost" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]<br />[RESX:BY]&nbsp;[DISPLAYNAME]<br />[LASTPOSTDATE]</div></td>
								</tr>
					[/SUBFORUMS]
				</tbody>
				[/FORUMS]
			</table>
			</div>
		[/GROUP]
		

</div>
[/GROUPSECTION]
<!-- Who's online -->
[WHOSONLINE]
<!-- Jump To -->
<div style="text-align:right;">[JUMPTO]</div>
</div>
