using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GradProject.Web.Startup))]
namespace GradProject.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
