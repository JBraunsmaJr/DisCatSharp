using System;
using System.Collections.Generic;
using DisCatSharp.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DisCatSharp.Configuration.Tests
{
    public class ConfigurationExtensionTests
    {
        class SampleClass
        {
            public int Amount { get; set; }
            public string Email { get; set; }
        }

        class SampleClass2
        {
            public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(7);
            public string Name { get; set; } = "Sample";

            public string ConstructorValue { get; }

            public SampleClass2(string value)
            {
                this.ConstructorValue = value;
            }
        }

        private IConfiguration BasicDiscordConfiguration() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"DisCatSharp:Discord:Token", "1234567890"},
                {"DisCatSharp:Discord:TokenType", "Bot" },
                {"DisCatSharp:Discord:MinimumLogLevel", "Information"},
                {"DisCatSharp:Discord:UseRelativeRateLimit", "true"},
                {"DisCatSharp:Discord:LogTimestampFormat", "yyyy-MM-dd HH:mm:ss zzz"},
                {"DisCatSharp:Discord:LargeThreshold", "250"},
                {"DisCatSharp:Discord:AutoReconnect", "true"},
                {"DisCatSharp:Discord:ShardId", "123123"},
                {"DisCatSharp:Discord:GatewayCompressionLevel", "Stream"},
                {"DisCatSharp:Discord:MessageCacheSize", "1024"},
                {"DisCatSharp:Discord:HttpTimeout", "00:00:20"},
                {"DisCatSharp:Discord:ReconnectIndefinitely", "false"},
                {"DisCatSharp:Discord:AlwaysCacheMembers", "true" },
                {"DisCatSharp:Discord:DiscordIntents", "AllUnprivileged"},
                {"DisCatSharp:Discord:MobileStatus", "false"},
                {"DisCatSharp:Discord:UseCanary", "false"},
                {"DisCatSharp:Discord:AutoRefreshChannelCache", "false"},
                {"DisCatSharp:Discord:Intents", "AllUnprivileged"}
            })
            .Build();

        private IConfiguration DiscordIntentsConfig() => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"DisCatSharp:Discord:Intents", "GuildEmojisAndStickers,GuildMembers,GuildInvites,GuildMessageReactions"}
                })
                .Build();

        private IConfiguration DiscordHaphazardConfig() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DisCatSharp:Discord:Intents", "GuildEmojisAndStickers,GuildMembers,Guilds" },
                { "DisCatSharp:Discord:MobileStatus", "true" },
                { "DisCatSharp:Discord:LargeThreshold", "1000" },
                { "DisCatSharp:Discord:HttpTimeout", "10:00:00" }
            })
            .Build();

        private IConfiguration SampleConfig() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Sample:Amount", "200" },
                { "Sample:Email", "test@gmail.com" }
            })
            .Build();

        private IConfiguration SampleClass2Configuration_Default() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Random:Stuff", "Meow"},
                {"SampleClass2:Name", "Purfection"}
            })
            .Build();

        private IConfiguration SampleClass2Configuration_Change() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "SampleClass:Timeout", "01:30:00" }, { "SampleClass:NotValid", "Something" }
            })
            .Build();

        [Fact]
        public void TestExtractDiscordConfig_Intents()
        {
            var source = this.DiscordIntentsConfig();

            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");

            var expected = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
                           DiscordIntents.GuildInvites | DiscordIntents.GuildMessageReactions;

            Assert.Equal(expected, config.Intents);
        }

        [Fact]
        public void TestExtractDiscordConfig_Haphzard()
        {
            var source = this.DiscordHaphazardConfig();

            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");
            var expectedIntents = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
                                  DiscordIntents.Guilds;

            Assert.Equal(expectedIntents, config.Intents);
            Assert.True(config.MobileStatus);
            Assert.Equal(1000, config.LargeThreshold);
            Assert.Equal(TimeSpan.FromHours(10), config.HttpTimeout);
        }

        [Fact]
        public void TestExtractDiscordConfig_Default()
        {
            var source = this.BasicDiscordConfiguration();
            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");

            Assert.Equal("1234567890", config.Token);
            Assert.Equal(TokenType.Bot, config.TokenType);
            Assert.Equal(LogLevel.Information, config.MinimumLogLevel);
            Assert.True(config.UseRelativeRatelimit);
            Assert.Equal("yyyy-MM-dd HH:mm:ss zzz", config.LogTimestampFormat);
            Assert.Equal(250, config.LargeThreshold);
            Assert.True(config.AutoReconnect);
            Assert.Equal(123123, config.ShardId);
            Assert.Equal(GatewayCompressionLevel.Stream, config.GatewayCompressionLevel);
            Assert.Equal(1024, config.MessageCacheSize);
            Assert.Equal(TimeSpan.FromSeconds(20), config.HttpTimeout);
            Assert.False(config.ReconnectIndefinitely);
            Assert.True(config.AlwaysCacheMembers);
            Assert.Equal(DiscordIntents.AllUnprivileged, config.Intents);
            Assert.False(config.MobileStatus);
            Assert.False(config.UseCanary);
            Assert.False(config.AutoRefreshChannelCache);
        }

        [Fact]
        public void TestSection()
        {
            var source = this.SampleConfig();
            SampleClass config = source.ExtractConfig<SampleClass>("Sample", null);

            Assert.Equal(200, config.Amount);
            Assert.Equal("test@gmail.com", config.Email);
        }

        [Fact]
        public void TestExtractConfig_V2_Default()
        {
            var source = this.SampleClass2Configuration_Default();
            var config = (SampleClass2) source.ExtractConfig("SampleClass", () => new SampleClass2("Test"), null);
            Assert.Equal(TimeSpan.FromMinutes(7), config.Timeout);
            Assert.Equal("Test", config.ConstructorValue);
            Assert.Equal("Sample", config.Name);
        }

        [Fact]
        public void TestExtractConfig_V2_Change()
        {
            var source = this.SampleClass2Configuration_Change();
            var config = (SampleClass2) source.ExtractConfig("SampleClass", () => new SampleClass2("Test123"), null);
            var span = new TimeSpan(0, 1, 30, 0);
            Assert.Equal(span, config.Timeout);
            Assert.Equal("Test123", config.ConstructorValue);
            Assert.Equal("Sample", config.Name);
        }

        [Fact]
        public void TestExtractConfig_V3_Default()
        {
            var source = this.SampleClass2Configuration_Default();
            var config =
                (SampleClass2)new ConfigSection(ref source, "SampleClass", null).ExtractConfig(() =>
                    new SampleClass2("Meow"));

            Assert.Equal("Meow", config.ConstructorValue);
            Assert.Equal(TimeSpan.FromMinutes(7), config.Timeout);
            Assert.Equal("Sample", config.Name);
        }

        [Fact]
        public void TestExtractConfig_V3_Change()
        {
            var source = this.SampleClass2Configuration_Change();
            var config =
                (SampleClass2)new ConfigSection(ref source, "SampleClass", null).ExtractConfig(() =>
                    new SampleClass2("Meow"));

            Assert.Equal("Meow", config.ConstructorValue);
            var span = new TimeSpan(0, 1, 30, 0);
            Assert.Equal(span, config.Timeout);
            Assert.Equal("Sample", config.Name);
        }
    }
}
