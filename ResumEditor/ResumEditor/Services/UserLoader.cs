using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumEditor.Services
{
    public static class UserLoader
    {
        public static UserProfile Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("User config not found.", path);

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UserProfile>(json);
        }
    }
}
