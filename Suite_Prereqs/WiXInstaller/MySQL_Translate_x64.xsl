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
  <xsl:key name="mysqldexe-element-id" match="wix:File[substring(@Source,string-length(@Source) - string-length('mysqld.exe') +1) ='mysqld.exe' ]" use="@Id" />
  <!-- Define the key containing the Id for the file element parent component to remove. -->
  <!-- XSLT 2.0 <xsl:key name="file-element-component-id" match="wix:File[@Source[ends-with(name(),'unwanted.file)]" use="parent::*/@Id" /> -->
  <xsl:key name="mysqldexe-element-component-id" match="wix:File[substring(@Source, string-length(@Source) - string-length('mysqld.exe') +1) = 'mysqld.exe' ]" use="parent::*/@Id" />
  <!-- Remove the file element -->
  <xsl:template match="wix:File[key('mysqldexe-element-id', @Id)]" />
  <!-- Remove the parent component element -->
  <xsl:template match="wix:Component[key('mysqldexe-element-component-id', @Id)]" />
  <!-- Remove ComponentRefs referencing of the component(s). -->
  <xsl:template match="wix:ComponentRef[key('mysqldexe-element-component-id', @Id)]" />

</xsl:stylesheet>