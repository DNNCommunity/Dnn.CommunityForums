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
            $('input[type=checkbox].amaf-chk-subs')
                .prop('checked', data);
            if (data === true) {
                $('input[type=button].dcf-btn-subs').prop('value', amaf.resx.Unsubscribe).toggleClass('dnnPrimaryAction').toggleClass('dnnSecondaryAction');
            } 
            else { 
                $('input[type=button].dcf-btn-subs').prop('value', amaf.resx.Subscribe).toggleClass('dnnPrimaryAction').toggleClass('dnnSecondaryAction');
            }
            amaf_UpdateTopicSubscriberCount(mid, fid, tid);
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
        $('input[type=checkbox].amaf-chk-subs')
            .prop('checked', data);
        if (data === true) {
            $('input[type=button].dcf-btn-subs').prop('value', amaf.resx.Unsubscribe).toggleClass('dnnPrimaryAction').toggleClass('dnnSecondaryAction');
        } 
        else { 
            $('input[type=button].dcf-btn-subs').prop('value', amaf.resx.Subscribe).toggleClass('dnnPrimaryAction').toggleClass('dnnSecondaryAction');
        }
        $('img#amaf-sub-' + fid).each(function () {
            var imgSrc = $(this).attr('src');
            if (data) {
                $(this).attr('src', imgSrc.replace(/email_unchecked/, 'email_checked'));
            }
            else {
                $(this).attr('src', imgSrc.replace(/email_checked/, 'email_unchecked'));
            }
        });
        amaf_UpdateForumSubscriberCount(mid, fid);
    }).fail(function (xhr, status) {
        alert('error subscribing to forum');
    });
};
function amaf_badgeAssign(mid, bid, uid, userBadgeId, assign) {
    var sf = $.ServicesFramework(mid);
    var params = {
        badgeId: bid,
        userId: uid,
        userBadgeId: userBadgeId,
        assign: assign
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/UserBadge/Assign',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
    }).fail(function (xhr, status) {
        alert('error assigning badge');
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
function amaf_topicDel(mid, fid, tid) {
    if (confirm(amaf.resx.DeleteConfirm)) {
        var sf = $.ServicesFramework(mid);
        $.ajax({
            type: "DELETE",
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Delete?forumId=' + fid + '&topicId=' + tid,
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            afreload();
        }).fail(function (xhr, status) {
            alert('error deleting topic');
        });
    };
};
function amaf_topicRestore(mid, fid, tid) {
    if (confirm(amaf.resx.RestoreConfirm)) {
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
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Restore',
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            afreload();
        }).fail(function (xhr, status) {
            alert('error restoring topic');
        });
    };
};
function amaf_replyRestore(mid, fid, tid, rid) {
    if (confirm(amaf.resx.RestoreConfirm)) {
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
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Reply/Restore',
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            afreload();
        }).fail(function (xhr, status) {
            alert('error restoring reply');
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
        $('#af-topicview-likes-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        /* TODO: these are for backward compatibility and can be removed in v10 */
        $('#af-topicview-likes1-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        $('#af-topicview-likes2-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        $('#af-topicview-likes3-' + cid).toggleClass('fa-thumbs-up').toggleClass('fa-thumbs-o-up').text(" " + data);
        /* TODO: these are for backward compatibility and can be removed in v10 */
    }).fail(function (xhr, status) {
        alert('error liking post');
    });
};



$(document).ready(function () {
    $('.dcf-collapsible').each(function (i, obj) { dcf_collapsible_showOrHideByCollapsibleId(obj.id) });
});
function dcf_collapsible_toggle(targetName) {
    var cookieName = dcf_collapsible_getCookieName(targetName);
    var targetElement = eval(document.getElementById(targetName));
    var isCollapsed = targetElement.style.display === 'none' ? true : false;
    var showCollapsible = dcf_getCookieParam(cookieName);
    if (showCollapsible === '') {
        showCollapsible = isCollapsed === true ? 'F' : 'T';
    }
    dcf_setCookieParam(cookieName, showCollapsible === 'T' ? 'F' : 'T', 30);
    dcf_collapsible_showOrHideTarget(targetName);
};
function dcf_collapsible_showOrHideByCollapsibleId(id) {
    dcf_collapsible_showOrHideTarget(dcf_collapsible_removeIdPrefix(id));
};
function dcf_collapsible_showOrHideTarget(targetName) {
    var cookieName = dcf_collapsible_getCookieName(targetName);
    var targetElement = eval(document.getElementById(targetName));
    var showCollapsible = dcf_getCookieParam(cookieName);
    if (showCollapsible === 'T') {
        targetElement.style.display = '';
        $(dcf_collapsible_getElementId(targetName)).removeClass(dcf_collapsible_getCssClassClosed()).addClass(dcf_collapsible_getCssClassOpened());
        $(dcf_collapsible_getElementId(targetName)).children().removeClass(dcf_collapsible_getFaCssClassClosed()).addClass(dcf_collapsible_getFaCssClassOpened());
        dcf_setCookieParam(cookieName, 'T', 30);
    }
    else if (showCollapsible === 'F') {
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


function amaf_Pin(mid, fid, tid) {
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
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Pin',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        $('#af-topicsview-pin-' + tid).toggleClass('fa-thumb-tack');
        $('#dcf-topic-pin-inner-' + tid).toggleClass('dcf-topic-pin-pin').toggleClass('dcf-topic-pin-unpin');
        if (data == true) {
            /* pinned --change icon and text to 'unpin' */
            $('#dcf-topic-pin-text-' + tid).text(amaf.resx.UnPin);
            $('#dcf-topic-pin-outer-' + tid).attr("onclick", "javascript:if (confirm('" + amaf.resx.UnPinConfirm + "')) { amaf_Pin(" + mid + ", " + fid + "," + tid + "); };");
            $('#dcf-topic-pin-outer-' + tid).attr("title", amaf.resx.UnPinTopic);
            $('#dcf-topic-pin-inner-' + tid).attr("title", amaf.resx.UnPinTopic);
        }
        else {
            /* unpinned --change icon and text to 'pin' */
            $('#dcf-topic-pin-text-' + tid).text(amaf.resx.Pin);
            $('#dcf-topic-pin-outer-' + tid).attr("onclick", "javascript:if (confirm('" + amaf.resx.PinConfirm + "')) { amaf_Pin(" + mid + ", " + fid + "," + tid + "); };");
            $('#dcf-topic-pin-outer-' + tid).attr("title", amaf.resx.PinTopic);
            $('#dcf-topic-pin-inner-' + tid).attr("title", amaf.resx.PinTopic);
        }
    }).fail(function (xhr, status) {
        alert('error pinning post');
    });
};
function amaf_Lock(mid, fid, tid) {
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
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Lock',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        $('#af-topicsview-lock-' + tid).toggleClass('fa-lock');
        $('#dcf-topic-lock-inner-' + tid).toggleClass('dcf-topic-lock-lock').toggleClass('dcf-topic-lock-unlock').toggleClass('fa-lock').toggleClass('fa-unlock');
        $('#dcf-topic-reply-link-' + tid).toggleClass('dcf-topic-reply-locked').toggleClass('dcf-topic-reply-unlocked'); /* enable/disable reply button */
        if (data == true) {
            /* locked -change icon and text to 'unlock'; hide quick reply and add message 'topic is locked'*/
            $('.dcf-quickreply-wrapper').css('display', 'none');
            $('#dcf-topic-lock-locked-label-' + tid).text(amaf.resx.TopicLocked)
            $('#dcf-topic-lock-text-' + tid).text(amaf.resx.UnLock);
            $('#dcf-topic-lock-outer-' + tid).attr("onclick", "javascript:if (confirm('" + amaf.resx.UnLockConfirm + "')) { amaf_Lock(" + mid + ", " + fid + "," + tid + "); };");
            $('#dcf-topic-lock-outer-' + tid).attr("title", amaf.resx.UnLockTopic);
            $('#dcf-topic-lock-inner-' + tid).attr("title", amaf.resx.UnLockTopic);
        }
        else {
            /* unlocked -change icon and text to lock; show quick reply and remove 'topic is locked' message */
            $('.dcf-quickreply-wrapper').css('display', 'block');
            $('#dcf-topic-lock-locked-label-' + tid).text('')
            $('#dcf-topic-lock-text-' + tid).text(amaf.resx.Lock);
            $('#dcf-topic-lock-outer-' + tid).attr("onclick", "javascript:if (confirm('" + amaf.resx.LockConfirm + "')) { amaf_Lock(" + mid + ", " + fid + "," + tid + "); };");
            $('#dcf-topic-lock-outer-' + tid).attr("title", amaf.resx.LockTopic);
            $('#dcf-topic-lock-inner-' + tid).attr("title", amaf.resx.LockTopic);
        }
    }).fail(function (xhr, status) {
        alert('error locking post');
    });
};
function amaf_openMove(mid, fid, tid) {
    $('#aftopicmove-topicid').val(tid);
    $('#aftopicmove-moduleid').val(mid);
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Forum/ListForHtml',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        am.UI.LoadDiv('aftopicmove');
        $('#drpForums').empty().append($(data));
        amaf_loadForMove(mid, fid, tid);
    }).fail(function (xhr, status) {
        alert('error moving post');
    });
};
function amaf_loadForMove(mid, fid, tid) {
    var sf = $.ServicesFramework(mid);
    $.ajax({
        type: "GET",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Load?forumId=' + fid + '&topicId=' + tid,
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        amaf_bindLoadMoveTopic(data);
    }).fail(function (xhr, status) {
        alert('error moving post');
    });
};
function amaf_bindLoadMoveTopic(data) { 
    var t = data;
    $('#aftopicmove-topicid').val(t.TopicId);
    $('#aftopicmove-subject').val(t.Subject);
    $('#aftopicmove-currentforum').val(t.ForumName);
};
function amaf_modMove(mid, fid, tid) {
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
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Move',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        afreload();
    }).fail(function (xhr, status) {
        alert('error moving post');
    });
};
function amaf_quickEdit(mid, fid, tid) {
    amaf_resetQuickEdit();
    am.UI.LoadDiv('aftopicedit');
    var sf = $.ServicesFramework(mid);
    $.ajax({
        type: "GET",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Load?forumId=' + fid + '&topicId=' + tid,
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        amaf_loadTopicComplete(data);
    }).fail(function (xhr, status) {
        alert('error loading post');
    });
};
function amaf_resetQuickEdit() {
    document.getElementById('aftopicedit-topic').value = '';
    document.getElementById('aftopicedit-moduleid').value = '';
    document.getElementById('aftopicedit-forumid').value = '';
    document.getElementById('aftopicedit-topicid').value = '';
    document.getElementById('aftopicedit-subject').value = '';
    document.getElementById('aftopicedit-tags').value = '';
    document.getElementById('aftopicedit-priority').value = 0;
    am.Utils.SetSelected('aftopicedit-status', '-1');
    document.getElementById('aftopicedit-locked').checked = false;
    document.getElementById('aftopicedit-pinned').checked = false;
    am.Utils.RemoveChildNodes('catlist');
    am.Utils.RemoveChildNodes('proplist');

};
function amaf_loadTopicComplete(data) {
    var t = data;
    document.getElementById('aftopicedit-topic').value = JSON.stringify(t);
    document.getElementById('aftopicedit-topicid').value = t.TopicId;
    document.getElementById('aftopicedit-forumid').value = t.ForumId;
    document.getElementById('aftopicedit-moduleid').value = t.ModuleId;
    document.getElementById('aftopicedit-subject').value = t.Subject;
    document.getElementById('aftopicedit-tags').value = t.Tags;
    document.getElementById('aftopicedit-priority').value = t.Priority;
    am.Utils.SetSelected('aftopicedit-status', t.StatusId);
    document.getElementById('aftopicedit-locked').checked = t.IsLocked;
    document.getElementById('aftopicedit-pinned').checked = t.IsPinned;
    amaf_loadCatList(t.Categories);
    amaf_loadProperties(t.ForumProperties, t.TopicProperties);
};
function amaf_loadCatList(cats) {
    var iCount = cats.length;
    var ul = document.getElementById('catlist');
    am.Utils.RemoveChildNodes('catlist');
    for (var i = 0; i < iCount; i++) {
        var c = cats[i];
        var li = document.createElement('li');
        li.setAttribute('id', c.id);
        var chk = document.createElement('input');
        chk.setAttribute('id', 'cat-' + c.id);
        chk.setAttribute('type', 'checkbox');
        chk.value = c.id;
        chk.checked = c.selected;
        li.appendChild(chk);
        li.appendChild(document.createTextNode(c.name));
        ul.appendChild(li);
    };
};
function amaf_loadProperties(propdefs, props) {
    if (props != null) {
        var iCount = props.length;
        var ul = document.getElementById('proplist');
        am.Utils.RemoveChildNodes('proplist');
        for (var j = 0; j < propdefs.length; j++) {
            var pd = propdefs[j];
            var li = document.createElement('li');
            li.setAttribute('id', pd.PropertyId);
            var lbl = document.createElement('label');
            lbl.setAttribute('for', 'prop-' + pd.PropertyId);
            lbl.appendChild(document.createTextNode(pd.Name));
            li.appendChild(lbl);
            for (var i = 0; i < iCount; i++) {
                if (props[i].PropertyId === pd.PropertyId) {
                    pd.DefaultValue = props[i].Value;
                };
            };
            switch (pd.DataType) {
                case 'text':
                    var txt = document.createElement('input');
                    txt.setAttribute('id', 'prop-' + pd.PropertyId);
                    txt.setAttribute('type', 'text');
                    txt.value = pd.DefaultValue;
                    li.appendChild(txt);
                    ul.appendChild(li);
                    break;
                case 'yesno':
                    var txt = document.createElement('input');
                    txt.setAttribute('id', 'prop-' + pd.PropertyId);
                    txt.setAttribute('type', 'checkbox');
                    txt.value = pd.DefaultValue;
                    txt.checked = pd.DefaultValue;
                    li.appendChild(txt);
                    ul.appendChild(li);
                    break;
                default:
                    var sel = document.createElement('select');
                    sel.setAttribute('id', 'prop-' + pd.PropertyId);
                    li.appendChild(sel);
                    ul.appendChild(li);
                    am.Utils.FillSelect(pd.listdata, sel);
                    am.Utils.SetSelected(sel, pd.DefaultValue);
            };
        };
    };
};


