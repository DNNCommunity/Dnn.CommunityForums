# About Dnn Community Forums Module Themes

### Community Forums's Stylesheets Load order:   
**/DesktopModules/ActiveForums/module.css**  
General Module Stylesheet.  
>Please note that for Module version 08.00.00, Legacy CSS in this file was moved to: 
>*/DesktopModules/ActiveForums/Themes/_legacy-module.css*  
>Legacy (pre v8) Themes should probably load this file by adding this at the top of it's theme.CSS  
>*@import url(../_legacy-module.css);*

**/DesktopModules/ActiveForums/themes/themes.css**  
Here you can add CSS for all Themes, this file is created on install of the module but not overwritten on upgrade.

**DesktopModules/ActiveForums/themes/<currenttheme>/theme.css**  
CSS for your theme 

**DesktopModules/ActiveForums/themes/<currenttheme>/custom/theme.css**  
You can use this file to overrule the CSS of an existing Theme
