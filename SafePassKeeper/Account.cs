using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SafePassKeeper
{
    public class Account
    {

        [JsonInclude]
        public string AccountId { get; private set; } = Guid.NewGuid().ToString();

        public string AccountName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public ObservableCollection<string> AttachmentNames { get; set; } = new ObservableCollection<string>();
    }
}