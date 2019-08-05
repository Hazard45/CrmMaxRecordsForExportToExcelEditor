using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Configuration;
using System.Net;
using System.ServiceModel.Description;

namespace CRMMaxRecordsForExportToExcelEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var uri = new Uri(ConfigurationManager.AppSettings["OrganizationServiceUrl"]);
                var credentials = new ClientCredentials();
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Username"]))
                {
                    credentials.UserName.UserName = ConfigurationManager.AppSettings["Username"];
                    credentials.UserName.Password = ConfigurationManager.AppSettings["Password"];
                }
                else
                {
                    credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                }
                IOrganizationService service;
                try
                {
                    service = new OrganizationServiceProxy(uri, null, credentials, null);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not connect to CRM service", ex);
                }

                var query = new QueryExpression("organization");
                query.ColumnSet.AddColumn("maxrecordsforexporttoexcel");
                EntityCollection organizations = null;
                try
                {
                    organizations = service.RetrieveMultiple(query);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not retrieve organizations. Check username and password.", ex);
                }
                if (organizations.Entities.Count == 1)
                {
                    Console.WriteLine("Current max records for export to excel count is " +
                        organizations[0].GetAttributeValue<int>("maxrecordsforexporttoexcel").ToString());

                    int newValue = GetNewValue();
                    organizations[0]["maxrecordsforexporttoexcel"] = newValue;
                    service.Update(organizations[0]);
                    Console.WriteLine("Max records count changed to " + newValue.ToString());
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static int GetNewValue()
        {
            Console.WriteLine("Enter new max records count");
            var input = Console.ReadLine();
            int newValue;
            int.TryParse(input, out newValue);
            if (newValue == 0)
            {
                Console.WriteLine("Wrong value: " + input);
                return GetNewValue();
            }
            else
            {
                return newValue;
            }
        }
    }
}