# UWGB Software Engineering, Spring 2020

Mobile Onboard-Diagnostics 2 (M.OBD.2)

## Project Description
The vision that we have for M.OBD.2 is to allow users of all types to be able to gain information
about their vehicle using a Bluetooth ELM327 OBD2 connector and our mobile application. 

## Mono.android.dll version
We have been using the v9 of Mono.Android.dll

## Mono.Android.dll path error:
Having the incorrect path to the *Mono.Android.dll* file will cause a failture when building the project.In order to fix this, you must edit the ~/UWGBSoftware/M.OBD.2/M.OBD.2/**M.OBD.2.csproj** file and replace the Mono.Android.dll reference to **your correct file path on your system**. 

example:
<Reference Include="Mono.Android">
      <HintPath>C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\v9.0\Mono.Android.dll</HintPath>
</Reference>

## How to debug in Visual Studio with running on connected Android device

**Driver Dependance**
Your system may require an explicit driver in order to have Visual Studio successfully debug through
your native Android device.

**List of Devices That Require Drivers**
-Samsung s10e

Developers:
Nathaniel Kennis
Dylan Hoffman
Jackson Massey
Tswjfwmeng Vang

University of Wisconsin-Green Bay