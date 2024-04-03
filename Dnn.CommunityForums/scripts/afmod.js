function amaf_modDel(mid, fid, tid) {
    if (confirm(amaf.resx.DeleteConfirm)) {
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
            url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Topic/Delete',
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            afreload();
        }).fail(function (xhr, status) {
            alert('error deleting topic');
        });
    };
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
    var t = data[0];
    $('#aftopicmove-topicid').val(t.TopicId);
    $('#aftopicmove-subject').val(t.Content.Subject);
    $('#aftopicmove-currentforum').val(t.Forum.ForumName);
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
function amaf_modPin(mid, fid, tid) {
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
        if (data == true) { 
            $('#af-topic-pin-a-' + tid)
                .attr("title", amaf.resx.UnPinTopic)
                .attr("onclick", "javascript:if (confirm('" + amaf.resx.UnPinConfirm + "')) { amaf_modPin(" + mid + ", " + fid + "," + tid + "); };")
                ;
        }
        else {
            $('#af-topic-pin-a-' + tid)
                .attr("title", amaf.resx.PinTopic)
                .attr("onclick", "javascript:if (confirm('" + amaf.resx.PinConfirm + "')) { amaf_modPin(" + mid + ", " + fid + "," + tid + "); };")
                ;
        }
    }).fail(function (xhr, status) {
        alert('error pinning post');
    });
};
function amaf_modLock(mid, fid, tid) {
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
        if (data == true) {
            $('#af-topic-lock-a-' + tid)
                .attr("title", amaf.resx.UnLockTopic)
                .attr("onclick", "javascript:if (confirm('" + amaf.resx.UnLockConfirm + "')) { amaf_modLock(" + mid + ", " + fid + "," + tid + "); };")
                ;
        }
        else {
            $('#af-topic-lock-a-' + tid)
                .attr("title", amaf.resx.LockTopic)
                .attr("onclick", "javascript:if (confirm('" + amaf.resx.LockConfirm + "')) { amaf_modLock(" + mid + ", " + fid + "," + tid + "); };")
                ;
        }
    }).fail(function (xhr, status) {
        alert('error locking post');
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
    var t = data[0];
    document.getElementById('aftopicedit-topicid').value = t.TopicId;
    document.getElementById('aftopicedit-forumid').value = t.ForumId;
    document.getElementById('aftopicedit-moduleid').value = t.ModuleId;
    document.getElementById('aftopicedit-subject').value = t.Content.Subject;
    document.getElementById('aftopicedit-tags').value = t.Tags;
    document.getElementById('aftopicedit-priority').value = t.Priority;
    am.Utils.SetSelected('aftopicedit-status', t.StatusId);
    document.getElementById('aftopicedit-locked').checked = t.IsLocked;
    document.getElementById('aftopicedit-pinned').checked = t.IsPinned;
    amaf_loadCatList(t.Categories);
    amaf_loadProperties(t.Forum.Properties, t.TopicProperties);
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
                pd.DefaultValue = props[i].DefaultValue;
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
                am.Utils.FillSelect(p.listdata, sel);
                am.Utils.SetSelected(sel, p.DefaultValue);
        };
    };
};


function amaf_saveTopic() {
    var t = {};
    var mid = document.getElementById('aftopicedit-moduleid').value;
    var fid = document.getElementById('aftopicedit-forumid').value;
    t.Topicid = document.getElementById('aftopicedit-topicid').value;
    t.Content = {};
    t.Content.Subject = document.getElementById('aftopicedit-subject').value;
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
                t.TopicProperties[i].DefaultValue = el.value;
            } else {
                t.TopicProperties[i].DefaultValue = el.checked;
            };
        } else {
            t.TopicProperties[i].DefaultValue = el.options[el.selectedIndex].value;
        };
    };
    var ul = document.getElementById('catlist');
    var cats = ul.getElementsByTagName('li');
    t.CategoriesAsString = '';
    for (var i = 0; i < cats.length; i++) {
        var li = cats[i];
        var chk = document.getElementById('cat-' + li.id);
        if (chk.checked) {
            t.CategoriesAsString += chk.value + ';';
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