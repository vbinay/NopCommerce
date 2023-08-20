using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Tridion.AppSettings;

namespace Nop.Web.Framework.Tridion.Profile
{
    /// <summary>
    /// SMW Profile Supporting enum SODMYWAY-2956
    /// </summary>
    public enum EmailType
    {
        MultiPart = 1,
        Text = 2,
        HTML = 3
    }

    /// <summary>
    /// SMW Profile Supporting enum SODMYWAY-2956
    /// </summary>
    public enum Status
    {
        OptIn = 1,
        Subscribed = 2,
        Unsubscribed = 3
    }

    /// <summary>
    /// Tridion SMW Profile Class SODMYWAY-2956
    /// </summary>
    [Serializable]
    public class SmwProfile
    {

        public String Id { get; set; }

        [Display(Name = "Username")]
        public String Username { get; set; }

        [Required]
        [Display(Name = "Prefix")]
        public String Prefix { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public String Firstname { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public String Lastname { get; set; }

        [Display(Name = "Phone")]
        public String Phone { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public String Email { get; set; }

        [Required]
        [Display(Name = "Confirm Email")]
        [DataType(DataType.EmailAddress)]
        //[Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]    
        public string ConfirmEmail { get; set; }

        [Display(Name = "Zip")]
        public String Zip { get; set; }

        [Required]
        [Display(Name = "I would like to receive email-only specials, offers, and updates from Dining Services.")]
        public bool SubscriptionStatus
        {
            get
            {
                return ((Status == Status.Subscribed || Status == Status.OptIn) ? true : false);
            }
            set
            {
                if (value == true)
                {
                    if (IsEnabled)
                        Status = Status.OptIn;
                    else
                        Status = Status.Subscribed;
                }
                else
                {
                    Status = Status.Unsubscribed;
                }
            }
        }

        [Display(Name = "Type")]
        public String UserType { get; set; }

        public String ProfileComplete { get; set; }

        public int AudienceTypeId { get; set; }

        [Display(Name = "Status")]
        public Status Status { get; set; }

        [Display(Name = "Email Type")]
        public EmailType EmailType { get; set; }

        public SmwProfileAttribute[] Attributes { get; set; }

        public bool IsEnabled { get; set; }

        [Required]
        public int PublicationId { get; set; }
    }
}
