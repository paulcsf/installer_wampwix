# Installer_WAMPWix README #

### What is this repository for? ###

Custom installer for WAMP (Windows, Apache, MySQL, PHP) using WIX designed to be extended for your project's use. Installs WAMP, phpMyAdmin, C# Windows service, default database creation.

* Release\SuiteSetup-Prereqs.exe will install all of the prerequisites (no longer included due to repository size limitations).
* Release\SuiteSetup-Apps.exe will install the web content and engine/service (no longer included due to repository size limitations).

### What's Installed (Now only 64bit)###

* Apache 2.4.41
* MySQL 8.0.17
* MySQL ODBC Connector 8.0.17
* PHP 7.3.9
* VC Redistributable 2010SP1, 2012U4, 2013U5, 2015-2019
* phpMyAdmin 4.9.0.1
* PHP example web content
* Redis 4.0.2.3
* Let's Encrypt Win Acme 2.0.4.227
* NodeJS 10.16.3
* NodeJS Websocket server
* ionCube loader 10.3.8
* (See older commits if you need 32/64 bit capable)

### Defaults ###

* MySQL root password: rootpass, port: 3333
* http://127.0.0.1:8888
* C:\SuiteDir\...
* Redis port 6888
* Websockets port 3088

### How do I get set up? ###

* Install Visual Studio 2012
* Install Wix
* Open and build Suite_Prereqs\Suite_Prereqs.sln
* Open and build Suite_Apps\Suite_Apps.sln
* Change Suite, SuiteCompany, SuiteDir, etc to appropriate names for your project.

### Contribution guidelines ###

* I consider this version 3 of my installer base. I am developing with/on this, and if you have fixes or improvements I'll gladly pull them in to help give back to the open source world.
* Version 1: [bitbucket.org/pharness/installer_wampnsis](https://bitbucket.org/pharness/installer_wampnsis)

