<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>

	<div class="aftopicjumper">
				<div class="afprevtopic">[PREVTOPIC] <span class="aftopicjumperdesc _hide">Go to previous topic</span></div>
				<div class="afnexttopic">[NEXTTOPIC] <span class="aftopicjumperdesc _hide">Go to next topic</span></div>
			</div>
<table class="afgrid">
	<tr>
		<td>
			[BREADCRUMB]<div class="afcrumb"><i class="fa fa-comments-o fa-grey"></i>  [FORUMMAINLINK] <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMGROUPLINK]  <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMLINK]</div>[/BREADCRUMB]

			<div class="aftopicsum">[RESX:LastPost] [AF:LABEL:LastPostDate] [RESX:BY] <strong><i class="fa fa-user fa-fw fa-blue"></i>&nbsp;[AF:LABEL:LastPostAuthor]</strong></div>
			<div class="aftitlelg">[SUBJECT]</div>
			<div class="aftopicreplies"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[AF:LABEL:ReplyCount] [RESX:REPLIES]</div>
		    <div class="afsubscribercount"><i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;<span id="af-topicview-topicsubscribercount">[TOPICSUBSCRIBERCOUNT]</span> [RESX:TOPICSUBSCRIBERCOUNT]</div>
		    <div class="afsubscribercount"><i class="fa fa-envelope fa-fw fa-grey"></i>&nbsp;[FORUMSUBSCRIBERCOUNT] [RESX:FORUMSUBSCRIBERCOUNT]</div>
		    <div class="afprop-list">
				<ul>
				[AF:PROPERTIES]
					<li><b>[AF:PROPERTY:LABEL]:</b> [AF:PROPERTY:VALUE]</li>
				[/AF:PROPERTIES]
				</ul>
			</div>
		</td>
		<td class="_hide_" align="right" valign="top">
			<div style="padding:5px;"> 
				<span class="afactionicon">[AF:CONTROL:PRINTER]</span>
				<span class="afactionicon">[AF:CONTROL:EMAIL]</span>
			</div>
			[POSTRATINGBUTTON][AF:CONTROL:STATUS]
			<div style="padding-top:5px;">[RESX:SortPosts]:[SORTDROPDOWN]</div>
		</td>
	</tr>
</table>

[AF:CONTROL:CALLBACK]
<table class="afgrid">
	<tr>
		<td class="afnormal">[TOPICSUBSCRIBE]</td>
	</tr>
	<tr>
		<td valign="bottom" style="white-space:nowrap;padding-bottom:5px;">
			<div class="afbuttonarea">[ADDREPLY]</div>
		</td>
		<td valign="bottom" align="right" width="100%" style="padding-bottom:5px;">[SPLITBUTTONS] [PAGER1]</td>
	</tr>
</table>	

<div id="afgrid" style="position:relative;">
<table class="afgrid" cellpadding="4">
	<tr>
		<td class="afgrouprow aftopicviewheader aftopicauthor">[RESX:Author]</td>
		<td class="afgrouprow aftopicviewheader aftopicmessage">[RESX:Messages]</td>
		<td class="afgrouprow aftopicviewheader aftopicstatus">[AF:CONTROL:STATUSICON]</td>
	</tr>
</table>
		<table class="afgrid" cellpadding="4">
			[TOPIC]
				<tr>
					<td class="[POSTTOPICCSS]">
					<div class="afpostinfo_responsive">[POSTINFO]<br />[SPACER:1:10]</div>

					<div class="afpostcontent_responsive">
						<table class="afgrid" cellpadding="4">
							<tr>
								<td class="afsubrow afsubrowdate"><a name="[POSTID]"></a>[POSTDATE]</td>
								<td class="afsubrow af-actions">[AF:CONTROL:TOPICACTIONS]</td>
							</tr>
							<tr>
								<td colspan="2">
									<div class="afpostbody">[AF:CONTROL:POLL][BODY]</div>
									<div class="afpostsig">[SIGNATURE]</div>
								</td>
							</tr>
							<tr>
								<td colspan="2" class="afpostattach">[ATTACHMENTS]</td>
							</tr>
							[AF:CONTROL:TAGS]
							<tr>
								<td colspan="2" class="afposttags">[RESX:Tags]: [AF:LABEL:TAGS]</td>
							</tr>
							[/AF:CONTROL:TAGS]
							<tr>
								<td colspan="2" class="afposteditdate" align="right">[MODEDITDATE][LIKESx2]</td>
							</tr>
						</table>				
					</div>
					</td>
				</tr>
				[/TOPIC]
				[REPLIES]
				<tr>
					<td class="[POSTREPLYCSS]">
						<div class="afpostinfo_responsive">[SPLITCHECKBOX][POSTINFO]<br />[SPACER:1:10]</div>
						<div class="afpostcontent_responsive">
							<table class="afgrid" cellpadding="4">
								<tr>
									<td class="afsubrow"><a name="[POSTID]"></a>[POSTDATE]</td>
									<td class="afsubrow af-actions">[AF:CONTROL:POSTACTIONS]</td>
								</tr>
								<tr>
									<td colspan="2">
										<div class="afpostbody">[BODY]</div>
										<div class="afpostsig">[SIGNATURE]</div>
									</td>
								</tr>
								<tr>
									<td colspan="2" class="afpostattach">[ATTACHMENTS]</td>
								</tr>
								<tr>
									<td colspan="2" class="afposteditdate" align="right">[MODEDITDATE][LIKESx2]</td>
								</tr>
							</table>				
						</div>
					</td>
				</tr>
		[/REPLIES]
	</table>
</div>	
<table class="afgrid" cellpadding="4">
	<tr>
		<td class="afbuttonarea">[ADDREPLY]</td>
		<td class="af-right"><div class="af-fright">[SPLITBUTTONS2][PAGER2]<br />[JUMPTO]</div></td>
	</tr>
</table>
[/AF:CONTROL:CALLBACK]
<table class="afgrid" cellpadding="4">
	<tr>
		<td><div class="afcrumb"><i class="fa fa-comments fa-grey"></i> [FORUMMAINLINK] <i class="fa fa-long-arrow-right fa-grey"></i> [FORUMGROUPLINK] <i class="fa fa-long-arrow-right fa-grey"></i> [FORUMLINK]</div><br /></td>
	</tr>
	<tr>
		<td>[QUICKREPLY]</td>
	</tr>
</table>
