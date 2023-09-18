using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Suket.Models
{
    public enum Genre
    {
        [Display(Name = "野球")]
        Baseball = 1,
        [Display(Name = "サッカー")]
        Soccer = 2,
        [Display(Name = "その他")]
        Q = 99
    }
    public enum Prefecture
    {
        [Display(Name = "北海道")]
        Hokkaido = 1,
        [Display(Name = "青森県")]
        Aomori = 2,
        [Display(Name = "岩手県")]
        Iwate = 3,
        [Display(Name = "宮城県")]
        Miyagi = 4,
        [Display(Name = "秋田県")]
        Akita = 5,
        [Display(Name = "山形県")]
        Yamagata = 6,
        [Display(Name = "福島県")]
        Fukushima = 7,
        [Display(Name = "茨城県")]
        Ibaraki = 8,
        [Display(Name = "栃木県")]
        Tochigi = 9,
        [Display(Name = "群馬県")]
        Gunma = 10,
        [Display(Name = "埼玉県")]
        Saitama = 11,
        [Display(Name = "千葉県")]
        Chiba = 12,
        [Display(Name = "東京都")]
        Tokyo = 13,
        [Display(Name = "神奈川県")]
        Kanagawa = 14,
        [Display(Name = "新潟県")]
        Niigata = 15,
        [Display(Name = "富山県")]
        Toyama = 16,
        [Display(Name = "石川県")]
        Ishikawa = 17,
        [Display(Name = "福井県")]
        Fukui = 18,
        [Display(Name = "山梨県")]
        Yamanashi = 19,
        [Display(Name = "長野県")]
        Nagano = 20,
        [Display(Name = "岐阜県")]
        Gifu = 21,
        [Display(Name = "静岡県")]
        Shizuoka = 22,
        [Display(Name = "愛知県")]
        Aichi = 23,
        [Display(Name = "三重県")]
        Mie = 24,
        [Display(Name = "滋賀県")]
        Shiga = 25,
        [Display(Name = "京都府")]
        Kyoto = 26,
        [Display(Name = "大阪府")]
        Osaka = 27,
        [Display(Name = "兵庫県")]
        Hyogo = 28,
        [Display(Name = "奈良県")]
        Nara = 29,
        [Display(Name = "和歌山県")]
        Wakayama = 30,
        [Display(Name = "鳥取県")]
        Tottori = 31,
        [Display(Name = "島根県")]
        Shimane = 32,
        [Display(Name = "岡山県")]
        Okayama = 33,
        [Display(Name = "広島県")]
        Hiroshima = 34,
        [Display(Name = "山口県")]
        Yamaguchi = 35,
        [Display(Name = "徳島県")]
        Tokushima = 36,
        [Display(Name = "香川県")]
        Kagawa = 37,
        [Display(Name = "愛媛県")]
        Ehime = 38,
        [Display(Name = "高知県")]
        Kochi = 39,
        [Display(Name = "福岡県")]
        Fukuoka = 40,
        [Display(Name = "佐賀県")]
        Saga = 41,
        [Display(Name = "長崎県")]
        Nagasaki = 42,
        [Display(Name = "熊本県")]
        Kumamoto = 43,
        [Display(Name = "大分県")]
        Oita = 44,
        [Display(Name = "宮崎県")]
        Miyazaki = 45,
        [Display(Name = "鹿児島県")]
        Kagoshima = 46,
        [Display(Name = "沖縄県")]
        Okinawa = 47
    }
    public enum State
    {
        [Display(Name = "募集中")]
        Recruiting = 1,
        [Display(Name = "募集終了")]
        End = 2,
        [Display(Name ="中止")]
        Cancel = 3,
    }

    public class Post
    {
        public int PostId { get; set; }
        [Display(Name = "タイトル")]
        [Required]
        public string Title { get; set; }
        [Display(Name = "募集人数")]
        public int PeopleCount { get; set; }
        [Display(Name = "都道府県")]
        [Required]
        public Prefecture Prefecture { get; set; }
        [Display(Name = "開催場所")]
        [Required]
        public string Place { get; set; }
        [Display(Name = "開催日時")]
        public DateTimeOffset Time { get; set; }
        [Display(Name = "持ち物")]
        public string Item { get; set; }
        [Display(Name = "報酬")]
        [Remote(action: "VerifyReward", controller: "Posts")]
        public int Reward { get; set; }
        [Display(Name = "メッセージ")]
        [StringLength(500, ErrorMessage = "メッセージは500文字以内で入力してください。")]
        public string Message { get; set; }
        [Display(Name = "投稿日時")]
        public DateTimeOffset Created { get; set; }
        [Display(Name = "ジャンル")]
        public Genre Genre { get; set; }
        [Display(Name = "状態")]
        public State State { get; set; } = (State)1;

        [Display(Name = "認証")]
        public int Certification { get; set; }


        public string UserAccountId { get; set; }
        [ForeignKey("UserAccountId")]
        [ValidateNever]
        public virtual UserAccount UserAccount { get; set; }
        [ValidateNever]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        [ValidateNever]
        public virtual ICollection<Adoption> Adoptions { get; set; }
        [ValidateNever]
        public virtual ICollection<Reply> Replys { get; set; }
        [ValidateNever]
        public virtual ICollection<RollCall> RollCalls { get; set; }
        [ValidateNever]
        public virtual ICollection<Review> Reviews { get; set; }
        [ValidateNever]
        public virtual ICollection<Confirm> Confirms { get; set; }
        [ValidateNever]
        public virtual ICollection<PaymentRecord> PaymentRecords { get; set; }
    }
}
