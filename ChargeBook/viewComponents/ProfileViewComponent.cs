using System.Threading.Tasks;
using chargebook.data.infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace chargebook.viewComponents {
    public class ProfileViewComponent : ViewComponent {
        private readonly IInfrastructureManager infrastructureManager;

        public ProfileViewComponent(IInfrastructureManager infrastructureManager) {
            this.infrastructureManager = infrastructureManager;
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var locations = infrastructureManager.getLocationNames();
            ViewData["locations"] = locations;
            return View();
        }
    }
}