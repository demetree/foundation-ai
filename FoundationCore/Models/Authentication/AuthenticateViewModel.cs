using System;
using System.ComponentModel.DataAnnotations;

namespace Foundation.Models
{
    public class AuthenticateViewModel
    {
        [Required]
        public string Username { get; set; }

        //[DataType(DataType.Password)]  commenting out this helps with the 2 factor login process where the password needs to be re-entered after the 1st login POST.  If this is on, the password needs to be re-entered
        public string Password { get; set; }


        public Boolean AutoLoginDefaultUser { get; set; }
        public string ReturnURL { get; set; }

        //
        // For Recaptcha integration
        //
        public string recaptchaToken { get; set; }



        //
        // For 2 factor that needs a code sent
        //
        public string twoFactorCode { get; set; }

        public AuthenticateViewModel Copy()
        {
            AuthenticateViewModel output = new AuthenticateViewModel();

            output.Password = this.Password;
            output.recaptchaToken = this.recaptchaToken;
            output.ReturnURL = this.ReturnURL;

            output.Username = this.Username;
            output.twoFactorCode = this.twoFactorCode;

            return output;
        }

    }
}