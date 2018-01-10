echo off

set /p iconName=please input icon name: 

echo %iconName%

md %~dp1AndroidIcon

md %~dp1AndroidIcon\drawable-hdpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-hdpi-v4\%iconName%.png 72

md %~dp1AndroidIcon\drawable-ldpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-ldpi-v4\%iconName%.png 36

md %~dp1AndroidIcon\drawable-mdpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-mdpi-v4\%iconName%.png 48

md %~dp1AndroidIcon\drawable-xhdpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-xhdpi-v4\%iconName%.png 96

md %~dp1AndroidIcon\drawable-xxhdpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-xxhdpi-v4\%iconName%.png 144

md %~dp1AndroidIcon\drawable-xxxhdpi-v4
%~dp0ImageHelper.exe %1 %~dp1AndroidIcon\drawable-xxxhdpi-v4\%iconName%.png 192