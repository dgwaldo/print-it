﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrintIt.WebHost {
    public class AppSettings {

        public AppSettings() {
        }

        public string AllowedHosts { get; set; }

        [Required]
        public string AllowedCors { get; set; }

        [Required]
        public SerilogAppSettings Serilog { get; set; }

        public string JwtTokenKey { get; set; }

        public RabbitMqSettings RabbitMqSettings { get; set; }


    }

    public class RabbitMqSettings {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }


    public class SerilogAppSettings {
        public class LevelOverride {
            public string System { get; set; }
            public string Microsoft { get; set; }
            public string Hangfire { get; set; }
        }

        public class MinimumLevelSettings {
            public string Default { get; set; }
            public LevelOverride Override { get; set; }
        }

        public class SerilogSink {
            public string Name { get; set; }
            public Dictionary<string, string> Args { get; set; }
        }

        public class SerilogProperties {
            public string Application { get; set; }
        }

        public SerilogAppSettings() { }
        public List<string> Using { get; set; }
        public MinimumLevelSettings MinimumLevel { get; set; }
        public List<SerilogSink> WriteTo { get; set; }
        public List<string> Enrich { get; set; }
        public SerilogProperties Properties { get; set; }

    }

}
