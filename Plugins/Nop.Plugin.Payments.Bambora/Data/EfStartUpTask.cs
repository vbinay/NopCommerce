﻿using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Data
{
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<BamboraObjectContext>(null);
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 0; }
        }
    }
}
