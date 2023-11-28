<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="profile_mysubscriptions.ascx.cs" Inherits="DotNetNuke.Modules.ActiveForums.profile_mysubscriptions" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls.Internal" %>

<TABLE width="100%" cellpadding="0" cellspacing="0" border="0">
			<TR>
				<TD class="afheader2">&nbsp;&nbsp;
					<asp:Label id="Label26" runat="server" resourcekey="Subscriptions">Subscriptions</asp:Label></TD>
			</TR>
			<TR>
				<TD class="afheader">&nbsp;
					<asp:Label id="Label27" runat="server" resourcekey="ThreadsTracked">Threads you are tracking:</asp:Label>
				</td>
			</tr>
			<tr>
				<td><dnn:DnnGrid ID="dgrdSubs" runat="server" AutoGenerateColumns="false" AllowPaging="True" PageSize="5"  BorderStyle="None" GridLines="None"
						HeaderStyle-CssClass="afheader2" CellPadding="0" CellSpacing="0" 
						DataKeyField="SubscribeID" width="100%">
    <PagerSettings Mode="NumericFirstLast"></PagerSettings>
    <Columns>
        <dnn:DnnGridBoundColumn Visibe HeaderText="<%#DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("SubjectHeader.Text")%>" DataField="Subject" />
        <dnn:DnnGridBoundColumn HeaderText="<%#DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("SubjectHeader.Text")%>" DataField="Subject" />
        <dnn:DnnGridBoundColumn HeaderText="<%#DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("DateHeader.Text")%>" DataField="LastPostDate"  />
        <dnn:DnnGridTemplateColumn>
				        <HeaderTemplate>
					        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
						        <tr>
							        <td><dnn:DnnImage ID="imgTopicSubscriptionDelete" runat="server" IconKey="ActionDelete" resourcekey="TopicSubscriptionDelete" /></td>
						        </tr>
					        </table>
				        </HeaderTemplate>
				        <ItemTemplate>
					        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
						        <tr style="vertical-align: top;">
							        <td><dnn:DnnImageButton ID="btnTopicSubscriptionDelete" runat="server" CommandName="Delete" IconKey="ActionDelete" Text="Delete" resourcekey="TopicSubscriptionDelete" /></td>
						        </tr>
					        </table>
				        </ItemTemplate>
        </dnn:DnnGridTemplateColumn>
    </Columns>
    <EmptyDataTemplate>
        <asp:Label ID="lblNoRecords1" runat="server" resourcekey="NoVersions" />
    </EmptyDataTemplate>
</dnn:DnnGrid>
<%--					<asp:DataGrid id="dgrdSubs" runat="server" CssClass="afgrid" BorderStyle="None" GridLines="None"
						HeaderStyle-CssClass="afheader2" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
						DataKeyField="SubscribeID" width="100%">
						<HeaderStyle CssClass="afheader2"></HeaderStyle>
						<Columns>
							<asp:TemplateColumn>
							    <HeaderTemplate>
							        &nbsp;&nbsp;&nbsp;<%#GetResourceString("SubjectHeader.Text")%>
							    </HeaderTemplate>
								<ItemTemplate>
									&nbsp;&nbsp;&nbsp;<%#DataBinder.Eval(Container.DataItem,"Subject")%>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn>
							    <HeaderTemplate>
							        <%#GetResourceString("DateHeader.Text")%>
							    </HeaderTemplate>
								<ItemTemplate>
									<%#DataBinder.Eval(Container.DataItem,"LastPostDate")%>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:ButtonColumn Text="X" CommandName="Delete"></asp:ButtonColumn>
						</Columns>
					</asp:DataGrid><br>--%>
				</td>
			</tr>
			<tr>
				<td class="afheader">&nbsp;
					<asp:Label id="Label28" runat="server" resourcekey="ForumsTracked">Forums you are tracking:</asp:Label>
				</td>
			</tr>
			<tr>
				<td>	<td>
					<dnn:DnnGrid ID="dgrdForumSubs" runat="server" AutoGenerateColumns="false" AllowPaging="True" PageSize="5"  BorderStyle="None" GridLines="None"
			HeaderStyle-CssClass="afheader2" CellPadding="0" CellSpacing="0" DataKeyField="SubscribeID" width="100%">
 <PagerSettings Mode="NumericFirstLast"></PagerSettings>
 <Columns>
     <dnn:DnnGridBoundColumn HeaderText="<%#DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("ForumHeader.Text")%>" DataField="Name" />
     <dnn:DnnGridTemplateColumn>
	        <HeaderTemplate>
		        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
			        <tr>
				        <td><dnn:DnnImage ID="imgForumSubscriptionDelete" runat="server" IconKey="ActionDelete" resourcekey="ForumSubscriptionDelete" /></td>
			        </tr>
		        </table>
	        </HeaderTemplate>
	        <ItemTemplate>
		        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
			        <tr style="vertical-align: top;">
				        <td><dnn:DnnImageButton ID="btnForumSubscriptionDelete" runat="server" CommandName="Delete" IconKey="ActionDelete" Text="Delete" resourcekey="ForumSubscriptionDelete" /></td>
			        </tr>
		        </table>
	        </ItemTemplate>
     </dnn:DnnGridTemplateColumn>
 </Columns>
 <EmptyDataTemplate>
     <asp:Label ID="Label1" runat="server" resourcekey="NoVersions" />
 </EmptyDataTemplate>
nnweb:DnnGrid>
					<%--<asp:DataGrid id="dgrdForumSubs" runat="server" CssClass="afgrid" BorderStyle="None" GridLines="None"
						HeaderStyle-CssClass="afheader2" CellPadding="0" CellSpacing="0" AutoGenerateColumns="False"
						DataKeyField="ForumSubscribeID" width="100%">
						<HeaderStyle CssClass="afheader2"></HeaderStyle>
						<Columns>
							<asp:TemplateColumn>
							    <HeaderTemplate>
							        &nbsp;&nbsp;&nbsp;<%#GetResourceString("ForumHeader.Text")%>
							    </HeaderTemplate>
								<ItemTemplate>
									&nbsp;&nbsp;&nbsp;<%#DataBinder.Eval(Container.DataItem,"Name")%>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:ButtonColumn Text="X" CommandName="Delete"></asp:ButtonColumn>
						</Columns>
					</asp:DataGrid>--%></TD>
			</TR>
		</TABLE>