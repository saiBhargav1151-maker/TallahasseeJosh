using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Dqe.ApplicationServices;
using Dqe.Infrastructure.DocumentConverterService;

namespace Dqe.Infrastructure.Services
{
    public class DocumentConverterService : IDocumentConverterService
    {
        public byte[] ConvertHtmlToPdf(string html)
        {
            return GetConvertedDocument(html,null,true);
        }
        
        public byte[] ConvertUrlToPdf(string url)
        {
            return GetConvertedDocument(null, url, false);
        }

        private static byte[] GetConvertedDocument(string html, string url, bool htmlOnly)
        {
            DocumentConverterServiceClient client = null;

            try
            {
                string sourceFileName = null;
                byte[] sourceFile = null;

                var serviceUrl = ConfigurationManager.AppSettings.Get("muhimbiServiceUrl");
                client = OpenService(serviceUrl);

                var openOptions = new OpenOptions {UserName = "", Password = ""};

                //** Specify optional authentication settings for the web page
                if (htmlOnly)
                {
                    //** Specify the HTML to convert
                    sourceFile = Encoding.UTF8.GetBytes(html);
                }
                else
                {
                    // ** Specify the URL to convert
                    openOptions.OriginalFileName = url;
                }

                 openOptions.FileExtension = "html";
                //** Generate a temp file name that is later used to write the PDF to
                sourceFileName = Path.GetTempFileName();
                File.Delete(sourceFileName);

                // ** Enable JavaScript on the page to convert. 
                openOptions.AllowMacros = MacroSecurityOption.All;

                // ** Set the various conversion settings
                var conversionSettings = new ConversionSettings
                {
                    Fidelity = ConversionFidelities.Full,
                    PDFProfile = PDFProfile.PDF_1_5,
                    PageOrientation = PageOrientation.Portrait,
                    Quality = ConversionQuality.OptimizeForPrint
                };

                // ** Carry out the actual conversion
                byte[] convertedFile = client.Convert(sourceFile, openOptions, conversionSettings);

                return convertedFile;
            }
            finally
            {
                CloseService(client);
            }
        }

        private static DocumentConverterServiceClient OpenService(string address)
        {
            DocumentConverterServiceClient client = null;
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
// ** Use standard Windows Security.
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType =
                    HttpClientCredentialType.Windows;
// ** Increase the client Timeout to deal with (very) long running requests.
                binding.SendTimeout = TimeSpan.FromMinutes(30);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(30);
                binding.MaxReceivedMessageSize = 50*1024*1024;
                binding.ReaderQuotas.MaxArrayLength = 50*1024*1024;
                binding.ReaderQuotas.MaxStringContentLength = 50*1024*1024;
// ** Specify an identity (any identity) in order to get it past .net3.5 sp1
                EndpointIdentity epi = EndpointIdentity.CreateUpnIdentity("unknown");
                EndpointAddress epa = new EndpointAddress(new Uri(address), epi);
                client = new DocumentConverterServiceClient(binding, epa);
                client.Open();
                return client;
            }
            catch (Exception)
            {
                CloseService(client);
                throw;
            }
        }

        /// <summary>
        /// Check if the client is open and then close it.
        /// </summary>
        /// <param name="client">The client to close</param>
        private static void CloseService(DocumentConverterServiceClient client)
        {
            if (client != null && client.State == CommunicationState.Opened)
            {
                client.Close();
            }
        }
    }
}