function amaf_saveTopic() {
    var t = JSON.parse(document.getElementById('aftopicedit-topic').value);
    var mid = document.getElementById('aftopicedit-moduleid').value;
    var fid = document.getElementById('aftopicedit-forumid').value;
    t.TopicId = document.getElementById('aftopicedit-topicid').value;
    t.Subject = document.getElementById('aftopicedit-subject').value;
    t.Tags = document.getElementById('aftopicedit-tags').value;
    t.Priority = document.getElementById('aftopicedit-priority').value;
    var stat = document.getElementById('aftopicedit-status');
    t.StatusId = stat.options[stat.selectedIndex].value;
    t.IsLocked = document.getElementById('aftopicedit-locked').checked;
    t.IsPinned = document.getElementById('aftopicedit-pinned').checked;
    var ul = document.getElementById('proplist');
    var props = ul.getElementsByTagName('li');
    t.TopicProperties = new Array();
    for (var i = 0; i < props.length; i++) {
        var l = props[i];
        var pname = 'prop-' + l.id;
        t.TopicProperties[i] = {};
        t.TopicProperties[i].Name = pname;
        t.TopicProperties[i].PropertyId = l.id;
        var el = document.getElementById(pname);
        if (el.tagName == 'INPUT') {
            if (el.type == 'text') {
                t.TopicProperties[i].Value = el.value;
            } else {
                t.TopicProperties[i].Value = el.checked;
            };
        } else {
            t.TopicProperties[i].Value = el.options[el.selectedIndex].value;
        };
    };
    var ul = document.getElementById('catlist');
    var cats = ul.getElementsByTagName('li');
    t.SelectedCategoriesAsString = '';
    for (var i = 0; i < cats.length; i++) {
        var li = cats[i];
        var chk = document.getElementById('cat-' + li.id);
        if (chk.checked) {
            t.SelectedCategoriesAsString += chk.value + ';';
        };
    };
    var sf = $.ServicesFramework(mid);
    var params = {
        forumId: fid,
        topic: t
    };
    $.ajax({
        type: "POST",
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Update',
        beforeSend: sf.setModuleHeaders
    }).done(function (data) {
        amaf_saveTopicComplete(data);
    }).fail(function (xhr, status) {
        alert('error saving post');
    });
};
function amaf_saveTopicComplete(result) {
    am.UI.CloseDiv('aftopicedit');
    window.location.href = window.location.href;

};
function amaf_splitRestore() {
    var split_topicid = amaf_getParam('splitId');
    if (typeof (split_topicid) != 'undefined') {
        if (split_topicid == current_topicid) {
            var sv = amaf_getParam('splitValue');
            if (sv != '') splitposts = sv.split('|');
            amaf_splitButtons(true);
            return;
        }
    }
    amaf_splitButtons(false);
}

