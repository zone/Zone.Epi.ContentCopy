// Variables
var sourceId,
    sourceName,
    destinationId,
    destinationName = "";
var activeLanguage = "en";

// Display state management
function _showOutputArea() {
    if (document.getElementById("outputArea").classList.contains("hidden")) {
        document.getElementById("outputArea").classList.remove("hidden");
    }
}

function _hideOutputArea() {
    if (!document.getElementById("outputArea").classList.contains("hidden")) {
        document.getElementById("outputArea").classList.add("hidden");
    }
}

function updateFromToTitleText() {
    var destination = destinationName !== "" ? "'" + destinationName + "'" : "a destination";

    document.getElementById("pageTitleText").innerHTML = "Select properties to copy from '" + sourceName + "' to " + destination + ".";
}

function createDropDown(languages, parent) {
    var parentDiv = document.getElementById(parent);

    //Create and append select list
    var selectList = document.createElement("select");
    selectList.id = "sourceLangaugeListSelect";
    parentDiv.appendChild(selectList);

    //Create and append the options
    for (var i = 0; i < languages.length; i++) {
        var option = document.createElement("option");
        option.value = languages[i].cultureCode;
        option.text = languages[i].name;
        selectList.appendChild(option);
    }

    // bind to events triggered on language dropdown change
    $("#sourceLangaugeListSelect").change(function () {
        activeLanguage = $(this).val();

        // Refresh the source and destination trees

        $('#jstreeSource').jstree("refresh");
        $('#jstreeDestination').jstree("refresh");
    });
}

function createInput(name, parent) {
    var input = document.createElement('input');
    input.id = name;
    input.type = "checkbox";

    var p = document.createElement('span');
    p.innerText = name;

    var label = document.createElement('label');
    label.for = name;
    label.appendChild(input);
    label.appendChild(p);

    var li = document.createElement('li');

    li.appendChild(label);

    var inject = document.getElementById(parent);
    inject.appendChild(li);
}

// GET calls
function updateSourcePropertyList() {
    if (sourceId === null || sourceId === "")
        return;

    $.get("/Admin/ZoneContentCopy/GetContentDetails",
        {
            id: sourceId,
            lang: $("#hiddenLanguage").val(),
            onlyCultureSpecific: $("#sourceCultureSpecific").prop('checked')
        },
        function (data) {
            // Clear the list
            document.getElementById("propertyListOfSource").innerHTML = null;

            // Toggle hide/show based on results
            if (data.properties.length === 0) {
                $("#pageTitleNoProps").show();

                $("#copyTypeSection").hide();
                $("#copySubmitButton").hide();
            }
            else {
                $("#pageTitleNoProps").hide();

                $("#copyTypeSection").show();
                $("#copySubmitButton").show();
            }

            for (var i = 0; i < data.properties.length; i++) {
                createInput(data.properties[i], "propertyListOfSource");
            }
        }
    );
}

function getAvailableLanguages() {
    $.get("/Admin/ZoneContentCopy/GetAvailableLanguages",
        {},
        function (data) {
            // Clear the dropdown parent
            document.getElementById("sourceLangaugeList").innerHTML = null;

            createDropDown(data, "sourceLangaugeList");
        }
    );
}

// POST calls
function submitCopy() {
    // Get the container element
    var container = document.getElementById('propertyListOfSource');
    var checkedPropertiesArr = [];

    // Find its child `input` elements
    var inputs = container.getElementsByTagName('input');
    for (var i = 0; i < inputs.length; ++i) {
        if ($(inputs[i]).prop('checked')) {
            checkedPropertiesArr.push(inputs[i].id);
        }
    }

    // Post to our endpoint
    $.ajax({
        type: "POST",
        url: "/Admin/ZoneContentCopy/SubmitCopyRequest",
        data: {
            __RequestVerificationToken: $("[name='__RequestVerificationToken']").val(),
            Source: sourceId,
            Destination: destinationId,
            CopyAndPublishAutomatically: $("#copyAndPublish").prop('checked'),
            CultureCode: activeLanguage,
            Properties: checkedPropertiesArr
        },
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        success: function (result) {
            document.getElementById("outputText").innerHTML = result.status;

            _showOutputArea();
        }
    });
}

//  Startup
$(function () {
    // SOURCE tree
    $('#jstreeSource').jstree({
        "core": {
            "data": {
                'url': "/Admin/ZoneContentCopy/GetContentChildren",
                "dataType": "json",
                "data": function (node) {
                    return {
                        "id": node.id === "#" ? "1" : node.id,
                        "lang": activeLanguage
                    };
                }
            }
        },
        "root": "1"
    });

    // bind to events triggered on the tree
    $('#jstreeSource').on("select_node.jstree", function (e, data) {
        var selected = data.selected[0];
        if (selected === null || selected === "undefined")
            return;

        sourceId = selected;
        sourceName = data.node.text;
        updateFromToTitleText();

        $("#sourcePropertiesArea").show();

        _hideOutputArea();
        updateSourcePropertyList();
    });

    // DESTINATION tree
    $('#jstreeDestination').jstree({
        "core": {
            "data": {
                'url': "/Admin/ZoneContentCopy/GetContentChildren",
                "dataType": "json",
                "data": function (node) {
                    return {
                        "id": node.id === "#" ? "1" : node.id,
                        "lang": activeLanguage
                    };
                }
            }
        },
        "root": "1"
    });

    // bind to events triggered on the tree
    $('#jstreeDestination').on("select_node.jstree", function (e, data) {
        var selected = data.selected[0];
        if (selected === null || selected === "undefined")
            return;

        destinationId = selected;
        destinationName = data.node.text;
        updateFromToTitleText();

        _hideOutputArea();
    });

    // bind to events on the culture specific checkbox change
    $("#sourceCultureSpecific").change(function () {
        // Refresh the source and destination trees
        $('#jstreeSource').jstree("refresh");
        $('#jstreeDestination').jstree("refresh");

        // as well as the Properties list
        updateSourcePropertyList();
    });

    // Get the available languages and create the drop down
    getAvailableLanguages();

    //  Bind to select publish alert
    $("#copyAndPublish").change(function () {
        alert("Warning! This will publish the Destination page automatically if all properties are copied successfully. Use with caution.");
    });
});