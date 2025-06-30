<!-- email notification template -->
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <style type="text/css">
            /* Reset Styles */
        html, body{height:100%;width:100%;}
        body{
            margin:0; padding:0;
            font-family:sans-serif;
            font-size:0.83em;
            color:#333;
        }
        img{border:0; height:auto; line-height:100%; outline:none; text-decoration:none;}
        table td{border-collapse:collapse;}
        p{margin: 0 0 1.6em 0;}
        a{color:#417CD3;}
        #backgroundTable{height:100% !important; margin:0; padding:0; width:100% !important;}
        body{ background-color:#fafafa; }        

        #templateContainer{
            border: 1px solid #eeeeee;
            box-shadow: 0px 0px 3px 0px #eee;
        }

            /* Pre Header Styles */
        .preheaderContent{
            padding:15px 0 5px 0;
            font-size:0.9em;
            color:#999;
            border-top: 5px solid #417CD3;
        }
        .headerContent {background-color:#bbb;}
        .headerContent a{display:block;}
        .headerContent img{ margin-bottom:0;}

            /* Sub Header Styles */
        #templateSubHeader{ color:#ffffff; }
        #templateSubHeader td{ padding:5px 20px; }

        
            /* Template Footer Styles */
        .footerContent table td{
            padding-top:20px;
            border-top:1px solid #ddd;
            font-size:0.8em;
        }


            /* Body Template Header Styles */
        #bodyTemplateHeader{
            padding:15px 0 5px 0;
            font-size:0.9em;
            color:#999;
        }
            /* Body Template Footer Styles */
        #bodyTemplateFooter{
            padding:15px 0 5px 0;
            font-size:0.9em;
            color:#999;
        }
		
		
	.dcf-avatar {
  padding: 0.5rem;
}
@media screen and (min-width: 992px) {
  .dcf-avatar {
    padding: 1rem;
  }
}
.dcf-avatar-img-wrap {
  position: relative;
}
.dcf-avatar-img-wrap .dcf-avatar-img {
  position: absolute;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  border-radius: 50%;
  overflow: hidden;
}
    </style>
</head>

