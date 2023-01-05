using audiamus.aaxconv.lib.ex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib
{
    interface IActivationCode
    {
        IEnumerable<string> ActivationCodes { get; }
        bool HasActivationCode { get; }
    }

    class ActivationCode : IActivationCode
    {

        private readonly IActivationSettings _settings;
        private List<string> _activationCodes = new List<string>();

        public IEnumerable<uint> NumericCodes => _activationCodes?.Select(s => Convert.ToUInt32(s, 16)).ToList();
        public IEnumerable<string> ActivationCodes => _activationCodes;
        public bool HasActivationCode => ActivationCodes?.Count() > 0;

        public ActivationCode(IActivationSettings settings)
        {
            _settings = settings;
            init();
        }

        private void init()
        {
            _activationCodes.Clear();

            // Code in settings

            if (_settings.ActivationCode.HasValue)
            {
                Log(2, this, "add from user settings");
                _activationCodes.Add(_settings.ActivationCode.Value.ToHexString());
            }

            // Audible-cli

            try
            {
                string audibleCliConfigPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Audible");

                if (!string.IsNullOrEmpty(audibleCliConfigPath))
                {
                    foreach (var file in Directory.GetFiles(audibleCliConfigPath, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            string activationBytes = Newtonsoft.Json.JsonConvert.DeserializeObject<AudibleCliProfile>(File.ReadAllText(file)).activation_bytes;

                            if (!string.IsNullOrEmpty(activationBytes))
                            {
                                if (!_activationCodes.Contains(activationBytes))
                                    _activationCodes.Add(activationBytes);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Audible app or Audible Manager

            ActivationCodeRegistry registryCodes = new ActivationCodeRegistry();
            if (registryCodes.HasActivationCode)
            {
                Log(2, this, $"add from registry (#={registryCodes.ActivationCodes.Count()})");
                _activationCodes = _activationCodes.Union(registryCodes.ActivationCodes).ToList();
            }

            ActivationCodeApp appCodes = new ActivationCodeApp();
            if (appCodes.HasActivationCode)
            {
                Log(2, this, $"add from app (#={appCodes.ActivationCodes.Count()})");
                _activationCodes = _activationCodes.Union(appCodes.ActivationCodes).ToList();
            }

            Log(2, this, $"#unique={_activationCodes.Count}");
        }

        public bool ReinitActivationCode()
        {
            init();
            return HasActivationCode;
        }
    }

    internal class AudibleCliProfile
    {
        public Website_Cookies website_cookies { get; set; }
        public string adp_token { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string device_private_key { get; set; }
        public Store_Authentication_Cookie store_authentication_cookie { get; set; }
        public Device_Info device_info { get; set; }
        public Customer_Info customer_info { get; set; }
        public float expires { get; set; }
        public string locale_code { get; set; }
        public bool with_username { get; set; }
        public string activation_bytes { get; set; }


        public class Website_Cookies
        {
            public string sessionid { get; set; }
            public string ubidacbuk { get; set; }
            public string xacbuk { get; set; }
            public string atacbuk { get; set; }
            public string sessatacbuk { get; set; }
        }

        public class Store_Authentication_Cookie
        {
            public string cookie { get; set; }
        }

        public class Device_Info
        {
            public string device_name { get; set; }
            public string device_serial_number { get; set; }
            public string device_type { get; set; }
        }

        public class Customer_Info
        {
            public string account_pool { get; set; }
            public string user_id { get; set; }
            public string home_region { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
        }
    }

    namespace ex
    {
        public static class ActivationCodeEx
        {
            public static string ToHexString(this uint code)
            {
                return code.ToString("X8");
            }

            public static string ToHexDashString(this uint? code)
            {
                if (code.HasValue)
                    return code.Value.ToHexDashString();
                else
                    return string.Empty;
            }

            public static string ToHexDashString(this uint code)
            {
                var bytes = BitConverter.GetBytes(code);
                Array.Reverse(bytes);
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    if (sb.Length > 0)
                        sb.Append('-');
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }

            public static uint ToUInt32(this IEnumerable<string> chars)
            {
                if (chars.Count() == 4)
                {
                    var sb = new StringBuilder();
                    foreach (string s in chars)
                        sb.Append(s);
                    try
                    {
                        return Convert.ToUInt32(sb.ToString(), 16);
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                }
                else
                    return 0;
            }

        }
    }
}
