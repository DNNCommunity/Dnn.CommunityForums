<style>


.dcf-drop-down-menu {position: relative;display: inline-block;}
.dcf-drop-down-menu-button {
  border: none;
  cursor: pointer;
}

/* Dropdown Content (Hidden by Default) */
.dcf-drop-down-menu-content {
  display: none;
  position: absolute;
  background-color: #f9f9f9;
  width:180px;
  box-shadow: 0px 8px 16px 0px rgba(0,0,0,0.2);
  z-index: 1;
}

/* Links inside the dropdown */
.dcf-drop-down-menu-content a {padding: 2px 2px;text-decoration: none; display: block;
}

.dcf-drop-down-menu-content a:hover {background-color: #f1f1f1}
.dcf-drop-down-menu:hover .dcf-drop-down-menu-button {background-color: #7da8cc;}
.dcf-drop-down-menu:hover .dcf-drop-down-menu-content {display: block;}

</style>

<div class="dcf-toolbars">
    <div class="dcf-toolbar dcf-toolbar-user">
        <ul>
            <li>[AF:TB:Forums]</li>
            <li>
                <div class="dcf-drop-down-menu">
                    <button class="dcf-drop-down-menu-button"><i class="fa fa-filter fa-fw fa-grey"></i><span class="dcf-link-text">[RESX:Filters]</span><i class="fa fa-chevron-down fa-fw fa-grey"></i></button>
                    <div class="dcf-drop-down-menu-content">
                        [AF:TB:MyTopics][AF:TB:NotRead][AF:TB:Unanswered][AF:TB:Unresolved][AF:TB:Announcements][AF:TB:ActiveTopics][AF:TB:MostLiked][AF:TB:MostReplies]
                    </div>
                </div>
            </li>
        </ul>
    </div>
    <div class="dcf-toolbar dcf-toolbar-manage">
        <ul>
            <li>[AF:TB:ModList]</li>
            <li>[AF:TB:MySettings]</li>
            <li>[AF:TB:MySubscriptions]</li>
            <li>[AF:TB:ControlPanel]</li>
            <li>[AF:TB:Search]</li>
        </ul>
    </div>
</div>
