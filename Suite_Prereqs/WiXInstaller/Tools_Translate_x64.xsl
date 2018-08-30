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

  
  <!-- Add <?include Configuration.wxi?> to heat generated .wxs file.-->
  <xsl:template match="wix:Wix">
    <xsl:copy>
      <xsl:processing-instruction name="include">Configuration.wxi</xsl:processing-instruction>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <!-- Define the key containing the Id for the file element to remove. -->
  <!-- XSLT 2.0 <xsl:key name="file-element-id" match="wix:File[@Source[ends-with(name(),'unwanted.file')]" use="@Id" /> -->
  <xsl:key name="redisserverexe-element-id" match="wix:File[substring(@Source,string-length(@Source) - string-length('redis-server.exe') +1) ='redis-server.exe' ]" use="@Id" />
  <!-- Define the key containing the Id for the file element parent component to remove. -->
  <!-- XSLT 2.0 <xsl:key name="file-element-component-id" match="wix:File[@Source[ends-with(name(),'unwanted.file)]" use="parent::*/@Id" /> -->
  <xsl:key name="redisserverexe-element-component-id" match="wix:File[substring(@Source, string-length(@Source) - string-length('redis-server.exe') +1) = 'redis-server.exe' ]" use="parent::*/@Id" />
  <!-- Remove the file element -->
  <xsl:template match="wix:File[key('redisserverexe-element-id', @Id)]" />
  <!-- Remove the parent component element -->
  <xsl:template match="wix:Component[key('redisserverexe-element-component-id', @Id)]" />
  <!-- Remove ComponentRefs referencing of the component(s). -->
  <xsl:template match="wix:ComponentRef[key('redisserverexe-element-component-id', @Id)]" />

  <!-- Transform directory IDs to clear error:
 Error	6	ICE99: The directory name: x64 is the same as one of the MSI Public Properties and can cause unforeseen side effects.
 -->
  <xsl:template match="wix:Directory">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:attribute name="Id">
        <xsl:text>Tools_dir_</xsl:text>
        <xsl:value-of select="@Id"/>
      </xsl:attribute>
      <xsl:apply-templates select="node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>