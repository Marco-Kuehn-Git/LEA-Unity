# LEA-Unity

## Installation
1. Create a new Unity Project in the git-folder
   
2. git init
   
3. git remote add origin \<link\>

4. clear Assests folder 
   
5. git pull origin master
   
6. git push --set-upstream origin master

7. Install NuGet for Unity: https://github.com/GlitchEnzo/NuGetForUnity/releases

8. Get the "websocket sharp-netstandard" via NuGet

9. Get the Plugins from https://github.com/jirihybek/unity-websocket-webgl

10. Install the Cinemachine-Package in the Unity package manager

---

## Building for WebGL
1. Change at PlayerSettings -> Publishing Settings -> Compression Format to __Gzip__

2. Check at PlayerSettings -> Resolution and Presentation -> __Run In Background__