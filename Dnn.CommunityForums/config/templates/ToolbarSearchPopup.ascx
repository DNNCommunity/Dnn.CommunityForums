<script type="text/javascript">
    //prevent submit from Enter key
    $(document).ready(function () {
        $('.dcf-search-input').keydown(function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                return false;
            }
        });
    });
    function CheckEnterPressed(a, e) {
        if (e.keyCode === 13) {
            // Enter pressed
            document.getElementById('btnSearch').click();
        }
    };
</script>
<div class="dcf-quick-search aftb-search" data-searchurl="[DCF:TOOLBAR-SEARCHURL]">
    <span class="dcf-search-link aftb-search-link">
        <span>
            <i class="fa fa-search fa-fw fa-blue"></i>[DCF:TOOLBAR-SEARCHTEXT|<span class="dcf-search-link-text">{0}</span>]
        </span>
        <span class="ui-icon ui-icon-triangle-1-s"></span>
    </span>
    <div class="dcf-search-popup aftb-search-popup">
        <div class="dcf-search-input">
            <input class="dcf-search-input" type="text" placeholder="[RESX:SearchFor]" maxlength="50" onkeydown="CheckEnterPressed(this, event)">
            <button id="btnSearch" type="submit" class="dcf-search-button">[RESX:Search]</button>
        </div>
        <div class="dcf-search-options">
            [DCF:TOOLBAR-SEARCHURL|<a class="dcf-search-option-advanced" href="{0}">[RESX:SearchAdvanced]</a>]
            <span class="dcf-search-option-topics">
                <input type="radio" name="afsrt" value="0" checked="checked" /><span class="dcf-search-option-text">[RESX:SearchByTopics]</span>
            </span>
            <span class="dcf-search-option-posts">
                <input type="radio" name="afsrt" value="1" /><span class="dcf-search-option-text">[RESX:SearchByPosts]</span>
            </span>
        </div>
    </div>
</div>
