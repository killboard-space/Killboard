using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Services;
using Killboard.Domain.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Killboard.Functions
{
    public class Functions
    {
        private readonly KillboardContext _ctx;
        private readonly IESIService _esiService;

        public Functions()
        {
            var options = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(Environment.GetEnvironmentVariable("SQLConnectionString") ??
                              throw new InvalidOperationException("Database connection string was null!")).Options;
            _ctx = new KillboardContext(options);
            _esiService = new ESIService();
        }

        [FunctionName("ItemSync")]
        public async Task ItemSync([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"[ItemSync] Starting Item Sync with Eve Online ESI at: {DateTime.Now}");

            var existingItems = await (from i in _ctx.items
                join g in _ctx.groups on i.group_id equals g.group_id
                select new
                {
                    i.type_id,
                    i.group_id,
                    g.category_id
                }).ToListAsync();
            log.LogInformation($"[ItemSync] Found {existingItems.Count} existing items in killboard_space DB.");

            var allItems = (await _esiService.GetItems()).ToList();
            log.LogInformation($"[ItemSync] Found {allItems.Count} items from Eve Online ESI.");

            var toAdd = allItems.Except(existingItems.Select(e => e.type_id)).ToList();
            log.LogInformation($"[ItemSync] {toAdd.Count} total new items to add to killboard_space DB from Eve Online ESI.");

            var newDetails = await Task.WhenAll(toAdd.Select(async i => await _esiService.GetItemDetail(i)));

            var newGroups = newDetails.Select(d => d.GroupId).Except(existingItems.Select(e => e.group_id))
                .ToList();
            log.LogInformation($"[ItemSync] {newGroups.Count} total new groups to add to killboard_space DB from Eve Online ESI.");

            var groupDetails = await Task.WhenAll(newGroups.Select(async g => await _esiService.GetGroupDetail(g)));

            var newCategories = groupDetails.Select(g => g.CategoryId).Except(existingItems.Select(e => e.category_id))
                .ToList();
            log.LogInformation($"[ItemSync] {newCategories.Count} total new categories to add to killboard_space DB from Eve Online ESI.");

            var categoryDetail = await Task.WhenAll(newCategories.Select(async c => await _esiService.GetCategoryDetail(c)));

            await _ctx.Database.OpenConnectionAsync();
            try
            {
                await _ctx.categories.AddRangeAsync(categoryDetail.Select(c => new categories
                {
                    category_id = c.CategoryId,
                    name = c.Name,
                    published = c.Published
                }));
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.categories ON;");
                await _ctx.SaveChangesAsync();
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.categories OFF");
                await _ctx.groups.AddRangeAsync(groupDetails.Select(g => new groups
                {
                    group_id = g.GroupId,
                    name = g.Name,
                    category_id = g.CategoryId,
                    published = g.Published
                }));
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.groups ON");
                await _ctx.SaveChangesAsync();
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.groups OFF");
                await _ctx.items.AddRangeAsync(newDetails.Select(i => new items
                {
                    type_id = i.TypeId,
                    description = i.Description,
                    capacity = i.Capacity,
                    name = i.Name,
                    group_id = i.GroupId,
                    icon_id = i.IconId,
                    mass = i.Mass,
                    portion_size = i.PortionSize,
                    published = i.Published,
                    radius = i.Radius,
                    volume = i.Volume
                }));
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.items ON");
                await _ctx.SaveChangesAsync();
                await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.items OFF");
            }
            catch (Exception e)
            {
                log.LogError(e, $"[ItemSync] Failed inserting synching new items | {e.Message}");
            }
            finally
            {
                await _ctx.Database.CloseConnectionAsync();
            }
        }

        [FunctionName("ItemDetailSync")]
        public async Task ItemDetailSync([TimerTrigger("0 1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"[ItemDetailSync] Starting Item Detail Sync with Eve Online ESI at: {DateTime.Now}");

            var existingItems = await (from i in _ctx.items
                join g in _ctx.groups on i.group_id equals g.group_id
                select new
                {
                    i.type_id,
                    i.group_id,
                    g.category_id
                }).ToListAsync();
            log.LogInformation($"[ItemDetailSync] Found {existingItems.Count} existing items in killboard_space DB.");

            var itemDetails = await Task.WhenAll(existingItems.Select(async i => await _esiService.GetItemDetail(i.type_id)));

            var toUpdate = itemDetails.Where(i => existingItems.All(e => !i.Equals(e))).ToList();

            log.LogInformation($"[ItemDetailSync] Found {toUpdate.Count} items to update values for in killboard_space DB from the Eve Online ESI.");

            var newGroups = toUpdate.Select(u => u.GroupId).Except(existingItems.Select(e => e.group_id)).ToList();

            // In the case of group/category re-association.
            if (newGroups.Count > 0)
            {
                log.LogInformation($"[ItemDetailSync] {newGroups.Count} total new groups to add to killboard_space DB from Eve Online ESI.");

                var groupDetails = await Task.WhenAll(newGroups.Select(async g => await _esiService.GetGroupDetail(g)));

                var newCategories = groupDetails.Select(g => g.CategoryId).Except(existingItems.Select(e => e.category_id))
                    .ToList();
                log.LogInformation($"[ItemDetailSync] {newCategories.Count} total new categories to add to killboard_space DB from Eve Online ESI.");

                var categoryDetail = await Task.WhenAll(newCategories.Select(async c => await _esiService.GetCategoryDetail(c)));

                await _ctx.Database.OpenConnectionAsync();
                try
                {
                    await _ctx.categories.AddRangeAsync(categoryDetail.Select(c => new categories
                    {
                        category_id = c.CategoryId,
                        name = c.Name,
                        published = c.Published
                    }));
                    await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.categories ON;");
                    await _ctx.SaveChangesAsync();
                    await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.categories OFF");
                    await _ctx.groups.AddRangeAsync(groupDetails.Select(g => new groups
                    {
                        group_id = g.GroupId,
                        name = g.Name,
                        category_id = g.CategoryId,
                        published = g.Published
                    }));
                    await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.groups ON");
                    await _ctx.SaveChangesAsync();
                    await _ctx.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.groups OFF");
                }
                catch (Exception e)
                {
                    log.LogError(e, $"[ItemSync] Failed inserting synching new items | {e.Message}");
                }
                finally
                {
                    await _ctx.Database.CloseConnectionAsync();
                }
            }

            var dbToUpdate = _ctx.items.Where(i => toUpdate.Any(t => t.TypeId == i.type_id));
            foreach (var itemToUpdate in dbToUpdate)
            {
                var newItem = toUpdate.FirstOrDefault(t => t.TypeId == itemToUpdate.type_id);
                itemToUpdate.name = newItem?.Name;
                itemToUpdate.capacity = (int?)newItem?.Capacity;
                itemToUpdate.description = newItem?.Description;
                if (newItem != null) itemToUpdate.group_id = newItem.GroupId;
                itemToUpdate.icon_id = newItem?.IconId;
                itemToUpdate.mass = newItem?.Mass;
                itemToUpdate.portion_size = newItem?.PortionSize;
                if (newItem != null) itemToUpdate.published = newItem.Published;
                itemToUpdate.radius = newItem?.Radius;
                itemToUpdate.volume = newItem?.Volume;
            }
            _ctx.items.UpdateRange(dbToUpdate);
            await _ctx.SaveChangesAsync();
        }

        
    }
}
