

                                                   Apache Lounge Distribution

                                                    Apache 2.4.33 win32 VC15

21 March 2018 
28 March 2018 Update OpenSSL to 1.1.0h, see change log

Original Home: httpd.apache.org
Binary by: Steffen
Build with Visual Studio® 2017 (VC15) x86
Mail: info@apachelounge.com
Home: www.apachelounge.com

Announcement & Dependencies: http://www.apachelounge.com/viewtopic.php?p=36661

Change Log: http://www.apachelounge.com/Changelog-2.4.html

Minimum system required
-----------------------

Windows 7 SP1
Windows 8 / 8.1
Windows 10
Windows Server 2016
Windows Server 2008 R2 SP1 
Windows Server 2012 / R2
Windows Vista SP2

Install
-------
   
  You must first install the Visual C++ Redistributable for Visual Studio 2017 x86. 
  Download and Install, if you have not done so already, see:

   http://www.apachelounge.com/download/

  Unzip the Apache24 folder to c:/Apache24 (that is the ServerRoot in the config).
  Default folder for your your webpages is DocumentRoot "c:/Apache24/htdocs"

  When you unzip to an other location, change ServerRoot in the httpd.conf,
  and change in httpd.conf the Documenroot, Directories, ScriptAlias,
  also when you use the extra folder config file(s) change to your location there. 

Run and test
------------

  Open a command prompt window and cd to the c:\Apace24\bin
  
  To Start Apache in the command prompt type:
  
    >httpd.exe
  
  Press Enter. If there are any errors it will tell you. 
  Warnings will not stop Apache from working, they do need to be addressed none the less. 
  If there are no errors the cursor will sit and blink on the next line. 
  
  You can test your installation by opening up your Browser and typing in the address:
  
     http://localhost
  
  You can shut down Apache by pressing Ctrl+C (It may take a few seconds)
  
  To install as a service. Open command prompt as Administrator and type:
  
    >httpd.exe -k install
  
  ApacheMonitor:
  
  Double click ApacheMonitor.exe, or put it in your Startup folder.
  
  
  To see all Command line options:
  
    >http -h


Upgrading
---------

- Upgrading from 2.2.x see: httpd.apache.org/docs/2.4/upgrading.html
  and see httpd.apache.org/docs/2.4/new_features_2_4.html .

- Updating from 2.3.x
  copy all the files over, except your changed .conf files.



When you have questions or want more info, post in the forum at www.apachelounge.com or mail me.

Enjoy,

Steffen



Legal note:

   This distribution includes cryptographic software.  The country in 
   which you are currently may have restrictions on the import, 
   possession, and use, and/or re-export to another country, of 
   encryption software.  BEFORE using any encryption software, please 
   check the country's laws, regulations and policies concerning the
   import, possession, or use, and re-export of encryption software, to 
   see if this is permitted.

   The U.S. Government Department of Commerce, Bureau of Industry and
   Security (BIS), has classified this software as Export Commodity 
   Control Number (ECCN) 5D002.C.1, which includes information security
   software using or performing cryptographic functions with asymmetric
   algorithms.  The form and manner of this Apache Software Foundation
   distribution makes it eligible for export under the License Exception
   ENC Technology Software Unrestricted (TSU) exception (see the BIS 
   Export Administration Regulations, Section 740.13) for both object 
   code and source code.

   The authors of the represented software packages and me, are not
   liable for any violations you make. Be careful, it is your responsibility. 
