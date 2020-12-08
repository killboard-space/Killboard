using System;
using Killboard.Data.Models;
using Killboard.Domain.Enums;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace Killboard.Domain.Services
{
    public class KillmailNotificationService : IDisposable
    {
        private readonly IHubContext<KillmailHub> _hub;
        private readonly IServiceScope _scope;

        private static SqlTableDependency<killmails> _tableDependency;

        private const string TableName = "killmails";

        public KillmailNotificationService(IConfiguration configuration, IServiceProvider services, IHubContext<KillmailHub> hub)
        {
            _hub = hub;
            _scope = services.CreateScope();

            _tableDependency = new SqlTableDependency<killmails>(configuration["Killboard:Sql"], TableName);
            _tableDependency.OnChanged += TableDependency_Changed;
            _tableDependency.Start();
        }

        private void TableDependency_Changed(object sender, RecordChangedEventArgs<killmails> e)
        {
            if(e.Entity.finished_processing != null)
                BroadcastKillmail(e.Entity);
        }

        private void BroadcastKillmail(killmails km)
        {
            using var kmService = _scope.ServiceProvider.GetRequiredService<IKillmailService>();
            _hub.Clients.All.SendAsync("NewKillmail", kmService.GetAllKillmails(ListTypes.EXACT, km.killmail_id));
        }

        public void Dispose()
        {
            _scope.Dispose();
            _tableDependency.Stop();
            _tableDependency.Dispose();
        }
    }
}
