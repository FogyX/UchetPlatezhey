using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УчётПлатежей.Helpers
{
    public static class AppState
    {
        public static пользователи CurrentUser { get; set; }
        public static bool IsAuthenticated => CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
