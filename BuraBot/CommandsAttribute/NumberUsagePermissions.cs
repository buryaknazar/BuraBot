using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuraBot.CommandsAttribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NumberUsagePermissions : CheckBaseAttribute
    {
        public int AllowedUsageTimes { get; private set; }

        public NumberUsagePermissions(int times)
        {
            AllowedUsageTimes = times;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(AllowedUsageTimes == ctx.Command.ExecutionChecks.Count);
        }
    }
}
