#xbuild /p:Configuration=Release ./Media.sln

mkdir -p dist
cd dist
mkdir -p Media
cd Media 
mkdir -p PCL
mkdir -p iOS
mkdir -p Android
mkdir -p iOSUnified

cp ../../Media/Media.Plugin/bin/Release/Plugin.Media.dll ./PCL
cp ../../Media/Media.Plugin.Abstractions/bin/Release/Plugin.Media.Abstractions.dll ./PCL

cp ../../Media/Media.Plugin.Android/bin/Release/Plugin.Media.dll ./Android
cp ../../Media/Media.Plugin.Abstractions/bin/Release/Plugin.Media.Abstractions.dll ./Android

cp ../../Media/Media.Plugin.iOSUnified/bin/iPhone/Release/Plugin.Media.dll ./iOSUnified
cp ../../Media/Media.Plugin.Abstractions/bin/Release/Plugin.Media.Abstractions.dll ./iOSUnified