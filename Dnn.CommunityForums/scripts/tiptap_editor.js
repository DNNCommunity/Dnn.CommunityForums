﻿function amaf_insertHTML(html) {
    window.tipTapEditorInstance.commands.insertContent(html);
};

function amaf_getBody() {
    return window.getTipTapHTML();
};