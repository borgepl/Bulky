using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Views.Shared
{
    public class _Notification : PageModel
    {
        private readonly ILogger<_Notification> _logger;

        public _Notification(ILogger<_Notification> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}