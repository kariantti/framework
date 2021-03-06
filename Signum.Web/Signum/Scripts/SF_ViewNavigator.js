﻿"use strict";

SF.registerModule("ViewNavigator", function () {

    SF.ViewNavigator = function (_viewOptions) {
        this.viewOptions = $.extend({
            containerDiv: null,
            onOk: null,
            onSave: null,
            onOkClosed: null,
            onCancelled: null,
            onLoaded: null,
            controllerUrl: null,
            type: null,
            id: null,
            prefix: "",
            partialViewName: null,
            navigate: null,
            requestExtraJsonData: null
        }, _viewOptions);

        this.backup = ""; //jquery object with the cloned original elements
    };

    SF.ViewNavigator.prototype = {

        tempDivId: function () {
            return SF.compose(this.viewOptions.prefix, "Temp");
        },

        viewOk: function () {
            if (SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "No containerDiv was specified to Navigator on viewOk mode";
            }
            if (this.isLoaded()) {
                return this.showViewOk(null);
            }
            if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                this.viewOptions.controllerUrl = SF.Urls.popupView;
            }
            var self = this;
            this.callServer(function (controlHtml) { self.showViewOk(controlHtml); });
        },

        createOk: function () {
            if (!SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "ContainerDiv cannot be specified to Navigator on createOk mode";
            }
            if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                this.viewOptions.controllerUrl = SF.Urls.popupView;
            }
            var self = this;
            this.callServer(function (controlHtml) { self.showCreateOk(controlHtml); });
        },

        viewEmbedded: function () {
            if (SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "No containerDiv was specified to Navigator on viewEmbedded mode";
            }
            if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                this.viewOptions.controllerUrl = SF.Urls.partialView;
            }
            var self = this;
            this.callServer(function (controlHtml) { $('#' + self.viewOptions.containerDiv).html(controlHtml); });
        },

        createEmbedded: function (onHtmlReceived) {
            if (!SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "ContainerDiv cannot be specified to Navigator on createEmbedded mode";
            }
            if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                this.viewOptions.controllerUrl = SF.Urls.partialView;
            }
            this.callServer(function (controlHtml) { onHtmlReceived(controlHtml) });
        },

        viewSave: function (html) {
            if (SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "No ContainerDiv was specified to Navigator on viewSave mode";
            }
            if ($('#' + this.viewOptions.containerDiv).length == 0) {
                $("body").append(SF.hiddenDiv(this.viewOptions.containerDiv, ""));
            }
            if (!SF.isEmpty(html)) {
                $('#' + this.viewOptions.containerDiv).html(html);
            }
            if (this.isLoaded()) {
                return this.showViewSave();
            }
            else {
                if (SF.isEmpty(this.viewOptions.type) && new SF.RuntimeInfo(this.viewOptions.prefix).find().length == 0) {
                    throw "Type must be specified to Navigator on viewSave mode";
                }
                if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                    this.viewOptions.controllerUrl = SF.Urls.popupNavigate;
                }
                var self = this;
                this.callServer(function (controlHtml) { self.showViewSave(controlHtml); });
            }
        },

        createSave: function () {
            if (!SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "ContainerDiv cannot be specified to Navigator on createSave mode";
            }
            if (SF.isEmpty(this.viewOptions.type)) {
                throw "Type must be specified to Navigator on createSave mode";
            }
            this.viewOptions.prefix = SF.compose("New", this.viewOptions.prefix);
            if (SF.isEmpty(this.viewOptions.controllerUrl)) {
                this.viewOptions.controllerUrl = SF.Urls.popupNavigate;
            }
            var self = this;
            this.callServer(function (controlHtml) { self.showCreateSave(controlHtml); });
        },

        navigate: function () {
            if (!SF.isEmpty(this.viewOptions.containerDiv)) {
                throw "ContainerDiv cannot be specified to Navigator on Navigate mode";
            }
            if (SF.isEmpty(this.viewOptions.type)) {
                throw "Type must be specified to Navigator on Navigate mode";
            }
            var self = this;
            this.callServer(function (url) { /*$.ajaxPrefilter will handle the redirect*/ });
        },

        isLoaded: function () {
            return !SF.isEmpty($('#' + this.viewOptions.containerDiv).html());
        },

        showViewOk: function (newHtml) {
            if (SF.isEmpty(newHtml)) {
                newHtml = $('#' + this.viewOptions.containerDiv).children().clone(true); //preloaded

                //Backup current Html (for cancel scenarios)
                this.backup = SF.cloneContents(this.viewOptions.containerDiv);
                $('#' + this.viewOptions.containerDiv).html(''); //avoid id-collision

                $("body").append($("<div></div>").attr("id", this.tempDivId()).css("display", "none").html(newHtml));
            }
            else {
                //Backup current Html (for cancel scenarios)
                this.backup = SF.cloneContents(this.viewOptions.containerDiv);
                $('#' + this.viewOptions.containerDiv).html(''); //avoid id-collision

                $("body").append(SF.hiddenDiv(this.tempDivId(), newHtml));
            }

            SF.triggerNewContent($("#" + this.tempDivId()));

            var self = this;
            $("#" + this.tempDivId()).data("viewOptions", this.viewOptions).popup({
                onOk: function () { self.onViewOk() },
                onCancel: function () { self.onViewCancel() }
            });
        },

        showViewSave: function (newHtml) {
            if (!SF.isEmpty(newHtml)) {
                $('#' + this.viewOptions.containerDiv).html(newHtml);
            }

            SF.triggerNewContent($("#" + this.viewOptions.containerDiv));

            var self = this;
            $("#" + this.viewOptions.containerDiv).data("viewOptions", this.viewOptions).popup({
                onSave: function () { self.onCreateSave() },
                onCancel: function () { self.onCreateCancel() }
            });
        },

        showCreateOk: function (newHtml) {
            var tempDivId = this.tempDivId();

            if (!SF.isEmpty(newHtml)) {
                $("body").append(SF.hiddenDiv(tempDivId, newHtml));
            }

            SF.triggerNewContent($("#" + tempDivId));

            var self = this;
            $("#" + tempDivId).data("viewOptions", this.viewOptions).popup({
                onOk: function () { self.onCreateOk() },
                onCancel: function () { self.onCreateCancel() }
            });

            if (this.viewOptions.onLoaded != null) {
                this.viewOptions.onLoaded(this.tempDivId());
            }
        },

        showCreateSave: function (newHtml) {
            var tempDivId = this.tempDivId();

            if (!SF.isEmpty(newHtml)) {
                $("body").append(SF.hiddenDiv(tempDivId, newHtml));
            }

            SF.triggerNewContent($("#" + tempDivId));

            var self = this;
            $("#" + tempDivId).data("viewOptions", this.viewOptions).popup({
                onSave: function () { self.onCreateSave() },
                onCancel: function () { self.onCreateCancel() }
            });

            if (this.viewOptions.onLoaded != null) {
                this.viewOptions.onLoaded(this.tempDivId());
            }
        },

        constructRequestData: function () {
            var options = this.viewOptions,
                serializer = new SF.Serializer()
                                .add({
                                    entityType: options.type,
                                    id: options.id,
                                    prefix: options.prefix
                                });

            if (!SF.isEmpty(options.partialViewName)) { //Send specific partialview if given
                serializer.add("partialViewName", options.partialViewName);
            }

            serializer.add(options.requestExtraJsonData);
            return serializer.serialize();
        },

        callServer: function (onSuccess) {
            $.ajax({
                url: this.viewOptions.controllerUrl || SF.Urls.popupView,
                data: this.constructRequestData(),
                async: false,
                success: function (newHtml) {
                    onSuccess(newHtml);
                }
            });
        },

        onViewOk: function () {
            var doDefault = (this.viewOptions.onOk != null) ? this.viewOptions.onOk() : true;
            if (doDefault != false) {
                $('#' + this.tempDivId()).popup('destroy');
                this.backup = SF.cloneContents(this.tempDivId());
                $('#' + this.tempDivId()).remove();
                $('#' + this.viewOptions.containerDiv).html(this.backup);

                if (this.viewOptions.onOkClosed != null) {
                    this.viewOptions.onOkClosed();
                }
            }
        },

        onViewCancel: function () {
            $('#' + this.tempDivId()).remove();
            var $popupPanel = $('#' + this.viewOptions.containerDiv);
            $popupPanel.html(this.backup);
            this.backup = "";

            if (this.viewOptions.onCancelled != null) {
                this.viewOptions.onCancelled();
            }
        },

        onCreateOk: function () {
            var doDefault = (this.viewOptions.onOk != null) ? this.viewOptions.onOk(SF.cloneContents(this.tempDivId())) : true;
            if (doDefault != false) {
                $('#' + this.tempDivId()).remove();
                if (this.viewOptions.onOkClosed != null) {
                    this.viewOptions.onOkClosed();
                }
            }
        },

        onCreateSave: function () {
            var doDefault = (this.viewOptions.onSave != null) ? this.viewOptions.onSave(this.tempDivId()) : true;
            if (doDefault != false) {
                var validatorResult = new SF.PartialValidator({ prefix: this.viewOptions.prefix, type: this.viewOptions.type }).trySave();
                if (!validatorResult.isValid) {
                    window.alert(lang.signum.popupErrorsStop);
                    return;
                }
                if (SF.isEmpty(this.viewOptions.containerDiv)) {
                    $('#' + this.tempDivId()).remove();
                }
                else {
                    $('#' + this.viewOptions.containerDiv).remove();
                }
                if (this.viewOptions.onOkClosed != null) {
                    this.viewOptions.onOkClosed();
                }
            }
        },

        onCreateCancel: function () {
            if (SF.isEmpty(this.viewOptions.containerDiv)) {
                $('#' + this.tempDivId()).remove();
            }
            else {
                $('#' + this.viewOptions.containerDiv).remove();
            }
            if (this.viewOptions.onCancelled != null) {
                this.viewOptions.onCancelled();
            }
        }
    }

    SF.closePopup = function (prefix) {
        $('#' + SF.compose(prefix, "panelPopup")).closest(".ui-dialog-content,.ui-dialog").remove();
    }

    /* chooserOptions: controllerUrl & types*/
    SF.openTypeChooser = function (prefix, onTypeChosen, chooserOptions) {
        chooserOptions = chooserOptions || {};
        var tempDivId = SF.compose(prefix, "Temp");
        $.ajax({
            url: chooserOptions.controllerUrl || SF.Urls.typeChooser,
            data: { prefix: tempDivId, types: (SF.isEmpty(chooserOptions.types) ? SF.StaticInfo(prefix).types() : chooserOptions.types) },
            async: false,
            success: function (chooserHTML) {
                $("body").append(SF.hiddenDiv(tempDivId, chooserHTML));
                SF.triggerNewContent($("#" + tempDivId));
                //Set continuation for each type button
                $('#' + tempDivId + " :button").each(function () {
                    $('#' + this.id).unbind('click').click(function () {
                        var option = this.id;
                        $('#' + tempDivId).remove();
                        onTypeChosen(option);
                    });
                });

                $("#" + tempDivId).popup({ onCancel: function () {
                    $('#' + tempDivId).remove();
                    if (onCancelled != null)
                        onCancelled();
                }
                });
            }
        });
    }

    /* chooserOptions */
    /* ids: List of ids */
    /* title: Window title */
    SF.openChooser = function (_prefix, onOptionClicked, jsonOptionsListFormat, onCancelled, chooserOptions) {
        //Construct popup
        var tempDivId = SF.compose(_prefix, "Temp");
        var requestData = "prefix=" + tempDivId;
        if (!SF.isEmpty(jsonOptionsListFormat)) {
            for (var i = 0; i < jsonOptionsListFormat.length; i++) {
                requestData += "&buttons=" + jsonOptionsListFormat[i];  //This will Bind to the List<string> "buttons"
                if (chooserOptions && chooserOptions.ids != null) {
                    requestData += "&ids=" + chooserOptions.ids[i];  //This will Bind to the List<string> "ids"            
                }
            }
        }
        if (chooserOptions && chooserOptions.title) {
            requestData += "&title=" + chooserOptions.title;
        }
        $.ajax({
            url: chooserOptions.controllerUrl,
            data: requestData,
            async: false,
            success: function (chooserHTML) {
                $("body").append(SF.hiddenDiv(tempDivId, chooserHTML));
                SF.triggerNewContent($("#" + tempDivId));
                //Set continuation for each type button
                $('#' + tempDivId + " :button").each(function () {
                    $('#' + this.id).unbind('click').click(function () {
                        var option = $(this).attr("data-id");
                        $('#' + tempDivId).remove();
                        onOptionClicked(option);
                    });
                });

                $("#" + tempDivId).popup({ onCancel: function () {
                    $('#' + tempDivId).remove();
                    if (onCancelled != null) {
                        onCancelled();
                    }
                } 
                });
            }
        });
    }
});
