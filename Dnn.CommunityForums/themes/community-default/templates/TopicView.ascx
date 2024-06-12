<%@ Register TagPrefix="am" Namespace="DotNetNuke.Modules.ActiveForums.Controls" Assembly="DotNetNuke.Modules.ActiveForums" %>

<div class="dcf-topic-view">
    <header>
		<div class="dcf-prev-next">
			<div class="dcf-cols">
				<div class="dcf-col-50"><span class="fa-prev">[PREVTOPIC]</span>
				</div>
				<div class="dcf-col-50 text-right"><span class="fa-next">[NEXTTOPIC]</span>
				</div>
			</div>
		</div>
		<div class="dcf-breadcrumb">[FORUMMAINLINK] <i class="fa fa-chevron-right"></i> [FORUMGROUPLINK] <i class="fa fa-chevron-right"></i> [FORUMLINK]</div>
		<div class="dcf-header-content">

			<div class="dcf-cols">
				<div class="dcf-col dcf-col-50-md">
					<h1 class="dcf-title dcf-title-1">[AF:CONTROL:STATUSICON]<span class="dcf-topic">[SUBJECT]</span></h1>
					<div class="dcf-topic-buttons">
                        <div class="dcf-button-reply">[ADDREPLY]</div>
						<div class="dcf-split-buttons-wrap">[SPLITBUTTONS]</div>
					</div>
				</div>
				<div class="dcf-col dcf-col-50-md">
                    <div class="dcf-forum-subscribers"><i class="fa fa-reply fa-fw fa-grey"></i>&nbsp;[AF:LABEL:ReplyCount] [RESX:REPLIES]</div>
                    <div class="dcf-forum-subscribers"><i class="fa fa-envelope-o fa-fw fa-grey"></i>&nbsp;<span id="af-topicview-topicsubscribercount">[TOPICSUBSCRIBERCOUNT]</span> [RESX:TOPICSUBSCRIBERCOUNT]</div>
                    <div class="dcf-forum-subscribers"><i class="fa fa-envelope fa-fw fa-grey"></i>&nbsp;[FORUMSUBSCRIBERCOUNT]&nbsp;[RESX:FORUMSUBSCRIBERCOUNT]</div>
					
					<div class="dcf-topic-controls">
						<div class="dcf-subscribe-topic">[TOPICSUBSCRIBE]</div>
						<div class="dcf-sort">[TRESX:SortPosts]:[SORTDROPDOWN]</div>
					</div>
				</div>
			</div>
		</div>
	</header>

    <div class="dcf-tools dcf-tools-top">
        
       	<div class="dcf-pager">[PAGER1]</div>
    </div>

    [AF:CONTROL:CALLBACK]


    <div class="dcf-topic-wrap">
        <div class="dcf-topic-head">
			<div class="dcf-cols dcf-topic-headings">
				<div class="dcf-col-25 dcf-topic-heading-author">[TRESX:Author]
				</div>
				<div class="dcf-col-75 dcf-topic-heading-content">[TRESX:Messages]
				</div>
			</div>
		</div>
	
		<div id="afgrid" class="dcf-topic-posts">
		
			[TOPIC]
			<div class="dcf-topic-post">
				<a id="[POSTID]"></a>
				<div class="dcf-cols">
					<div class="dcf-col-25">
						<div class="dcf-topic-info">
							[POSTINFO]
						</div>
					</div>
					<div class="dcf-col-75">
						<div class="dcf-topic-content">
							<header class="dcf-topic-content-top">
								<div class="dcf-cols">
									<div class="dcf-col-25">
										<div class="dcf-postdate">[POSTDATE] </div>
										
									</div>
									<div class="dcf-col-75">
										<div class="dcf-toolbar dcf-topic-actions">[AF:CONTROL:TOPICACTIONS]</div>
									</div>
								</div>
							</header>
							<section class="dcf-topic-content-main">
							
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
								<div class="dcf-cols dcf-post-footer-bottom">
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
				</div>

			[/TOPIC]
            [REPLIES]
			<div class="dcf-topic-post dcf-topic-reply">
				<a id="[POSTID]"></a>
				<div class="dcf-cols">
					<div class="dcf-col-25">
						<div class="dcf-topic-info">
							[POSTINFO]
						</div>
					</div>
					<div class="dcf-col-75">
						<div class="dcf-topic-content">
							<header class="dcf-topic-content-top">
								<div class="dcf-cols">
									<div class="dcf-col-25">
										<div class="dcf-postdate">[POSTDATE] </div>
										
									</div>
									<div class="dcf-col-75">
										<div class="dcf-toolbar dcf-topic-actions">[AF:CONTROL:TOPICACTIONS]<span class="dcf-split-checkbox-wrap">[SPLITCHECKBOX]</span></div>
                                        
									</div>
								</div>
							</header>
							<section class="dcf-topic-content-main">
							
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
								<div class="dcf-cols dcf-post-footer-bottom">
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