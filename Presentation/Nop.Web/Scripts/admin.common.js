function setLocation(url) {
    window.location.href = url;
}

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=1, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function showThrobber(message) {
    $('.throbber-header').html(message);
    window.setTimeout(function () {
        $(".throbber").show();
    }, 1000);
}

$(document).ready(function () {
    $('.multi-store-override-option').each(function (k, v) {
        checkOverriddenStoreValue(v, $(v).attr('data-for-input-selector'));
    });
});

function checkAllOverriddenStoreValue(item) {
    $('.multi-store-override-option').each(function (k, v) {
        $(v).attr('checked', item.checked);
        checkOverriddenStoreValue(v, $(v).attr('data-for-input-selector'));
    });
}

function checkOverriddenStoreValue(obj, selector) {
    var elementsArray = selector.split(",");
    if (!$(obj).is(':checked')) {
        $(selector).attr('disabled', true);
        //Kendo UI elements are enabled/disabled some other way
        $.each(elementsArray, function(key, value) {
            var kenoduiElement = $(value).data("kendoNumericTextBox");
            if (kenoduiElement !== undefined && kenoduiElement !== null) {
                kenoduiElement.enable(false);
            }
        }); 
    }
    else {
        $(selector).removeAttr('disabled');
        //Kendo UI elements are enabled/disabled some other way
        $.each(elementsArray, function(key, value) {
            var kenoduiElement = $(value).data("kendoNumericTextBox");
            if (kenoduiElement !== undefined && kenoduiElement !== null) {
                kenoduiElement.enable();
            }
        });
    };
}

function tabstrip_on_tab_select(e) {
    //we use this function to store selected tab index into HML input
    //this way we can persist selected tab between HTTP requests
    $("#selected-tab-index").val($(e.item).index());
}

function display_kendoui_grid_error(e) {
    if (e.errors) {
        if ((typeof e.errors) == 'string') {
            //single error
            //display the message
            alert(e.errors);
        } else {
            //array of errors
            //source: http://docs.kendoui.com/getting-started/using-kendo-with/aspnet-mvc/helpers/grid/faq#how-do-i-display-model-state-errors?
            var message = "The following errors have occurred:";
            //create a message containing all errors.
            $.each(e.errors, function (key, value) {
                if (value.errors) {
                    message += "\n";
                    message += value.errors.join("\n");
                }
            });
            //display the message
            alert(message);
        }
    } else {
        alert('Error happened');
    }
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


// Ajax activity indicator bound to ajax start/stop document events
$(document).ajaxStart(function () {
    $('#ajaxBusy').show();
}).ajaxStop(function () {
    $('#ajaxBusy').hide();
});

/// #region SODMYWAY-2946
function readOnlyMode() {
    /*
    $("DIV[role='tabpanel'").each(function () {
        $(this).find("INPUT[type='text']").each(function () {
            if ($(this).parent("TABLE[readOnly='ignore']").length == 0)
            {
                $(this).parent().html($(this).val());
            }
        });

        $(this).find("SELECT").each(function () {
            if ($(this).parent("TABLE[readOnly='ignore']").length == 0)
            {
                $(this).parent().html($(this).find(":selected").text());
            }
        });

        $(this).find("INPUT[type='radio']").each(function () {
            if ($(this).parent("TABLE[readOnly='ignore']").length == 0) {
                if ($(this).attr("checked") != null) {
                    $(this).remove();
                } else {
                    $(this).parent().remove();
                }
            }
        });

        $(this).find("INPUT[type='checkbox']").each(function () {
            if ($(this).parent("TABLE[readOnly='ignore']").length == 0) {
                var addCaption = false;
                if ($(this).val() == "true") {
                    addCaption = true;
                }

                if ($(this).attr("checked") != null) {
                    if (addCaption) {
                        if ($(this).val() == "true") {
                            $(this).parent().html("Yes");
                        }
                    }
                    else {
                        $(this).remove();
                    }
                } else {
                    if (addCaption) {
                        if ($(this).val() == "true") {
                            $(this).parent().html("No");
                        }
                    }
                    else {
                        $(this).parent().remove();
                    }
                }
            }
        });

        $(this).find("TEXTAREA").each(function () {
            if ($(this).parent("TABLE[readOnly='ignore']").length == 0)
            {
                $(this).parent().html($(this).val());
            }
        });

        if ($(this).parent("TABLE[readOnly='ignore']").length == 0)
        {
            $(this).find("[class*='k-datepicker']").remove();
        }
    });
    */
}
/// #endregion

/// #region SODMYWAY-2954
function disableElement(name)
{
    $(document).ready(function () {
        var obj = $("#" + name);
        var text = "";
        var val = "";
        var tag = "";

        if (obj.is("SELECT")) {
            text = obj.find(":selected").text();
            val = obj.find(":selected").val();
            tag = "SELECT";
        }
        else if (obj.is("TEXTAREA")) {
            text = obj.val();
            val = text;
            tag = "TEXTAREA";
        }
        else if (obj.is("IFRAME")) {
            text = obj.find("body").html();
            val = text;
            tag = "IFRAME";
        }
        else if (obj.attr("type") == "checkbox") {
            if (obj.prop("checked") == true) {
                text = "yes";
                val = "true";
            } else {
                text = "no";
                val = "false";
            }
            tag = "CHECKBOX";
        }
        else {
            text = obj.val();
            val = text;
            tag = "INPUT";
        }

        objParent = obj.parents("TD");
        console.log(tag + ":" + name + ":" + text + ":" + val);

        objParent.html(text + "<INPUT TYPE='hidden' ID='" + name + "' NAME='" + name + "' VALUE='" + val + "' />");
    });
}


/// #endregion