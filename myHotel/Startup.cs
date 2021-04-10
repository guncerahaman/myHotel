using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(myHotel.Startup))]
namespace myHotel
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
