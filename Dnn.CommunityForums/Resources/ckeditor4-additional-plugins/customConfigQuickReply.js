CKEDITOR.plugins.addExternal('mentions', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/mentions/');
CKEDITOR.plugins.addExternal('ajax', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/ajax/');
CKEDITOR.plugins.addExternal('autocomplete', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/autocomplete/');
CKEDITOR.plugins.addExternal('textmatch', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/textmatch/');
CKEDITOR.plugins.addExternal('textwatcher', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/textwatcher/');
CKEDITOR.plugins.addExternal('xml', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/xml/');
CKEDITOR.plugins.addExternal('codeTag', '/DesktopModules/ActiveForums/Resources/ckeditor4-additional-plugins/codeTag/');

CKEDITOR.instances[afeditor + "_txtBody"].on('instanceReady', function () {
        CKEDITOR.instances[afeditor + "_txtBody"].addCommand('submitForm', {
            exec: function () {
                afQuickSubmitCkEditor4();
            }
        });
        CKEDITOR.instances[afeditor + "_txtBody"].setKeystroke([
            [CKEDITOR.CTRL + 13, 'submitForm'] /* ctrl-Enter key code */
        ]);
    });