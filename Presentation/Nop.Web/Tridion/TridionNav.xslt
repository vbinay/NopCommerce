<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes"/>
  <xsl:param name="TridionHost"></xsl:param>
  <xsl:template match="/">
    <xsl:for-each select="/Node/Node[@display='true']">
      <xsl:sort select="@SortOrder"/>
      <xsl:call-template name="RenderFirstNode">
        <xsl:with-param name="node" select="." />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="RenderFirstNode">
    <xsl:param name="node">
    </xsl:param>
    <xsl:if test="$node/@title!='Shop Now'">
      <li>
        <xsl:variable name="relPath" select="normalize-space($node/@path)" />
        <xsl:variable name="currPath" select="concat($TridionHost, $relPath)" />
        <xsl:variable name="pathValue">
          <xsl:choose>
            <xsl:when test="$currPath = '' or (substring($currPath, string-length($currPath), string-length($currPath)) = '/' and contains($currPath, 'http')=false)">
              <xsl:call-template name="findIndexPage" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$currPath" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test ="$pathValue!=''">
          <xsl:choose>
            <xsl:when test="$node/@NewWindow='YES'">
              <a href="{$pathValue}" target="_blank">
                <xsl:value-of select="$node/@title"/>
              </a>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$pathValue}">
                <xsl:value-of select="$node/@title"/>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
        <xsl:if test ="$pathValue=''">
          <a href="#">
            <xsl:value-of select="$node/@title"/>
          </a>
        </xsl:if>
        <xsl:if test="Node">
          <div class="sublist-toggle"></div>
          <ul class="sublist">
            <xsl:for-each select="Node[@display='true']">
              <xsl:sort select="@SortOrder"/>
              <xsl:call-template name="RenderSecondLevel">
                <xsl:with-param name="node" select="."/>
              </xsl:call-template>
            </xsl:for-each>
          </ul>
        </xsl:if>
      </li>
    </xsl:if>
  </xsl:template>
  <xsl:template name="RenderSecondLevel">
    <xsl:param name="node">
    </xsl:param>
    <xsl:variable name="relPath" select="normalize-space($node/@path)" />
    <xsl:variable name="currPath" select="concat($TridionHost, $relPath)" />
    <xsl:variable name="pathValue">
      <xsl:choose>
        <xsl:when test="$currPath = '' or (substring($currPath, string-length($currPath), string-length($currPath)) = '/' and contains($currPath, 'http')=false)">
          <xsl:call-template name="findIndexPage" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currPath" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <li >
      <xsl:if test="$pathValue!=''">
        <xsl:choose>
          <xsl:when test="$node/@NewWindow='YES'">
            <a href="{$pathValue}" target="_blank">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <a href="{$pathValue}">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test ="$pathValue=''">
        <a>
          <xsl:value-of select="$node/@title"/>
        </a>
      </xsl:if>
      <xsl:if test="Node">
        <div class="sublist-toggle"></div>
        <ul class="sublist">
          <xsl:for-each select="Node[@display='true']">
            <xsl:sort select="@SortOrder"/>
            <xsl:call-template name="RenderThirdLevel">
              <xsl:with-param name="node" select="."/>
            </xsl:call-template>
          </xsl:for-each>
        </ul>
      </xsl:if>
    </li>
  </xsl:template>
  <xsl:template name="RenderThirdLevel">
    <xsl:param name="node">
    </xsl:param>
    <xsl:variable name="relPath" select="normalize-space($node/@path)" />
    <xsl:variable name="currPath" select="concat($TridionHost, $relPath)" />
    <xsl:variable name="pathValue">
      <xsl:choose>
        <xsl:when test="$currPath = '' or (substring($currPath, string-length($currPath), string-length($currPath)) = '/' and contains($currPath, 'http')=false)">
          <xsl:call-template name="findIndexPage" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currPath" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <li>
      <xsl:if test ="$pathValue!=''">
        <xsl:choose>
          <xsl:when test="$node/@NewWindow='YES'">
            <a href="{$pathValue}" target="_blank">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <a href="{$pathValue}">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test ="$pathValue=''">
        <a>
          <xsl:value-of select="$node/@title"/>
        </a>
      </xsl:if>
      <xsl:if test="Node">
        <div class="sublist-toggle"></div>
        <ul class="sublist">
          <xsl:for-each select="Node[@display='true']">
            <xsl:sort select="@SortOrder"/>         
            <xsl:call-template name="RenderForthLevel">
              <xsl:with-param name="node" select="."/>
            </xsl:call-template>          
          </xsl:for-each>
        </ul>
      </xsl:if>
    </li>
  </xsl:template>
  <xsl:template name="RenderForthLevel">
    <xsl:param name="node">
    </xsl:param>
    <xsl:variable name="relPath" select="normalize-space($node/@path)" />
    <xsl:variable name="currPath" select="concat($TridionHost, $relPath)" />
    <xsl:variable name="pathValue">
      <xsl:choose>
        <xsl:when test="$currPath = '' or (substring($currPath, string-length($currPath), string-length($currPath)) = '/' and contains($currPath, 'http')=false)">
          <xsl:call-template name="findIndexPage" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currPath" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <li>
      <xsl:if test ="$pathValue!=''">
        <xsl:choose>
          <xsl:when test="$node/@NewWindow='YES'">
            <a href="{$pathValue}" target="_blank">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <a href="{$pathValue}">
              <xsl:value-of select="$node/@title"/>
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test ="$pathValue=''">
        <a>
          <xsl:value-of select="$node/@title"/>
        </a>
      </xsl:if>
    </li>
  </xsl:template>
  <xsl:template name="findIndexPage">
    <xsl:variable name="main" select="child::*[contains(@path, '/main.html') and @type='Page']/@path"/>
    <xsl:variable name="index" select="child::*[contains(@path, '/index.html') and @type='Page']/@path"/>
    <xsl:choose>
      <xsl:when test="$main != ''">
        <xsl:value-of select="$main" />
      </xsl:when>
      <xsl:when test ="$index != ''">
        <xsl:value-of select="$index" />
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="showChildren">
    <xsl:for-each select="child::*">
      <i>
        <xsl:value-of select="@title" />
        - <xsl:value-of select="@path" />
      </i>
      <br />
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
