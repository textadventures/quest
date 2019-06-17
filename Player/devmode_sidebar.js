// ----------------------------------------------------------------------------------------------------
// MARK Public Variables
// ----------------------------------------------------------------------------------------------------
var selectedname = "game";
var opengroups = ["Gameobject", "Objects", "Rooms"];
var opensidebar = "";

// ----------------------------------------------------------------------------------------------------
// MARK Initialisation the DevModeSideBar
// ----------------------------------------------------------------------------------------------------
function initDevModeSideBar(pos, size, verbs) {

    // ----------------------------------------------------------------------------------------------------
	// MARK Set Stylesheet
	// ----------------------------------------------------------------------------------------------------
    css = '#devmode_sidebar{box-sizing:border-box;display:flex;height:100%;width:500px;top:0;display:flex;position:fixed;border:none;box-shadow:0 0 10px rgba(0,0,0,.2);background-color:ghostwhite;z-index:200}#devmode_sidebar_placeholder{width:55px;height:100%}#devmode_sidebar_container{width:100%;height:100%}#devmode_sidebar_strip{box-sizing:border-box;text-align:center;position:fixed;top:0;height:100%;width:50px;background-color:transparent;transition:background-color 0.5s;z-index:210}.devmode_sidebar_label{width:32px;height:32px;color:#FFF;font-family:webdings;text-align:center;line-height:32px;background:#1D1B61;border-radius:20px;margin:8px;cursor:default;font-size:32px}#devmode_sidebar_debugtable{width:100%;height:100%}#devmode_sidebar_debugtable_names{box-sizing:border-box;float:left;height:100%;width:40%}#devmode_sidebar_debugtable_attr{box-sizing:border-box;float:left;height:100%;width:60%}#devmode_sidebar_debugtable_popupmenu{display:none;z-index:1000;position:absolute;overflow:hidden;border:1px solid #CCC;white-space:nowrap;font-family:Verdana,Geneva,Tahoma,sans-serif;background:#FFF;color:#333;list-style:none;padding-left:0;margin:0}#devmode_sidebar_debugtable_popupmenu li{padding:8px 12px;cursor:default}#devmode_sidebar_debugtable_popupmenu li:hover{background-color:#DEF}.ui-resizable-w{width:10px}#devmode_sidebar_strip,#devmode_sidebar_debugtable_popupmenu,#devmode_sidebar_debugtable:not(input){-webkit-touch-callout:none;-webkit-user-select:none;-khtml-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none}.tabulator{position:relative;font-family:Verdana,Geneva,Tahoma,sans-serif;background-color:ghostwhite;font-size:14px;text-align:left;overflow:hidden;-ms-transform:translatez(0);transform:translatez(0)}.tabulator[tabulator-layout="fitDataFill"] .tabulator-tableHolder .tabulator-table{min-width:100%}.tabulator.tabulator-block-select{-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none}.tabulator .tabulator-header{position:relative;box-sizing:border-box;width:100%;color:#333;background-color:ghostwhite;font-weight:700;white-space:nowrap;overflow:hidden;-moz-user-select:none;-khtml-user-select:none;-webkit-user-select:none;-o-user-select:none}.tabulator .tabulator-header .tabulator-col{display:inline-block;position:relative;box-sizing:border-box;background:ghostwhite;text-align:left;vertical-align:bottom;overflow:hidden}.tabulator .tabulator-header .tabulator-col.tabulator-moving{position:absolute;border:1px solid #999;background:#cdcdcd;pointer-events:none}.tabulator .tabulator-header .tabulator-col .tabulator-col-content{box-sizing:border-box;position:relative;padding:4px}.tabulator .tabulator-header .tabulator-col .tabulator-col-content .tabulator-col-title{box-sizing:border-box;width:100%;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;vertical-align:bottom}.tabulator .tabulator-header .tabulator-col .tabulator-col-content .tabulator-col-title .tabulator-title-editor{box-sizing:border-box;width:100%;border:1px solid #999;padding:1px;background:#fff}.tabulator .tabulator-header .tabulator-col .tabulator-col-content .tabulator-arrow{display:inline-block;position:absolute;top:9px;right:8px;width:0;height:0;border-left:6px solid transparent;border-right:6px solid transparent;border-bottom:6px solid #333}.tabulator .tabulator-header .tabulator-col.tabulator-col-group .tabulator-col-group-cols{position:relative;display:-ms-flexbox;display:flex;border-top:1px solid #aaa;overflow:hidden}.tabulator .tabulator-header .tabulator-col.tabulator-col-group .tabulator-col-group-cols .tabulator-col:last-child{margin-right:-1px}.tabulator .tabulator-header .tabulator-col:first-child .tabulator-col-resize-handle.prev{display:none}.tabulator .tabulator-header .tabulator-col.ui-sortable-helper{position:absolute;background-color:#e6e6e6!important;border:1px solid #aaa}.tabulator .tabulator-header .tabulator-col .tabulator-header-filter{position:relative;box-sizing:border-box;margin-top:2px;width:100%;text-align:center}.tabulator .tabulator-header .tabulator-col .tabulator-header-filter textarea{height:auto!important}.tabulator .tabulator-header .tabulator-col .tabulator-header-filter svg{margin-top:3px}.tabulator .tabulator-header .tabulator-col .tabulator-header-filter input::-ms-clear{width:0;height:0}.tabulator .tabulator-header .tabulator-col .tabulator-header-filter input{font-family:Verdana,Geneva,Tahoma,sans-serif;outline:none;border:1px solid #1D1B61}.tabulator .tabulator-header .tabulator-col.tabulator-sortable .tabulator-col-title{padding-right:25px}.tabulator .tabulator-header .tabulator-col.tabulator-sortable:hover{cursor:default;background-color:#D9D9D9}.tabulator .tabulator-header .tabulator-col.tabulator-sortable[aria-sort="none"] .tabulator-col-content .tabulator-arrow{border-top:none;border-bottom:6px solid #333}.tabulator .tabulator-header .tabulator-col.tabulator-sortable[aria-sort="asc"] .tabulator-col-content .tabulator-arrow{border-top:none;border-bottom:6px solid #333}.tabulator .tabulator-header .tabulator-col.tabulator-sortable[aria-sort="desc"] .tabulator-col-content .tabulator-arrow{border-top:6px solid #333;border-bottom:none}.tabulator .tabulator-header .tabulator-frozen{display:inline-block;position:absolute;z-index:10}.tabulator .tabulator-header .tabulator-frozen.tabulator-frozen-left{border-right:2px solid #aaa}.tabulator .tabulator-header .tabulator-frozen.tabulator-frozen-right{border-left:2px solid #aaa}.tabulator .tabulator-header .tabulator-calcs-holder{box-sizing:border-box;min-width:200%;background:#f3f3f3!important;border-top:1px solid #aaa;border-bottom:1px solid #aaa;overflow:hidden}.tabulator .tabulator-header .tabulator-calcs-holder .tabulator-row{background:#f3f3f3!important}.tabulator .tabulator-header .tabulator-calcs-holder .tabulator-row .tabulator-col-resize-handle{display:none}.tabulator .tabulator-header .tabulator-frozen-rows-holder{min-width:200%}.tabulator .tabulator-header .tabulator-frozen-rows-holder:empty{display:none}.tabulator .tabulator-tableHolder{position:relative;width:100%;white-space:nowrap;overflow:auto;-webkit-overflow-scrolling:touch}.tabulator .tabulator-tableHolder:focus{outline:none}.tabulator .tabulator-tableHolder .tabulator-placeholder{box-sizing:border-box;display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;width:100%}.tabulator .tabulator-tableHolder .tabulator-placeholder[tabulator-render-mode="virtual"]{position:absolute;top:0;left:0;height:100%}.tabulator .tabulator-tableHolder .tabulator-placeholder span{display:inline-block;margin:0 auto;padding:10px;color:#ccc;font-weight:700;font-size:20px}.tabulator .tabulator-tableHolder .tabulator-table{position:relative;display:inline-block;background-color:#fff;white-space:nowrap;overflow:visible;color:#333}.tabulator .tabulator-tableHolder .tabulator-table .tabulator-row.tabulator-calcs{font-weight:700;background:#e2e2e2!important}.tabulator .tabulator-footer{padding:5px 10px;border-top:1px solid #999;background-color:#e6e6e6;text-align:right;color:#333;font-weight:700;white-space:nowrap;-ms-user-select:none;user-select:none;-moz-user-select:none;-khtml-user-select:none;-webkit-user-select:none;-o-user-select:none}.tabulator .tabulator-footer .tabulator-calcs-holder{box-sizing:border-box;width:calc(100% + 20px);margin:-5px -10px 5px -10px;text-align:left;background:#f3f3f3!important;border-bottom:1px solid #aaa;border-top:1px solid #aaa;overflow:hidden}.tabulator .tabulator-footer .tabulator-calcs-holder .tabulator-row{background:#f3f3f3!important}.tabulator .tabulator-footer .tabulator-calcs-holder .tabulator-row .tabulator-col-resize-handle{display:none}.tabulator .tabulator-footer .tabulator-calcs-holder:only-child{margin-bottom:-5px;border-bottom:none}.tabulator .tabulator-footer .tabulator-pages{margin:0 7px}.tabulator .tabulator-footer .tabulator-page{display:inline-block;margin:0 2px;padding:2px 5px;border:1px solid #aaa;border-radius:3px;background:rgba(255,255,255,.2);color:#333;font-family:inherit;font-weight:inherit;font-size:inherit}.tabulator .tabulator-footer .tabulator-page.active{color:#d00}.tabulator .tabulator-footer .tabulator-page:disabled{opacity:.5}.tabulator .tabulator-footer .tabulator-page:not(.disabled):hover{cursor:default;background:rgba(0,0,0,.2);color:#fff}.tabulator .tabulator-col-resize-handle{position:absolute;right:0;top:0;bottom:0;width:5px}.tabulator .tabulator-col-resize-handle.prev{left:0;right:auto}.tabulator .tablulator-loader{position:absolute;display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;top:0;left:0;z-index:100;height:100%;width:100%;background:rgba(0,0,0,.4);text-align:center}.tabulator .tablulator-loader .tabulator-loader-msg{display:inline-block;margin:0 auto;padding:10px 20px;border-radius:10px;background:#fff;font-weight:700;font-size:16px}.tabulator .tablulator-loader .tabulator-loader-msg.tabulator-loading{border:4px solid #333;color:#000}.tabulator .tablulator-loader .tabulator-loader-msg.tabulator-error{border:4px solid #D00;color:#590000}.tabulator-row{position:relative;box-sizing:border-box;min-height:22px;background-color:#fff}.tabulator-row.tabulator-row-even{background-color:ghostwhite}.tabulator-row.tabulator-selectable:hover{background-color:#D9D9D9;cursor:default}.tabulator-row.tabulator-selected{color:white;background-color:#1D1B61}.tabulator-row.tabulator-selected:hover{background-color:#769BCC;cursor:default}.tabulator-row.tabulator-row-moving{border:1px solid #000;background:#fff}.tabulator-row.tabulator-moving{position:absolute;border-top:1px solid #aaa;border-bottom:1px solid #aaa;pointer-events:none;z-index:15}.tabulator-row .tabulator-row-resize-handle{position:absolute;right:0;bottom:0;left:0;height:5px}.tabulator-row .tabulator-row-resize-handle.prev{top:0;bottom:auto}.tabulator-row .tabulator-frozen{display:inline-block;position:absolute;background-color:inherit;z-index:10}.tabulator-row .tabulator-frozen.tabulator-frozen-left{border-right:2px solid #aaa}.tabulator-row .tabulator-frozen.tabulator-frozen-right{border-left:2px solid #aaa}.tabulator-row .tabulator-responsive-collapse{box-sizing:border-box;padding:5px;border-top:1px solid #aaa;border-bottom:1px solid #aaa}.tabulator-row .tabulator-responsive-collapse:empty{display:none}.tabulator-row .tabulator-responsive-collapse table{font-size:14px}.tabulator-row .tabulator-responsive-collapse table tr td{position:relative}.tabulator-row .tabulator-responsive-collapse table tr td:first-of-type{padding-right:10px}.tabulator-row .tabulator-cell{display:inline-block;position:relative;box-sizing:border-box;padding:4px;vertical-align:middle;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.tabulator-row .tabulator-cell.tabulator-editing{border:1px solid #1D1B61;padding:0}.tabulator-row .tabulator-cell.tabulator-editing input,.tabulator-row .tabulator-cell.tabulator-editing select{font-family:Verdana,Geneva,Tahoma,sans-serif;background:white}.tabulator-row .tabulator-cell.tabulator-editing input,.tabulator-row .tabulator-cell.tabulator-editing select:focus{border:none;outline:none;background:white}.tabulator-row .tabulator-cell.tabulator-validation-fail{border:1px solid #d00}.tabulator-row .tabulator-cell.tabulator-validation-fail input,.tabulator-row .tabulator-cell.tabulator-validation-fail select{border:1px;background:transparent;color:#d00}.tabulator-row .tabulator-cell:first-child .tabulator-col-resize-handle.prev{display:none}.tabulator-row .tabulator-cell.tabulator-row-handle{display:-ms-inline-flexbox;display:inline-flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;-moz-user-select:none;-khtml-user-select:none;-webkit-user-select:none;-o-user-select:none}.tabulator-row .tabulator-cell.tabulator-row-handle .tabulator-row-handle-box{width:80%}.tabulator-row .tabulator-cell.tabulator-row-handle .tabulator-row-handle-box .tabulator-row-handle-bar{width:100%;height:3px;margin-top:2px;background:#333}.tabulator-row .tabulator-cell .tabulator-responsive-collapse-toggle{display:-ms-inline-flexbox;display:inline-flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;-moz-user-select:none;-khtml-user-select:none;-webkit-user-select:none;-o-user-select:none;height:15px;width:15px;border-radius:20px;background:#333;color:#fff;font-weight:700;font-size:1.1em}.tabulator-row .tabulator-cell .tabulator-responsive-collapse-toggle:hover{opacity:.7}.tabulator-row .tabulator-cell .tabulator-responsive-collapse-toggle.open .tabulator-responsive-collapse-toggle-close{display:initial}.tabulator-row .tabulator-cell .tabulator-responsive-collapse-toggle.open .tabulator-responsive-collapse-toggle-open{display:none}.tabulator-row .tabulator-cell .tabulator-responsive-collapse-toggle .tabulator-responsive-collapse-toggle-close{display:none}.tabulator-row.tabulator-group{box-sizing:border-box;border-bottom:0;border-right:0;border-top:0;padding:5px;padding-left:10px;background:#F8F8FF;font-weight:700;min-width:100%;color:#333}.tabulator-row.tabulator-group:hover{cursor:default;background-color:rgba(0,0,0,.1)}.tabulator-row.tabulator-group.tabulator-group-visible .tabulator-arrow{margin-right:10px;border-left:6px solid transparent;border-right:6px solid transparent;border-top:6px solid #333;border-bottom:0}.tabulator-row.tabulator-group.tabulator-group-level-1 .tabulator-arrow{margin-left:20px}.tabulator-row.tabulator-group.tabulator-group-level-2 .tabulator-arrow{margin-left:40px}.tabulator-row.tabulator-group.tabulator-group-level-3 .tabulator-arrow{margin-left:60px}.tabulator-row.tabulator-group.tabulator-group-level-4 .tabulator-arrow{margin-left:80px}.tabulator-row.tabulator-group.tabulator-group-level-5 .tabulator-arrow{margin-left:100px}.tabulator-row.tabulator-group .tabulator-arrow{display:inline-block;width:0;height:0;margin-right:16px;border-top:6px solid transparent;border-bottom:6px solid transparent;border-right:0;border-left:6px solid #333;vertical-align:middle}.tabulator-row.tabulator-group span{margin-left:10px;color:transparent}#devmode_sidebar_logarea{height:100%;margin:1em;font-family:Verdana,Geneva,Tahoma,sans-serif;font-size:14px;color:black}#devmode_logarea{height:calc(100% - 5em);width:100%;font:inherit;overflow:scroll;white-space:nowrap;background-color:white;padding:5px;}span.highlight{color:white;background:#1D1B61}';

    $('<style />').text(css).appendTo($('head'));

    // ----------------------------------------------------------------------------------------------------
    // MARK Create DevMode-Sidebar
    // ----------------------------------------------------------------------------------------------------
    $('body').append(
        '<div id="devmode_sidebar_strip"></div>' +
        '<div id="devmode_sidebar" style="width:' + size + '">' +
        '<div id="devmode_sidebar_container"></div>' +
        '</div>'
    );

    // ----------------------------------------------------------------------------------------------------
    // MARK Prepare the sidebar for positioning
    // ----------------------------------------------------------------------------------------------------
    var placeholder = '<div id="devmode_sidebar_placeholder"></div>';
    switch (pos) {
        case "left":
            $('#devmode_sidebar_strip').css({
                left: 0
            });
            $('#devmode_sidebar_container').before(placeholder);
            // ----------------------------------------------------------------------------------------------------
            // MARK Resizing the sidebar (Left)
            // ----------------------------------------------------------------------------------------------------
            $('#devmode_sidebar').resizable({
                handles: 'e',
                distance: 30,
                minWidth: 300
            });
            break;
        case "right":
            $('#devmode_sidebar_strip').css({
                right: 0
            });
            $('#devmode_sidebar_container').after(placeholder);
            // ----------------------------------------------------------------------------------------------------
            // MARK Resizing the sidebar (Right)
            // ----------------------------------------------------------------------------------------------------
            $('#devmode_sidebar').resizable({
                handles: 'w',
                distance: 30,
                minWidth: 300,
                resize: function (event, ui) {
                    ui.position.left = ($(window).width() - $(this).width());
                },
                stop: function (event, ui) {
                    $(this).css({
                        left: "initial"
                    });
                }
            });
            break;
    }

    // ----------------------------------------------------------------------------------------------------
    // MARK Create Label for DevModeDebugTable
    // ----------------------------------------------------------------------------------------------------
    $('#devmode_sidebar_strip').append(
        '<div class="devmode_sidebar_label" id="devmode_sidebar_label_debugtable">@</div>'
    );

    // ----------------------------------------------------------------------------------------------------
    // MARK Create Label for DevModeLogArea
    // ----------------------------------------------------------------------------------------------------
    $('#devmode_sidebar_strip').append(
        '<div class="devmode_sidebar_label" id="devmode_sidebar_label_logarea">a</div>'
    );

    // ----------------------------------------------------------------------------------------------------
    // MARK Show and hide sidebarstrip
    // ----------------------------------------------------------------------------------------------------
    $('.devmode_sidebar_label').on({
        mouseenter: function() {
            $('#devmode_sidebar_strip').css({
                "background-color": "ghostwhite"
            });
        }
    });

    $('#devmode_sidebar_strip').on({
        mouseleave: function() {
            $( this ).css({
                "background-color": "transparent"
            });
        }
    });

    // ----------------------------------------------------------------------------------------------------
    // MARK Initialisation the Sidebar
    // ----------------------------------------------------------------------------------------------------
    $('#devmode_sidebar').sidebar({
        side: pos,
        speed: 100
    });

    // ----------------------------------------------------------------------------------------------------
    // MARK If you click on a label, it is displayed and hidden
    // ----------------------------------------------------------------------------------------------------
    $('.devmode_sidebar_label').on('click', function () {
        sidebar = '#devmode_sidebar_' + this.id.replace("devmode_sidebar_label_", "");
        if (sidebar === opensidebar) {
            $('#devmode_sidebar').trigger("sidebar:close");
            opensidebar = "";
        }
        else {
            $('#devmode_sidebar_container > div').hide();
            $(sidebar).show();
            $('#devmode_sidebar').trigger("sidebar:open");
            opensidebar = sidebar;
        }
        $('#devmode_sidebar').resize();
    });

    // ----------------------------------------------------------------------------------------------------
    // MARK Initialisation the DevModeDebugTable
    // ----------------------------------------------------------------------------------------------------
    function initDevModeDebugTable() {

        // ----------------------------------------------------------------------------------------------------
        // MARK Create DevMode-DebugTable
        // ----------------------------------------------------------------------------------------------------
        $('#devmode_sidebar_container').append(
            '<div id="devmode_sidebar_debugtable">' +
            '<div id="devmode_sidebar_debugtable_names"></div>' +
            '<div id="devmode_sidebar_debugtable_attr"></div>' +
            '</div>'
        );

        // ----------------------------------------------------------------------------------------------------
        // MARK Debugtable - Names
        // ----------------------------------------------------------------------------------------------------
        $("#devmode_sidebar_debugtable_names").tabulator({
            index: "name",
            groupBy: "typ",
            groupToggleElement: "header",
            selectable: 1,
            layout: "fitColumns",
            columns: [{
                    title: "Name",
                    field: "name",
                    sorter: "string",
                    headerFilter: "input",
                    headerFilterPlaceholder: "Filter by...",
                    resizable: false
                },
                {
                    title: "Typ",
                    field: "typ",
                    visible: false
                }
            ],
            groupStartOpen: function (value, count, data, group) {
                return (opengroups.indexOf(value) > -1);
            },
            groupVisibilityChanged: function (group, visible) {
                if (visible) opengroups.push(group.getKey());
                else opengroups.splice(opengroups.indexOf(group.getKey()), 1);
            },
            rowClick: function (e, row) { // For name selectiony
                row.select();
            },
            rowDblClick: function (e, row) {
                row.select();
                switch (row.getData().typ) {
                    case "Objects":
                        ASLEvent("setCommand", "#take " + selectedname);
                        break;
                    case "Rooms":
                        ASLEvent("setCommand", "#go " + selectedname);
                        break;
                }
            },
            rowContext: function (e, row) {
                row.select();
                $('#devmode_sidebar_debugtable_popupmenu li').css({
                    "font-weight": "normal"
                });
                switch (row.getData().typ) {
                    case "Objects":
                        $('#devmode_sidebar_debugtable_popupmenu li[data-action="#take"]').css({
                            "font-weight": "bold"
                        });
                        break;
                    case "Rooms":
                        $('#devmode_sidebar_debugtable_popupmenu li[data-action="#go"]').css({
                            "font-weight": "bold"
                        });
                        break;
                }
            },
            rowSelected: function (row) {
                selectedname = row.getData().name;
                ASLEvent("getTableDataAttr", selectedname);
            },
            dataLoaded: function (data) {
                var hasMatch = false;
                for (var index = 0; index < data.length; ++index) {
                    var element = data[index];
                    if (element.name === selectedname) {
                        hasMatch = true;
                        break;
                    }
                }
                if (hasMatch) {
                    setTimeout( function () {
                        $("#devmode_sidebar_debugtable_names").tabulator("selectRow", selectedname);
                    }, 500); 
                }
            }
        });

        // ----------------------------------------------------------------------------------------------------
        // MARK Debugtable - Attributes
        // ----------------------------------------------------------------------------------------------------
        $("#devmode_sidebar_debugtable_attr").tabulator({
            index: "Attribute",
            layout: "fitColumns",
            initialSort: [{
                    column: "attribute",
                    dir: "asc"
                },
                {
                    column: "value",
                    dir: "asc"
                }
            ],
            columns: [{
                    title: "Attribute",
                    field: "attribute",
                    sorter: "string",
                    headerFilter: "input",
                    headerFilterPlaceholder: "Filter by...",
                    resizable: false
                },
                {
                    title: "Value",
                    field: "value",
                    sorter: "string",
                    editor: "input",
                    headerFilter: "input",
                    headerFilterPlaceholder: "Filter by...",
                    resizable: false
                }
            ],
            cellEdited: function (cell) {
                ASLEvent("setAttribute", selectedname + "." + cell.getRow().getData().attribute + "=" + cell.getRow().getData().value);
            }
        });

        // ----------------------------------------------------------------------------------------------------
        // Debugtable - Read Names
        // ----------------------------------------------------------------------------------------------------
        setTimeout( function () {
            ASLEvent("getTableDataNames", "");
        }, 500);

        // ----------------------------------------------------------------------------------------------------
        // MARK Popop-Menu
        // ----------------------------------------------------------------------------------------------------
        function initDevModePopupMenu(verbs) {

            // ----------------------------------------------------------------------------------------------------
            // MARK Set Verbs in Popup-Menu
            // ----------------------------------------------------------------------------------------------------
            verbs = verbs.split(",");
            menuitems = "";
            verbs.forEach(function (s, i, o) {
                menuitems += '<li data-action="' + s + '">' + s + '</li>';
            });

            // ----------------------------------------------------------------------------------------------------
            // MARK Create Popupmenu
            // ----------------------------------------------------------------------------------------------------   
            $('#devmode_sidebar').after(
                '<ul id="devmode_sidebar_debugtable_popupmenu">' +
                menuitems +
                '<li data-action="refresh">Refresh Names</li>' +
                '</ul>'
            );

            $("#devmode_sidebar_debugtable_names").bind("contextmenu", function (event) {
                event.preventDefault();
                $("#devmode_sidebar_debugtable_popupmenu").finish().toggle(100).
                css({
                    top: event.pageY + "px",
                    left: event.pageX + "px"
                });
            });

            $(document).bind("mousedown", function (e) {
                if (!$(e.target).parents("#devmode_sidebar_debugtable_popupmenu").length > 0) {
                    $("#devmode_sidebar_debugtable_popupmenu").hide(100);
                }
            });

            $("#devmode_sidebar_debugtable_popupmenu li").click(function () {
                switch ($(this).attr("data-action")) {
                    case "refresh":
                        ASLEvent("getTableDataNames", "");
                        break;
                    default:
                        ASLEvent("setCommand", $(this).attr("data-action") + " " + selectedname);
                        break;
                }
                $("#devmode_sidebar_debugtable_popupmenu").hide(100);
            });

        }

        initDevModePopupMenu(verbs);

    }

    // ----------------------------------------------------------------------------------------------------
    // MARK Initialisation the DevModeLogArea
    // ----------------------------------------------------------------------------------------------------
    function initDevModeLogArea() {

        // ----------------------------------------------------------------------------------------------------
        // MARK Create DevMode-LogArea
        // ----------------------------------------------------------------------------------------------------
        $('#devmode_sidebar_container').append(
            '<div id="devmode_sidebar_logarea">' +
            '<span style="float:left">Log:</span>' +
            '<button style="float:right" type="button" onclick="clearDevModeLogArea()">Clear</button>' +
            '<button style="float:right" type="button" onclick="printDevModeLogArea()">Print</button>' +
            '<button style="float:right" type="button" onclick="updateDevModeLogArea()">Update</button>' +
            '<input style="float:right" type="search" placeholder="Search..." onsearch="searchDevModeLogArea(this.value)"></input>' +
            '<div id="devmode_logarea"></div>' +
            '</div>'
        );

        updateDevModeLogArea();
    }

    initDevModeDebugTable();
    initDevModeLogArea();
}

