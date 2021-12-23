[![Build status](https://waal.visualstudio.com/BizTalk%20Components/_apis/build/status/DBLookuphelper)](https://waal.visualstudio.com/BizTalk%20Components/_build/latest?definitionId=13)

# DBLookupHelper
DBLookupHelper is an extension object that can be used in XSLT to retreive information from a table or a view in a database.

## Methods
#### SetConnection
Sets the connection to the database.

`SetConnection(string connection)`

|   Parameters |   |  | |
| ------------ | ------------ | ------------ | ------------ |
|connection  | string   | optional  | refers to the name of the connection string defined in the application configuration file, or a valid connection string to the database.|

By default, this function is called internally, and it uses the default connection string name **DBLookupHelper_DefaultConnection**, the connection string must be defined in the application configuration file under the connectionstrings section.

#### GetValue
Retreives a value from a table of a view based on the filter applied,** if the filter returns mulitple records, the value will be retreived from the first record.**

` GetValue(string tableName, string filter, string order)`

|   Parameters |   |  | |
| ------------ | ------------ | ------------ | ------------ |
| tableName | string| required | the table or the view to retreive the record from.|
| filter | string | optional |  a where clause to filter the result based on i.e. `field1 = 'value1' and field2 is null`|
| order | string | optional | an order by clause to sort the result according to i.e. ` field1 desc, field3 asc` |


#### GetRecord
Retreives a record from a table or a view as XPathNodeIterator based on the filter applied, ** if the filter returns mulitple records, only the first record will be retreived.**

`XPathNodeIterator GetRecord(string tableName,string filter, string order)`

|   Parameters |   |  | |
| ------------ | ------------ | ------------ | ------------ |
| tableName | string| required | the table or the view to retreive the record from.|
| filter | string | optional |  a where clause to filter the result based on i.e. `field1 = 'value1' and field2 is null`|
| order | string | optional | an order by clause to sort the result according to i.e. `order by field1 desc, field3 asc` |

#### GetRecords
Retreives a record from a table or a view as XPathNodeIterator based on the filter applied, ** if the filter returns mulitple records, only the first record will be retreived.**

`XPathNodeIterator GetRecords(string tableName,string filter, string order,int maxRecords)`

|   Parameters |   |  | |
| ------------ | ------------ | ------------ | ------------ |
| tableName | string| required | the table or the view to retreive the record from.|
| filter | string | optional |  a where clause to filter the result based on i.e. `field1 = 'value1' and field2 is null`|
| order | string | optional | an order by clause to sort the result according to i.e. `order by field1 desc, field3 asc` |
| maxRecords | int | optional | limits the result to the number specified.|

## Exntension Object definition in Xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ExtensionObjects>
  <ExtensionObject Namespace ="http://BizTalkComponents.ExtensionObjects.DBLookupHeleper"
                 AssemblyName="BizTalkComponents.ExtensionObjects.DBLookupHelper, 
                 Version=1.0.0.0, 
                 Culture=neutral, 
                 PublicKeyToken=7410abceb6b530bb"
                 ClassName="BizTalkComponents.ExtensionObjects.DBLookupHeleper.DatabaseHelper" />
</ExtensionObjects>
```


## Using DBLookupHelper in XSLT

```xml
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:dbhelper="http://BizTalkComponents.ExtensionObjects.DBLookupHeleper"
                exclude-result-prefixes="dbhelper xsl">
  <xsl:template match="/">
    <TestingDbHelper>
      <xsl:variable name="setCnxn" select="dbhelper:SetConnection('')"/>
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
        <xsl:apply-templates select="$filteredCustomers"/>
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
  <xsl:template match="LookupResult">
    <Customer>
      <xsl:copy-of select="*"/>
    </Customer>
  </xsl:template>
</xsl:stylesheet>
```
