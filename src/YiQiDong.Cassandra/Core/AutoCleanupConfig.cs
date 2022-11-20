using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiQiDong.Cassandra.Core
{
    public class AutoCleanupConfig
    {
        public const string CONFIG_FILE = "YiQiDong.Cassandra.Core.AutoCleanupConfig.json";

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = false;
        /// <summary>
        /// 触发时机
        /// </summary>
        public string Trigger { get; set; } = "0 1 * * *";
    }
}
