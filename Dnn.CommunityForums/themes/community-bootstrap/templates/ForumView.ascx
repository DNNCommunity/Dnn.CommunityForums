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
			<div id="forum-group-[FORUMGROUP:FORUMGROUPID]" class="dcf-forums-group collapse show">
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
                                    [FORUM:FORUMICONCSS|<div class="dcf-forum-icon pr-2 pe-2"><i class="fa {0} fa-2x"></i></div>
                                    ]
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
								<div class="dcf-last-post">
									[FORUM:LASTPOSTSUBJECT:25]
								<div class="dcf-last-profile">
									[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK|
									[RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>&nbsp;<a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUM:LASTPOSTAUTHORDISPLAYNAME]</a>|
									[RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>[FORUM:LASTPOSTAUTHORDISPLAYNAME]
									]
									</div>
									<div class="dcf-last-date">
									[FORUM:LASTPOSTDATE]
									</div>
								</div>
							</td>
						</tr>

					[SUBFORUMS]
					 <tr class="dcf-table-body-row dcf-sub-forums">
						<td class="dcf-col pl-5 px-5" colspan="5">
							<h5 class="dcf-sub-forum-title h6 my-1">[RESX:Child] [RESX:FORUMS]</h5>
						</td>
					</tr> 
						<tr class="dcf-table-body-row dcf-sub-forums">
							
							<td class="dcf-col dcf-col-text pl-5 ps-5">
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
								<div class="dcf-last-post">
									[FORUM:LASTPOSTSUBJECT:25]
								<div class="dcf-last-profile">
									[FORUM:LASTPOSTAUTHORDISPLAYNAMELINK|
									[RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>&nbsp;<a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUM:LASTPOSTAUTHORDISPLAYNAME]</a>|
									[RESX:BY] <i class="fa fa-user fa-fw fa-blue"></i>[FORUM:LASTPOSTAUTHORDISPLAYNAME]
									]
									</div>
									<div class="dcf-last-date">
									[FORUM:LASTPOSTDATE]
									</div>
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
		<div class="text-end">[JUMPTO]</div>
	</div>