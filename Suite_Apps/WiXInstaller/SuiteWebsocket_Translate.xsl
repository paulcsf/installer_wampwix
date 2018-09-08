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

  <!-- Add <?include Configuration.wxi?> to heat generated .wxs file.-->
  <xsl:template match="wix:Wix">
    <xsl:copy>
      <xsl:processing-instruction name="include">Configuration.wxi</xsl:processing-instruction>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>