<?xml version="1.0" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  <!-- Copy all attributes and elements to the output. -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <xsl:output method="xml" indent="yes" />

  
  <!-- Search directories for the components that will be removed. -->
  <xsl:key name="node_modules-search" match="wix:Directory[@Name = 'node_modules']" use="descendant::wix:Component/@Id" />
  <!-- Remove directories. -->
  <xsl:template match="wix:Directory[@Name='node_modules']" />
  <!-- Remove componentsrefs referencing components in those directories. -->
  <xsl:template match="wix:ComponentRef[key('node_modules-search', @Id)]" />

  <!-- Search directories for the components that will be removed. -->
  <xsl:key name="nbproject-search" match="wix:Directory[@Name = 'nbproject']" use="descendant::wix:Component/@Id" />
  <!-- Remove directories. -->
  <xsl:template match="wix:Directory[@Name='nbproject']" />
  <!-- Remove componentsrefs referencing components in those directories. -->
  <xsl:template match="wix:ComponentRef[key('nbproject-search', @Id)]" />



  <!-- Define the key containing the Id for the file element to remove. -->
  <!-- XSLT 2.0 <xsl:key name="file-element-id" match="wix:File[@Source[ends-with(name(),'unwanted.file')]" use="@Id" /> -->
  <xsl:key name="dotenv-element-id" match="wix:File[substring(@Source,string-length(@Source) - string-length('.env') +1) ='.env' ]" use="@Id" />
  <!-- Define the key containing the Id for the file element parent component to remove. -->
  <!-- XSLT 2.0 <xsl:key name="file-element-component-id" match="wix:File[@Source[ends-with(name(),'unwanted.file)]" use="parent::*/@Id" /> -->
  <xsl:key name="dotenv-element-component-id" match="wix:File[substring(@Source, string-length(@Source) - string-length('.env') +1) = '.env' ]" use="parent::*/@Id" />
  <!-- Remove the file element -->
  <xsl:template match="wix:File[key('dotenv-element-id', @Id)]" />
  <!-- Remove the parent component element -->
  <xsl:template match="wix:Component[key('dotenv-element-component-id', @Id)]" />
  <!-- Remove ComponentRefs referencing of the component(s). -->
  <xsl:template match="wix:ComponentRef[key('dotenv-element-component-id', @Id)]" />


  <!-- Add <?include Configuration.wxi?> to heat generated .wxs file.-->
  <xsl:template match="wix:Wix">
    <xsl:copy>
      <xsl:processing-instruction name="include">Configuration.wxi</xsl:processing-instruction>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <!-- Identity transform. 
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>
-->

 <!-- Transform directory IDs to clear error:
 Error	6	ICE99: The directory name: Time is the same as one of the MSI Public Properties and can cause unforeseen side effects.
 -->
  <xsl:template match="wix:Directory">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:attribute name="Id">
        <xsl:text>SuiteWeb_dir_</xsl:text>
        <xsl:value-of select="@Id"/>
      </xsl:attribute>
      <xsl:apply-templates select="node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>