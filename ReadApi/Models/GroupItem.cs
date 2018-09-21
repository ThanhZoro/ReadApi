namespace ReadApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupItem
    {
        /// <summary>
        /// 
        /// </summary>
        public KeyGroup Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Count { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class KeyGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public KeyGroup(string companyId, string surveyHeaderId)
        {
            CompanyId = companyId;
            SurveyHeaderId = surveyHeaderId;
        }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SurveyHeaderId { get; set; }
    }
}
