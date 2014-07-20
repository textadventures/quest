---
layout: index
title: template element
---

    <template name="name">text</template>

Creates a template of the specified name. You can print the template's text using the [Template](../functions/template.html) function.

Within a language library, a template may define a **templatetype** of "command", for example:

     <template templatetype="command" name="undo">^undo$</template>

This simply is a flag to the Editor to prevent it from showing the template in the list of templates (as the way to edit it would be to edit the associated command pattern).

Note that it is important to have templates defined in the right place in the code. If your template is to override an existing template, then it has to come *after* the language file include. However, it has to come *before* the template is used in the code, which should be before the core library file include. As of version 5.2 Quest does not do this, so you will need to manually move the templates to the right place. Your game file should start something like this:

      <!--Saved by Quest 5.2.4515.34846-->
      <asl version="520">
        <include ref="English.aslx"/>
        <template name="SeeListHeader">There's</template>
        <template name="GoListHeader"> Go to </template>
        <template name="UnrecognisedCommand">Unknown command.</template>
        <template name="YouAreIn"></template>
        <template name="PlacesObjectsLabel">Places / Objects</template>
        <include ref="Core.aslx" />
        <game name="Test_1">
        ...
