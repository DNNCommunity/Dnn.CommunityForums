<div class="afgrid">
	<div class="afgrouprow">
		<div class="afgrouprow"><div class="afcontrolheader">[RESX:QuickReply]</div></div>
		<div class="afgrouprow">[AF:CONTROLS:GROUPTOGGLE]</div>
	</div>
	<div class="afgrouprow">
		<div class="afgrouprow">
			<div id="groupQR" style="background-color:white;"><asp:PlaceHolder ID="plhMessage" runat="server" />
				<div>
					<div class="afgrouprow"> 
						<div class="NormalBold">[RESX:Subject]:</div> 
						<br />
						<div><input type="text" id="txtSubject" class="aftextbox" readonly="readonly" value="[SUBJECT]" /></div>
						 <br />
					</div> <br /> <br />
					<div class="afgrouprow">
						<div><asp:Label ID="reqBody" runat="server" Visible="false" /></div>
						<div class="NormalBold">[RESX:Body]:</div>
						<div>
							<div id="btnToolBar" runat="server">	
							
								 
								<i class="fa fa-bold fa-fw fa-blue" accesskey="b" onclick="insertCode('[b] [/b]');" onmouseover="window.status='[RESX:BoldDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-italic fa-fw fa-blue" accesskey="i" onclick="insertCode('[i] [/i]');" onmouseover="window.status='[RESX:ItalicsDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-underline fa-fw fa-blue" accesskey="u" onclick="insertCode('[u] [/u]');" onmouseover="window.status='[RESX:UnderlineDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-quote-left fa-fw fa-blue" accesskey="q" onclick="insertQuote();" onmouseover="window.status='[RESX:QuoteDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-image fa-fw fa-blue" accesskey="m" onclick="insertCode('[img] [/img]');" onmouseover="window.status='[RESX:ImageDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-code fa-fw fa-code" accesskey="c" onclick="insertCode('[code] [/code]');" onmouseover="window.status='[RESX:CodeDesc]'; return true;" title='[RESX:CodeDesc]' onmouseout="window.status=''; return true;"></i>
						 <br /> <br /> <br />
							<!--
									<input type="button" class="afButton" accesskey="b" name="afBold" value="[RESX:Bold]" style="font-weight:bold;" onclick="insertCode('[b] [/b]');" onmouseover="window.status='[RESX:BoldDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="afButton" accesskey="i" name="afBold" value="[RESX:Italics]"  style="font-weight:bold;" onclick="insertCode('[i] [/i]');" onmouseover="window.status='[RESX:ItalicsDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="afButton" accesskey="u" name="afBold" value="[RESX:Underline]" style="font-weight:bold;" onclick="insertCode('[u] [/u]');" onmouseover="window.status='[RESX:UnderlineDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="afButton" accesskey="q" name="afBold" value="[RESX:Quote]" style="font-weight:bold;" onclick="insertQuote();" onmouseover="window.status='[RESX:QuoteDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="afButton" accesskey="m" name="afBold" value="[RESX:Image]" style="font-weight:bold;" onclick="insertCode('[img] [/img]');" onmouseover="window.status='[RESX:ImageDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="afButton" accesskey="c" name="afBold" value="[RESX:Code]" style="font-weight:bold;" onclick="insertCode('[code] [/code]');" onmouseover="window.status='[RESX:CodeDesc]';  return true;" title='[RESX:CodeDesc]' onmouseout="window.status=''; return true;" />
					-->
								
							</div> <br />
							<textarea id="txtBody" name="txtBody" class="aftextbox" style="height:120px;width:1000px;" rows="5" cols="250"></textarea>
						</div>
					</div> <br />
					[AF:UI:ANON]
						<div class="afgrouprow">
							<div class="NormalBold">[RESX:Username]:[AF:REQ:USERNAME]</div>
							<div style="width:150px;">[AF:INPUT:USERNAME]</div>
							<div></div>
						</div>
						<div class="afgrouprow">
							<div class="NormalBold">[RESX:SecurityCode]:[AF:REQ:SECURITYCODE]</div>
							<div>[AF:INPUT:CAPTCHA]</div> 
						</div> <br />
					[/AF:UI:ANON]
					<div class="afgrouprow">
						<div id="divSubscribe" runat="server" />
					</div> 
					<br /> <br /> <br /> <br />
					<div class="afgrouprow">
						<div>
							<div class="amtbwrapper" style="text-align:center;">
								<div style="margin:0px auto;min-width:50px;max-width:60px;">
									[AF:BUTTON:SUBMIT]
								</div>   
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>