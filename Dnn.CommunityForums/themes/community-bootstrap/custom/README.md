# A way to overule Forum Theme Styles 

**New DNN Community Forums Module v 08.00.00**

To overrule a Forum Themes Styling, you can create a folder named "custom" and place a file "theme.css" in it.
The Forums module will load this custom/theme.css Styleheet if it exists. 
This is a way to overrule some default Styling of a Forum Theme. 

###### Warning on the Forum Module Default Themes

> When you upgrade the Module:
> - The "custom/theme.css" file will not be overwritten.
> - All other files of the default Forum Themes will be overwritten though. 

So when you need to edit more than just "custom/theme.css"; 
- Make a copy of the Theme folder first and make your changes in there. 

**This will avoid your changes getting overwritten on a Module upgrade.**

See also: https://github.com/DNNCommunity/Dnn.CommunityForums/wiki/Creating-Themes
 