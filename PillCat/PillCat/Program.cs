using Microsoft.AspNetCore;

namespace PillCat
{
    ///<Summary>
    /// Program
    ///</Summary>
    public class Program
    {
        ///<Summary>
        /// Main
        ///</Summary>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        ///<Summary>
        /// BuildWebHost
        ///</Summary>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .Build();
    }
}