echo off

md %~dp1IosIcon

copy %~dp0Contents.json %~dp1IosIcon\Contents.json

copy %1 %~dp1IosIcon\Icon-Store.png

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon.png 57

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon@2x.png 114

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-72.png 72

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-76.png 76

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-120.png 120

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-144.png 144

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-152.png 152

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-167.png 167

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-180.png 180

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Notification.png 20

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Notification@2x.png 40

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Notification@3x.png 60

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small.png 29

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small@2x.png 58

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small@3x.png 87

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small-40.png 40

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small-80.png 80

%~dp0ImageHelper.exe %1 %~dp1IosIcon\Icon-Small-120.png 120