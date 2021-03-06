﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using DemoMethods.Entities;
using DemoMethods.Helpers;
using Raven.Abstractions.Data;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace DemoMethods
{
    public partial class MenuController : DemoApiController
    {
        [HttpGet]
        [Demo("Deploy Last.fm", DemoOutputType.String, demoOrder: 310)]
        public object CreateLastFmDataset(string path = null, bool deleteDatabase = false)
        {
            // path = @"C:\Users\adi\Downloads\lastfm_subset.zip";
            // path = @"C:\Users\adi\Downloads\lastfm_train.zip";
            try
            {
                if (deleteDatabase)
                {
                    DocumentStoreHolder.MediaStore
                        .DatabaseCommands
                        .GlobalAdmin
                        .DeleteDatabase(DocumentStoreHolder.MediaDatabaseName, hardDelete: true);
                }

                DocumentStoreHolder.MediaStore
                    .DatabaseCommands
                    .GlobalAdmin
                    .EnsureDatabaseExists(DocumentStoreHolder.MediaDatabaseName);

                AddDocumentsToDb(path);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return string.Format("Last.fm was deployed to {0} database.", DocumentStoreHolder.MediaDatabaseName);
        }

        public void AddDocumentsToDb(string path)
        {
            int count = 1;
            using (var stream = string.IsNullOrWhiteSpace(path) ? GetEmbeddedLastFmSubset() : File.OpenRead(path))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            using (var bulkInsert = DocumentStoreHolder.MediaStore.BulkInsert(options: new BulkInsertOptions { OverwriteExisting = true, BatchSize = 256 }))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Length == 0)
                        continue;
                    using (var entryStream = entry.Open())
                    {
                        var docAsJson = RavenJObject.Load(new JsonTextReader(new StreamReader(entryStream)));
                        var doc = new LastFm
                        {
                            Artist = docAsJson.Value<string>("artist"),
                            TimeStamp = DateTime.Parse(docAsJson.Value<string>("timestamp")),
                            Title = docAsJson.Value<string>("title"),
                            TrackId = docAsJson.Value<string>("track_id"),
                            Tags =
                                docAsJson.Value<RavenJArray>("tags")
                                    .Select(x => ((RavenJArray)x)[0].Value<string>())
                                    .ToList()
                        };
                        bulkInsert.Store(doc, "lastfm/" + (count++));
                    }
                }
            }
        }

        private static Stream GetEmbeddedLastFmSubset()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream("DemoMethods.Data.lastfm_subset.zip");
        }
    }
}