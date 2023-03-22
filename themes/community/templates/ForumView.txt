<%@ Register TagPrefix="af" Assembly="DotNetNuke.Modules.ActiveForums" Namespace="DotNetNuke.Modules.ActiveForums.Controls"%>
[BREADCRUMB]
[GROUPSECTION]
<div class="afgrid">
	<div class="afgrid-inner">
		<div class="afgroupsection">
			<table width="100%">
				<tr>
					<td class="afgrouprow af-groupname"><div class="afgroupsectiontitle">[GROUPNAME]</div></td>
					<td class="afgrouprow af-groupcollapse">[GROUPCOLLAPSE]</td>
				</tr>
			</table>
		</div>
		<div class="forum-group">
			[GROUP]
				<table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
					<tr class="forum-headers">
						<td class="afgrouprow afgrouprow-f">
							<div class="afcontrolheader">[RESX:FORUMHEADER]</div>
						</td>
						<td class="afgrouprow afgrouprow-m">
							<div class="afcontrolheader"><i class="fa fa-files-o fa-fw fa-grey"></i>&nbsp;[RESX:TOPICSHEADER]</div>
						</td>
						<td class="afgrouprow afgrouprow-m">
							<div class="afcontrolheader"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[RESX:REPLIESHEADER]</div>
						</td>
						<td class="afgrouprow afgrouprow-m">
							<div class="afcontrolheader"><i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;[RESX:SUBSCRIBERS]</div>
						</td>
						<td class="afgrouprow afgrouprow-l">
							<div class="afcontrolheader"><i class="fa fa-file-o fa-fw fa-grey"></i>&nbsp;[RESX:LASTPOSTHEADER]</div>
						</td>

					</tr>
						
					[FORUMS]
					<tr class="forum-cols">
						<td colspan="5">
							 <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
								<tr>
									<td class="aftopicrow af-icons">[FORUMICONCSS]</td>
									<td class="aftopicrow af-content">
										<table>
											<tr>
												<td rowspan="2" class="afsubject">
													<span class="afhiddenstats">[TOTALTOPICS] [RESX:TOPICSHEADER] and [TOTALREPLIES] [RESX:REPLIESHEADER] and [FORUMSUBSCRIBERCOUNT] [RESX:SUBSCRIBERS]</span>
													<span class="aftopictitle">[FORUMNAME]</span>
													<span class="af-colstats_responsive">
														<i class="fa fa-files-o fa-fw fa-grey"></i>&nbsp;[TOTALTOPICS] 
														<i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[TOTALREPLIES]
														<i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;[FORUMSUBSCRIBERCOUNT]
													</span>
													<span class="aftopicsubtitle">[FORUMDESCRIPTION]</span>
													<div class="af_lastpost_responsive" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]<br />[RESX:BY] [DISPLAYNAME]<br />[LASTPOSTDATE]</div>
												</td>
											</tr>
										</table>
									</td>
									<td class="aftopicrow af-colstats af-colstats-replies">[TOTALTOPICS]</td>
									<td class="aftopicrow af-colstats af-colstats-views">[TOTALREPLIES]</td>
									<td class="aftopicrow af-colstats af-colstats-views">[FORUMSUBSCRIBERCOUNT]</td>
									<td class="aftopicrow af-lastpost"><div class="af_lastpost" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]<br />[RESX:BY] [DISPLAYNAME]<br />[LASTPOSTDATE]</div></td>
								</tr>
							</table>	
						</td>
					</tr>

					[SUBFORUMS]
					<tr class="sub-forum-cols">
						<td colspan="5">
							<table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
								<tr>
									<td colspan="0">
										 <table class="afgrid" cellspacing="0" cellpadding="0" width="100%">
											<tr>
												<td class="aftopicrow af-icons"></td>
												<td class="aftopicrow af-content">
													<table>
														<tr>
															<td rowspan="2" class="afsubject">
															
															<span class="afhiddenstats">[TOTALTOPICS] [RESX:TOPICS] and [TOTALREPLIES] [RESX:REPLIES] and [FORUMSUBSCRIBERCOUNT] [RESX:SUBSCRIBERS]</span>
															<span class="aftopictitle">[FORUMICONSM][FORUMNAME]</span>
															<span class="af-colstats_responsive">
																<i class="fa fa-files-o fa-fw fa-grey"></i>&nbsp;[TOTALTOPICS] 
																<i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[TOTALREPLIES]
																<i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;[FORUMSUBSCRIBERCOUNT]
															</span>
															<span class="aftopicsubtitle">[FORUMDESCRIPTION]</span>
															<div class="af_lastpost_responsive" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]<br />[RESX:BY] [DISPLAYNAME]<br />[LASTPOSTDATE]</div>
															</td>
														</tr>
													</table>
												</td>
												<td class="aftopicrow af-colstats af-colstats-replies">[TOTALTOPICS]</td>
												<td class="aftopicrow af-colstats af-colstats-views">[TOTALREPLIES]</td>
												<td class="aftopicrow af-colstats af-colstats-views">[FORUMSUBSCRIBERCOUNT]</td>
												<td class="aftopicrow af-lastpost"><div class="af_lastpost" style="white-space:nowrap;">[LASTPOSTSUBJECT:25]<br />[RESX:BY] [DISPLAYNAME]<br />[LASTPOSTDATE]</div></td>
											</tr>
										</table>	
									</td>
								</tr>
							</table>
						</td>
					</tr>
					[/SUBFORUMS]
				[/FORUMS]
			</table>		
		[/GROUP]
		</div>
	</div>
</div>
[/GROUPSECTION]
<!-- Who's online -->
[WHOSONLINE]
<!-- Jump To -->
<div style="text-align:right;">[JUMPTO]</div>