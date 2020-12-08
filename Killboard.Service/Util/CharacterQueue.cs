﻿using Killboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Killboard.Service.Util
{
    public class CharacterQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly Queue<characters> _objs = new Queue<characters>();

        private readonly ILogger<CharacterQueue> _logger;

        public CharacterQueue(ILogger<CharacterQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(characters obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int charId) => _objs.Any(a => a.character_id == charId);

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                characters item;
                lock (_objs)
                {
                    if (_objs.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    item = _objs.Dequeue();
                }

                try
                {
                    _logger.LogInformation($"Processing Character for Character ID {item.character_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Character for Character ID: {CharacterID} - Possible Duplicate Insert", item.character_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Character for Character ID {CharacterID}", item.character_id);
                }
            }
        }

        private static void AddObjectToDatabase(characters obj)
        {
            using var ctx = new KillboardContext();
            
            if (ctx.characters.Any(k => k.character_id == obj.character_id)) return;

            ctx.characters.Add(obj);
            ctx.SaveChanges();
        }
    }
}
