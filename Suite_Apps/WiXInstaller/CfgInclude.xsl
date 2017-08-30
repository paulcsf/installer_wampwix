<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:wix="http://schemas.microsoft.com/wix/2006/wi" xmlns:util="http://schemas.microsoft.com/wix/UtilExtension">
    <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
    <xsl:template match="wix:Wix">
        <xsl:copy>
            <xsl:processing-instruction  name="include">Configuration.wxi</xsl:processing-instruction>
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>

    <!-- Add the service install/control entries to mybinary.exe -->
    <!--<xsl:template match="wix:Component[wix:File[@Source='$(var.ID_FilesBackendApp)\service.exe']]">
        <xsl:copy>
            <xsl:apply-templates select="node() | @*" />
            <util:User Id="UpdateUserLogonAsService" Name="[WINSVC_USERNAME]" LogonAsService="yes" FailIfExists="no" UpdateIfExists="yes" CreateUser="no" RemoveOnUninstall="no" />
            <wix:ServiceInstall Id="SvcInstall" Name="$(var.JavaWinSvc_Name)" DisplayName="$(var.JavaWinSvc_DisplayName)" Description="$(var.JavaWinSvc_Description)" Type="ownProcess" Start="auto" Account="[WINSVC_USERNAME]" Password="[WINSVC_PASSWORD]" ErrorControl="normal" />
            <wix:ServiceControl Id="SvcControl" Name="$(var.JavaWinSvc_Name)" Start="install" Stop="both" Remove="uninstall" Wait="yes" />
        </xsl:copy>
    </xsl:template>-->

    <!-- Remove service.exe file; I will handle it-->
    <xsl:key name="REM_service" match="wix:Component[contains(wix:File/@Source, '$(var.ID_FilesBackendApp)\service')]" use="@Id" />

    <xsl:template match="*[self::wix:Component or self::wix:ComponentRef] [key('REM_service', @Id)]" />


    <xsl:template match="wix:File[contains(@Source, '$(var.ID_FilesJavaRuntime_x86)')]">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
            <xsl:attribute name="DiskId">2</xsl:attribute>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="wix:File[contains(@Source, '$(var.ID_FilesJavaRuntime_x64)')]">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
            <xsl:attribute name="DiskId">3</xsl:attribute>
        </xsl:copy>
    </xsl:template>


    <!-- Identity transform. -->
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
    </xsl:template>
</xsl:stylesheet>