// ----------------------------------------------------------------------------------------------------
// MARK Remove DevMode-Sidebar
// ----------------------------------------------------------------------------------------------------
function removeDevModeSideBar() {
    $("#devmode_sidebar_strip, #devmode_sidebar").remove();
}

// ----------------------------------------------------------------------------------------------------
// MARK Clear the content of Logarea
// ----------------------------------------------------------------------------------------------------
function clearDevModeLogArea() {
    $('#devmode_logarea').text("");
}

// ----------------------------------------------------------------------------------------------------
// MARK Update the content of LogArea
// ----------------------------------------------------------------------------------------------------
function updateDevModeLogArea() {
    clearDevModeLogArea();
    logVar_arr = logVar.split(/NEW_LINE/g);
    logVar_arr.forEach(function(logVar_str) {
        $('#devmode_logarea').append(logVar_str + '<br>');
    });
}

// ----------------------------------------------------------------------------------------------------
// MARK Print the content of Logarea
// ----------------------------------------------------------------------------------------------------
function printDevModeLogArea() {
    $('#devmode_sidebar_label_logarea').click();
    showLogDiv();
}

// ----------------------------------------------------------------------------------------------------
// MARK Searching
// ----------------------------------------------------------------------------------------------------
function searchDevModeLogArea(val) {
    $('#devmode_logarea').removeHighlight();
    $('#devmode_logarea').highlight(val);
}

