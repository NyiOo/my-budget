﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;


namespace MyBudget
{
    class MefBootstrapper : BootstrapperBase
    {
        private CompositionContainer container;
        public MefBootstrapper() { Initialize(); }

        protected override void Configure()
        {
            container = new CompositionContainer(new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));

            var batch = new CompositionBatch();
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
           // batch.AddExportedValue<Func<MasterViewModel>>(() => container.GetExportedValue<MasterViewModel>());

            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        protected override object GetInstance(Type service, string key)
        {
            String contract = String.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(service) : key;

            var exports = container.GetExportedValues<object>(contract);

            if(exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(service));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }
       

        protected override void OnStartup(object sender, StartupEventArgs e)
        {             
            DisplayRootViewFor<IShell>();
        }

    }
}
