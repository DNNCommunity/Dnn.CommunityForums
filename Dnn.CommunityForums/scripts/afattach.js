

function AFAttachmentManager($, options) {

    var $element, $attachments, $attachmentTable, $attachmentRows;
    var attachments;
    var sf = $.ServicesFramework(options.moduleId);

    var self = this;

    this.addAttachment = function (attachment) {

        if (!attachment || (options.maxAttachmentCount > 0 && attachments.length >= options.maxAttachmentCount))
            return;

        attachments.push(attachment);
        self.updateAttachments();
    };

    this.updateAttachments = function () {

        $attachments.val(JSON.stringify(attachments));
        self.renderAttachments();
        self.setAttachButtonVisibility();
    };

    this.renderAttachments = function () {

        $attachmentRows.empty();

        $.each(attachments, function (index, item) {
            var $row = $("<tr>");
            var $icon = $("<i>").attr("class", self.getIconClass(item));
            var $fileName = $("<span>").text(item.fileName || "");
            var $imageButton = $("<span>")
                .addClass("fa fa-plus fa-fw fa-blue")
                .toggle(
                    !!(
                        (item.contentType && item.contentType.match(/^image\/(jpeg|png|gif)$/i)) ||
                        (item.fileName && item.fileName.match(/\.(jpe?g|png|gif)$/i))
                    )
                )
                .on("click", function () { self.insertImage(item); });
            var $removeButton = $("<span>")
                .addClass("fa fa-trash fa-fw fa-blue")
                .on("click", function () { self.removeAttachment(item); });

            $row.append($("<td>").append($icon).append(" ").append($fileName));
            $row.append($("<td>").text(item.fileSize || item.fileSizeText));
            $row.append($("<td>").append($imageButton));
            $row.append($("<td>").append($removeButton));
            $attachmentRows.append($row);
        });

        $attachmentTable.toggle(attachments.length > 0);
    };

    this.setAttachButtonVisibility = function () {

        if (options.maxAttachmentCount > 0 && attachments.length >= options.maxAttachmentCount) {
            $element.find(".af-attach-upload").hide();
        }
        else {
            $element.find(".af-attach-upload").show();
            $element.find(".fileupload-error").hide();
            $element.find(".fileupload-progress").hide();
        }
    };

    this.addUserFileAttachment = function (file) {

        if (!file || !file.id)
            return;

        // Make sure we don't already have the file in the list
        for (var i = 0; i < attachments.length; i++) {
            var item = attachments[i];
            if (item.userFileId === file.id) {
                return;
            }
        }

        var attachment = {
            fileName: file.name,
            fileUrl: file.thumb_url,
            contentType: file.type,
            fileSize: 0,
            fileSizeText: file.size,
            fileId: file.id
        };

        attachments.push(attachment);
        self.updateAttachments();
        self.insertImage(attachment);
    };

    this.removeAttachment = function (item) {

        if (confirm(options.confirmRemoveText)) {
            var index = $.inArray(item, attachments);
            if (index > -1) {
                attachments.splice(index, 1);
                self.updateAttachments();
            }
        }
    };

    this.insertImage = function (item) {
        var fileid = item.fileId;

        var filetype = item.type || item.contentType || '';

        if (filetype) {
            var imageTypes = "gif,jpg,jpeg,png";

            filetype = filetype.substr(filetype.lastIndexOf("/") + 1);

            if ($.inArray(filetype, imageTypes.split(',')) > -1) {
                $.ajax({
                    type: "GET",
                    url: dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Attachment/GetUserFileUrl?ForumId=' + options.forumId + '&FileId=' + fileid,
                    beforeSend: sf.setModuleHeaders
                })
                .done(function (data) { self.getImageUrl(data); })
                .fail(function (xhr, status) {alert('Topics Not Found')});

                return;
            }
        }
    };

    this.getFileUrl = function (url) {
        var isTE = window.isTextEditor;
        if (!isTE) amaf_insertHTML('<a href="' + url + '" target="_blank" >' + url.substr(url.lastIndexOf("/") + 1) + '</a>');
        //else amaf_insertHTML(url);
    };

    this.getImageUrl = function (url) {
        var isTE = window.isTextEditor;
        if (!isTE) amaf_insertHTML('<img src="' + url + '" border="0" class="afpostimg" />');
        else amaf_insertHTML('[img]' + url + '[/img]');
    };

    this.getIconClass = function (item) {

        var fileName = item.fileName || "";

        var ext = fileName.substr((~ -fileName.lastIndexOf(".") >>> 0) + 2);

        return "af-fileicon af-fileicon-" + ext;
    };

    this.initialize = function () {

        $element = $('#' + options.elementId);
        $attachments = $('#' + options.attachmentsClientId);
        $attachmentTable = $element.find(".af-attach-editor-list table");
        $attachmentRows = $attachmentTable.find("tbody");
        attachments = $.parseJSON($attachments.val()) || [];

        //Make DNN 7.2 consistent with 7.0
        var $fileInput = $element.find("input[type=file]");
        if (!$fileInput.parent().hasClass('dnnInputFileWrapper')) {
            var wrapper = $("<span class='dnnInputFileWrapper'></span>");
            $fileInput.wrap(wrapper);
            var btn = $("<span class='dnnSecondaryAction'>Choose File</span>");
            btn.insertBefore($fileInput);
        }

        self.updateAttachments();

        // Init jQuery plugins

        $element.find('.af-attach-upload').afFileUpload({
            maxFileSize: options.maxUploadSize,
            serverErrorMessage: options.serverErrorMessage,
            fileTypeNotAllowedMsg: options.fileTypeNotAllowedMsg,
            allowedFileTypes: options.allowedFileTypes,
            maxFileSizeExceededMsg: options.maxFileSizeExceededMsg,
            uploadButtonText: options.uploadButtonText,
            beforeSend: sf.setModuleHeaders,
            callback: self.addAttachment,
            formData: { "forumId": options.forumId }
        });

        if (!options.allowBrowseSite) {

            $element.find('#photoFromSite').remove();
        }
        else {

            $element.find('.af-userFileManager').userFileManager({
                title: options.titleText,
                cancelText: options.cancelText,
                attachText: options.attachText,
                getItemsServiceUrl: sf.getServiceRoot('InternalServices') + 'UserFile/GetItems',
                nameHeaderText: options.nameHeaderText,
                typeHeaderText: options.typeHeaderText,
                lastModifiedHeaderText: options.lastModifiedHeaderText,
                fileSizeText: options.fileSizeText,
                templatePath: options.templatePath,
                templateName: 'Default',
                templateExtension: '.html',
                attachCallback: self.addUserFileAttachment
            });
        }


        // Have to call this after initializing the upload controls
        self.setAttachButtonVisibility();






    };
};