using Common.Logging;
using StructureMap;
using Topshelf;
using Topshelf.StructureMap;

namespace NSBWindowsService
{
    class Program
    {
        #region "Logging..."

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        static void Main()
        {
            log.Debug("Entering Main");

            HostFactory.Run(c =>
            {
                var container = new Container(cfg =>
                {
                    cfg.For<IDependency>().Use<Dependency>();
                });
                // Init StructureMap container 
                c.UseStructureMap(container);

                c.Service<Service>(s =>
                {
                    //Construct topshelf service instance with StructureMap
                    s.ConstructUsingStructureMap();

                    s.WhenStarted((service, control) => service.Start());
                    s.WhenStopped((service, control) => service.Stop());

                   
                });
            });
        }
    }
}
