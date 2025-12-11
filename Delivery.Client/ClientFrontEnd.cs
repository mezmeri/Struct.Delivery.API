using System.Text.Json;

namespace Delivery.Client
{
    /// <summary>
    /// Mock frontend console application - completely agnostic to domain models
    /// Displays data from Redis and receives webhook notifications
    /// </summary>
    internal class ClientFrontEnd
    {
        private readonly RedisMonitor _redisMonitor;
        private readonly List<(DateTime receivedAt, string payload)> _webhookHistory = new();
        private readonly object _lock = new();

        public ClientFrontEnd(RedisMonitor redisMonitor)
        {
            _redisMonitor = redisMonitor;
        }

        public async Task ShowMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
                Console.WriteLine("║         DELIVERY API - MOCK FRONTEND                  ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("1. View Cached Items (Products in Redis)");
                Console.WriteLine("2. View Queue Items (Ready to Deliver)");
                Console.WriteLine("3. View Queue ID Map (Latest Timestamps)");
                Console.WriteLine("4. View Queue Statistics");
                Console.WriteLine("5. View Webhook History");
                Console.WriteLine("6. Clear Webhook History");
                Console.WriteLine("7. Refresh View");
                Console.WriteLine("8. Exit");
                Console.WriteLine();
                Console.Write("Select an option: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ShowCachedItemsAsync();
                            break;
                        case "2":
                            await ShowQueueItemsAsync();
                            break;
                        case "3":
                            await ShowQueueIdMapAsync();
                            break;
                        case "4":
                            await ShowQueueStatsAsync();
                            break;
                        case "5":
                            ShowWebhookHistory();
                            break;
                        case "6":
                            ClearWebhookHistory();
                            break;
                        case "7":
                            // Just loop back to menu
                            break;
                        case "8":
                            return;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid option. Press any key to continue...");
                            Console.ResetColor();
                            Console.ReadKey();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nError: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private async Task ShowCachedItemsAsync()
        {
            Console.Clear();
            PrintHeader("CACHED ITEMS (Products in Redis)");

            var items = await _redisMonitor.GetCachedItemsAsync();

            if (!items.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No items currently cached.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Total cached items: {items.Count}\n");
                Console.ResetColor();

                foreach (var item in items)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Key: {item.Key}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    
                    // Pretty print JSON if possible
                    var formatted = TryFormatJson(item.Value);
                    Console.WriteLine($"Value:\n{formatted}");
                    Console.ResetColor();
                    Console.WriteLine(new string('-', 80));
                }
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private async Task ShowQueueItemsAsync()
        {
            Console.Clear();
            PrintHeader("QUEUE ITEMS (Ready to Deliver)");

            var items = await _redisMonitor.GetQueueItemsAsync();

            if (!items.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No items in queue.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Total queued items: {items.Count}\n");
                Console.ResetColor();

                // Group by EventType if possible
                var grouped = items
                    .Select((json, index) => (json, index))
                    .GroupBy(x => TryGetProperty(x.json, "EventType") ?? "Unknown");

                foreach (var group in grouped)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\n{group.Key} ({group.Count()} items)");
                    Console.ResetColor();

                    foreach (var item in group)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"  [{item.index}] {TryFormatJson(item.json)}");
                        Console.ResetColor();
                    }
                }
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private async Task ShowQueueIdMapAsync()
        {
            Console.Clear();
            PrintHeader("QUEUE ID MAP (Latest Timestamps)");

            var idMap = await _redisMonitor.GetQueueIdMapAsync();

            if (!idMap.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No items in ID map.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Total items in map: {idMap.Count}\n");
                Console.ResetColor();

                // Sort by timestamp if possible
                var sorted = idMap
                    .OrderByDescending(x => TryGetTimestamp(x.Value));

                foreach (var entry in sorted)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"ID: {entry.Key}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"{TryFormatJson(entry.Value)}");
                    Console.ResetColor();
                    Console.WriteLine(new string('-', 80));
                }
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private async Task ShowQueueStatsAsync()
        {
            Console.Clear();
            PrintHeader("QUEUE STATISTICS");

            var (queueLength, mapSize) = await _redisMonitor.GetQueueStatsAsync();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Queue Length (items waiting): {queueLength}");
            Console.WriteLine($"Unique IDs in Map: {mapSize}");
            Console.ResetColor();

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private void ShowWebhookHistory()
        {
            Console.Clear();
            PrintHeader("WEBHOOK HISTORY (Notifications Received)");

            lock (_lock)
            {
                if (!_webhookHistory.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No webhooks received yet.");
                    Console.WriteLine("\nℹ️  Webhooks are sent when QueueWorker processes items.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Total webhooks received: {_webhookHistory.Count}\n");
                    Console.ResetColor();

                    foreach (var (receivedAt, payload) in _webhookHistory.OrderByDescending(x => x.receivedAt))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Received: {receivedAt:yyyy-MM-dd HH:mm:ss.fff}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(TryFormatJson(payload));
                        Console.ResetColor();
                        Console.WriteLine(new string('-', 80));
                    }
                }
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private void ClearWebhookHistory()
        {
            lock (_lock)
            {
                _webhookHistory.Clear();
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Webhook history cleared. Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }

        /// <summary>
        /// Records a webhook notification (called by Program.cs webhook endpoint)
        /// </summary>
        public void RecordWebhook(string payload)
        {
            lock (_lock)
            {
                _webhookHistory.Add((DateTime.Now, payload));
            }

            // Extract some info for the notification
            var eventType = TryGetProperty(payload, "EventType") ?? "Unknown";
            var idsCount = TryGetArrayLength(payload, "Ids");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] ✓ Webhook received: {eventType} ({idsCount} IDs)");
            Console.ResetColor();
        }

        // Helper methods for JSON manipulation without models
        private static string TryFormatJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }

        private static string? TryGetProperty(string json, string propertyName)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(propertyName, out var property))
                {
                    return property.ToString();
                }
            }
            catch { }
            return null;
        }

        private static long TryGetTimestamp(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Timestamp", out var property))
                {
                    return property.GetInt64();
                }
            }
            catch { }
            return 0;
        }

        private static int TryGetArrayLength(string json, string propertyName)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(propertyName, out var property) 
                    && property.ValueKind == JsonValueKind.Array)
                {
                    return property.GetArrayLength();
                }
            }
            catch { }
            return 0;
        }

        private static void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            Console.WriteLine($"  {title}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
