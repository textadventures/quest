cd C:\VBProjects\Quest (CodePlex)\trunk
rem C:\Windows\Microsoft.NET\Framework\v3.5\msbuild /property:Configuration=Release .\Quest\Quest.vbproj
"C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\devenv" ".\Quest 5.0.sln" /build "ReleaseSetup"
rd /s /q .\Output\Samples
svn export .\WorldModel\WorldModel\Examples-Public .\Output\Samples
"C:\Program Files\7-Zip\7z.exe" a -r .\Output\samples.zip .\Output\Samples\*.*
copy .\Setup\ReleaseSetup\setup.msi .\Output\quest500preview.msi
pause