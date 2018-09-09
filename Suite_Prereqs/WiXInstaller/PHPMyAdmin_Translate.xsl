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


</xsl:stylesheet>