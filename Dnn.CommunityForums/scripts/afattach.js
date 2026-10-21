

function AFAttachmentManager(jQuery, options) {

    let element, attachmentsField, attachmentTable, attachmentRows;
    let attachments;
    const sf = jQuery.ServicesFramework(options.moduleId);

    const self = this;

    this.addAttachment = function (attachment) {

        if (!attachment || (options.maxAttachmentCount > 0 && attachments.length >= options.maxAttachmentCount))
            return;

        attachments.push(attachment);
        self.updateAttachments();
    };

    this.updateAttachments = function () {

        attachmentsField.value = JSON.stringify(attachments);
        self.renderAttachments();
        self.setAttachButtonVisibility();
    };

    this.renderAttachments = function () {

        attachmentRows.replaceChildren();

        attachments.forEach(function (item) {
            const row = document.createElement("tr");
            const fileNameCell = document.createElement("td");
            const icon = document.createElement("i");
            const fileName = document.createElement("span");
            const fileSizeCell = document.createElement("td");
            const insertImageCell = document.createElement("td");
            const removeCell = document.createElement("td");
            const imageButton = document.createElement("span");
            const removeButton = document.createElement("span");

            icon.className = self.getIconClass(item);
            fileName.textContent = item.fileName || "";
            fileNameCell.append(icon, " ", fileName);
            fileSizeCell.textContent = item.fileSize || item.fileSizeText || "";

            imageButton.className = "fa fa-plus fa-fw fa-blue";
            imageButton.hidden = !(
                (item.contentType && item.contentType.match(/^image\/(jpeg|png|gif)$/i)) ||
                (item.fileName && item.fileName.match(/\.(jpe?g|png|gif)$/i))
            );
            imageButton.addEventListener("click", function () { self.insertImage(item); });
            insertImageCell.append(imageButton);

            removeButton.className = "fa fa-trash fa-fw fa-blue";
            removeButton.addEventListener("click", function () { self.removeAttachment(item); });
            removeCell.append(removeButton);

            row.append(fileNameCell, fileSizeCell, insertImageCell, removeCell);
            attachmentRows.append(row);
        });

        attachmentTable.hidden = attachments.length === 0;
    };

    this.setAttachButtonVisibility = function () {

        if (options.maxAttachmentCount > 0 && attachments.length >= options.maxAttachmentCount) {
            element.querySelector(".af-attach-upload").style.display = "none";
        }
        else {
            element.querySelector(".af-attach-upload").style.removeProperty("display");
            element.querySelector(".fileupload-error").style.display = "none";
            element.querySelector(".fileupload-progress").style.display = "none";
        }
    };

    this.addUserFileAttachment = function (file) {

        if (!file || !file.id)
            return;

        // Make sure we don't already have the file in the list
        for (let i = 0; i < attachments.length; i++) {
            if (attachments[i].userFileId === file.id) {
                return;
            }
        }

        const attachment = {
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
            const index = attachments.indexOf(item);
            if (index > -1) {
                attachments.splice(index, 1);
                self.updateAttachments();
            }
        }
    };

    this.insertImage = function (item) {
        const fileid = item.fileId;

        let filetype = item.type || item.contentType || '';

        if (filetype) {
            const imageTypes = "gif,jpg,jpeg,png";

            filetype = filetype.substr(filetype.lastIndexOf("/") + 1);

            if (imageTypes.split(',').indexOf(filetype) > -1) {
                const requestHeaders = new Headers();
                sf.setModuleHeaders({
                    setRequestHeader: function (name, value) {
                        requestHeaders.set(name, value);
                    }
                });
                fetch(dnn.getVar("sf_siteRoot", "/") + 'API/ActiveForums/Attachment/GetUserFileUrl?ForumId=' + options.forumId + '&FileId=' + fileid, {
                    headers: requestHeaders
                })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error("Unable to get the attachment URL.");
                    }

                    return response.json();
                })
                .then(function (url) { self.getImageUrl(url); })
                .catch(function () { alert('Topics Not Found'); });

                return;
            }
        }
    };

    this.getFileUrl = function (url) {
        const isTE = window.isTextEditor;
        if (!isTE) amaf_insertHTML('<a href="' + url + '" target="_blank" >' + url.substr(url.lastIndexOf("/") + 1) + '</a>');
        //else amaf_insertHTML(url);
    };

    this.getImageUrl = function (url) {
        const isTE = window.isTextEditor;
        if (!isTE) amaf_insertHTML('<img src="' + url + '" border="0" class="afpostimg" />');
        else amaf_insertHTML('[img]' + url + '[/img]');
    };

    this.getIconClass = function (item) {

        const fileName = item.fileName || "";

        const ext = fileName.substr((~ -fileName.lastIndexOf(".") >>> 0) + 2);

        return "af-fileicon af-fileicon-" + ext;
    };

    this.initialize = function () {

        element = document.getElementById(options.elementId);
        attachmentsField = document.getElementById(options.attachmentsClientId);
        attachmentTable = document.getElementById(options.attachmentTableId);
        attachmentRows = attachmentTable.querySelector("tbody");
        attachments = JSON.parse(attachmentsField.value || "[]") || [];

        //Make DNN 7.2 consistent with 7.0
        const fileInput = element.querySelector("input[type=file]");
        if (!fileInput.parentElement.classList.contains("dnnInputFileWrapper")) {
            const wrapper = document.createElement("span");
            wrapper.className = "dnnInputFileWrapper";
            fileInput.parentElement.insertBefore(wrapper, fileInput);
            wrapper.append(fileInput);
            const button = document.createElement("span");
            button.className = "dnnSecondaryAction";
            button.textContent = "Choose File";
            wrapper.insertBefore(button, fileInput);
        }

        self.updateAttachments();

        // Init jQuery plugins

        jQuery(element.querySelector(".af-attach-upload")).afFileUpload({
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

            element.querySelector("#photoFromSite").remove();
        }
        else {

            jQuery(element.querySelector(".af-userFileManager")).userFileManager({
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