<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
[NOPAGING]
<div style="padding:5px;float:right;clear:all;text-align:right;"><span class="afactionicon">[RSSLINK]</span><span class="afactionicon">[AF:CONTROL:PRINTER]</span><span class="afactionicon">[AF:CONTROL:EMAIL]</span>
[POSTRATINGBUTTON]<br />[PREVTOPIC]&nbsp;[NEXTTOPIC]
</div>
<div class="afcrumb"><i class="fa fa-comments-o fa-grey"></i> [FORUMMAINLINK] <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMGROUPLINK]  <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMLINK]</div>

[AF:CONTROL:CALLBACK]
<div id="afgrid" style="position:relative;">
	<div class="aftopic">
		[TOPIC]
		<h1>[SUBJECT]</h1>
		<h3>[RESX:BY] [DISPLAYNAME] [RESX:On] [DATECREATED] </h3>
		[AF:CONTROL:TAGS]
		<div class="afposttags">[RESX:Tags]: [AF:LABEL:TAGS]</div>
		[/AF:CONTROL:TAGS]
		<div class="aftopicbody">
			[BODY]
		</div>
		<div class="afpostattach">[ATTACHMENTS]</div>
		<div>[SIGNATURE]</div>
		[ACTIONS:DELETE][ACTIONS:EDIT]
		[/TOPIC]
	</div>
	<div class="afcomments">
		<h1>[REPLYCOUNT] [RESX:Comments] [RESX:For] [SUBJECT]</h1>
		[REPLIES]
			<div class="afreply">
				<div class="afreplyheader" style="height:80px;">
					<div class="afavatar" style="width:80px;height:80px;float:right;text-align:center;">[AF:PROFILE:AVATAR]</div>
					<div class="afreplyinfo">[AF:PROFILE:DISPLAYNAME][AF:PROFILE:USERSTATUS]<br />[POSTDATE]<br />
						[WEBSITE]<br />
						[MODIPADDRESS]	
					</div>
				</div>
				<div class="afreplybody">
					[BODY]
				</div>
				<div class="afpostattach">[ATTACHMENTS]</div>
				[ACTIONS:DELETE]
									[ACTIONS:EDIT]
			</div>
		[/REPLIES]
	</div>
</div>	
<table cellspacing="0" cellpadding="4" width="100%" border="0">
	<tr>
		<td class="afnormal"></td>
		<td class="afcrumb" align="right">[PAGER1]</td>
	</tr>
</table>
[/AF:CONTROL:CALLBACK]
<table cellspacing="0" cellpadding="4" width="100%" border="0">
	<tr>
		<td>[QUICKREPLY]</td>
	</tr>
</table>
