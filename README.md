# ARMeshingTest Readme, Build Guide
Experimental Repo For FYP development

## Testing device specs:
- Android 7.0 or above 
- Min Android SDK Level 24
- Target Android SDK level: 30

## System Requirement:
- Windows
- Macbook

## Software Need:
### Unity Editor 2021.3.11f1 (From Unity Hub)
- Android Build Support (Android SDK, NDK, and OpenJDK) must be checked when installing in Unity Hub Installer to reduce debug trouble of Java & gradle build
- Not suggest adding Android Build Support after finish installing Unity Editor
- (Optional: iOS Support)
  
### Blender version: ?


## Build Guide
### Pure Unity
1. In Project Setting, change to Android Platform (Check the "Export" box, if you would like to build the apk file via Android Studio; Yet not suggested)
2. In Preference, Goto External Tool, Under Android, if you installed correctly with Android Build Support at the beginning then Tick all FOUR boxes under Android: JDK(1.8.0), Android SDK, Android NDK, Gradle installed with Unity. (If you add Android Support Modules after finish installing Unity Editor, you need to download these FOUR kinds of tools with correct version, for details, please contact Kenneth)
3. Click Build in Build Setting and Select target directory and build name

### (Hard-core) Unity Export to Android Studio to build Android .apk file to install in the phone, contact Kenneth for more information
#### Android Studio Setup and Specs:
- Gradle Version: > 4.2.0
- Java Version 1.8 (Java 8)

#### AVD Manager Dev Download:
SDK Platform:
- Depends on your testing devices

SDK Tools:
- SDK: Min SDK Level 24, target 30
- NDK: 21.3.6528147 (r21d)
#### Emulator: Not recommended, use your own device
- Gradle: 6.7.1 or above for ARDK V2.3.0
  
### Blender version: ?

## Build Guide
### MUST!! Change the gradle and android setting according to the [website] (https://lightship.dev/docs/ardk/ardk_fundamentals/building_android.html#building-android-api-31) of ARDK

### Pure Unity

### Unity Export to Android Studio to build Android .apk file to install in the phone
