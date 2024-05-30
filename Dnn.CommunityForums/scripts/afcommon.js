function af_loadComplete() {
    window.scrollTo(0, 0);
};
function afreload() {
    af_showLoad();
    document.location.reload(true);
};
var amaf_timerID = null;
var amaf_timerRunning = false;
var amaf_runtime = 0;
function amaf_startTimer() {
    amaf_runtime = amaf_runtime + 1;
    amaf_timerRunning = true;
    amaf_timerID = setTimeout('amaf_startTimer()', 1);

};
function amaf_stopTimer() {
    if (amaf_timerRunning) {
        clearTimeout(amaf_timerID);
    };
    amaf_timerRunning = false;
};
function amaf_trackTime() {
    amaf_runtime = 0;
    amaf_stopTimer();
    amaf_startTimer();
};
var amaf = {
    callback: function (data, cb) {
        amaf_trackTime();
        var http = new XMLHttpRequest();
        var url = afHandlerURL;
        http.open('POST', url, true);
        http.setRequestHeader('content-type', 'application/x-www-form-urlencoded');
        http.onreadystatechange = function () {
            if (http.readyState == 4 && http.status == 200) {
                try {
                    var result = http.responseText;
                    amaf_stopTimer();
                    if (result.indexOf('[') == 0 || result.indexOf('{') == 0) {
                        result = JSON.parse(result);
                        if (typeof (result[1]) != 'undefined') {
                            amaf_handleDebug(result[1]);
                        };
                    }


                    if (cb != null) {
                        cb(result);
                    };
                } catch (err) {
                    amaf_stopTimer();
                    alert(err.message + '\n' + http.responseText);
                };
            };
        };
        data = JSON.stringify(data);
        http.send(data);
    }
};
function amaf_updateuseronline(mid) {
    var sf = $.ServicesFramework(mid);
    $.ajax({
        type: "POST",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/User/UpdateUserIsOnline',
        beforeSend: sf.setModuleHeaders
    })
};
function amaf_uo(mid) {
    var sf = $.ServicesFramework(mid);
    $.ajax({
        type: "GET",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/User/GetUsersOnline',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
            var u = document.getElementById('af-usersonline');
            u.innerHTML = data;
    }).fail(function (xhr, status) {
            alert('error getting users online');
    });
};
function amaf_topicSubscribe(mid, fid, tid) {
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid,
        topicId: tid
    };
    /* can only subscribe using API if existing topic; new topic will be handled in code-behind when saving the topic */
    if (tid > 0) {
        $.ajax({
            type: "POST",
            data: JSON.stringify(params),
            contentType: "application/json",
            dataType: "json",
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Subscribe',
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            amaf_UpdateTopicSubscriberCount(mid, fid, tid);
            $('input[type=checkbox].amaf-chk-subs')
                .prop('checked', data);
        }).fail(function (xhr, status) {
            alert('error subscribing to topic');
        });
    }
};
function amaf_UpdateTopicSubscriberCount(mid, fid, tid) {
    var u = document.getElementById('af-topicview-topicsubscribercount');
    if (u != null) {
        var sf = $.ServicesFramework(mid);
        $.ajax({
            type: "GET",
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/SubscriberCount?forumId=' + fid + '&topicId=' + tid,
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            u.innerHTML = data;
        }).fail(function (xhr, status) {
            alert('error updating topic subscriber count');
        });
    }
};
function amaf_forumSubscribe(mid, fid) {
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Forum/Subscribe',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        amaf_UpdateForumSubscriberCount(mid, fid);
        $('input[type=checkbox].amaf-chk-subs')
            .prop('checked', data);
        $('img#amaf-sub-' + fid).each(function () {
            var imgSrc = $(this).attr('src');
            if (data) {
                $(this).attr('src', imgSrc.replace(/email_unchecked/, 'email_checked'));
            }
            else {
                $(this).attr('src', imgSrc.replace(/email_checked/, 'email_unchecked'));
            }
        });
    }).fail(function (xhr, status) {
        alert('error subscribing to forum');
    });
};
function amaf_UpdateForumSubscriberCount(mid, fid) {
    var u = document.getElementById('af-topicsview-forumsubscribercount');
    if (u != null) {
        var sf = $.ServicesFramework(mid);
        $.ajax({
            type: "GET",
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Forum/SubscriberCount?forumId=' + fid,
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            u.innerHTML = data;
        }).fail(function (xhr, status) {
            alert('error updating forum subscriber count');
        });
    }
};
function amaf_ChangeTopicRating(mid, fid, tid, rating) {
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid,
        topicId: tid
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + '/API/ActiveForums/Topic/Rate?rating=' + rating,
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        var r = document.getElementById('af-rater');
        var rate = data;
        var rv = document.getElementById('af-rate-value');
        rv.value = rate;
        if (typeof (r) != 'undefined') {
            r.className = 'fa-rater fa-rate' + rate;
        };
    }).fail(function (xhr, status) {
        alert('error updating topic rating');
    });
};
function amaf_hoverRate(obj, r) {
    var p = obj.parentNode;
    var rv = document.getElementById('af-rate-value');
    if (typeof (r) == 'undefined') {
        r = rv.value;
    };
    p.className = 'fa-rater fa-rate' + r;
};
function amaf_MarkAsAnswer(mid, fid, tid, rid) {
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid,
        topicId: tid,
        replyId: rid
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Reply/MarkAsAnswer',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        afreload();
    }).fail(function (xhr, status) {
        alert('error marking as answer');
    });
};
function amaf_loadSuggest(field, prepop, type) {
    if (typeof (type) == 'undefined') {
        type = -1;
    };
    if (prepop !== null) {
        prepop = [prepop];
    };
    var url = afHandlerURL + '&action=11';
    jQuery("#" + field).tokenInput(url, {
        tokenLimit: 100, prePopulate: prepop,
        classes: {
            tokenList: "token-input-list-facebook",
            token: "token-input-token-facebook",
            tokenDelete: "token-input-delete-token-facebook",
            selectedToken: "token-input-selected-token-facebook",
            highlightedToken: "token-input-highlighted-token-facebook",
            dropdown: "token-input-dropdown-facebook",
            dropdownItem: "token-input-dropdown-item-facebook",
            dropdownItem2: "token-input-dropdown-item2-facebook",
            selectedDropdownItem: "token-input-selected-dropdown-item-facebook",
            inputToken: "token-input-input-token-facebook"
        }
    });
};
function amaf_postDel(mid, fid, tid, rid) {
    if (confirm(amaf.resx.DeleteConfirm)) {
        var sf = $.ServicesFramework(mid);
        $.ajax({
            type: "DELETE",
            url: dnn.getVar("sf_siteRoot", "/") + (rid > 0 ? 'API/ActiveForums/Reply/Delete?forumId=' + fid + '&replyId=' + rid : 'API/ActiveForums/Topic/Delete?forumId=' + fid + '&topicId=' + tid),
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            afreload();
        }).fail(function (xhr, status) {
            alert('error deleting post');
        });
    };
};
function amaf_likePost(mid, fid, cid) {
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid,
        contentId: cid
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Like/Like',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        $('#af-topicview-likes1-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        $('#af-topicview-likes2-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        $('#af-topicview-likes3-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
    }).fail(function (xhr, status) {
        alert('error liking post');
    });
};



