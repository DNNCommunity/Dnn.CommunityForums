<%@ Register TagPrefix="af" Assembly="DotNetNuke.Modules.ActiveForums" Namespace="DotNetNuke.Modules.ActiveForums.Controls"%>
	<div class="dcf-forum-view">
[BREADCRUMB]
[GROUPSECTION]
		<div class="dcf-forums">
            <div class="dcf-group-title-wrap d-flex align-items-center py-2">
				<h3 class="dcf-group-title h5 flex-grow-1">[RESX:Group]: [FORUMGROUP:GROUPLINK|<a href="{0}" class="dcf-forumgroup-link">[FORUMGROUP:GROUPNAME]</a>]</h3>
				<a class="bs-collapse" data-toggle="collapse" href="#forum-group-[FORUMGROUP:FORUMGROUPID]" role="button" data-bs-toggle="collapse">
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
										<h4 class="dcf-forum-title h5 mt-0 mb-2">[FORUM:FORUMLINK|
                                            <a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>
                                            ]</h4>
										<div class="dcf-forum-description">[FORUM:FORUMDESCRIPTION]</div>
									</div>
								</div>
							</td>
							<td class="dcf-col dcf-col-topics">[FORUM:TOTALTOPICS] </td>
							<td class="dcf-col dcf-col-replies">[FORUM:TOTALREPLIES]</td>
							<td class="dcf-col dcf-col-subscribers">[FORUM:SUBSCRIBERCOUNT]</td>
							<td class="dcf-col dcf-col-last-post">
								<span class="dcf-lastpost-subject">[FORUM:LASTPOSTSUBJECT:25]</span>
								<span class="dcf-lastpost-author">
                                    [FORUM:LASTPOSTAUTHORDISPLAYNAMELINK|
                                    [RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>&nbsp;<a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUM:LASTPOSTAUTHORDISPLAYNAME]</a>|
                                    [RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>[FORUM:LASTPOSTAUTHORDISPLAYNAME]
                                    ]
								</span>
								<span class="dcf-lastpost-date">[FORUM:LASTPOSTDATE]</span>
							</td>
						</tr>

					[SUBFORUMS]
						 <tr class="dcf-table-body-row dcf-sub-forums">
						<td class="dcf-col dcf-col-icon"></td>
						<td class="dcf-col" colspan="5">
							<h5 class="dcf-sub-forum-title">[RESX:Child] [RESX:FORUMS]</h5>
						</td>
					</tr> 
						<tr class="dcf-table-body-row dcf-sub-forums">
							
							<td class="dcf-col dcf-col-text">
								<span class="dcf-forum-title h6">[FORUM:FORUMLINK|
                                    <a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>
                                    ]</span>
								<span class="dcf-forum-description">[FORUM:FORUMDESCRIPTION]</span>
                                <span class="dcf-forum-description">([RESX:Child] [RESX:of] [FORUM:PARENTFORUMNAME])</span>
							</td>
							<td class="dcf-col dcf-col-topics">[FORUM:TOTALTOPICS]</td>
							<td class="dcf-col dcf-col-replies">[FORUM:TOTALREPLIES]</td>
							<td class="dcf-col dcf-col-subscribers">[FORUM:SUBSCRIBERCOUNT]</td>
							<td class="dcf-col dcf-col-last-post">
								<div class="af_lastpost" style="white-space:nowrap;">[FORUM:LASTPOSTSUBJECT:25]
									<br />
                                    [FORUM:LASTPOSTAUTHORDISPLAYNAMELINK|
                                    [RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>&nbsp;<a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUM:LASTPOSTAUTHORDISPLAYNAME]</a>|
                                    [RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>[FORUM:LASTPOSTAUTHORDISPLAYNAME]
                                    ]
									<br />
                                    [FORUM:LASTPOSTDATE]
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