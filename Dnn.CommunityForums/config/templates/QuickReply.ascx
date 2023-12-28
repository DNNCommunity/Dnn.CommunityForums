<table class="afgrid">
	<tr>
		<td class="afgrouprow"><div class="afcontrolheader">[RESX:QuickReply]</div></td>
		<td class="afgrouprow" align="right" style="text-align:right;padding-right:10px;">[AF:CONTROLS:GROUPTOGGLE]</td>
	</tr>
	<tr>
		<td colspan="2" class="afborder">
			<div id="groupQR"><asp:PlaceHolder ID="plhMessage" runat="server" />
				<table width="100%" cellspacing="0" cellpadding="4">	
					<tr>
						<td></td>
						<td class="NormalBold">[RESX:Subject]:</td>
						<td><input type="text" id="txtSubject" class="aftextbox" readonly="readonly" value="[SUBJECT]" /></td>
						<td></td>
					</tr>
					<tr>
						<td valign="top"><asp:Label ID="reqBody" runat="server" Visible="false" /></td>
						<td valign="top" class="NormalBold">[RESX:Body]:</td>
						<td width="100%"><div id="btnToolBar" runat="server">
							<input type="button" class="afButton" accesskey="b" name="afBold" value="[RESX:Bold]" style="font-weight:bold;" onclick="insertCode('[b] [/b]');" onmouseover="window.status='[RESX:BoldDesc]';  return true;" onmouseout="window.status=''; return true;" />
							<input type="button" class="afButton" accesskey="i" name="afItalics" value="[RESX:Italics]"  style="font-weight:bold;" onclick="insertCode('[i] [/i]');" onmouseover="window.status='[RESX:ItalicsDesc]';  return true;" onmouseout="window.status=''; return true;" />
							<input type="button" class="afButton" accesskey="u" name="afUnderline" value="[RESX:Underline]" style="font-weight:bold;" onclick="insertCode('[u] [/u]');" onmouseover="window.status='[RESX:UnderlineDesc]';  return true;" onmouseout="window.status=''; return true;" />
							<input type="button" class="afButton" accesskey="q" name="afQuote" value="[RESX:Quote]" style="font-weight:bold;" onclick="insertQuote();" onmouseover="window.status='[RESX:QuoteDesc]';  return true;" onmouseout="window.status=''; return true;" />
							<input type="button" class="afButton" accesskey="m" name="afImage" value="[RESX:Image]" style="font-weight:bold;" onclick="insertCode('[img] [/img]');" onmouseover="window.status='[RESX:ImageDesc]';  return true;" onmouseout="window.status=''; return true;" />
							<input type="button" class="afButton" accesskey="c" name="afCode" value="[RESX:Code]" style="font-weight:bold;" onclick="insertCode('[code] [/code]');" onmouseover="window.status='[RESX:CodeDesc]';  return true;" title='[RESX:CodeDesc]' onmouseout="window.status=''; return true;" />
							</div>
							<textarea id="txtBody" name="txtBody" class="aftextbox" style="height:120px" rows="5" cols="250"></textarea></td>
						<td valign="top">
						</td>
					</tr>
				</table>
				<table width="100%" cellspacing="0" cellpadding="4">	
				[AF:UI:ANON]
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
				[/AF:UI:ANON]
				</table>
				<table width="100%" cellspacing="0" cellpadding="4">	
					<tr>
						<td align="right" colspan="4"><div id="divSubscribe" runat="server"></td>
					</tr>
				</table>
				<table width="100%" cellspacing="0" cellpadding="4">	
					<tr>
						<td align="center" colspan="3">
							<div class="amtbwrapper" style="text-align:center;">
								<div style="margin:0px auto;min-width:50px;max-width:60px;">
									[AF:BUTTON:SUBMIT]
								</div>   
							</div>
						</td>
					</tr>
				</table>
			</div>
		</td>
	</tr>
</table>