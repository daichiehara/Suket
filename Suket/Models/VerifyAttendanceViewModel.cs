using System.ComponentModel.DataAnnotations;

namespace Suket.Models
{
    public class VerifyAttendanceViewModel
    {

        public int PostId { get; set; }
        public string UserAccountId { get; set; }
        [Display(Name = "認証コード")]
        public int CertificationCode { get; set; }

    }
}
