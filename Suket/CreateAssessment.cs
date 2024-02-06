namespace Suket
{
    using System;
    using Google.Api.Gax.ResourceNames;
    using Google.Cloud.RecaptchaEnterprise.V1;

    public class RecaptchaResult
    {
        public bool IsValid { get; set; }
        public double Score { get; set; }
        public List<string> ErrorReasons { get; set; }

        public RecaptchaResult()
        {
            ErrorReasons = new List<string>();
        }
    }

    public class CreateAssessment
    {
        // 評価を作成して UI アクションのリスクを分析する。
        // projectID: Google Cloud プロジェクト ID
        // recaptchaKey: サイト / アプリに関連付けられた reCAPTCHA キー
        // token: クライアントから取得した生成トークン。
        // recaptchaAction: トークンに対応するアクション名。
        public RecaptchaResult createAssessment(string token)
        {
            string projectID = "mint-sports-1705906453461";
            string recaptchaKey = "6LfkLlgpAAAAAHfVfEflnhOKh02lfbWX5pirZaqH";
            string recaptchaAction = "submit";
            var client = RecaptchaEnterpriseServiceClient.Create();
            var projectName = new ProjectName(projectID);

            Console.WriteLine("read createassessment");
            var createAssessmentRequest = new CreateAssessmentRequest
            {
                Assessment = new Assessment
                {
                    Event = new Event
                    {
                        SiteKey = recaptchaKey,
                        Token = token,
                        ExpectedAction = recaptchaAction
                    },
                },
                ParentAsProjectName = projectName
            };

            var response = client.CreateAssessment(createAssessmentRequest);
            var result = new RecaptchaResult
            {
                IsValid = response.TokenProperties.Valid,
                Score = response.RiskAnalysis.Score
            };

            if (!result.IsValid)
            {
                result.ErrorReasons.Add(response.TokenProperties.InvalidReason.ToString());
            }

            return result;
        }

    }
}
