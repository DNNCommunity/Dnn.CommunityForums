<div class="dcf-likes-wrap">
    <asp:Panel ID="pnlMessage" runat="server" Visible="true" CssClass="dcf-likes-noresults">
        <asp:literal id="litMessage" runat="server" />
    </asp:panel>
    <div class="dcf-likes-results" style="position: relative;">
        <div class="dcf-likes-header">
            <am:pagernav id="PagerTop" runat="server" />
        </div>
        <asp:repeater runat="server" id="rptLikes" visible="False">
            <HeaderTemplate>
                <div class="dcf-likes-header">
                    <h1 class="dcf-likes-title">[RESX:Likes] [RESX:For]</h1>
                    <h2>[FORUMPOST:SUBJECTLINK|<a href="{0}" class="dcf-topic-link">[FORUMPOST:SUBJECT]</a>]</h2>
                    <label>[FORUMPOST:SUMMARY:50]</label>
                    <div class="dcf-postinfo">
                        <label>[RESX:posted] [FORUMPOST:DATECREATED] [RESX:BY] [FORUMPOST:AUTHORDISPLAYNAMELINK|<a href="{0}" class="dcf-profile-link" rel="nofollow">[FORUMPOST:AUTHORDISPLAYNAME]</a></label>]
                    </div>
                    <div class="dcf-postinfo">
                        <label>[RESX:likedby]:</label>
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate>
                <div class="dcf-like">
                    <div class="dcf-like-content">
                        [FORUMLIKE:USERDISPLAYNAMELINK|<a href="{0}" class="dcf-profile-link">[FORUMLIKE:USERDISPLAYNAME]</a>]
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
            </FooterTemplate>
        </asp:repeater>
        <div class="dcf-likes-footer">
            <am:pagernav id="PagerBottom" runat="server" />
        </div>
    </div>
</div>
