using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mstdnCats.Services
{
    public class CheckEnv
    {
        public static Boolean IsValid(){
            if (DotNetEnv.Env.GetString("DB_NAME") == null ||
            DotNetEnv.Env.GetString("BOT_TOKEN") == null ||
            DotNetEnv.Env.GetString("TAG") == null ||
            DotNetEnv.Env.GetString("CHANNEL_NUMID") == null ||
            DotNetEnv.Env.GetString("ADMIN_NUMID") == null ){
                return false;
            }
            return true;
        }
    }
}