$(document).ready(function () {
    $('.dcf-collapsible').each(function (i, obj) { dcf_collapsible_showOrHideByCollapsibleId(obj.id) });
});
function dcf_collapsible_toggle(targetName) {
    var cookieName = dcf_collapsible_getCookieName(targetName);
    var showCollapsible = dcf_getCookieParam(cookieName);
    if (showCollapsible == 'T' || showCollapsible == '') {
        dcf_setCookieParam(cookieName, 'F', 30);
    }
    else {
        dcf_setCookieParam(cookieName, 'T', 30);
    }
    dcf_collapsible_showOrHideTarget(targetName);
};
function dcf_collapsible_showOrHideByCollapsibleId(id) {
    dcf_collapsible_showOrHideTarget(dcf_collapsible_removeIdPrefix(id));
};
function dcf_collapsible_showOrHideTarget(targetName) {
    var cookieName = dcf_collapsible_getCookieName(targetName);
    var targetElement = eval(document.getElementById(targetName));
    var showCollapsible = dcf_getCookieParam(cookieName);
    if (showCollapsible == 'T') {
        targetElement.style.display = '';
        $(dcf_collapsible_getElementId(targetName)).removeClass(dcf_collapsible_getCssClassClosed()).addClass(dcf_collapsible_getCssClassOpened());
        $(dcf_collapsible_getElementId(targetName)).children().removeClass(dcf_collapsible_getFaCssClassClosed()).addClass(dcf_collapsible_getFaCssClassOpened());
        dcf_setCookieParam(cookieName, 'T', 30);
    }
    else if (showCollapsible == 'F') {
        targetElement.style.display = 'none';
        $(dcf_collapsible_getElementId(targetName)).removeClass(dcf_collapsible_getCssClassOpened()).addClass(dcf_collapsible_getCssClassClosed());
        $(dcf_collapsible_getElementId(targetName)).children().removeClass(dcf_collapsible_getFaCssClassOpened()).addClass(dcf_collapsible_getFaCssClassClosed());
        dcf_setCookieParam(cookieName, 'F', 30);
    }
};
function dcf_collapsible_getFaCssClassOpened() {
    return 'fa-chevron-down';
};
function dcf_collapsible_getFaCssClassClosed() {
    return 'fa-chevron-left';
};
function dcf_collapsible_getCssClassOpened() {
    return 'dcf-collapsible-opened';
};
function dcf_collapsible_getCssClassClosed() {
    return 'dcf-collapsible-closed';
};
function dcf_collapsible_getElementId(targetName) {
    return '#dcf-collapsible-' + targetName;
};
function dcf_collapsible_removeIdPrefix(id) {
    return id.replace('dcf-collapsible-', '');
};
function dcf_collapsible_getCookieName(targetName) {
    return 'dcf-collapsible-' + targetName + '-show';
};
function dcf_setCookieParam(name, value, days) {
    var date = new Date();
    date.setDate(date.getDate() + 30);
    var expires = "; expires=" + date.toGMTString();
    document.cookie = name + "=" + value + expires + "; path=/";
};
function dcf_getCookieParam(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
    return "";
};