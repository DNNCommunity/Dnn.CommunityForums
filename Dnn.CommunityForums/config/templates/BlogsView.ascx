[AF:SORT:TOPICCREATED]
<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<div style="padding:5px;float:right;clear:all;text-align:right;"><span class="afactionicon">[RSSLINK]</span><span class="afactionicon">[AF:CONTROL:EMAIL]</span>
</div>
<div class="afcrumb"><i class="fa fa-comments-o fa-grey"></i>  [FORUMMAINLINK]  <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMGROUPLINK]</div>
<div class="aftitlelg">[FORUMLINK]</div>
<div id="afgrid" style="position:relative;">
[TOPICS]
	<div class="aftopic">
		<a href="[TOPICURL]"><h1>[SUBJECT]</h1></a>
		<h3>[RESX:BY] [STARTEDBY] [RESX:On] [DATECREATED] </h3>
		<div class="afsummary">
			[BODY:150] <a href="[TOPICURL]">[RESX:ReadMoreSmall]</a>
		</div>
		<div class="afmore">
			<a href="[TOPICURL]">[RESX:Comments]: [REPLIES]</a> | [RESX:Views]: [VIEWS] | <a href="[TOPICURL]">[RESX:ReadMore]</a>
		</div>
	</div>
[/TOPICS]
</div>
<table cellspacing="0" cellpadding="0" width="100%" border="0">
		<tr>
			<td class="afnormal" valign="top"><div class="afbuttonarea">[ADDTOPIC]</div>
			<div class="afcrumb"><i class="fa fa-comments-o fa-grey"></i>  [FORUMMAINLINK]  <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMGROUPLINK]  <i class="fa fa-long-arrow-right fa-grey"></i>  [FORUMLINK]</div>
			[FORUMSUBSCRIBE]
			</td>
			<td align="right">[PAGER1]<br />[RSSLINK]</td>
		</tr>
	</tbody>
</table>