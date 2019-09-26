# Arma3MissionObfuscator
This app is designed to obfuscate Arma 3 client and server side mission files to slightly ehance security.

> Developed in C#, UI template from <a href="https://marketplace.visualstudio.com/items?itemName=WASTeamAccount.WindowsTemplateStudio" target="_blank">**Windows Template Studio**</a> to create a nice starting point.

![Function reference renaming](https://i.imgur.com/CiGcCYj.png)

** Current Functionality **

- Removing all comments
- Renaming all possible file names
  - Function files that do not match any other file names get renamed
  - All references to the original file/function get renamed
- Removal of whitespace with consideration for preprocessor commands requiring line breaks


