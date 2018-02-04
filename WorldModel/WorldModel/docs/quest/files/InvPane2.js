var Inventory2Verbs;
 $(function() {
   var s = "<div id='Inventory2Holder' style='padding-left:0px'> \
             <h3 id='Inventory2Label'><a href='#'>Inventory 2</a></h3> \
             <div id='Inventory2Accordion'> \
                 <div id='Inventory2Wrapper' class='elementListWrapper'> \
                     <ol id='lstInventory2' class='elementList ui-selectable'> \
                     </ol> \
                 </div> \
                 <div class='verbButtons'> \
                     <button id='cmdInventory21' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory22' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory23' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory24' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory25' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory26' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory27' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory28' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                     <button id='cmdInventory29' type='button' onclick='paneButtonClick(\"#lstInventory2\",$(this));' style='display:none' role='button' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only'><span class='ui-button-text'></span></button> \
                 </div> \
             </div> \
             </div>";
     $(s).insertBefore("#statusVarsLabel");
     $("#Inventory2Holder").multiOpenAccordion({ active: [0] });
     $("#lstInventory2").selectable({
         selected: function (event, ui) {
             $(ui.selected).siblings().removeClass("ui-selected");
             updateVerbButtons($(ui.selected), Inventory2Verbs, "cmdInventory2");
         }
     });
 
 });
 
 function updateInventory2(listName,listData) {
  listData = JSON.parse(listData);
     var listElement = "#lst" + listName;
     var buttonPrefix = "cmd" + listName;
  var idPrefix = buttonPrefix;
  eval(listName + "Verbs = new Array();");
 
  var verbsArray = eval(listName + "Verbs");
 
     var previousSelectionText = "";
     var previousSelectionKey = "";
     var foundPreviousSelection = false;
 
     var $selected = $(listElement + " .ui-selected");
     if ($selected.length > 0) {
         previousSelectionText = $selected.first().text();
         previousSelectionKey = $selected.first().data("key");
     }
 
     $(listElement).empty();
     var count = 0;
     $.each(listData, function (key, value) {
         var data = value;
         var objectDisplayName = data["Text"];
 
         verbsArray.push(data);
 
         if (true) {
             var $newItem = $("<li/>").data("key", key).data("elementid", data["ElementId"]).data("elementname", data["ElementName"]).data("index", count).html(objectDisplayName);
             if (objectDisplayName == previousSelectionText && key == previousSelectionKey) {
                 $newItem.addClass("ui-selected");
                 foundPreviousSelection = true;
                 updateVerbButtons($newItem, verbsArray, idPrefix);
             }
             $(listElement).append($newItem);
             count++;
         }
     });
 
     var selectSize = count;
     if (selectSize < 3) selectSize = 3;
     if (selectSize > 12) selectSize = 12;
     $(listElement).attr("size", selectSize);
     
     if (!foundPreviousSelection) {
         for (var i = 1; i <= verbButtonCount; i++) {
             var target = $("#" + buttonPrefix + i);
             target.hide();
         }
     }
 }