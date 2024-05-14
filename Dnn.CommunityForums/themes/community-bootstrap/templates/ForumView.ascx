<%@ Register TagPrefix="af" Assembly="DotNetNuke.Modules.ActiveForums" Namespace="DotNetNuke.Modules.ActiveForums.Controls"%>
	<div class="dcf-forum-view">
[BREADCRUMB]
[GROUPSECTION]
		<div class="dcf-forums">
			<div class="dcf-group-title-wrap d-flex align-items-center py-2">
				<h3 class="dcf-group-title h5 flex-grow-1">[RESX:Group]: [GROUPNAME]</h3>
				<a class="bs-collapse" data-toggle="collapse" href="#forum-group-[FORUMGROUPID]" role="button" data-bs-toggle="collapse">
					<i class="fa fa-chevron-circle-down fa-lg"></i>
				</a>
			</div>
		
			[GROUP]
			<div id="forum-group-[FORUMGROUPID]"  class="dcf-forums-group collapse show">
				<table class="dcf-table dcf-table-100 table table-responsive-md">
					<thead>
						<tr class="dcf-table-head-row">
							<th scope="col" class="dcf-th dcf-col-text w-75">
								<div class="dcf-th-text">[RESX:FORUMHEADER]</div>
							</th>
							<th scope="col" class="dcf-th dcf-col-topics">
								<div class="dcf-icon-text">
									<i class="fa fa-files-o"></i>
									<span class="dcf-link-text">[RESX:TOPICSHEADER]</span>
								</div>
							</th>
							<th scope="col" class="dcf-th dcf-col-replies">
								<div class="dcf-icon-text">
									<i class="fa fa-reply"></i>
									<span class="dcf-link-text">[RESX:REPLIESHEADER]</span>
								</div>
							</th>
							<th scope="col" class="dcf-th dcf-col-subscribers">
								<div class="dcf-icon-text">
									<i class="fa fa-envelope-o"></i>
									<span class="dcf-link-text">[RESX:SUBSCRIBERS]</span>
								</div>
							</th>
							<th scope="col" class="dcf-th dcf-col-last-post w-25">
								<div class="dcf-icon-text">
									<i class="fa fa-file-o"></i>
									<span class="dcf-link-text">[RESX:LASTPOSTHEADER]</span>
								</div>
							</th>
						</tr>
					</thead>
					[FORUMS]
					<tbody>
						<tr class="dcf-table-body-row">
							<td class="dcf-col dcf-col-text">
								<div class="d-flex">
								[FORUMICONCSS]
									<div class="dcf-forum-title-text">
										<h4 class="dcf-forum-title h5 mt-0 mb-2">[FORUMNAME]</h4>
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
							<td class="dcf-col dcf-col-last-post">
								<div class="af_lastpost" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]
									<br />[RESX:BY] [DISPLAYNAME]
									<br />[LASTPOSTDATE]
								</div>
							</td>
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