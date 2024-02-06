using Microsoft.AspNetCore.Mvc;

namespace Suket.Components
{
    public class ActiveLinkViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string controller = null, string action = null, string[] pagePaths = null)
        {
            var activeClass = "";
            var routeValues = ViewContext.RouteData.Values;

            // コントローラとアクションが指定されている場合
            if (!string.IsNullOrEmpty(controller) && !string.IsNullOrEmpty(action))
            {
                var currentController = routeValues.ContainsKey("controller") ? routeValues["controller"].ToString() : string.Empty;
                var currentAction = routeValues.ContainsKey("action") ? routeValues["action"].ToString() : string.Empty;

                if (string.Equals(controller, currentController, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(action, currentAction, StringComparison.OrdinalIgnoreCase))
                {
                    activeClass = "active";
                }
            }
            // ページパスが指定されている場合
            else if (pagePaths != null && pagePaths.Any())
            {
                // 複数のページパスが指定されている場合
                var currentPagePath = routeValues.ContainsKey("page") ? routeValues["page"].ToString() : string.Empty;
                if (pagePaths.Any(path => string.Equals(path, currentPagePath, StringComparison.OrdinalIgnoreCase)))
                {
                    activeClass = "active";
                }
            }

            return Content(activeClass);
        }
    }


}