<body leftmargin="0" marginwidth="0" topmargin="0" marginheight="0" offset="0">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" height="100%" id="backgroundTable">
        <tr>
            <td align="center" valign="top">
                <!-- // Begin Template Preheader \\ -->
                <table border="0" cellpadding="0" cellspacing="0" id="templatePreheader" width="600">
                    <tr align="right">
                        <td valign="top" class="preheaderContent">

                            <!-- // Begin Module: Standard Preheader \ -->
                            <table border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td align="right"></td>
                                </tr>
                            </table>
                            <!-- // End Module: Standard Preheader \ -->
                        </td>
                    </tr>
                </table>
                <!-- // End Template Preheader \\ -->
                <!-- // Begin Template body \\ -->
                <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateContainer" style="border: 1px solid #eeeeee;">
                    <tr>
                        <td align="center" valign="top">
                            <!-- // Begin Template Header \\ -->
                            <table border="0" cellpadding="30" cellspacing="0" width="600" id="templateHeader">
                                <tr>
                                    <td class="headerContent">
                                        <a href="[PORTAL:URL]">
                                            <img src="[PORTAL:SCHEME]://[PORTAL:URL][PORTAL:HOMEDIRECTORY][PORTAL:LOGOFILE]" style="max-width: 600px;" id="headerImage campaign-icon" width="200" />
                                        </a>

                                    </td>
                                </tr>
                            </table>
                            <!-- // End Template Header \\ -->
                        </td>
                    </tr>
                    <tr>
                        <td align="center" valign="top">
                            <!-- // Begin Template Sub Header \\ -->
                            <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateSubHeader">
                                <tr>
                                    <td class="subHeaderContent" bgcolor="#417CD3" width="66%" align="left">
                                        <h2 style="color: #fff; font-weight: 100; font-size: 24px;">Notifications</h2>
                                    </td>
                                    <!--close subHeaderContent-->
                                    <td width="33%" bgcolor="#417CD3" align="right">
                                        <p style="color: #fff;">
                                        </p>
                                    </td>
                                </tr>
                            </table>
                            <!-- // End Template Sub Header \\ -->
                        </td>
                    </tr>
                    <tr>
                        <td align="center" valign="top" bgcolor="#ffffff">
                            <!-- // Begin Template Body \\ -->
                            <table border="0" cellpadding="0" cellspacing="0" width="600" id="templateBody">
                                <tr>
                                    <td valign="top" align="right">
                                        <strong>[RESX:EmailNotificationGreeting]</strong>
                                        <br />
                                        [RESX:Subject]: [FORUMPOST:SUBJECT]
									<br />
                                        [RESX:SearchBy] [FORUMPOST:AUTHORDISPLAYNAMELINK|<a href="{0}" class="af-profile-link" rel="nofollow">[FORUMPOST:AUTHORDISPLAYNAME]</a>|[FORUMPOST:AUTHORDISPLAYNAME]]
									<br />
                                        [RESX:SearchPosted] [FORUMPOST:DATECREATED]
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <table border="0" cellpadding="30" cellspacing="0" width="100%">
                                            <tr>
                                                <td valign="top">
                                                    <table width="80" border="0" cellspacing="0" cellpadding="8" align="left">
                                                        <tr>
                                                            <td>[FORUMAUTHOR:USERPROFILELINK|<a href="{0}" class="af-profile-link" rel="nofollow">[FORUMAUTHOR:AVATAR:60]<br />
                                                                [FORUMAUTHOR:DISPLAYNAME]</a>|[FORUMAUTHOR:DISPLAYNAME]]
                                                            </td>
                                                        </tr>
                                                    </table>

                                                    <table width="350" border="0" cellspacing="0" cellpadding="8" align="left">
                                                        <tr>
                                                            <td>[FORUMPOST:BODY]
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                        <!-- // End Module: Standard Content \\ -->
                                    </td>
                                </tr>
                            </table>
                            <!-- // End Template Body \\ -->
                        </td>
                    </tr>
                    <tr>
                        <td align="center" valign="top">
                            <table border="0" cellpadding="30" cellspacing="0" width="600" id="templateFooter">
                                <tr>
                                    <td valign="top" class="footerContent">
                                        <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                            <tr>
                                                <td align="center">
                                                    <p style="font-size: 12pt;">
                                                        [RESX:EmailNotificationVisitLinkMessage]
                                                        <br />
                                                        [FORUMPOST:LINK]
                                                    </p>
                                                    <p style="font-size: 12pt;">
                                                        [RESX:EmailNotificationMySettingsLinkText] [DCF:TOOLBAR-MYSUBSCRIPTIONS-ONCLICK|<a href="{0}" rel="nofollow">[RESX:MySubscriptions]</a>.]
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                <!-- // End Template Body \\ -->
                <!-- // Begin Template footer \\ -->
                <table border="0" cellpadding="0" cellspacing="0" id="bodyTemplateFooter" width="600">
                    <tr>
                        <td valign="top" class="bodyFooterContent">

                            <!-- // Begin Module: Standard Preheader \ -->
                            <table border="0" cellpadding="0" cellspacing="0" align="center">

                                <tr>
                                    <td align="center">

                                        <p>
                                            [RESX:ThankYou]<br />
                                            [PORTAL:PORTALNAME]
                                        </p>
                                        <br />
                                        <p><small>[PORTAL:FOOTERTEXT]</small></p>
                                    </td>
                                </tr>
                            </table>
                            <!-- // End Module: Standard Preheader \ -->
                        </td>
                    </tr>
                </table>
                <!-- // End Template footer \\ -->
            </td>
        </tr>
    </table>
    <!-- wrapper table -->
</body>
</html>