// ----------------------------------------------------------------------------------------------------
// MARK Debugtable update
// ----------------------------------------------------------------------------------------------------
function setTableData(table, datastr) {

    var data = JSON.parse(datastr);
    $("#devmode_sidebar_debugtable_" + table).tabulator("clearData");
    $("#devmode_sidebar_debugtable_" + table).tabulator("setData", data);
    $("#devmode_sidebar_debugtable_" + table).tabulator("redraw", true);

}

// ----------------------------------------------------------------------------------------------------
// MARK Search Highlight
// ----------------------------------------------------------------------------------------------------

/*

highlight v5

Highlights arbitrary terms.

<http://johannburkard.de/blog/programming/javascript/highlight-javascript-text-higlighting-jquery-plugin.html>

MIT license.

Johann Burkard
<http://johannburkard.de>
<mailto:jb@eaio.com>

*/

jQuery.fn.highlight = function(pat) {
    function innerHighlight(node, pat) {
     var skip = 0;
     if (node.nodeType == 3) {
      var pos = node.data.toUpperCase().indexOf(pat);
      pos -= (node.data.substr(0, pos).toUpperCase().length - node.data.substr(0, pos).length);
      if (pos >= 0) {
       var spannode = document.createElement('span');
       spannode.className = 'highlight';
       var middlebit = node.splitText(pos);
       var endbit = middlebit.splitText(pat.length);
       var middleclone = middlebit.cloneNode(true);
       spannode.appendChild(middleclone);
       middlebit.parentNode.replaceChild(spannode, middlebit);
       skip = 1;
      }
     }
     else if (node.nodeType == 1 && node.childNodes && !/(script|style)/i.test(node.tagName)) {
      for (var i = 0; i < node.childNodes.length; ++i) {
       i += innerHighlight(node.childNodes[i], pat);
      }
     }
     return skip;
    }
    return this.length && pat && pat.length ? this.each(function() {
     innerHighlight(this, pat.toUpperCase());
    }) : this;
   };
   
   jQuery.fn.removeHighlight = function() {
    return this.find("span.highlight").each(function() {
     this.parentNode.firstChild.nodeName;
     with (this.parentNode) {
      replaceChild(this.firstChild, this);
      normalize();
     }
    }).end();
};