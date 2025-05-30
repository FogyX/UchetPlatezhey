using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УчётПлатежей.Helpers
{
    internal static class DatabaseContext
    {
        private static УчётПлатежейEntities _context;

        public static УчётПлатежейEntities Context
        {
            get
            {
                if (_context == null)
                    _context = new УчётПлатежейEntities();
                return _context;
            }
        }
    }
}
