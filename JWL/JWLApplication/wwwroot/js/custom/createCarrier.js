$(function () {
    $(".wrap-div").show();
    $("#signaturePath").css('display', 'none');
    $(".attachmentPath").css('display', 'none');
    $(".add-attachment").css('display', 'none');
    var duplicate = $("#cloneAttachDocument").clone();
    $('#additionalAttachment').append(duplicate);
    $('#additionalAttachment').children('div').show();
    var phones = [{ "mask": "(###) ###-####" }];
    $('#telephone').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });
    $('#fax').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });
    $('#factoryTelephone').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });
    $('#factoryFax').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });
    $('#addtionalPersonTelephone').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });
    $('#addtionalAfterHoursPersonTelephone').inputmask({
        mask: phones,
        greedy: false,
        definitions: { '#': { validator: "[0-9]", cardinality: 1 } }
    });

    $("#selectMarket").change(function () {
        $("#additionalMajorMakets").val($('#selectMarket').val());
    });

    $("#selectedCargos").change(function () {
        $("#cargoSpecification").val($('#selectedCargos').val());
    });

    $("#selectedPayment").change(function () {
        $("#paymentMethods").val($('#selectedPayment').val());
    });
    $("#serviceArea").change(function () {
        if ($("#serviceArea option:selected").val() == "National") {
            $("#selectMarket").attr('disabled', true);
        }
        else {
            $("#selectMarket").attr('disabled', false);
        }
    });
    var isValidTax = false;
    $("#additionalFedaralID").keyup(function () {
        var val = this.value;
        var tax = new RegExp('[0-9]{3}\-[0-9]{2}\-[0-9]{4}$');
        var tax1 = new RegExp('[0-9]{2}\-[0-9]{7}$');
        if (val == null || val == "" || $.trim(val) == "") {
            $("#fedralTaxRequiredSpan").show();
            $("#fedralTaxSpan").hide();
            isValidTax = false;
        }
        else if (tax.test(val) || tax1.test(val)) {
            $("#fedralTaxSpan").hide();
            isValidTax = true;
        }
        else {
            $("#fedralTaxSpan").show();
            $("#fedralTaxRequiredSpan").hide();
            isValidTax = false;
        }
    });
    $("body").on("change", ".document-select", function (event) {
        if ($(this).val() == "Other") {
            $(this).parent('div').children('.text-documenttype').show();
        }
        else {
            $(this).parent('div').children('.text-documenttype').hide();
        }
    });
    $("#addtionalDot").on('change', function () {
        if ($(this).val() != null && $(this).val() != "" && $.trim($(this).val()) != "") {
            $.ajax({
                url: "/Carrier/CheckDOT",
                data: {
                    dot: $("#addtionalDot").val(),
                    isEdit: "false"
                },
                dataType: "json",
                type: 'POST',
                async: false,
                beforeSend: function () {
                    showLoader();
                },
                success: function (result) {                   
                    if (result == "null") {                       
                        var win = window.open("/users/login", "_self"); return;
                    }
                    if (result == "True") {
                        $("#alreadyDOT").show();
                    }
                    else {
                        $("#alreadyDOT").hide();
                    }
                },
                complete: function () {
                    hideLoader();
                }
            });
        }
    });
    $("#cuEmail").on('change', function () {
        if ($(this).val() != null && $(this).val() != "" && $.trim($(this).val()) != "") {
            $.ajax({
                url: "/Carrier/CheckEmail",
                data: {
                    email: $("#cuEmail").val(),
                    isEdit: "false"
                },
                dataType: "json",
                type: 'POST',
                async: false,
                beforeSend: function () {
                    showLoader();
                },
                success: function (result) {
                    if (result == "null") {
                        var win = window.open("/users/login", "_self"); return;
                    }
                    if (result == "True") {
                        $("#alreadyEmail").show();
                    }
                    else {
                        $("#alreadyEmail").hide();
                    }
                },
                complete: function () {
                    hideLoader();
                }
            });
        }
    });

    $("#additionalScac").on('change', function () {
        $("#alreadyadditionalScac").hide();
        if ($(this).val() != null && $(this).val() != "" && $.trim($(this).val()) != "") {
            $.ajax({
                url: "/Carrier/CheckSCACCode",
                data: {
                    additionalScac: $("#additionalScac").val(),
                    isEdit: "false"
                },
                dataType: "json",
                type: 'POST',
                async: false,
                beforeSend: function () {
                    showLoader();
                },
                success: function (result) {
                    if (result == "null") {
                        var win = window.open("/users/login", "_self"); return;
                    }
                    if (result == "True") {
                        $("#alreadyadditionalScac").show();
                    }
                    else {
                        $("#alreadyadditionalScac").hide();
                    }
                },
                complete: function () {
                    hideLoader();
                }
            });
        }
    });

    $("#createCarrier").submit(function () {
        showLoader();
        if (isValidTax == true) {
            if ($("#alreadyDOT").is(":visible") == false && $("#alreadyEmail").is(":visible") == false) {
                return true;
            }
            else {
                $("#addtionalDot").focus();
                hideLoader();
                return false;
            }
        }
        else {
            if ($("#fedralTaxSpan").is(":visible") == false) {
                $("#fedralTaxRequiredSpan").show();
            }
            hideLoader();
            return false;
        }
    });
    
    var vehicleType = [];
    $("body").on("click", ".add-vehicle", function (event) {
        var selectedVal = $(this).parent('div').parent('div').parent('.divVehicle').find("select").val();
        var textVal = $(this).parent('div').parent('div').parent('.divVehicle').find("input[type='text']").val();
        if (selectedVal != null && selectedVal != "" && $.trim(selectedVal) != ""
            && textVal != null && textVal != "" && $.trim(textVal) != "") {

            array = $.grep(vehicleType, function (check) {
                return (check.selectedVehicle == selectedVal);
            });
            if (array.length > 0) {
                alert("Same vehicle Type is already added please update in that");
                return;
            }

            var obj = {};
            obj.selectedVehicle = selectedVal;
            obj.numberOfVehicle = $.trim(textVal);
            vehicleType.push(obj);
            $("#CarrierwiseVehicle").val(JSON.stringify(vehicleType));
            $(this).parent('div').children('.remove-vehicle').show();
            $(this).hide();
            var duplicate = $("#cloneVehicleType").clone();
            duplicate.children('div').children('.col-md-4:nth-child(2)').children('div').children('input[type="text"]').val('');
            duplicate.children('.divVehicle').children('.remove-vehicle-btn').children('div').append('<input type="button" class="remove-vehicle file-up-btn mr-top" value="Remove" />');
            duplicate.children('.divVehicle').children('.remove-vehicle-btn').children('div').children('.add-vehicle').show();
            duplicate.children('.divVehicle').children('.remove-vehicle-btn').children('div').children('.remove-vehicle').hide();
            $('#additionalVehicle').append(duplicate);
        }
        else {
            alert("Please select Vehicle Type and Fleet Size");
        }

    });
    $("body").on("click", ".remove-vehicle", function (event) {
        var removeVehicle = $(this).parent('div').parent('div').parent('div').children('.col-md-4:first-child').children('div').children('select').val();
        if (removeVehicle != null && removeVehicle != "") {
            vehicleType = $.grep(vehicleType, function (value) {
                return value.selectedVehicle != removeVehicle;
            });
            $("#CarrierwiseVehicle").val(JSON.stringify(vehicleType));
        }
        $(this).parent('div').parent('div').parent('div').parent('div').remove();
    });


    var trailerType = [];
    $("body").on("click", ".add-trailer", function (event) {
        var selectedVal = $(this).parent('div').parent('div').parent('.divTrailer').find("select").val();
        var textVal = $(this).parent('div').parent('div').parent('.divTrailer').find("input[type='text']").val();
        if (selectedVal != null && selectedVal != "" && $.trim(selectedVal) != ""
            && textVal != null && textVal != "" && $.trim(textVal) != "") {

            array = $.grep(trailerType, function (check) {
                return (check.selectedTrailer == selectedVal);
            });
            if (array.length > 0) {
                alert("Same Trailer Type is already added please update in that");
                return;
            }

            var obj = {};
            obj.selectedTrailer = selectedVal;
            obj.numberOfVehicle = $.trim(textVal);
            trailerType.push(obj);
            $("#CarrierwiseTrailer").val(JSON.stringify(trailerType));
            $(this).parent('div').children('.remove-trailer').show();
            $(this).hide();
            var duplicate = $("#cloneTrailerType").clone();
            duplicate.children('div').children('.col-md-4:nth-child(2)').children('div').children('input[type="text"]').val('');
            duplicate.children('.divTrailer').children('.remove-trailer-btn').children('div').append('<input type="button" class="remove-trailer file-up-btn mr-top" value="Remove" />');
            duplicate.children('.divTrailer').children('.remove-trailer-btn').children('div').children('.add-trailer').show();
            duplicate.children('.divTrailer').children('.remove-trailer-btn').children('div').children('.remove-trailer').hide();
            $('#additionalTrailer').append(duplicate);
        }
        else {
            alert("Please select Trailer Type and Trailer Count");
        }

    });
    $("body").on("click", ".remove-trailer", function (event) {
        var remoTrailer = $(this).parent('div').parent('div').parent('div').children('.col-md-4:first-child').children('div').children('select').val();
        if (remoTrailer != null && remoTrailer != "") {
            trailerType = $.grep(trailerType, function (value) {
                return value.selectedTrailer != remoTrailer;
            });
            $("#CarrierwiseTrailer").val(JSON.stringify(trailerType));
        }
        $(this).parent('div').parent('div').parent('div').parent('div').remove();
    });
    $("body").on("click", ".remove-vehicle", function (event) {
        var removeVehicle = $(this).parent('div').parent('div').parent('div').children('.col-md-4:first-child').children('div').children('select').val();
        if (removeVehicle != null && removeVehicle != "") {
            vehicleType = $.grep(vehicleType, function (value) {
                return value.selectedVehicle != removeVehicle;
            });
            $("#CarrierwiseVehicle").val(JSON.stringify(vehicleType));
        }
        $(this).parent('div').parent('div').parent('div').parent('div').remove();
    });

    var attachments = [];
    $("body").on("change", ".attachDocument", function (event) {
        var selectedVal = $(this).parent('div').parent('div').parent('div').parent('.addDocument').find("select").val();
        if (selectedVal == "Other") {
            selectedVal = "_txt" + $(this).parent('div').parent('div').parent('div').parent('.addDocument').find('.text-documenttype').val();
        }
        if (selectedVal == null || selectedVal == "" || selectedVal == "_txt") {
            alert('Please select valid Document Type');
            return;
        }
        var fileUpload = $(this).parent('div').parent('div').parent('div').parent('.addDocument').find("input[name='myfile']").get(0);
        var files = fileUpload.files;
        if (files.length == 0) {
            alert('Please select valid document');
            return;
        }
        array = $.grep(attachments, function (check) {
            return (check.selectedVal == selectedVal);
        });
        if (array.length > 0) {
            alert("Same Document Type is already added please update in that");
            return;
        }
        var data = new FormData();
        data.set('selectedVal', selectedVal);
        data.append(files[0].name, files[0]);
        var path = "";
        $.ajax({
            type: "POST",
            url: "/carrier/UploadAttachment",
            contentType: false,
            processData: false,
            data: data,
            async: false,
            beforeSend: function () {
                showLoader();
            },
            success: function (message) { 
                if (message == "null") {
                    var win = window.open("/users/login", "_self"); return;
                }
                var obj = {};
                obj.url = message;
                obj.selectedVal = $.trim(selectedVal);
                attachments.push(obj);
                $("#CarrierDocument").val(JSON.stringify(attachments));
                path = message;
            },
            error: function () {
                alert("Something wrong! Please try again.");
            },
            complete: function () {
                hideLoader();
            }
        });
        if ($(this).parent('div').parent('div').parent('div').parent('div').parent('div').parent('div').children('#cloneAttachDocument').length == 1) {
            $(this).parent('div').parent('div').parent('div').parent('.addDocument').children('.col-md-5').children('.attachmentPath').children('.attachment-remove').hide();
        }

        $(this).parent('div').parent('div').parent('div').parent('.addDocument').children('.col-md-5').children('.attachmentPath').show();
        $(this).parent('div').parent('div').parent('div').parent('.addDocument').children('.col-md-5').children('div').children('.add-attachment').show();
        $(this).parent('div').parent('div').parent('div').parent('.addDocument').children('.col-md-5').children('div').children('.add-attachment').children('.attachment-remove').show();
        $(this).parent('div').parent('div').parent('div').parent('.addDocument').children('.col-md-5').children('.attachmentPath').children('a').attr('href', path);

    });
    $("body").on("click", ".add-attachment", function (event) {
        $(this).hide();
        var duplicate = $("#cloneAttachDocument").clone();
        duplicate.removeAttr('style');
        $('#additionalAttachment').append(duplicate);
    });

    $("body").on("click", ".attachment-remove", function (event) {
        var selectedVal = $(this).closest('div').children('a').attr('href');
        $.ajax({
            url: "/Carrier/RemoveFile",
            data: {
                file: selectedVal
            },
            dataType: "json",
            type: 'POST',
            async: false,
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {              
                if (result == "null") { var win = window.open("/users/login", "_self"); return;}
                if (result == "Success") {
                    attachments = $.grep(attachments, function (value) {
                        return value.url != selectedVal;
                    });
                    $("#CarrierDocument").val(JSON.stringify(attachments));
                }
            },
            error: function (err, xhr, text) {

                console.log(err);
            },
            complete: function () {
                hideLoader();
            }
        });
        if ($(this).parent('div').parent('div').children('div:first-child').children('.add-attachment').is(":visible")) {
            $(this).parent('div').parent('div').parent('div').remove();
            var duplicate = $("#cloneAttachDocument").clone();
            duplicate.removeAttr('style');
            $('#additionalAttachment').append(duplicate);
        }
        else {
            $(this).parent('div').parent('div').parent('div').remove();
        }
    });

});
function showLoader() {
    $(".preloader-backdrop").show();
}
function hideLoader() {
    setTimeout(function () {
        $(".preloader-backdrop").hide();
    }, 1000);
}
function uploadFiles() {
    var fileUpload = $("input[name='mysignature']").get(0);
    var files = fileUpload.files;
    var data = new FormData();
    data.append(files[0].name, files[0]);
    $.ajax({
        type: "POST",
        url: "/carrier/UploadSignature",
        contentType: false,
        processData: false,
        data: data,
        async: false,
        beforeSend: function () {
            showLoader();
        },
        success: function (message) {
            setTimeout(function () {
                $("#authorizedSignaturePath").val(message);
                $("#signaturePath").css('display', 'inline');
                $(".imageLink").attr('href', message);
            }, 2000);
        },
        error: function () {
            alert("Error!");
        },
        complete: function () {
            hideLoader();
        }
    });
}
function removeFile() {
    $.ajax({
        url: "/Carrier/RemoveFile",
        data: {
            file: $("#authorizedSignaturePath").val()
        },
        dataType: "json",
        type: 'POST',
        async: false,
        beforeSend: function () {
            showLoader();
        },
        success: function (result) {

            if (result == "Success") {
                setTimeout(function () {
                    $("#authorizedSignaturePath").val("");
                    $("#signaturePath").css('display', 'none');
                    $(".imageLink").attr('href', '#');
                }, 2000);
            }
        },
        error: function (err, xhr, text) {
            console.log(err);
        },
        complete: function () {
            hideLoader();
        }
    });
}