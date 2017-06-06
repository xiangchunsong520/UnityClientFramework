@echo off
%~dp0Java\jdk1.8.0_101\bin\jarsigner.exe -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore %~dp0game.keystore -storepass 123456 -keypass 123456 %1 game
%~dp0zipalign.exe -f -v 4 %1 %1.apk
del %1
move %1.apk %1