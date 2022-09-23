<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:dbhelper="http://BizTalkComponents.ExtensionObjects.DBLookupHelper"
                exclude-result-prefixes="dbhelper xsl">
  <xsl:template match="/">
    <TestingDbHelper>
      <xsl:variable name="setCnxn" select="dbhelper:SetConnection()"/>
      <xsl:variable name="city" select="'NewYork'"/>
      <!--the filter is the where clause used in T-SQL-->
      <xsl:variable name="filter">
        <!--in case you want to use a value from the original message, xsl:text would be very helpful in building/concatenating your filter. -->
        <xsl:text>city='</xsl:text>
        <xsl:value-of select="$city"/>
        <xsl:text>' and customername is not null</xsl:text>
      </xsl:variable>
      <!--Get all customers-->
      <xsl:variable name="customers" select="dbhelper:GetRecords('Customers')"/>
      <customers>
        <xsl:copy-of select="$customers"/>
      </customers>
      <FilteredCustomers>
        <!--Get customers using filter and order by-->
        <xsl:variable name="filteredCustomers" select="dbhelper:GetRecords('Customers',$filter,'CustomerId desc')"/>
        <xsl:apply-templates select="$filteredCustomers/Customers"/>
      </FilteredCustomers>

      <xsl:variable name="customer1" select="dbhelper:GetRecords('Customers',$filter,'CustomerId desc',1)"/>
      <xsl:variable name="customer2" select="dbhelper:GetRecord('Customers',$filter,'CustomerId asc')"/>
      <Customer1>
        <xsl:copy-of select="$customer1/*"/>
      </Customer1>
      <Customer2>
        <xsl:copy-of select="$customer2/*"/>
      </Customer2>
    </TestingDbHelper>
  </xsl:template>
  <xsl:template match="LookupResult/Customers">
    <Customer>
      <xsl:copy-of select="*"/>
    </Customer>
  </xsl:template>
</xsl:stylesheet>