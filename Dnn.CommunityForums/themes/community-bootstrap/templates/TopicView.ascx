<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>
<div class="dcf-topic-view">
    <header>
		<div class="dcf-prev-next border-bottom border-top py-2">
			<div class="d-flex">
				<div class="w-50">
				<span class="fa-prev">[FORUMTOPIC:PREVIOUSTOPICLINK|<a href="{0}" rel="nofollow" title="[RESX:PrevTopic]"><i class="fa fa-chevron-left" aria-hidden="true"></i><span>[RESX:PrevTopic]</span></a>]</span>
				</div>
				<div class="w-50 text-right text-end">
				<span class="fa-next">[FORUMTOPIC:NEXTTOPICLINK|<a href="{0}" rel="nofollow" title="[RESX:NextTopic]"><span>[RESX:NextTopic]</span><i class="fa fa-chevron-right" aria-hidden="true"></i></a>]</span>
				</div>
			</div>
		</div>
		<div class="dcf-breadcrumb py-2 border-bottom">[FORUM:FORUMMAINLINK|<a href="{0}" class="dcf-forums-link">[RESX:ForumMain]</a>] <i class="fa fa-chevron-right"></i> [FORUMGROUP:GROUPLINK|<a href="{0}" class="dcf-forumgroup-link">[FORUMGROUP:GROUPNAME]</a>] <i class="fa fa-chevron-right"></i> [FORUM:FORUMLINK|<a href="{0}" class="dcf-forum-link">[FORUM:FORUMNAME]</a>]</div>
		<div class="dcf-header-content mt-4">

			<div class="d-md-flex">
				<div class="flex-grow-1">
					<h1 class="dcf-title h2 mt-0 d-flex align-items-center">[AF:CONTROL:STATUSICON]<span class="dcf-topic">[FORUMTOPIC:SUBJECT]</span></h1>
					<div class="dcf-topic-buttons">
						<div class="dcf-button-reply">[ADDREPLY]</div>
						<div class="dcf-split-buttons-wrap">[SPLITBUTTONS]</div>
					</div>
				</div>
				<div class="">
					<div class="dcf-topic-controls text-right text-end">
                        <div class="dcf-forum-subscribers"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[FORUMTOPIC:REPLYCOUNT] [RESX:REPLIES]</div>
                        <div class="dcf-forum-subscribers"><i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;<span id="af-topicview-topicsubscribercount">[FORUMTOPIC:SUBSCRIBERCOUNT]</span> [RESX:TOPICSUBSCRIBERCOUNT]</div>
                        <div class="dcf-forum-subscribers"><i class="fa fa-envelope fa-fw fa-grey"></i>&nbsp;[FORUM:SUBSCRIBERCOUNT]&nbsp;[RESX:FORUMSUBSCRIBERCOUNT]</div>

						<div class="dcf-subscribe-topic">
                            [FORUMTOPIC:SUBSCRIBEONCLICK|<input type="button" class="dcf-btn-subs [FORUMTOPIC:SUBSCRIBE-UNSUBSCRIBE-CSSCLASS]" value="[FORUMTOPIC:SUBSCRIBE-UNSUBSCRIBE-LABEL]" onclick="{0}" />]
                        </div>
                        <span class="dcf-sort">[TRESX:SortPosts]:[SORTDROPDOWN]</span>
					
					</div>
				</div>
			</div>
		</div>
	</header>

    <div class="dcf-tools dcf-tools-top">
        
       	<div class="dcf-pager"> [PAGER1]</div>
    </div>

    [AF:CONTROL:CALLBACK]


    <div class="dcf-topic-wrap">
        <div class="dcf-topic-head">
			<div class="dcf-topic-headings d-flex border-bottom border-primary p-3 mb-4">
				<div class="dcf-topic-heading-author flex-grow-1">[TRESX:Author]
				</div>
				<div class="dcf-topic-heading-content">[TRESX:Messages]
				</div>
			</div>
		</div>
	
		<div id="afgrid" class="dcf-topic-posts">
		
			[TOPIC]
			<div class="dcf-topic-post py-3 pb-5 mb-4 border-bottom border-primary">
				<div class="d-flex flex-wrap">

			
						<div class="dcf-topic-info px-3 pb-2 mb-2 mb-md-0">
							[DCF:TEMPLATE-PROFILEINFO]
						</div>

					<div class="dcf-topic-content px-3">
							<header class="dcf-topic-content-top ">
								<div class="d-flex flex-wrap pb-2">
									<div class="dcf-postdate">[FORUMPOST:DATECREATED]</div>
									<div class="dcf-toolbar dcf-topic-actions flex-grow-1 text-right text-end">[DCF:TEMPLATE-POSTACTIONS]</div>
								</div>
							</header>
							<section class="dcf-topic-content-main py-4">
							
								<div class="dcf-post-body">
									[FORUMPOST:BODY]
								</div>
								<div class="dcf-post-attachements">
									[ATTACHMENTS]
								</div>
								<div class="dcf-post-meta">
									[AF:CONTROL:TAGS][TRESX:Tags]: [AF:LABEL:TAGS]
								[/AF:CONTROL:TAGS]
								</div>
								
							</section>
							<footer class="dcf-post-footer">
                                [FORUMAUTHOR:SIGNATURE|<div class="dcf-post-signature">{0}</div>]
								<div class="dcf-post-footer-bottom">
									<div class="dcf-col-50">
                                        [FORUMPOST:MODEDITDATE]
									</div>
                                    <div class="dcf-col-50 text-right text-end dcf-post-likes">
										[FORUMPOST:LIKEONCLICK|<i id="af-topicview-likes-[FORUMPOST:CONTENTID]" class="dcf-post-like fa [FORUMPOST:ISLIKED|fa-thumbs-o-up|fa-thumbs-up] fa-2x" onclick="{0}"> [FORUMPOST:LIKECOUNT]</i>|<i id="af-topicview-likes" class="dcf-post-liked fa [FORUMPOST:ISLIKED|fa-thumbs-o-up|fa-thumbs-up] fa-2x"> [FORUMPOST:LIKECOUNT]</i>]
										[FORUMPOST:LIKESLINK|<div id="dcf-likes-[FORUMPOST:CONTENTID]" class="dcf-likes">[RESX:likedby] <a href="{0}">[RESX:Members]</a></div>]
									</div>
                                </div>
							</footer>
						</div>
					</div>
				</div>

			[/TOPIC]
            [REPLIES]
			<div class="dcf-topic-post dcf-topic-reply py-3 pb-5 mb-4 border-bottom border-primary">
				<a id="[FORUMPOST:POSTID]"></a>
				<div class="d-flex flex-wrap">

						<div class="dcf-topic-info px-3 pb-2  mb-2 mb-md-0">
							[DCF:TEMPLATE-PROFILEINFO]
						</div>

						<div class="dcf-topic-content px-3">
							<header class="dcf-topic-content-top ">
								<div class="d-flex flex-wrap pb-2">
									<div class="dcf-postdate">[FORUMPOST:DATECREATED]</div>
									<div class="dcf-toolbar dcf-topic-actions flex-grow-1 text-right text-end">[DCF:TEMPLATE-POSTACTIONS]<span class="dcf-split-checkbox-wrap">[SPLITCHECKBOX]</span></div>
								</div>
							</header>
							<section class="dcf-topic-content-main py-4">
							
								<div class="dcf-post-poll">
									[AF:CONTROL:POLL]
								</div>
								<div class="dcf-post-body">
									[FORUMPOST:BODY]
								</div>
								<div class="dcf-post-attachements">
									[ATTACHMENTS]
								</div>
								
							</section>
							<footer class="dcf-post-footer">
                                [FORUMAUTHOR:SIGNATURE|<div class="dcf-post-signature">{0}</div>]
								<div class="dcf-post-footer-bottom">
									<div class="dcf-col-50">
                                        [FORUMPOST:MODEDITDATE]
									</div>
                                    <div class="dcf-col-50 text-right text-end dcf-post-likes">
										[FORUMPOST:LIKEONCLICK|<i id="af-topicview-likes-[FORUMPOST:CONTENTID]" class="dcf-post-like fa [FORUMPOST:ISLIKED|fa-thumbs-o-up|fa-thumbs-up] fa-2x" onclick="{0}"> [FORUMPOST:LIKECOUNT]</i>|<i id="af-topicview-likes" class="dcf-post-liked fa [FORUMPOST:ISLIKED|fa-thumbs-o-up|fa-thumbs-up] fa-2x"> [FORUMPOST:LIKECOUNT]</i>]
										[FORUMPOST:LIKESLINK|<div id="dcf-likes-[FORUMPOST:CONTENTID]" class="dcf-likes">[RESX:likedby] <a href="{0}">[RESX:Members]</a></div>]
				                   </div>
								</div>
							</footer>
						</div>
					</div>
			</div>
            [/REPLIES]
		</div>
    </div>

    <div class="dcf-tools dcf-tools-bottom d-flex align-items-top gap-1">
        <div class="dcf-button-reply">[ADDREPLY]</div>
        <div class="dcf-subscribe-topic flex-grow-1">
            [FORUMTOPIC:SUBSCRIBEONCLICK|<input id="amaf-chk-subs" type="checkbox" checked="[FORUMTOPIC:SUBSCRIBED]" class="amaf-chk-subs" onclick="{0}" /><label for="amaf-chk-subs">[RESX:Subscribe]</label>]

        </div>
       	<div class="dcf-pager">[PAGER2]</div>
    </div>


    [/AF:CONTROL:CALLBACK]

    <div class="dcf-quickreply">[QUICKREPLY]</div>

</div>