function amaf_splitCheck(el) {
    if (el.checked) {
        if (splitposts.indexOf(el.value) < 0) splitposts.push(el.value);
        var saved_split = splitposts.join('|');
        amaf_setParam('splitValue', saved_split, 0);
    }
    else {
        var index = splitposts.indexOf(el.value);
        splitposts.splice(index, 1);
        var saved_split = splitposts.join('|');
        amaf_setParam('splitValue', saved_split, 0);
    }
};
function amaf_splitCreate(el, tid) {
    amaf_setParam('splitId', tid, 0);
    amaf_setParam('splitValue', '', 0);
    splitposts = new Array();
    amaf_splitButtons(true);
};

function amaf_splitButtons(opt) {
    var btn1 = document.getElementById('dcf-split-buttons-create');
    var btn2 = document.getElementById('dcf-split-buttons');
    if (typeof (btn1) == 'undefined' || typeof (btn2) == 'undefined') return;
    if (opt) {
        btn1.style.display = 'none';
        btn2.style.display = 'block';
        var objs = am.Utils.GetElementsByClassName('dcf-split-checkbox', 'afgrid');
        for (var i = 0; i < objs.length; i++) {
            objs[i].style.display = 'block';
            if (splitposts.indexOf(objs[i].firstChild.value) > -1) objs[i].firstChild.checked = true;
        };
    }
    else {
        btn1.style.display = 'block';
        btn2.style.display = 'none';
        var objs = am.Utils.GetElementsByClassName('dcf-split-checkbox', 'afgrid');
        for (var i = 0; i < objs.length; i++) {
            objs[i].style.display = 'none';
            objs[i].firstChild.checked = false;
        };
    }
};

function amaf_splitCancel() {
    amaf_setParam('splitId', '', 0);
    amaf_setParam('splitValue', '', 0);
    splitposts = new Array();
    amaf_splitButtons(false);
};

// Prevent double-clicks on topic and forum links, which can create duplicate tracking records
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.dcf-topic-link, .dcf-forum-link').forEach(link => {
        link.addEventListener('click', function () {
            link.style.pointerEvents = 'none';
        });
    });
});
