using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ReadXmlFromController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            var content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>" +
                "<soap:Body><RetornaDadosRemessa xmlns='http://tempuri.org/'><AWBNumber>" + value + "</AWBNumber></RetornaDadosRemessa></soap:Body></soap:Envelope>";

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string url = "https://ups.com.br/upsbilling/UPS_Billing.asmx";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.ProtocolVersion = HttpVersion.Version10;

            byte[] requestInFormOfBytes = Encoding.ASCII.GetBytes(content);

            request.Method = "POST";
            request.ContentType = "text/xml;charset=utf-8";
            request.ContentLength = requestInFormOfBytes.Length;

            var requestStream = request.GetRequestStream();

            requestStream.Write(requestInFormOfBytes, 0, requestInFormOfBytes.Length);
            requestStream.Close();

            var response = (HttpWebResponse)request.GetResponse();

            var respStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

            string receivedResponse = respStream.ReadToEnd();

            var info = ConvertXmlToObject(receivedResponse);

            respStream.Close();
            response.Close();
        }

        public ArquivoInfos ConvertXmlToObject(string xmlFile)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlFile);

            var conteudo = doc.GetElementsByTagName("Table")[0].OuterXml;

            //return Serializador.Deserializar<ArquivoInfos>(conteudo.RemoveAllNamespaces());

            return new ArquivoInfos { };
        }

        [XmlRoot("Table")]
        public class ArquivoInfos
        {
            [XmlElement("AWB")]
            public string AWB { get; set; }

            [XmlElement("RazaoSocial")]
            public string RazaoSocial { get; set; }

            [XmlElement("BLDG")]
            public string BLDG { get; set; }

            [XmlElement("Numero")]
            public string Numero { get; set; }

            [XmlElement("Bairro")]
            public string Bairro { get; set; }

            [XmlElement("Endereco")]
            public string Endereco { get; set; }

            [XmlElement("Cep")]
            public string Cep { get; set; }

            [XmlElement("Cidade")]
            public string Cidade { get; set; }

            [XmlElement("UF")]
            public string UF { get; set; }

            [XmlElement("CNPJ_CPF")]
            public string CNPJ_CPF { get; set; }

            [XmlElement("IE_RG")]
            public string IE_RG { get; set; }

            [XmlElement("Fone")]
            public string Fone { get; set; }

            [XmlElement("NumeroCte")]
            public string NumeroCte { get; set; }

            [XmlElement("ChaveCte")]
            public string ChaveCte { get; set; }

            [XmlElement(ElementName = "Peso")]
            public Decimal Peso { get; set; }

            [XmlElement(ElementName = "AWB_Filho")]
            public string AWB_Filho { get; set; }

            [XmlElement(ElementName = "Tipo")]
            public string Tipo { get; set; }

            [XmlElement(ElementName = "qtdepacotes")]
            public int qtdepacotes { get; set; }

            [XmlElement(ElementName = "complemento")]
            public string complemento { get; set; }

            [XmlElement(ElementName = "dataremessa")]
            public string dataremessa { get; set; }

            [XmlElement(ElementName = "notadebito")]
            public int notadebito { get; set; }

            [XmlElement(ElementName = "vl_valor_notadebito")]
            public string vl_valor_notadebito { get; set; }

            [XmlElement(ElementName = "fl_cobra_cliente")]
            public string fl_cobra_cliente { get; set; }
        }
        public static class Serializador
        {
            public static T Deserializar<T>(string xml)
            {
                var serializer = new XmlSerializer(typeof(T));

                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, new XmlReaderSettings()))
                    {
                        var resultado = (T)serializer.Deserialize(xmlReader);

                        return resultado;
                    }
                }
            }

            private static XElement RemoveAllNamespaces(XElement xmlDocument)
            {
                if (!xmlDocument.HasElements)
                {
                    XElement xElement = new XElement(xmlDocument.Name.LocalName);
                    xElement.Value = xmlDocument.Value;

                    foreach (XAttribute attribute in xmlDocument.Attributes())
                        xElement.Add(attribute);

                    return xElement;
                }
                return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
            }
        }
    }
}
