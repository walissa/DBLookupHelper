using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Configuration;
using System.Xml.Serialization;

namespace BizTalkComponents.ExtensionObjects.DBLookupHelper.UnitTests
{
    [TestClass]
    public class TestDBLookupHelper
    {
        public TestDBLookupHelper()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            path = Path.Combine(Path.GetDirectoryName(path), "AppData");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            for (int i = 0; i < config.ConnectionStrings.ConnectionStrings.Count; i++)
            {
                config.ConnectionStrings.ConnectionStrings[i].ConnectionString =
                    config.ConnectionStrings.ConnectionStrings[i].ConnectionString.Replace("|AppData|", path);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("ConnectionStrings");
        }


        [TestMethod]
        public void TestDefaultConnectionString()
        {
            var dbhelper = new DBLookupHelper();
            var result = dbhelper.GetRecords("Customers");
        }

        [TestMethod]
        public void TestSetConnectionString()
        {
            var dbhelper = new DBLookupHelper();
            var IsConnectionSet = dbhelper.SetConnection("TestConnStr");
            var result = dbhelper.GetRecords("Customers");
        }


        [TestMethod]
        public void TestReadOneRecord()
        {
            var dbhelper = new DBLookupHelper();
            var result = dbhelper.GetRecord("Customers", "", "CustomerId desc");
        }
        [TestMethod]
        public void TestGetFilteredResult()
        {
            var dbhelper = new DBLookupHelper();
            var result = dbhelper.GetRecords("Customers", "City='NewYork' and customerName is not null", "");
        }
        [TestMethod]
        public void TestXsltFile()
        {
            var trasnform = new XslCompiledTransform();

            var xslReader = XmlReader.Create(GetEmbdedResourceAsStream("TestFiles.Transform.xsl"));
            trasnform.Load(xslReader);
            var argList = new XsltArgumentList();
            argList.AddExtensionObject("http://BizTalkComponents.ExtensionObjects.DBLookupHelper", new DBLookupHelper());
            var xmlInput = XmlReader.Create(new StringReader("<test/>"));
            var sw = new StringWriter();
            var xmlOutput = XmlWriter.Create(sw);
            trasnform.Transform(xmlInput, argList, xmlOutput);
            xmlOutput.Flush();
            var result = sw.ToString();
        }
        [TestMethod]
        public void TestRetreiveRecord()
        {
            var dbhelper = new DBLookupHelper();
            var result = dbhelper.RetreiveRecord("Customers", "", "CustomerId desc");
            dbhelper.RetreiveField("Email");
            dbhelper.RetreiveField("CustomerId");
        }

        [TestMethod]
        public void TestSerialization()
        {
            var serializer = new XmlSerializer(typeof(DBLookupHelper));
            var dbhelper = new DBLookupHelper();
            var writer = new StringWriter();
            var value = dbhelper.GetValue("Customers", "Email", "CustomerId=1");
            serializer.Serialize(writer, dbhelper);

            var result = dbhelper.RetreiveRecord("Customers", "", "CustomerId desc");
            dbhelper.RetreiveField("Email");
            dbhelper.RetreiveField("CustomerId");
            writer = new StringWriter();
            serializer.Serialize(writer, dbhelper);
            writer.Flush();
            string xml = writer.ToString();
            var reader = new StringReader(xml);
            var dbh = serializer.Deserialize(reader);
        }

        private Stream GetEmbdedResourceAsStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"{this.GetType().Namespace}.{resourceName}");
        }
    }
}
