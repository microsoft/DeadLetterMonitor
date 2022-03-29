using System.Collections.Generic;

namespace Microsoft.DeadLetterMonitor.Model 
{
    /// <summary>
    /// Dead letter monitor rules.
    /// </summary>
    public class DeadLetterMonitorRules 
    {
        private string discard = string.Empty;
        private string retry = string.Empty;
        private string park = string.Empty;

        /// <summary>
        /// Gets or sets the discard.
        /// </summary>
        /// <value>
        /// The discard.
        /// </value>
        public string Discard 
        {
            get 
            {
                return discard;
            }

            set 
            {
                DiscardRules = GenerateRules(value);
                discard = value;
            }
        }

        /// <summary>
        /// Gets or sets the retry.
        /// </summary>
        /// <value>
        /// The retry.
        /// </value>
        public string Retry 
        {
            get 
            {
                return retry;
            }

            set 
            {
                RetryRules = GenerateRules(value);
                retry = value;
            }
        }

        /// <summary>
        /// Gets or sets the park.
        /// </summary>
        /// <value>
        /// The park.
        /// </value>
        public string Park 
        {
            get 
            {
                return park;
            }

            set 
            {
                ParkRules = GenerateRules(value);
                park = value;
            }
        }

        /// <summary>
        /// Gets or sets the discard rules.
        /// </summary>
        /// <value>
        /// The discard rules.
        /// </value>
        public List<MonitorRule> DiscardRules { get; set; } = null!;

        /// <summary>
        /// Gets or sets the discard rules.
        /// </summary>
        /// <value>
        /// The discard rules.
        /// </value>
        public List<MonitorRule> RetryRules { get; set; } = null!;

        /// <summary>
        /// Gets or sets the discard rules.
        /// </summary>
        /// <value>
        /// The discard rules.
        /// </value>
        public List<MonitorRule> ParkRules { get; set; } = null!;

        private List<MonitorRule> GenerateRules(string rulesOption)
        {
            // Convert options string into MonitorRule list
            var ruleArray = rulesOption.Split(";");

            List<MonitorRule> rules = new List<MonitorRule>();

            foreach (var rule in ruleArray)
            {
                var ruleParams = rule.Split(",");
                rules.Add(new MonitorRule { OriginalExchange = ruleParams[0], MessageType = ruleParams[1], DeathReason = ruleParams[2] });
            }

            return rules;
        }
    }
}
