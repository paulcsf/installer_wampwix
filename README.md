# Installer_WAMPWix README #

### What is this repository for? ###

Custom installer for WAMP (Windows, Apache, MySQL, PHP) designed to be extended for your project's use. Installs WAMP, phpMyAdmin, C# Windows service, default database creation.

* Release\SuiteSetup-Prereqs.exe is built from the sources included and will install all of the prerequisites.
* Release\SuiteSetup-Apps.exe is built from the sources included and will install the web content and engine/service.

### What's Installed ###

* Apache 2.4.18, 32 or 64bit as appropriate
* MySQL 5.7.11, 32 or 64bit as appropriate
* MySQL ODBC Connector 5.3.4, 32 or 64bit as appropriate
* PHP 7.0.3, 32 or 64bit as appropriate
* VC Redistributable 2010SP1, 2012U4, 2015U1, 32 or 64bit as appropriate
* phpMyAdmin 4.5.5
* PHP example web content

### Defaults ###

* MySQL root password: rootpass, port: 3333
* http://127.0.0.1:8888
* C:\SuiteDir\...

### How do I get set up? ###

* Install Visual Studio 2012
* Install Wix
* Open and build Suite_Prereqs\Suite_Prereqs.sln
* Open and build Suite_Apps\Suite_Apps.sln
* Change Suite, SuiteCompany, SuiteDir, etc to appropriate names for your project.

### Contribution guidelines ###

* I consider this version 3 of my installer base, and have moved to a Laravel based content. I am not developing with/on this, but if you have fixes or improvements I'll gladly pull them in to help give back to the open source world.
* Version 1: [bitbucket.org/pharness/installer_wampnsis](https://bitbucket.org/pharness/installer_wampnsis)

