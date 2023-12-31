﻿/*
** nopCommerce custom js functions
*/



function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading-block-window').show();
    }
    else {
        $('.ajax-loading-block-window').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog-notifications-success');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-notifications-error');
    }
    else {
        //other
        container = $('#dialog-notifications-success');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }

    container.html(htmlcode);

    var isModal = (modal ? true : false);
    container.dialog({
        modal: isModal,
        width: 350
    });
}
function displayPopupContentFromUrl(url, title, modal, width) {
    var isModal = (modal ? true : false);
    var targetWidth = (width ? width : 550);

    $('<div></div>').load(url)
        .dialog({
            modal: isModal,
            width: targetWidth,
            title: title,
            close: function(event, ui) {
                $(this).dialog('destroy').remove();
            }
        });
}

var barNotificationTimeout;
function displayBarNotification($control, message, messagetype, timeout) {
    
    clearTimeout(barNotificationTimeout);
    if ($(".required-field-error").length > 0) {
        $(".required-field-error").hide();
        $(".aria-invalid").attr("aria-invalid", "false");
        $(".attributes").find('input,textarea,select').removeClass('focus-outline');
    }
    
    //types: success, error
    var cssclass = 'success';
    if (messagetype == 'success') {
        cssclass = 'success';
    }
    else if (messagetype == 'error') {
        cssclass = 'error';
    }
    //remove previous CSS classes and notifications
    $('#bar-notification')
        .removeClass('success')
        .removeClass('error');
    $('#bar-notification .content').remove();
    $('#bar-notification .errorTxt').remove();
    $('#bar-notification').html('');
    

    //we do not encode displayed message

    //add new notifications
    var htmlcode = '';
    var focuselement = '';
    if ((typeof message) == 'string') {
        if (!message.includes('FieldError'))
            htmlcode = '<div><p class="content">' + message + '</p></div>';

        if (message.includes('FieldError')) {
            var field = message.split(':');
            var id = field[1];
            if ($("#required-error-" + id.replace(".","_")).length > 0) {
                $("#required-error-" + id.replace(".", "_")).show();
                focuselement = id;
                $('[name="' + id + '"]').attr("aria-invalid", "true");
                $('[name="' + id + '"]').attr("aria-describedby", "required-error-" + id.replace(".", "_"));
                //$('[name="' + id + '"]').focus();
                //$('[name="' + id + '"]').addClass('focus-outline');
            }

        }        

    } else {
        var focuscount = 0;
        if (messagetype == 'error')
            htmlcode = htmlcode + '<div class="errorTxt">Error:';
        else
            htmlcode = htmlcode + '<div>';

        for (var i = 0; i < message.length; i++) {
            if (!message[i].includes('FieldError')) {
                if (i == message.length - 1) {
                    htmlcode = htmlcode + '<p class="content">' + message[i] + '</p> </label>';
                }
                else if (i < message.length - 1 && message[i + 1].includes('FieldError')) {
                    var linkId = $('[name="' + message[i + 1].split(':')[1] + '"]').first().attr("id");
                    htmlcode = htmlcode + '<p class="content"> Please provide <a href=#' + linkId + '>'+ message[i] + '</a></p>';
                }
                else {
                    htmlcode = htmlcode + '<p class="content">' + message[i] + '</p>';
                }                
            }
            if (message[i].includes('FieldError')) {
                var field = message[i].split(':');
                var id = field[1];
                if ($("#required-error-" + id.replace(".", "_")).length > 0) {
                    $("#required-error-" + id.replace(".", "_")).show();
                    $('[name="' + id + '"]').attr("aria-invalid", "true");
                    $('[name="' + id + '"]').attr("aria-describedby", "required-error-" + id.replace(".", "_"));

                    if (focuscount == 0) {
                        focuselement = id;
                        //$('[name="' + id + '"]').first().focus();
                        //$('[name="' + id + '"]').first().addClass('focus-outline');
                        focuscount = focuscount + 1;
                    }
                }

            }
                        
        }
    }
 

    //$('#bar-notification').append(htmlcode)
    //    .addClass(cssclass)
    //    .fadeIn('slow')
    //    .mouseenter(function ()
    //        {
    //            clearTimeout(barNotificationTimeout);
    //    });

    $('#bar-notification').append(htmlcode);

    $('#bar-notification').append('<button aria-label="Close" class="close">&nbsp;</button>')
        .addClass(cssclass)
        .fadeIn('slow')
        .mouseenter(function () {
            clearTimeout(barNotificationTimeout);
        });

    $("#bar-notification div").attr("tabIndex", -1);
    $("#bar-notification div").focus();


    $('#bar-notification .close').unbind('click').click(function () {
        $('#bar-notification').fadeOut('slow');
        if ($control != undefined) {
            $("#" + $control).focus();
        }
    });

    //timeout (if set)
    //if (timeout > 0) {
    //    barNotificationTimeout = setTimeout(function () {
    //        $('#bar-notification').fadeOut('slow');
    //    }, timeout);
    //}
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};