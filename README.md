# Installer_WAMPWix README #

### What is this repository for? ###

Custom installer for WAMP (Windows, Apache, MySQL, PHP) designed to be extended for your project's use. Installs WAMP, phpMyAdmin, C# Windows service, default database creation.

* Release\SuiteSetup-Prereqs.exe will install all of the prerequisites (no longer included due to repository size limitations).
* Release\SuiteSetup-Apps.exe will install the web content and engine/service (no longer included due to repository size limitations).

### What's Installed (Now only 64bit)###

* Apache 2.4.37
* MySQL 8.0.13
* MySQL ODBC Connector 5.3.10
* PHP 7.2.13
* VC Redistributable 2010SP1, 2012U4, 2015U1, 2017
* phpMyAdmin 4.8.4
* PHP example web content
* Redis 4.0.2.2
* Let's Encrypt Win Simple 1.9.3
* NodeJS 8.11.2
* NodeJS Websocket server
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

