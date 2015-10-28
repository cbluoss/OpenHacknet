# OpenHacknet

OpenHacknet is a decompile of Hacknet, a game written in C# and XNA.

OpenHacknet is supposed to support modding of Hacknet, not to encourage piracy.

To acquire the game resource files, buy Hacknet on Steam.

# Setup
Create a new folder lib/ in the same folder as the Hacknet.sln. Go to the Hacknet game content folder in the 
steamapps/common folder and copy Steamworks.NET.dll into lib/. Then, copy CSteamworks.dll and steam_api.dll 
into the bin/x86/Release and bin/x86/Debug folders. Finally, either copy the Content folder to the same 
directories or create a directory junction using mklink.

# Contributing
Please do not contribute your own mods to the master branch. If you wish to modify Hacknet's functionality 
(other than fixing typos), create a fork. Contributions should consist of source cleanups or typo fixes only 
and should not change the gameplay or functionality of Hacknet.

Please support the developer by purchasing Hacknet on Steam.
