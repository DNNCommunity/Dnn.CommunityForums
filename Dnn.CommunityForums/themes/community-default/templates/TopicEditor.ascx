<div class="afcrumb">[AF:LINK:FORUMMAIN] > [AF:LINK:FORUMGROUP] > [AF:LINK:FORUMNAME]</div>
<div style="text-align:center;padding-top:10px;">
<div class="afeditor">
	<table cellpadding="10" cellspacing="0" width="100%">
		<tr>
			<td style="border-bottom:solid 1px #cdcdcd;text-align:left;"><span class="aftitle">[RESX:CreateNewTopic]</span></td>
			<td style="border-bottom:solid 1px #cdcdcd;text-align:right;"><span style="font-weight:bold;">[RESX:Forum]</span>:[AF:LINK:FORUMNAME]</td>
		</tr>
		<tr>
			<td colspan="2" align="center">			
				<table cellpadding="0" cellspacing="4" border="0" width="99%">
					<tr>
						<td>
							[AF:UI:ANON]
							<table cellpadding="0" cellspacing="4" border="0" width="99%">
								<tr>
									<td style="text-align:left;">[RESX:Username]:[AF:REQ:USERNAME]</td>
									<td></td>
								</tr>
								<tr>
									<td style="text-align:left;"><div style="width:150px;">[AF:INPUT:USERNAME]</div></td>
									<td></td>
								</tr>
								<tr>
									<td style="text-align:left;">[RESX:SecurityCode]:[AF:REQ:SECURITYCODE]</td>
									<td></td>
								</tr>
								<tr>
									<td style="text-align:left;"><div style="width:150px;">[AF:INPUT:CAPTCHA]</div></td>
									<td></td>
								</tr>
							</table>
							[/AF:UI:ANON]
							<table cellpadding="0" cellspacing="4" border="0" width="99%">
                <tr>
									<td style="text-align: left; width:99%;"><span class="aftitle">[RESX:Subject]:[AF:REQ:SUBJECT]</span>[AF:INPUT:SUBJECT]</td>
                  <td style="text-align: right;"><span class="aftitle">[RESX:TopicStatus]:</span>&nbsp;[AF:CONTROL:STATUS]</td>
                </tr>
							</table>
						</td>
					</tr>
                    [AF:UI:SECTION:POSTICONS]
					<tr>
						<td style="text-align:left;">
							[AF:UI:FIELDSET:POSTICONS]
								[AF:CONTROL:POSTICONS]
							[/AF:UI:FIELDSET:POSTICONS]
						</td>
						<td></td>
					</tr>
                    [/AF:UI:SECTION:POSTICONS]
					[AF:UI:SECTION:PROPERTIES]
					<tr>
						<td colspan="2">
							<table class="afprop-table">
					[AF:PROPERTIES]
						
						<tr>
							<td class="afprop-label">[AF:PROPERTY:LABEL]:</td><td>[AF:PROPERTY:CONTROL]</td><td>[AF:PROPERTY:REQUIRED]</td>
						</tr>
						
					
					
					[/AF:PROPERTIES]
							</table>
						</td>
					</tr>
                    [/AF:UI:SECTION:PROPERTIES]
					<tr>
                        <td style="text-align: left;"><span class="aftitle">[RESX:Message]:</span>[AF:REQ:BODY]</td>
                    </tr>
					<tr>
						<td width="100%">[AF:INPUT:BODY]
						
						</td>
						<td style="width:70px;">[AF:CONTROL:EMOTICONS]</td>
					</tr>
					<tr>
						<td style="text-align:left;">
							[AF:UI:SECTION:SUMMARY]
								[AF:INPUT:SUMMARY]
                            [/AF:UI:SECTION:SUMMARY]
							
							[AF:UI:SECTION:TAGS]
								[AF:CONTROL:TAGS]
							[/AF:UI:SECTION:TAGS]
							[AF:UI:SECTION:CATEGORIES]
								[AF:CONTROL:CATEGORIES]
							[/AF:UI:SECTION:CATEGORIES]
							[AF:UI:SECTION:ATTACH]
								[AF:CONTROL:UPLOAD]
							[/AF:UI:SECTION:ATTACH]
							[AF:UI:SECTION:POLL]
								[AF:CONTROL:POLL]
							[/AF:UI:SECTION:POLL]
							[AF:UI:SECTION:OPTIONS]
								[AF:CONTROL:OPTIONS]
							[/AF:UI:SECTION:OPTIONS]
						</td>
						<td></td>
					</tr>		
					<tr>
						<td>
							<div class="amtbwrapper">
								<div class="amtbwrapper" style="text-align:center;">
									<div style="margin-left:0 auto;margin-right:0 auto;min-width:50px;max-width:160px;">
										[AF:BUTTON:SUBMIT][AF:BUTTON:CANCEL][AF:BUTTON:PREVIEW]
									</div>
								</div>
								
							</div>
							
						</td>
						<td></td>
					</tr>
					<tr>
						<td colspan="2">
						
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
</div>
</div>