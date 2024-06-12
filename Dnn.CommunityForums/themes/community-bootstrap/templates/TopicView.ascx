<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>

<div class="dcf-topic-view">
    <header>
		<div class="dcf-prev-next border-bottom border-top py-2">
			<div class="d-flex">
				<div class="w-50">
				<span class="fa-prev">[PREVTOPIC]</span>
				</div>
				<div class="w-50 text-right text-end">
				<span class="fa-next">[NEXTTOPIC]</span>
				</div>
			</div>
		</div>
		<div class="dcf-breadcrumb py-2 border-bottom">[FORUMMAINLINK] <i class="fa fa-chevron-right"></i> [FORUMGROUPLINK] <i class="fa fa-chevron-right"></i> [FORUMLINK]</div>
		<div class="dcf-header-content mt-4">

			<div class="d-md-flex">
				<div class="flex-grow-1">
					<h1 class="dcf-title h2 mt-0 d-flex align-items-center">[AF:CONTROL:STATUSICON]<span class="dcf-topic">[SUBJECT]</span></h1>
				</div>
				<div class="">
					<div class="dcf-topic-controls text-right text-end">
                        <div class="dcf-forum-subscribers"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[AF:LABEL:ReplyCount] [RESX:REPLIES]</div>
                        <div class="dcf-forum-subscribers"><i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;<span id="af-topicview-topicsubscribercount">[TOPICSUBSCRIBERCOUNT]</span> [RESX:TOPICSUBSCRIBERCOUNT]</div>
                        <div class="dcf-forum-subscribers"><i class="fa fa-envelope fa-fw fa-grey"></i>&nbsp;[FORUMSUBSCRIBERCOUNT]&nbsp;[RESX:FORUMSUBSCRIBERCOUNT]</div>

						<div class="dcf-subscribe-topic">[TOPICSUBSCRIBE]</div>
                        <span class="dcf-sort">[TRESX:SortPosts]:[SORTDROPDOWN]</span>
                        <div class="dcf-split-buttons-wrap">[SPLITBUTTONS]</div>
						<div class="dcf-button-reply">[ADDREPLY]</div>
						
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
			<div class="dcf-topic-post py-3 bg-light mb-4">
				<div class="d-flex flex-wrap">

			
						<div class="dcf-topic-info bg-light px-3 pb-2 mb-2 mb-md-0">
							[POSTINFO]
						</div>

					<div class="dcf-topic-content px-3">
							<header class="dcf-topic-content-top ">
								<div class="d-flex flex-wrap pb-2">
									<div class="dcf-postdate">[POSTDATE] </div>
									<div class="dcf-toolbar dcf-topic-actions flex-grow-1 text-right text-end">[AF:CONTROL:TOPICACTIONS]</div>
								</div>
							</header>
							<section class="dcf-topic-content-main py-4">
							
								<div class="dcf-post-poll">
									[AF:CONTROL:POLL]
								</div>
								<div class="dcf-post-body">
									[BODY]
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
								<div class="dcf-post-signature">
									[SIGNATURE]
								</div>
								<div class="dcf-post-footer-bottom">
									<div class="dcf-col-50">
										[MODEDITDATE]
									</div>
									<div class="dcf-col-50 text-right dcf-post-likes">
										[LIKESx2]
									</div>
								</div>
							</footer>
						</div>
					</div>
				</div>

			[/TOPIC]
            [REPLIES]
			<div class="dcf-topic-post dcf-topic-reply py-3 bg-light mb-4">
				<a id="[POSTID]"></a>
				<div class="d-flex flex-wrap">

						<div class="dcf-topic-info bg-light px-3 pb-2  mb-2 mb-md-0">
							[POSTINFO]
						</div>

						<div class="dcf-topic-content px-3">
							<header class="dcf-topic-content-top ">
								<div class="d-flex flex-wrap pb-2">
									<div class="dcf-postdate">[POSTDATE] </div>
									<div class="dcf-toolbar dcf-topic-actions flex-grow-1 text-right text-end">[AF:CONTROL:TOPICACTIONS]<span class="dcf-split-checkbox-wrap">[SPLITCHECKBOX]</span></div>
								</div>
							</header>
							<section class="dcf-topic-content-main py-4">
							
								<div class="dcf-post-poll">
									[AF:CONTROL:POLL]
								</div>
								<div class="dcf-post-body">
									[BODY]
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
								<div class="dcf-post-signature">
									[SIGNATURE]
								</div>
								<div class="dcf-post-footer-bottom">
									<div class="dcf-col-50">
										[MODEDITDATE]
									</div>
									<div class="dcf-col-50 text-right dcf-post-likes">
										[LIKESx2]
									</div>
								</div>
							</footer>
						</div>
					</div>
			</div>
            [/REPLIES]
		</div>
    </div>

    <div class="dcf-tools dcf-tools-bottom">
        <div class="dcf-button-reply">[ADDREPLY]</div>
       	<div class="dcf-pager">[PAGER2]</div>
    </div>


    [/AF:CONTROL:CALLBACK]

    <div class="dcf-quickreply">[QUICKREPLY]</div>

</div>