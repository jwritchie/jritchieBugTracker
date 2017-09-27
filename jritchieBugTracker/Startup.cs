using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(jritchieBugTracker.Startup))]
namespace jritchieBugTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
