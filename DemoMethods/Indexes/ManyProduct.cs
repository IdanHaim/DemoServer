﻿using System.Linq;
using DemoMethods.Entities;
using Raven.Client.Indexes;

namespace DemoMethods.Indexes
{
    public class ManyProduct : AbstractIndexCreationTask<ProductName>
    {
        public ManyProduct()
        {
            Map = products => from product in products
                              select new
                              {
                                  product
                              };
        }
    }
}