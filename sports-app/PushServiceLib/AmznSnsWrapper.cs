using System;
using System.Configuration;
using System.Linq;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
using Newtonsoft.Json.Linq;

namespace PushServiceLib
{
    public class AmznSnsWrapper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AmznSnsWrapper));

        public bool IsSandboxMode = false;

        protected AmazonSimpleNotificationServiceClient client;
        protected string platformArnAPNS_Basketball = "";
        protected string platformArnAPNS_Volleyball = "";
        protected string platformArnAPNS_Netball = "";
        protected string platformArnAPNS_Waterpolo = "";

        protected string platformArnAPNS_SANDBOX_Basketball = "";
        protected string platformArnAPNS_SANDBOX_Volleyball = "";
        protected string platformArnAPNS_SANDBOX_Netball = "";
        protected string platformArnAPNS_SANDBOX_Waterpolo = "";

        protected string platformArnGCM = "arn:aws:sns:eu-west-1:650058904732:app/GCM/LogLig-Android";
        protected string platformArnGCM_TEST = "arn:aws:sns:eu-west-1:650058904732:app/GCM/LogLig-Android";

        public enum MobileAppForSports
        {
            Basketball,
            Waterpolo,
            Volleyball,
            Netball
        }

        public enum Platform
        {
            ADM,
            APNS,
            APNS_SANDBOX,
            Baidu,
            GCM,
            MPNS,
            WNS
        }

        public AmznSnsWrapper()
        {
            try
            {
                IsSandboxMode = bool.Parse(ConfigurationSettings.AppSettings.Get("AmazonSNS_UseSandbox"));
            } catch(Exception e)
            {
                IsSandboxMode = false;
            }

            try
            {
                client = new AmazonSimpleNotificationServiceClient("AKIAJAQG4D7VQCOEXHOA", "TaFqFTKMJZMH+GmBZT5HCc7O9SXA9pV2UDOpilyw");

                platformArnAPNS_Basketball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_Basketball");
                platformArnAPNS_Volleyball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_Volleyball");
                platformArnAPNS_Netball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_Netball");
                platformArnAPNS_Waterpolo = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_Waterpolo");

                platformArnAPNS_SANDBOX_Basketball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_SANDBOX_Basketball");
                platformArnAPNS_SANDBOX_Volleyball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_SANDBOX_Volleyball");
                platformArnAPNS_SANDBOX_Netball = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_SANDBOX_Netball");
                platformArnAPNS_SANDBOX_Waterpolo = ConfigurationSettings.AppSettings.Get("PlatformArnAPNS_SANDBOX_Waterpolo");

                platformArnGCM = ConfigurationSettings.AppSettings.Get("PlatformArnGCM");
                platformArnGCM_TEST = ConfigurationSettings.AppSettings.Get("PlatformArnGCM_TEST");
            } catch (Exception e)
            {
                log.ErrorFormat("Failed to create AmazonSimpleNotificationServiceClient Instance. Exception Details: {0}", e);
            }
        }

        public async Task prepareServiceClient()
        {

        }

        /**
         * 1. register endpoint
         */
        public async Task<string> CreateEndpoint(String deviceToken, Platform platformAppArn, MobileAppForSports mobApp)
        {
            String platformApplicationArn = "";
            switch (platformAppArn)
            {
                case Platform.APNS:
                    if (IsSandboxMode)
                    {
                        switch (mobApp)
                        {
                            case MobileAppForSports.Basketball:
                                platformApplicationArn = platformArnAPNS_SANDBOX_Basketball;
                                break;
                            case MobileAppForSports.Netball:
                                platformApplicationArn = platformArnAPNS_SANDBOX_Netball;
                                break;
                            case MobileAppForSports.Volleyball:
                                platformApplicationArn = platformArnAPNS_SANDBOX_Volleyball;
                                break;
                            case MobileAppForSports.Waterpolo:
                                platformApplicationArn = platformArnAPNS_SANDBOX_Waterpolo;
                                break;
                        }
                    }
                    else
                    {
                        switch (mobApp)
                        {
                            case MobileAppForSports.Basketball:
                                platformApplicationArn = platformArnAPNS_Basketball;
                                break;
                            case MobileAppForSports.Netball:
                                platformApplicationArn = platformArnAPNS_Netball;
                                break;
                            case MobileAppForSports.Volleyball:
                                platformApplicationArn = platformArnAPNS_Volleyball;
                                break;
                            case MobileAppForSports.Waterpolo:
                                platformApplicationArn = platformArnAPNS_Waterpolo;
                                break;
                        }
                    }
                    break;
                case Platform.GCM:
                    if (IsSandboxMode)
                    {
                        platformApplicationArn = platformArnGCM_TEST;
                    }
                    else
                    {
                        platformApplicationArn = platformArnGCM;
                    }
                    break;
            }
            try
            {
                CreatePlatformEndpointRequest request = new CreatePlatformEndpointRequest();
                request.Token = deviceToken;
                request.PlatformApplicationArn = platformApplicationArn;
                var resp = await client.CreatePlatformEndpointAsync(request);
                return resp.EndpointArn;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error in function:CreateEndpoint. Error was: {0}", e.ToString());
            }
            
            return null;
        }

        public async Task RemoveEndpoint(string endpointArn)
        {
            log.Info("RemoveEndpoint:" + endpointArn);
            try
            {
                client.DeleteEndpoint(new DeleteEndpointRequest()
                {
                    EndpointArn = endpointArn
                });
            }
            catch (Exception e)
            {
                log.ErrorFormat("RemoveEndpoint: Error: {0}", e);
            }
        }

        /**
         * 2. send push message
         */
        public int SendMessage(string endpointArn, string rawMessage)
        {
            try
            {
                log.InfoFormat("function:SendMessage: Sending message to Endpoint:{0}", endpointArn);
                var resp = client.Publish(new PublishRequest()
                {
                    MessageStructure = "json",
                    TargetArn = endpointArn,
                    Message = rawMessage
                });
            }
            catch (EndpointDisabledException e)
            {
                log.InfoFormat("function:SendMessage: Endpoint Disabled, deleting Endpoint:{0}", endpointArn);
                client.DeleteEndpoint(new DeleteEndpointRequest()
                {
                    EndpointArn = endpointArn
                });
                return -1;
            }
            catch(Exception e)
            {
                log.InfoFormat("function:SendMessage: Unknown error: {0}, message: {1}", e, rawMessage);
                return -9999;
            }
            return 0;
        }

        public int SendPlatformMessage(string endpointArn, IPlatformMessage message)
        {
            return SendMessage(endpointArn, message.ToString());
        }

        public int SendPlatformMessage(string endpointArn, Platform platform, String message)
        {
            IPlatformMessage msg = BuildPlatformMessage(platform, message);
            if (msg != null)
                return SendPlatformMessage(endpointArn, msg);
            return 0;
        }

        /**
         * Build platform dependent message from string message
         */
        public IPlatformMessage BuildPlatformMessage(Platform platform, String message)
        {
            if (platform == Platform.APNS)
            {
                ApnsPlatformMessage apnsMsg = new ApnsPlatformMessage();
                apnsMsg.Alert = Regex.Replace(message, @"\r\n?|\n", " "); // replace new line characters with spaces
                apnsMsg.Badge = 1;
                apnsMsg.Sound = "default";
                return apnsMsg;
            }
            else if (platform == Platform.GCM)
            {
                GcmPlatformMessage gcmMsg = new GcmPlatformMessage();
                gcmMsg.Default = "LogLig Message";
                gcmMsg.CollapseKey = "LogLig";
                gcmMsg.TimeToLive = 125;
                gcmMsg.DryRun = false;
                gcmMsg.NotificationBody = Regex.Replace(message, @"\r\n?|\n", " "); // replace new line characters with spaces
                gcmMsg.NotificationTitle = "LogLig";
                gcmMsg.NotificationBadge = "1";
                gcmMsg.NotificationSound = "default";
                return gcmMsg;
            }
            return null;
        }
    }


    public interface IPlatformMessage {
        string ToString();
    }

    public class ApnsPlatformMessage : IPlatformMessage
    {
        //APS
        public string Alert { get; set; }
        public int Badge { get; set; }
        public string Sound { get; set; }
        public int ContentAvailable { get; set; }
        public string Category { get; set; }
        public string ThreadId { get; set; }

        // Alert Detail(optional)
        public string AlertTitle { get; set; }
        public string AlertBody { get; set; }
        public string AlertTitleLocKey { get; set; }
        public string AlertTitleLocArgs { get; set; }
        public string AlertActionLocKey { get; set; }
        public string AlertLocKey { get; set; }
        public string AlertLocArgs { get; set; }
        public string AlertLaunchImage { get; set; }

        // Custom
        public string DataLink { get; set; }

        override public string ToString()
        {
            if(Alert != null)
            {
                return new JObject(
                    new JProperty("APNS",
                        new JObject(
                            new JProperty("aps", 
                                new JObject(
                                    new JProperty("alert", Alert),
                                    new JProperty("badge", Badge),
                                    new JProperty("sound", Sound)
                                )
                            ),
                            new JProperty("link", "notifications")
                        ).ToString(Newtonsoft.Json.Formatting.None)
                    )
                ).ToString(Newtonsoft.Json.Formatting.None);
            }
            else
            {
                return new JObject(
                    new JProperty("APNS",
                        new JObject(
                            new JProperty("aps",
                                new JObject(
                                    new JProperty("alert",
                                        new JObject(
                                            new JProperty("title", AlertTitle),
                                            new JProperty("body", AlertBody)
                                        )
                                    ),
                                    new JProperty("badge", Badge),
                                    new JProperty("sound", Sound)
                                )
                            ),
                            new JProperty("link", "notifications")
                        ).ToString(Newtonsoft.Json.Formatting.None)
                    )
                ).ToString(Newtonsoft.Json.Formatting.None);
            }
            
        }
    }

    public class GcmPlatformMessage : IPlatformMessage
    {
        // default
        public string Default { get; set; }
        // Options
        public string CollapseKey { get; set; }
        public string Priority { get; set; }
        public bool? ContentAvailable { get; set; }
        public int? TimeToLive { get; set; }
        public bool DryRun { get; set; }
        // Payload - Data

        // Payload - Notification
        public string NotificationTitle { get; set; } // Android, iOS
        public string NotificationBody { get; set; } // Android, iOS
        public string NotificationIcon { get; set; } // Android
        public string NotificationSound { get; set; } // Android, iOS
        public string NotificationBadge { get; set; } // iOS
        public string NotificationTag { get; set; } // Android
        public string NotificationColor { get; set; } // Android
        public string NotificationClickAction { get; set; } // Android, iOS(corresponds to category in APNS payload)
        public string NotificationBodyLocKey { get; set; } // Android, iOS(corresponds to loc-key in APNS payload)
        public string NotificationBodyLocArgs { get; set; } // Android, iOS(corresponds to loc-args in APNS payload)
        public string NotificationTitleLocKey { get; set; } // Android, iOS(corresponds to title-loc-key in APNS payload)
        public string NotificationTitleLocArgs { get; set; } // Android, iOS(corresponds to title-loc-args in APNS payload)


        override public string ToString()
        {
            return new JObject(
                new JProperty("default", Default),
                new JProperty("GCM",
                    new JObject(
                        new JProperty("collapse-key", CollapseKey),
                        //new JProperty("priority", Priority),
                        //new JProperty("content_available", ContentAvailable),
                        new JProperty("time-to-live", TimeToLive),
                        new JProperty("dry-run", DryRun),
                        new JProperty("notification", 
                            new JObject(
                                new JProperty("title", NotificationTitle),
                                new JProperty("body", NotificationBody),
                                new JProperty("sound", NotificationSound)
                            )
                        ),
                        new JProperty("data",
                            new JObject(
                                new JProperty("link", "notifications")
                            )
                        )
                    ).ToString(Newtonsoft.Json.Formatting.None)
                )
            ).ToString(Newtonsoft.Json.Formatting.None);
        }
    }

}