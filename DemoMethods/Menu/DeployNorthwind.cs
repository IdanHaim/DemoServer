﻿using System;
using System.Net.Http;
using System.Web.Http;
using DemoMethods.Helpers;
using Raven.Client.Connection;

namespace DemoMethods
{
    public partial class MenuController : DemoApiController
    {
        [HttpGet]
        [Demo("Deploy Northwind", DemoOutputType.String, demoOrder: 305)]
        public object DeployNorthwind(bool deleteDatabase = false)
        {
            try
            {
                if (deleteDatabase)
                {
                    DocumentStoreHolder.Store
                        .DatabaseCommands
                        .GlobalAdmin
                        .DeleteDatabase(DocumentStoreHolder.NorthwindDatabaseName, hardDelete: true);
                }

                DocumentStoreHolder.Store
                    .DatabaseCommands
                    .GlobalAdmin
                    .EnsureDatabaseExists(DocumentStoreHolder.NorthwindDatabaseName);

                var url = string.Format("{0}/studio-tasks/createSampleData", DocumentStoreHolder.Store.Url.ForDatabase(DocumentStoreHolder.NorthwindDatabaseName));
                var requestFactory = DocumentStoreHolder.Store.JsonRequestFactory;

                var request = requestFactory.CreateHttpJsonRequest(new CreateHttpJsonRequestParams(null, url, "POST", DocumentStoreHolder.Store.DatabaseCommands.PrimaryCredentials, DocumentStoreHolder.Store.Conventions));
                request.ExecuteRequest();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return string.Format("Northwind was deployed to '{0}' database.", DocumentStoreHolder.NorthwindDatabaseName);
        }
    }
}